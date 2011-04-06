
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Peach.Core;
using Peach.Core.Dom;

namespace Peach.Core.Agent
{
	[Agent("tcp")]
	public class AgentServerTcpRemoting : AgentServer
	{
		AgentServiceTcpRemote proxy = null;

		public AgentServerTcpRemoting()
		{
		}

		public override bool SupportedProtocol(string protocol)
		{
			protocol = protocol.ToLower();
			if (protocol == "tcp")
				return true;

			return false;
		}

		public override void AgentConnect(string name, string url, string password)
		{
			if (proxy != null)
				AgentDisconnect();

			TcpChannel chan = new TcpChannel();
			ChannelServices.RegisterChannel(chan, false); // Disable security for speed
			AgentServiceTcpRemote remObject = (AgentServiceTcpRemote)Activator.GetObject(typeof(AgentServiceTcpRemote), url);
			if (remObject == null)
				throw new ApplicationException("Error, unable to connect to remote agent " + url);
		}

		public override void AgentDisconnect()
		{
			proxy.AgentDisconnect();
			proxy = null;
		}

		public override void StartMonitor(string name, string cls, Dictionary<string, Variant> args)
		{
			proxy.StartMonitor(name, cls, args);
		}

		public override void StopMonitor(string name)
		{
			proxy.StopMonitor(name);
		}

		public override void StopAllMonitors()
		{
			proxy.StopAllMonitors();
		}

		public override void SessionStarting()
		{
			proxy.SessionStarting();
		}

		public override void SessionFinished()
		{
			proxy.SessionFinished();
		}

		public override void IterationStarting(int iterationCount, bool isReproduction)
		{
			proxy.IterationStarting(iterationCount, isReproduction);
		}

		public override bool IterationFinished()
		{
			return proxy.IterationFinished();
		}

		public override bool DetectedFault()
		{
			return proxy.DetectedFault();
		}

		public override Hashtable GetMonitorData()
		{
			return proxy.GetMonitorData();
		}

		public override bool MustStop()
		{
			return proxy.MustStop();
		}

		public override Variant Message(string name, Variant data)
		{
			return proxy.Message(name, data);
		}
	}

	/// <summary>
	/// Implement agent service running over XML-RPC.
	/// </summary>
	public class AgentServiceTcpRemote : MarshalByRefObject, IAgent
	{
		public IAgent agent = null;

		public void AgentConnect(string password)
		{
			agent.AgentConnect(password);
		}

		public void AgentDisconnect()
		{
			agent.AgentDisconnect();
		}

		public void StartMonitor(string name, string cls, Dictionary<string, Variant> args)
		{
			agent.StartMonitor(name, cls, args);
		}

		public void StopMonitor(string name)
		{
			agent.StopMonitor(name);
		}

		public void StopAllMonitors()
		{
			agent.StopAllMonitors();
		}

		public void SessionStarting()
		{
			agent.SessionStarting();
		}

		public void SessionFinished()
		{
			agent.SessionFinished();
		}

		public void IterationStarting(int iterationCount, bool isReproduction)
		{
			agent.IterationStarting(iterationCount, isReproduction);
		}

		public bool IterationFinished()
		{
			return agent.IterationFinished();
		}

		public bool DetectedFault()
		{
			return agent.DetectedFault();
		}

		public Hashtable GetMonitorData()
		{
			return agent.GetMonitorData();
		}

		public bool MustStop()
		{
			return agent.MustStop();
		}

		public Variant Message(string name, Variant data)
		{
			return agent.Message(name, data);
		}
	}
}

// end
