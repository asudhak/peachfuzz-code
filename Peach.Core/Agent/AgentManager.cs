
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using Peach.Core.Dom;
using NLog;
using Peach.Core.Agent;
using Peach.Core.IO;

namespace Peach.Core.Agent
{
	/// <summary>
	/// Manages all agents.  This includes
	/// full lifetime.
	/// </summary>
	[Serializable]
	public class AgentManager
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		static int UniqueNames = 0;
		[NonSerialized]
		OrderedDictionary<string, AgentClient> _agents = new OrderedDictionary<string, AgentClient>();
		[NonSerialized]
		Dictionary<string, Dom.Agent> _agentDefinitions = new Dictionary<string, Dom.Agent>();

		public AgentManager(RunContext context)
		{
            context.CollectFaults += new RunContext.CollectFaultsHandler(context_CollectFaults);
		}

        void context_CollectFaults(RunContext context)
        {
			// If the engine has recorded faults or any monitor detected a fault,
			// gather data from all monitors.
			// NOTE: We must test DetectedFault() first, as monitors expect this
			// call to occur before any call to GetMonitorData()
            if (DetectedFault() || context.faults.Count > 0)
            {
				logger.Debug("Fault detected.  Collecting monitor data.");

                var agentFaults = GetMonitorData();

                foreach (var item in agentFaults)
                {
                    var faults = item.Value;

                    foreach (var fault in faults)
                    {
                        if (fault == null)
                            continue;

                        fault.agentName = item.Key.name;
                        context.faults.Add(fault);
                    }
                }
            }
        }

		private void AddAgent(Dom.Agent agentDef)
		{
			Uri uri = new Uri(agentDef.url);
			var type = ClassLoader.FindTypeByAttribute<AgentAttribute>((x, y) => y.protocol == uri.Scheme);
			if (type == null)
				throw new PeachException("Error, unable to locate agent that supports the '" + uri.Scheme + "' protocol.");

			var agent = Activator.CreateInstance(type, agentDef.name, agentDef.url, agentDef.password) as AgentClient;
			_agents[agentDef.name] = agent;
			_agentDefinitions[agentDef.name] = agentDef;
		}

		private void AgentConnect(string name)
		{
			logger.Trace("AgentConnect: {0}", name);

			Dom.Agent def = _agentDefinitions[name];
			AgentClient agent = _agents[name];

			try
			{
				agent.AgentConnect(def.name, def.url, def.password);
			}
			catch
			{
				_agents.Remove(name);
				_agentDefinitions.Remove(name);
				throw;
			}

			foreach (Dom.Monitor mon in def.monitors)
			{
				string monitorName = mon.name != null ? mon.name : "Monitor_" + UniqueNames++;
				agent.StartMonitor(monitorName, mon.cls, mon.parameters);
			}
		}

		public virtual void AgentConnect(Dom.Agent agent)
		{
			if (!_agents.Keys.Contains(agent.name))
				AddAgent(agent);

			AgentConnect(agent.name);
		}

		public virtual AgentClient GetAgent(string name)
		{
			return _agents[name];
		}

		#region AgentServer

		public virtual Publisher CreatePublisher(string agentName, string pubName, SerializableDictionary<string, Variant> args)
		{
			AgentClient agent;
			if (!_agents.TryGetValue(agentName, out agent))
				throw new KeyNotFoundException("Could not find agent named '" + agentName + "'.");

			return agent.CreatePublisher(pubName, args);
		}

		public virtual BitwiseStream CreateBitwiseStream(string agentName)
		{
			AgentClient agent;
			if (!_agents.TryGetValue(agentName, out agent))
				throw new KeyNotFoundException("Could not find agent named '" + agentName + "'.");

			return agent.CreateBitwiseStream();
		}

		public virtual void StopAllMonitors()
		{
			logger.Trace("StopAllMonitors");
			foreach (var agent in _agents.Values.Reverse())
			{
				Guard("StopAllMonitors", () =>
				{
					agent.StopAllMonitors();
				});
			}
		}

		public virtual void Shutdown()
		{
			logger.Trace("Shutdown");
			foreach (AgentClient agent in _agents.Values.Reverse())
			{
				Guard("Shutdown", () =>
				{
					agent.AgentDisconnect();
				});
			}
		}

		public virtual void SessionStarting()
		{
			logger.Trace("SessionStarting");
			foreach (AgentClient agent in _agents.Values)
			{
				agent.SessionStarting();
			}
		}

		public virtual void SessionFinished()
		{
			logger.Trace("SessionFinished");
			foreach (AgentClient agent in _agents.Values.Reverse())
			{
				Guard("SessionFinished", () =>
				{
					agent.SessionFinished();
				});
			}
		}

		public virtual void IterationStarting(uint iterationCount, bool isReproduction)
		{
			logger.Trace("IterationStarting");
			foreach (AgentClient agent in _agents.Values)
			{
				Guard("IterationStarting", () =>
				{
					agent.IterationStarting(iterationCount, isReproduction);
				});
			}
		}

		public virtual bool IterationFinished()
		{
			logger.Trace("IterationFinished");
			bool ret = false;

			foreach (AgentClient agent in _agents.Values.Reverse())
			{
				Guard("IterationFinished", () =>
				{
					if (agent.IterationFinished())
						ret = true;
				});
			}

			return ret;
		}

		public virtual bool DetectedFault()
		{
			bool ret = false;

			foreach (AgentClient agent in _agents.Values)
			{
				Guard("DetectedFault", () =>
				{
					if (agent.DetectedFault())
						ret = true;
				});
			}

			logger.Trace("DetectedFault: {0}", ret);
			return ret;
		}

		public virtual Dictionary<AgentClient, Fault[]> GetMonitorData()
		{
			logger.Trace("GetMonitorData");
			Dictionary<AgentClient, Fault[]> faults = new Dictionary<AgentClient, Fault[]>();

			foreach (AgentClient agent in _agents.Values)
			{
				Guard("GetMonitorData", () =>
				{
					faults[agent] = agent.GetMonitorData();
				});
			}

			return faults;
		}

		public virtual bool MustStop()
		{
			bool ret = false;

			foreach (AgentClient agent in _agents.Values)
			{
				Guard("MustStop", () =>
				{
					if (agent.MustStop())
						ret = true;
				});
			}

			logger.Trace("MustStop: {0}", ret.ToString());
			return ret;
		}

		public virtual Variant Message(string name, Variant data)
		{
			logger.Debug("Message: {0} => {1}", name, data.ToString());
			Variant ret = null;
			Variant tmp = null;

			foreach (AgentClient agent in _agents.Values)
			{
				Guard("Message", () =>
				{
					tmp = agent.Message(name, data);
					if (tmp != null)
						ret = tmp;
				});
			}

			return ret;
		}

		private static void Guard(string what, System.Action action)
		{
			try
			{
				action();
			}
			catch (SoftException)
			{
				throw;
			}
			catch (PeachException)
			{
				throw;
			}
			catch (Exception ex)
			{
				logger.Warn("Ignoring exception calling {0}: {1}", what, ex.Message);
			}
		}

		#endregion
	}
}

// end
