
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using Peach.Core;
using Peach.Core.Dom;
using NLog;
using Peach.Core.Agent;

namespace Peach.Core.Agent.Channels
{
	[Agent("tcp", true)]
	public class AgentClientTcpRemoting : AgentClient
	{
		AgentServiceTcpRemote proxy = null;
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public AgentClientTcpRemoting(string name, string uri, string password)
		{
		}

		public override bool SupportedProtocol(string protocol)
		{
			logger.Trace("SupportedProtocol");
			OnSupportedProtocolEvent(protocol);

			protocol = protocol.ToLower();
			if (protocol == "tcp")
				return true;

			return false;
		}

		public override void AgentConnect(string name, string url, string password)
		{
			logger.Trace("AgentConnect");
			OnAgentConnectEvent(name, url, password);

			if (proxy != null)
				AgentDisconnect();

			if (url.EndsWith("/"))
				url += "PeachAgent";
			else
				url += "/PeachAgent";

			TcpChannel chan = new TcpChannel();
			ChannelServices.RegisterChannel(chan, false); // Disable security for speed
			proxy = (AgentServiceTcpRemote)Activator.GetObject(typeof(AgentServiceTcpRemote), url);
			if (proxy == null)
				throw new ApplicationException("Error, unable to connect to remote agent " + url);
		}

		public override void AgentDisconnect()
		{
			logger.Trace("AgentDisconnect");
			OnAgentDisconnectEvent();

			proxy.AgentDisconnect();
			proxy = null;
		}

		public override void StartMonitor(string name, string cls, Dictionary<string, Variant> args)
		{
			logger.Trace("StartMonitor: {0}, {1}", name, cls);
			OnStartMonitorEvent(name, cls, args);
			proxy.StartMonitor(name, cls, args);
		}

		public override void StopMonitor(string name)
		{
			logger.Trace("AgentConnect: {0}", name);
			OnStopMonitorEvent(name);
			proxy.StopMonitor(name);
		}

		public override void StopAllMonitors()
		{
			logger.Trace("StopAllMonitors");
			OnStopAllMonitorsEvent();
			proxy.StopAllMonitors();
		}

		public override void SessionStarting()
		{
			logger.Trace("SessionStarting");
			OnSessionStartingEvent();
			proxy.SessionStarting();
		}

		public override void SessionFinished()
		{
			logger.Trace("SessionFinished");
			OnSessionFinishedEvent();
			proxy.SessionFinished();
		}

		public override void IterationStarting(int iterationCount, bool isReproduction)
		{
			logger.Trace("IterationStarting: {0}, {1}", iterationCount, isReproduction);
			OnIterationStartingEvent(iterationCount, isReproduction);
			proxy.IterationStarting(iterationCount, isReproduction);
		}

		public override bool IterationFinished()
		{
			logger.Trace("IterationFinished");
			OnIterationFinishedEvent();
			return proxy.IterationFinished();
		}

		public override bool DetectedFault()
		{
			logger.Trace("DetectedFault");
			OnDetectedFaultEvent();
			return proxy.DetectedFault();
		}

		public override Hashtable GetMonitorData()
		{
			logger.Trace("GetMonitorData");
			OnGetMonitorDataEvent();
			return proxy.GetMonitorData();
		}

		public override bool MustStop()
		{
			logger.Trace("MustStop");
			OnMustStopEvent();
			return proxy.MustStop();
		}

		public override Variant Message(string name, Variant data)
		{
			logger.Trace("Message: {0}", name);
			OnMessageEvent(name, data);
			return proxy.Message(name, data);
		}
	}

	/// <summary>
	/// Implement agent service running over XML-RPC.
	/// </summary>
	public class AgentServiceTcpRemote : MarshalByRefObject, IAgent
	{
		public Agent agent = null;
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public AgentServiceTcpRemote()
		{
			agent = new Agent("AgentServiceTcpRemote");
		}

		public void AgentConnect(string password)
		{
			logger.Trace("AgentConnect");
			agent.AgentConnect();
		}

		public void AgentDisconnect()
		{
			logger.Trace("AgentDisconnect");
			agent.AgentDisconnect();
		}

		public void StartMonitor(string name, string cls, Dictionary<string, Variant> args)
		{
			logger.Trace("StartMonitor: {0}, {1}", name, cls);
			agent.StartMonitor(name, cls, args);
		}

		public void StopMonitor(string name)
		{
			logger.Trace("AgentConnect: {0}", name);
			agent.StopMonitor(name);
		}

		public void StopAllMonitors()
		{
			logger.Trace("StopAllMonitors");
			agent.StopAllMonitors();
		}

		public void SessionStarting()
		{
			logger.Trace("SessionStarting");
			agent.SessionStarting();
		}

		public void SessionFinished()
		{
			logger.Trace("SessionFinished");
			agent.SessionFinished();
		}

		public void IterationStarting(int iterationCount, bool isReproduction)
		{
			logger.Trace("IterationStarting: {0}, {1}", iterationCount, isReproduction);
			agent.IterationStarting(iterationCount, isReproduction);
		}

		public bool IterationFinished()
		{
			logger.Trace("IterationFinished");
			return agent.IterationFinished();
		}

		public bool DetectedFault()
		{
			logger.Trace("DetectedFault");
			return agent.DetectedFault();
		}

		public Hashtable GetMonitorData()
		{
			logger.Trace("GetMonitorData");
			return agent.GetMonitorData();
		}

		public bool MustStop()
		{
			logger.Trace("MustStop");
			return agent.MustStop();
		}

		public Variant Message(string name, Variant data)
		{
			logger.Trace("Message: {0}", name);
			return agent.Message(name, data);
		}
	}

	[AgentServer("tcp")]
	public class AgentServerTcpRemoting : IAgentServer
	{
		#region IAgentServer Members

		public void Run(Dictionary<string, string> args)
		{
			int port = 9001;

			if (args.ContainsKey("port"))
				port = int.Parse(args["port"]);

			//select channel to communicate
			TcpChannel chan = new TcpChannel(port);
			ChannelServices.RegisterChannel(chan, false);    //register channel

			//register remote object
			RemotingConfiguration.RegisterWellKnownServiceType(
				typeof(AgentServiceTcpRemote),
				"PeachAgent", WellKnownObjectMode.Singleton);

			//inform console
			Console.WriteLine(" -- Press ENTER to quit agent -- ");
			Console.ReadLine();
		}

		#endregion
	}
}

// end
