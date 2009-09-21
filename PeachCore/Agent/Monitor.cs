
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeachCore.Agent
{
	/// <summary>
	/// Monitors are hosted by agent processes and are
	/// able to report detected faults and gather information
	/// that is usefull when a fualt is detected.
	/// </summary>
	internal class Monitor
	{
		internal Monitor(Dictionary<string, string> args)
		{
		}

		/// <summary>
		/// Called before start of test.
		/// </summary>
		public abstract void OnTestStarting();

		/// <summary>
		/// Called when test is completed.
		/// </summary>
		public abstract void OnTestFinished();

		/// <summary>
		/// Allows monitor to indicate a fault was detected.
		/// </summary>
		/// <returns>True if fault was detected, else False</returns>
		public abstract bool DetectedFault();

		/// <summary>
		/// Called when a fault was detected.
		/// </summary>
		public abstract void OnFault();

		/// <summary>
		/// Called to get any data that was collected.
		/// </summary>
		public abstract void GetData();

		/// <summary>
		/// Called to shutdown current monitor.
		/// </summary>
		public abstract void OnShutdown();

		/// <summary>
		/// Allows monitor to stop test run by returning false.
		/// </summary>
		/// <returns>False to stop run, else True</returns>
		public abstract bool StopRun();

		/// <summary>
		/// Called when a call action is being performed.  Call
		/// actions are used to launch programs, this gives the
		/// monitor a chance to determin if it should be running
		/// the program under a debugger instead.
		/// 
		/// Note: This is a bit of a hack to get this working
		/// </summary>
		/// <param name="method"></param>
		/// <param name="args"></param>
		public abstract void PublisherCall(string method, object args);
	}
}
