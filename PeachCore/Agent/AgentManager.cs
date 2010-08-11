using System;
using System.Collections.Generic;
using System.Text;

namespace PeachCore.Agent
{
	/// <summary>
	/// Managed a set of Agents and handles
	/// multiplexing messages, reconnecting, etc.
	/// </summary>
    public class AgentManager : AgentServer
    {
		Dictionary<string, AgentServer> _agents = new Dictionary<string, AgentServer>();
		List<AgentServer> _agentsOrdered = new List<AgentServer>();

		uint _reconnectCount = 10;
		uint _connectTimeout = 10;

		public AgentManager()
		{
		}

		public uint ReconnectCount
		{
			get { return _reconnectCount; }
			set { _reconnectCount = value; }
		}

		public uint ConnectTimeout
		{
			get { return _connectTimeout; }
			set { _connectTimeout = value; }
		}

		public override bool SupportedProtocol(string protocol)
		{
			// Should not be asking the manager :)
			throw new NotImplementedException();
		}

		public override void AgentConnect(string name, string url, string password)
		{
			// Add a new agent to our list.

			// TODO - Locate all agentservers and ask them if they support
			//        our URL.

			// TODO - Add reconnect and timeout logic

			AgentServer agent = new AgentServerXmlRpc();
			agent.AgentConnect(name, url, password);

			_agents[name] = agent;
			_agentsOrdered.Add(agent);
		}

		public virtual void AgentDisconnect(string name)
		{
			AgentServer agent = _agents[name];

			try
			{
				agent.AgentDisconnect();
			}
			catch
			{
			}

			_agents.Remove(name);
			_agentsOrdered.Remove(agent);
		}

		public override void AgentDisconnect()
		{
			foreach (AgentServer agent in _agentsOrdered)
			{
				try
				{
					agent.AgentDisconnect();
				}
				catch
				{
				}
			}

			_agents.Clear();
			_agentsOrdered.Clear();
		}

		public override void StartMonitor(string name, string cls, Dictionary<string, string> args)
		{
			throw new NotImplementedException();
		}

		public override void StopMonitor(string name)
		{
			throw new NotImplementedException();
		}

		public override void StopAllMonitors()
		{
			throw new NotImplementedException();
		}

		public override void SessionStarting()
		{
			throw new NotImplementedException();
		}

		public override void SessionFinished()
		{
			throw new NotImplementedException();
		}

		public override void IterationStarting(int iterationCount, bool isReproduction)
		{
			throw new NotImplementedException();
		}

		public override bool IterationFinished()
		{
			throw new NotImplementedException();
		}

		public override bool DetectedFault()
		{
			throw new NotImplementedException();
		}

		public override System.Collections.Hashtable GetMonitorData()
		{
			throw new NotImplementedException();
		}

		public override bool MustStop()
		{
			throw new NotImplementedException();
		}
	}
}

// end
