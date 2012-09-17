
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
using System.Reflection;
using Peach.Core.Dom;
using NLog;
using Peach.Core.Agent;

namespace Peach.Core.Agent
{
	/// <summary>
	/// Manages all agents.  This includes
	/// full lifetime.
	/// </summary>
	public class AgentManager
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		static int UniqueNames = 0;
		OrderedDictionary<string, AgentClient> _agents = new OrderedDictionary<string, AgentClient>();
		Dictionary<string, Dom.Agent> _agentDefinitions = new Dictionary<string, Dom.Agent>();

		public AgentManager()
		{
		}

		public virtual void AddAgent(Dom.Agent agentDef)
		{
			Uri uri = new Uri(agentDef.url);
			var type = ClassLoader.FindTypeByAttribute<AgentAttribute>((x, y) => y.protocol == uri.Scheme);
			if (type == null)
				throw new PeachException("Error, unable to locate agent that supports the '" + uri.Scheme + "' protocol.");

			var agent = Activator.CreateInstance(type, agentDef.name, agentDef.url, agentDef.password) as AgentClient;
			_agents[agentDef.name] = agent;
			_agentDefinitions[agentDef.name] = agentDef;
		}

		public virtual void AgentConnect(string name)
		{
			logger.Trace("AgentConnect: {0}", name);

			Dom.Agent def = _agentDefinitions[name];
			AgentClient agent = _agents[name];

			agent.AgentConnect(def.name, def.url, def.password);

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

		#region AgentServer

		public virtual void StopAllMonitors()
		{
			logger.Trace("StopAllMonitors");
			foreach (AgentClient agent in _agents.Values)
				agent.StopAllMonitors();
		}

		public virtual void SessionStarting()
		{
			logger.Trace("SessionStarting");
			foreach (AgentClient agent in _agents.Values)
				agent.SessionStarting();
		}

		public virtual void SessionFinished()
		{
			logger.Trace("SessionFinished");
			foreach (AgentClient agent in _agents.Values)
				agent.SessionFinished();
		}

		public virtual void IterationStarting(uint iterationCount, bool isReproduction)
		{
			logger.Trace("IterationStarting");
			foreach (AgentClient agent in _agents.Values)
				agent.IterationStarting(iterationCount, isReproduction);
		}

		public virtual bool IterationFinished()
		{
			logger.Trace("IterationFinished");
			bool ret = false;

			foreach (AgentClient agent in _agents.Values)
				if (agent.IterationFinished())
					ret = true;

			return ret;
		}

		public virtual bool DetectedFault()
		{
			bool ret = false;

			foreach (AgentClient agent in _agents.Values)
				if (agent.DetectedFault())
					ret = true;

			logger.Trace("DetectedFault: {0}", ret);
			return ret;
		}

		public virtual Dictionary<AgentClient, System.Collections.Hashtable> GetMonitorData()
		{
			logger.Trace("GetMonitorData");
			Dictionary<AgentClient, System.Collections.Hashtable> data = new Dictionary<AgentClient, System.Collections.Hashtable>();

			foreach (AgentClient agent in _agents.Values)
				data[agent] = agent.GetMonitorData();

			return data;
		}

		public virtual bool MustStop()
		{
			bool ret = false;
			foreach (AgentClient agent in _agents.Values)
				if (agent.MustStop())
					ret = true;

			logger.Trace("MustStop: {0}", ret.ToString());
			return ret;
		}

		public virtual Variant Message(string name, Variant data)
		{
			logger.Trace("Message: {0}", name);
			Variant ret = null;
			Variant tmp = null;

			foreach (AgentClient agent in _agents.Values)
			{
				tmp = agent.Message(name, data);
				if (tmp != null)
					ret = tmp;
			}

			return ret;
		}

		#endregion
	}
}
