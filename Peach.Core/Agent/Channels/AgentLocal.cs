
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
using Peach.Core.Dom;
using NLog;
using Peach.Core.Agent;

namespace Peach.Core.Agent.Channels
{
	/// <summary>
	/// This is an agent that runs in the local
	/// process, instead of a remote process.  This
	/// is much faster for things like file fuzzing.
	/// </summary>
	[Agent("local", true)]
	public class AgentServerLocal : AgentClient
	{
		Agent agent = null;

		public AgentServerLocal(string name, string uri, string password)
		{
			agent = new Agent(name);
		}

		public override bool SupportedProtocol(string protocol)
		{
			if (protocol == "local")
				return true;

			return false;
		}

		public override void AgentConnect(string name, string url, string password)
		{
			agent.AgentConnect();
		}

		public override void AgentDisconnect()
		{
			agent.AgentDisconnect();
		}

		public override Publisher CreatePublisher(string cls, SerializableDictionary<string, Variant> args)
		{
			return agent.CreatePublisher(cls, args);
		}

		public override void StartMonitor(string name, string cls, SerializableDictionary<string, Variant> args)
		{
			agent.StartMonitor(name, cls, args);
		}

		public override void StopMonitor(string name)
		{
			agent.StopMonitor(name);
		}

		public override void StopAllMonitors()
		{
			agent.StopAllMonitors();
		}

		public override void SessionStarting()
		{
			agent.SessionStarting();
		}

		public override void SessionFinished()
		{
			agent.SessionFinished();
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			agent.IterationStarting(iterationCount, isReproduction);
		}

		public override bool IterationFinished()
		{
			return agent.IterationFinished();
		}

		public override bool DetectedFault()
		{
			return agent.DetectedFault();
		}

		public override Fault[] GetMonitorData()
		{
			return agent.GetMonitorData();
		}

		public override bool MustStop()
		{
			return agent.MustStop();
		}

		public override Variant Message(string name, Variant data)
		{
			return agent.Message(name, data);
		}
	}
}
