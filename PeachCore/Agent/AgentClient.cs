
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
using System.Text;
using CookComputing.XmlRpc;

namespace PeachCore.Agent
{
	/// <summary>
	/// </summary>
	public class Agent : IAgent
	{
		public object parent;
        Dictionary<string, Monitor> monitors = new Dictionary<string, Monitor>();
        string name;
        string url;
        string password;

        public Agent(string name, string url, string password)
        {
        }

        /// <summary>
        /// Agent will not return from this method.
        /// </summary>
        public void Run()
        {
        }

        #region IAgent Members

        public void AgentConnect(string password)
        {
            if (this.password == null)
            {
                if(password != null)
                    throw new Exception("Authentication failure");
            }
            else if (this.password == password)
            {
                // All good!
            }
        }

        public void AgentDisconnect()
        {
            StopAllMonitors();
            monitors.Clear();
        }

        public void StartMonitor(string name, string cls, Dictionary<string, string> args)
        {
            throw new NotImplementedException();
        }

        public void StopMonitor(string name)
        {
            monitors[name].StopMonitor();
            monitors.Remove(name);
        }

        public void StopAllMonitors()
        {
            foreach (Monitor monitor in monitors.Values)
                monitor.StopMonitor();

            monitors.Clear();
        }

        public void SessionStarting()
        {
            throw new NotImplementedException();
        }

        public void SessionFinished()
        {
            throw new NotImplementedException();
        }

        public void IterationStarting(int iterationCount, bool isReproduction)
        {
            foreach (Monitor monitor in monitors.Values)
                monitor.IterationStarting(iterationCount, isReproduction);
        }

        public bool IterationFinished()
        {
            bool replay = false;
            foreach (Monitor monitor in monitors.Values)
                if (monitor.IterationFinished())
                    replay = true;

            return replay;
        }

        public bool DetectedFault()
        {
            bool detectedFault = false;
            foreach (Monitor monitor in monitors.Values)
                if (monitor.DetectedFault())
                    detectedFault = true;

            return detectedFault;
        }

        public Hashtable GetMonitorData()
        {
            throw new NotImplementedException();
        }

        public bool MustStop()
        {
            foreach (Monitor monitor in monitors.Values)
                if (monitor.MustStop())
                    return true;
            
            return false;
        }

        #endregion
    }

    public interface IAgent
    {
        void AgentConnect(string password);
        void AgentDisconnect();
        void StartMonitor(string name, string cls, Dictionary<string, string> args);
        void StopMonitor(string name);
        void StopAllMonitors();
        void SessionStarting();
        void SessionFinished();
        void IterationStarting(int iterationCount, bool isReproduction);
        bool IterationFinished();
        bool DetectedFault();
        Hashtable GetMonitorData();
        bool MustStop();
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
        public void StartMonitor(string name, string cls, Dictionary<string, string> args)
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
    }
}
