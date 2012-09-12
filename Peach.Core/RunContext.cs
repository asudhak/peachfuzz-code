
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
using System.Reflection;
using System.Threading;

using Peach.Core.Agent;
using Peach.Core.Dom;

namespace Peach.Core
{
	[Serializable]
	public class RunContext
	{
		public delegate void DebugEventHandler(DebugLevel level, RunContext context, string from, string msg);
		public event DebugEventHandler Debug;

		public void CriticalMessage(string from, string msg)
		{
			if (Debug != null)
				Debug(DebugLevel.Critical, this, from, msg);
		}

		public void WarningMessage(string from, string msg)
		{
			if (Debug != null)
				Debug(DebugLevel.Warning, this, from, msg);
		}

		public void DebugMessage(DebugLevel level, string from, string msg)
		{
			if (config.debug && Debug != null)
				Debug(level, this, from, msg);
		}

		public RunConfiguration config = null;
		public Dom.Dom dom = null;
		public Test test = null;
		public AgentManager agentManager = null;

		public bool needDataModel = true;

		/// <summary>
		/// Controls if we continue fuzzing or exit
		/// after current iteration.  This can be used
		/// by UI code to stop Peach.
		/// </summary>
		public bool continueFuzzing = true;

		#region Reproduce Fault

		/// <summary>
		/// True when we have found a fault and are in the process
		/// of reproducing it.
		/// </summary>
		/// <remarks>
		/// Many times, especially with network fuzzing, the iteration we detect a fault on is not the
		/// correct iteration, or the fault requires multiple iterations to reproduce.
		/// 
		/// Peach will start reproducing at the current iteration count then start moving backwards
		/// until we locate the iteration causing the crash, or reach our max back search value.
		/// </remarks>
		public bool reproducingFault = false;

		/// <summary>
		/// Number of iteration to search backwards trying to reproduce a fault.
		/// </summary>
		/// <remarks>
		/// Many times, especially with network fuzzing, the iteration we detect a fault on is not the
		/// correct iteration, or the fault requires multiple iterations to reproduce.
		/// 
		/// Peach will start reproducing at the current iteration count then start moving backwards
		/// until we locate the iteration causing the crash, or reach our max back search value.
		/// </remarks>
		public uint reproducingMaxBacksearch = 100;

		/// <summary>
		/// The initial iteration we detected fault on
		/// </summary>
		public uint reproducingInitialIteration = 0;

		/// <summary>
		/// This value times current iteration change is next iteration change.
		/// </summary>
		/// <remarks>
		/// Intial search process:
		/// 
		/// Move back 1
		/// Move back 1 * reproducingSkipMultiple = N
		/// Move back N * reproducingSkipMultiple = M
		/// Move back M * reproducingSkipMultiple = O
		/// Move back O * reproducingSkipMultiple ...
		/// 
		/// </remarks>
		public uint reproducingSkipMultiple = 2;

		/// <summary>
		/// Number of iterations to jump.
		/// </summary>
		/// <remarks>
		/// Initializes to 1, then multiply against reproducingSkipMultiple
		/// </remarks>
		public uint reproducingIterationJumpCount = 1;

		#endregion
	}
}

// end
