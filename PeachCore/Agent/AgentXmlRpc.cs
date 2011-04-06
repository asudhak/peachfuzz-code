
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
using Peach.Core;
using Peach.Core.Dom;
using CookComputing.XmlRpc;
using NLog;

namespace Peach.Core.Agent
{
	[Agent("http")]
	[Agent("https")]
	public class AgentServerXmlRpc : AgentServer
	{
		IAgentClientXmlRpc proxy = null;

		public AgentServerXmlRpc()
		{
		}

		public override bool SupportedProtocol(string protocol)
		{
			protocol = protocol.ToLower();
			if (protocol == "http" || protocol == "https")
				return true;

			return false;
		}

		public override void AgentConnect(string name, string url, string password)
		{
			if (proxy != null)
				AgentDisconnect();

			proxy = (IAgentClientXmlRpc)XmlRpcProxyGen.Create(typeof(IAgentClientXmlRpc));
			proxy.Url = url;
			proxy.AgentConnect(password);
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
	public class AgentService : XmlRpcService, IAgent
	{
		public IAgent agent = null;

		[XmlRpcMethod("AgentConnect")]
		public void AgentConnect(string password)
		{
			agent.AgentConnect(password);
		}

		[XmlRpcMethod("AgentDisconnect")]
		public void AgentDisconnect()
		{
			agent.AgentDisconnect();
		}

		[XmlRpcMethod("StartMonitor")]
		public void StartMonitor(string name, string cls, Dictionary<string, Variant> args)
		{
			agent.StartMonitor(name, cls, args);
		}

		[XmlRpcMethod("StopMonitor")]
		public void StopMonitor(string name)
		{
			agent.StopMonitor(name);
		}

		[XmlRpcMethod("StopAllMonitors")]
		public void StopAllMonitors()
		{
			agent.StopAllMonitors();
		}

		[XmlRpcMethod("SessionStarting")]
		public void SessionStarting()
		{
			agent.SessionStarting();
		}

		[XmlRpcMethod("SessionFinished")]
		public void SessionFinished()
		{
			agent.SessionFinished();
		}

		[XmlRpcMethod("IterationStarting")]
		public void IterationStarting(int iterationCount, bool isReproduction)
		{
			agent.IterationStarting(iterationCount, isReproduction);
		}
		[XmlRpcMethod("IterationFinished")]
		public bool IterationFinished()
		{
			return agent.IterationFinished();
		}

		[XmlRpcMethod("DetectedFault")]
		public bool DetectedFault()
		{
			return agent.DetectedFault();
		}

		[XmlRpcMethod("GetMonitorData")]
		public Hashtable GetMonitorData()
		{
			return agent.GetMonitorData();
		}

		[XmlRpcMethod("MustStop")]
		public bool MustStop()
		{
			return agent.MustStop();
		}

		[XmlRpcMethod("Message")]
		public Variant Message(string name, Variant data)
		{
			return agent.Message(name, data);
		}
	}

}
