
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
using System.Text;
using Peach.Core.Dom;

namespace Peach.Core.Agent
{
	/// <summary>
	/// Monitors are hosted by agent processes and are
	/// able to report detected faults and gather information
	/// that is usefull when a fualt is detected.
	/// </summary>
	public abstract class Monitor
	{
		IAgent _agent;

		public Monitor(IAgent agent, string name, Dictionary<string, Variant> args)
		{
			_agent = agent;
			Name = name;
		}

		public string Name { get; set; }

		protected IAgent Agent { get { return _agent; } }

        public abstract void StopMonitor();

        /// <summary>
        /// Starting a fuzzing session.  A session includes a number of test iterations.
        /// </summary>
        public abstract void SessionStarting();
        /// <summary>
        /// Finished a fuzzing session.
        /// </summary>
        public abstract void SessionFinished();

        /// <summary>
        /// Starting a new iteration
        /// </summary>
        /// <param name="iterationCount">Iteration count</param>
        /// <param name="isReproduction">Are we re-running an iteration</param>
        public abstract void IterationStarting(uint iterationCount, bool isReproduction);
        /// <summary>
        /// Iteration has completed.
        /// </summary>
        /// <returns>Returns true to indicate iteration should be re-run, else false.</returns>
        public abstract bool IterationFinished();


        /// <summary>
        /// Was a fault detected during current iteration?
        /// </summary>
        /// <returns>True if a fault was detected, else false.</returns>
        public abstract bool DetectedFault();

        /// <summary>
        /// Return a Fault instance
        /// </summary>
        /// <returns></returns>
        public abstract Fault GetMonitorData();

        /// <summary>
        /// Can the fuzzing session continue, or must we stop?
        /// </summary>
        /// <returns>True if session must stop, else false.</returns>
        public abstract bool MustStop();

		/// <summary>
		/// Send a message to the monitor and possibly get data back.
		/// </summary>
		/// <param name="name">Message name</param>
		/// <param name="data">Message data</param>
		/// <returns>Returns data or null.</returns>
		public abstract Variant Message(string name, Variant data);

		/// <summary>
		/// Process query from another monitor.
		/// </summary>
		/// <remarks>
		/// This method is used to respond to an information request
		/// from another monitor.  Debugger monitors may expose specific
		/// queryies such as "QueryPid" to get the running processes PID.
		/// </remarks>
		/// <param name="query">Query</param>
		/// <returns>Non-null response indicates query was handled.</returns>
		public virtual object ProcessQueryMonitors(string query)
		{
			return null;
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class MonitorAttribute : PluginAttribute
	{
		public MonitorAttribute(string name, bool isDefault = false)
			: base(typeof(Monitor), name, isDefault)
		{
		}
	}
}

// end
