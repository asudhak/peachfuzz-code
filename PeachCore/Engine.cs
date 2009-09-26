
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
using PeachCore.Agent;

namespace PeachCore
{
	/// <summary>
	/// The main Peach fuzzing engine!
	/// </summary>
	public class Engine
	{
		#region Events

		public static delegate void RunStartingEventHandler(Engine engine, Dom dom, string run);
		public static delegate void RunFinishedEventHandler(Engine engine, Dom dom, string run);
		public static delegate void RunErrorEventHandler(Engine engine, Dom dom, string run);
		public static delegate void TestStartingEventHandler(Engine engine, Dom dom, string run);
		public static delegate void IterationStartingEventHandler(Engine engine, Dom dom, uint currentIteration, uint totalIterations);
		public static delegate void IterationFinishedEventHandler(Engine engine, Dom dom, uint currentIteration);
		public static delegate void FaultEventHandler(Engine engine, Dom dom, uint currentIteration, object[] stateModelData, object[] faultData);
		public static delegate void TestFinishedEventHandler(Engine engine, Dom dom, string run);
		public static delegate void TestErrorEventHandler(Engine engine, Dom dom, string run);

		public static event RunStartingEventHandler RunStarting;
		public static event RunFinishedEventHandler RunFinished;
		public static event RunErrorEventHandler RunError;
		public static event TestStartingEventHandler TestStarting;
		public static event IterationStartingEventHandler IterationStarting;
		public static event IterationFinishedEventHandler IterationFinished;
		public static event FaultEventHandler Fault;
		public static event TestFinishedEventHandler TestFinished;
		public static event TestErrorEventHandler TestError;

		public static void OnRunStarting(Engine engine, Dom dom, Run run)
		{
			if (RunStarting != null)
				RunStarting(engine, dom, run);
		}
		public static void OnRunFinished(Engine engine, Dom dom, Run run)
		{
			if (RunFinished != null)
				RunFinished(engine, dom, run);
		}
		public static void OnRunError(Engine engine, Dom dom, Run run)
		{
			if (RunError != null)
				RunError(engine, dom, run);
		}
		public static void OnTestStarting(Engine engine, Dom dom, Run run)
		{
			if (TestStarting != null)
				TestStarting(engine, dom, run);
		}
		public static void OnIterationStarting(Engine engine, Dom dom, Run run)
		{
			if (IterationStarting != null)
				IterationStarting(engine, dom, run);
		}
		public static void OnIterationFinished(Engine engine, Dom dom, Run run)
		{
			if (IterationFinished != null)
				IterationFinished(engine, dom, run);
		}
		public static void OnRunStarting(Engine engine, Dom dom, Run run)
		{
			if (Fault != null)
				Fault(engine, dom, run);
		}
		public static void OnFault(Engine engine, Dom dom, Run run)
		{
			if (RunStarting != null)
				RunStarting(engine, dom, run);
		}
		public static void OnTestFinished(Engine engine, Dom dom, Run run)
		{
			if (TestFinished != null)
				TestFinished(engine, dom, run);
		}
		public static void OnTestError(Engine engine, Dom dom, Run run)
		{
			if (TestError != null)
				TestError(engine, dom, run);
		}

		#endregion

		public Engine(Watcher watcher)
		{
		}

		public Dom parseXml(string fileName)
		{
			return Analyzer.defaultParser.asParser(null, fileName);
		}

		public uint count(Dom dom, Run run)
		{
		}

		/// <summary>
		/// Run the default fuzzing run in the specified dom.
		/// </summary>
		/// <param name="dom"></param>
		public void run(Dom dom, RunConfiguration config)
		{
		}

		public void run(Dom dom, Run run, RunConfiguration config)
		{
		}

		protected void runRun(Dom dom, Run run, RunContext context)
		{
			try
			{
				context.run = run;
				context.test = null;

				OnRunStarting(this, dom, run, context);

				foreach (Test test in dom.tests)
				{
					context.test = test;
					runTest(dom, test, context);
				}
			}
			catch (Exception e)
			{
				OnRunError(this, dom, run, e);
			}
			finally
			{
				OnRunFinished(this, dom, run);

				context.run = null;
			}
		}

		protected void runTest(Dom dom, Test test, RunContext context)
		{
			try
			{
				context.test = null;
				OnTestStarting(this, dom, test);

				// - Get state engine
				// - Start agents
				// - Get mutation strategy

				while (true)
				{
					// - Did we finish our test range?

					// - Should we skip ahead?

					// - Iteration Starting
				}

				//   - 
			}
			catch(Exception e)
			{
				OnTestError(this, dom, test, e);
				throw e;
			}
			finally
			{
				OnTestFinished(this, dom, test);

				context.test = null;
			}
		}
	}

	public enum DebugLevel
	{
		Critical,
		Warning,
		DebugNormal,
		DebugVerbose,
		DebugSuperVerbose
	}

	public class RunContext
	{
		public static delegate void DebugEventHandler(DebugLevel level, RunContext context, string from, string msg);
		public static event DebugEventHandler Debug;

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
		public Dom dom = null;
		public Run run = null;
		public Test test = null;

		public List<Agent> agents = new List<Agent>();
	}

	/// <summary>
	/// Configure the current run
	/// </summary>
	public class RunConfiguration
	{
		/// <summary>
		/// Just get the count of mutations
		/// </summary>
		public bool countOnly = false;

		/// <summary>
		/// Perform a single iteration
		/// </summary>
		public bool singleIteration = false;
		
		/// <summary>
		/// Specify the test range to perform
		/// </summary>
		public bool range = false;
		public uint rangeStart = 0;
		public uint rangeStop = 0;

		/// <summary>
		/// Skip to a specific iteration
		/// </summary>
		public bool skipToIteration = false;
		public uint skipToIterationNumber = 0;

		/// <summary>
		/// Enable or disable debugging output
		/// </summary>
		public bool debug = false;

		/// <summary>
		/// Fuzzing strategy to use
		/// </summary>
		public MutationStrategy strategy = null;
	}
}

// end
