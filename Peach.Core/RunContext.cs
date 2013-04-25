
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
	/// <summary>
	/// Contains state information regarding the current fuzzing run.
	/// </summary>
	[Serializable]
	public class RunContext
	{
		#region Events

        public delegate void CollectFaultsHandler(RunContext context);

        /// <summary>
        /// This event is triggered after an interation has occured to allow
        /// collection of faults into RunContext.faults collection.
        /// </summary>
        public event CollectFaultsHandler CollectFaults;

        public void OnCollectFaults()
        {
            if (CollectFaults != null)
                CollectFaults(this);
        }

		#endregion

		/// <summary>
		/// Configuration settings for this run
		/// </summary>
		public RunConfiguration config = null;

		/// <summary>
		/// Engine instance for this run
		/// </summary>
		[NonSerialized]
		public Engine engine = null;

		/// <summary>
		/// Dom to use for this run
		/// </summary>
		[NonSerialized]
		public Dom.Dom dom = null;

		/// <summary>
		/// Current test being run
		/// </summary>
		/// <remarks>
		/// Currently the Engine code sets this.
		/// </remarks>
		[NonSerialized]
		public Test test = null;

		/// <summary>
		/// Current agent manager for this run.
		/// </summary>
		/// <remarks>
		/// Currently the Engine code sets this.
		/// </remarks>
		[NonSerialized]
		public AgentManager agentManager = null;

		public bool needDataModel = true;

		/// <summary>
		/// An object store that will last entire run.  For use
		/// by Peach code to store some state.
		/// </summary>
		[NonSerialized]
		public Dictionary<string, object> stateStore = new Dictionary<string, object>();

		/// <summary>
		/// An object store that will last current iteration.
		/// </summary>
		[NonSerialized]
		public Dictionary<string, object> iterationStateStore = new Dictionary<string, object>();

		#region Control Iterations

		/// <summary>
		/// Is this a control iteration.  Control iterations are used
		/// to verify the system can still reliably fuzz and are performed
		/// with out any mutations applied.
		/// </summary>
		/// <remarks>
		/// The first iteration is a special control iteration.  We also
		/// perform control iterations after we have collected a fault.
		/// 
		/// In later version we will likely inject control iterations every 
		/// N iterations where N is >= 100.
		/// </remarks>
		public bool controlIteration = false;

		/// <summary>
		/// Is this control operation also a recording iteration?
		/// </summary>
		/// <remarks>
		/// Recording iterations set our controlActionsExecuted and 
		/// controlStatesExecuted arrays.
		/// </remarks>
		public bool controlRecordingIteration = false;

		/// <summary>
		/// Actions performed during first control iteration.  Used to validate
		/// control iterations that come later have same action coverage.
		/// </summary>
		public List<Dom.Action> controlRecordingActionsExecuted = new List<Dom.Action>();

		/// <summary>
		/// States performed during first control iteration.  Used to validate
		/// control iterations that come later have same state coverage.
		/// </summary>
		/// <remarks>
		/// This may not be required with action coverage.
		/// </remarks>
		public List<Dom.State> controlRecordingStatesExecuted = new List<State>();

		/// <summary>
		/// Actions performed during later control iterations.  Used to validate
		/// control iterations that come later have same action coverage.
		/// </summary>
		public List<Dom.Action> controlActionsExecuted = new List<Dom.Action>();

		/// <summary>
		/// States performed during later control iterations.  Used to validate
		/// control iterations that come later have same state coverage.
		/// </summary>
		/// <remarks>
		/// This may not be required with action coverage.
		/// </remarks>
		public List<Dom.State> controlStatesExecuted = new List<State>();

		#endregion

		#region Faults

		/// <summary>
        /// Faults for current iteration of fuzzing.  This collection
        /// is cleared after each iteration.
        /// </summary>
        /// <remarks>
        /// This collection should only be added to from the CollectFaults event.
        /// </remarks>
        public List<Fault> faults = new List<Fault>();

		/// <summary>
		/// Controls if we continue fuzzing or exit
		/// after current iteration.  This can be used
		/// by UI code to stop Peach.
		/// </summary>
		private bool _continueFuzzing = true;

		public bool continueFuzzing 
		{
			get
			{
				if (!_continueFuzzing)
					return false;
				if (config != null && config.shouldStop != null)
					return !config.shouldStop();
				return true;
			}
			set
			{
				_continueFuzzing = value;
			}
		}

		#endregion

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

    /// <summary>
    /// Type of fault
    /// </summary>
    public enum FaultType
    {
        Unknown,
        // Actual fault
        Fault,
        // Data collection
        Data
    }

    /// <summary>
    /// Fault detected during fuzzing run
    /// </summary>
    [Serializable]
    public class Fault
    {
        public Fault()
        {
        }

        /// <summary>
        /// Iteration fault was detected on
        /// </summary>
        public uint iteration = 0;

        /// <summary>
        /// Is this a control iteration.
        /// </summary>
        public bool controlIteration = false;

        /// <summary>
        /// Is this control operation also a recording iteration?
        /// </summary>
        public bool controlRecordingIteration = false;

        /// <summary>
        /// Type of fault
        /// </summary>
        public FaultType type = FaultType.Unknown;

        /// <summary>
        /// Who detected this fault?
        /// </summary>
        /// <remarks>
        /// Example: "PageHeap Monitor"
        /// Example: "Name (PageHeap Monitor)"
        /// </remarks>
        public string detectionSource = null;

        /// <summary>
        /// Title of finding
        /// </summary>
        public string title = null;

        /// <summary>
        /// Multiline description and collection of information.
        /// </summary>
        public string description = null;

        /// <summary>
        /// Major hash of fault used for bucketting.
        /// </summary>
        public string majorHash = null;
        /// <summary>
        /// Minor hash of fault used for bucketting.
        /// </summary>
        public string minorHash = null;
        /// <summary>
        /// Exploitability of fault, used for bucketting.
        /// </summary>
        public string exploitability = null;

        /// <summary>
        /// Folder for fault to be collected under.  Only used when
        /// major/minor hashes and exploitability are not defined.
        /// </summary>
        public string folderName = null;

        /// <summary>
        /// Binary data collected about fault.  Key is filename, value is content.
        /// </summary>
        public SerializableDictionary<string, byte[]> collectedData = new SerializableDictionary<string, byte[]>();
    }
}

// end
