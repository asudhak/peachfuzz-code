
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
using PeachCore.Dom;

namespace PeachCore
{
	/// <summary>
	/// The main Peach fuzzing engine!
	/// </summary>
	public class Engine
	{
		#region Events

		public delegate void RunStartingEventHandler(Engine engine, Dom.Dom dom, Run run);
		public delegate void RunFinishedEventHandler(Engine engine, Dom.Dom dom, Run run);
		public delegate void RunErrorEventHandler(Engine engine, Dom.Dom dom, Run run);
		public delegate void TestStartingEventHandler(Engine engine, Dom.Dom dom, Test test);
		public delegate void IterationStartingEventHandler(Engine engine, Dom.Dom dom, Test test, uint currentIteration, uint? totalIterations);
		public delegate void IterationFinishedEventHandler(Engine engine, Dom.Dom dom, Test test, uint currentIteration);
		public delegate void FaultEventHandler(Engine engine, Dom.Dom dom, Test test, uint currentIteration, object[] stateModelData, object[] faultData);
		public delegate void TestFinishedEventHandler(Engine engine, Dom.Dom dom, Test test);
		public delegate void TestErrorEventHandler(Engine engine, Dom.Dom dom, Test test);

		public static event RunStartingEventHandler RunStarting;
		public static event RunFinishedEventHandler RunFinished;
		public static event RunErrorEventHandler RunError;
		public static event TestStartingEventHandler TestStarting;
		public static event IterationStartingEventHandler IterationStarting;
		public static event IterationFinishedEventHandler IterationFinished;
		public static event FaultEventHandler Fault;
		public static event TestFinishedEventHandler TestFinished;
		public static event TestErrorEventHandler TestError;

		public static void OnRunStarting(Engine engine, Dom.Dom dom, Run run)
		{
			if (RunStarting != null)
				RunStarting(engine, dom, run);
		}
		public static void OnRunFinished(Engine engine, Dom.Dom dom, Run run)
		{
			if (RunFinished != null)
				RunFinished(engine, dom, run);
		}
		public static void OnRunError(Engine engine, Dom.Dom dom, Run run)
		{
			if (RunError != null)
				RunError(engine, dom, run);
		}
		public static void OnTestStarting(Engine engine, Dom.Dom dom, Test test)
		{
			if (TestStarting != null)
				TestStarting(engine, dom, test);
		}
		public static void OnIterationStarting(Engine engine, Dom.Dom dom, Test test, uint currentIteration, uint? totalIterations)
		{
			if (IterationStarting != null)
				IterationStarting(engine, dom, test, currentIteration, totalIterations);
		}
		public static void OnIterationFinished(Engine engine, Dom.Dom dom, Test test, uint currentIteration)
		{
			if (IterationFinished != null)
				IterationFinished(engine, dom, test, currentIteration);
		}
		public static void OnFault(Engine engine, Dom.Dom dom, Test test, uint currentIteration, object[] stateModelData, object[] faultData)
		{
			if (Fault != null)
				Fault(engine, dom, test, currentIteration, stateModelData, faultData);
		}
		public static void OnTestFinished(Engine engine, Dom.Dom dom, Test test)
		{
			if (TestFinished != null)
				TestFinished(engine, dom, test);
		}
		public static void OnTestError(Engine engine, Dom.Dom dom, Test test)
		{
			if (TestError != null)
				TestError(engine, dom, test);
		}

		#endregion

		public Engine(Watcher watcher)
		{
		}

		public Dom.Dom parseXml(string fileName)
		{
			return Analyzer.defaultParser.asParser(null, fileName);
		}

		public uint count(Dom.Dom dom, Run run)
		{
		}

		/// <summary>
		/// Run the default fuzzing run in the specified dom.
		/// </summary>
		/// <param name="dom"></param>
		public void run(Dom.Dom dom, RunConfiguration config)
		{
		}

		public void run(Dom.Dom dom, Run run, RunConfiguration config)
		{
		}

		protected void runRun(Dom.Dom dom, Run run, RunContext context)
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

		protected void runTest(Dom.Dom dom, Test test, RunContext context)
		{
			try
			{
				context.test = null;
				OnTestStarting(this, dom, test);

				// TODO: Get state engine
				// TODO: Start agents
				// TODO: Get mutation strategy
				MutationStrategy mutationStrategy = null;

				uint iterationCount = 0;
				uint? totalIterationCount;

				uint? iterationRangeStart;
				uint? iterationRangeStop;

				uint redoCount = 0;

				if (context.config.range)
				{
					context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
						"context.config.range == true, start: " +
						context.config.rangeStart +
						", stop: " +
						context.config.rangeStop);

					iterationRangeStart = context.config.rangeStart;
					iterationRangeStop = context.config.rangeStop;
				}
				if (context.config.singleIteration)
				{
					context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
						"context.config.singleIteration == true");
					
					iterationRangeStop = 1;
				}
				if (context.config.skipToIteration.HasValue)
				{
					context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
						"context.config.skipToIteration == " + 
						context.config.skipToIteration);
					
					iterationRangeStart = context.config.skipToIteration;
				}

				while (context.continueFuzzing)
				{
					try
					{
						iterationCount++;

						// - Did we finish our test range?
						if (iterationRangeStop.HasValue && iterationRangeStop > iterationCount)
							break;

						// - Get total count?
						if (iterationCount == 2 && !totalIterationCount.HasValue)
						{
							totalIterationCount = mutationStrategy.count;

							if (iterationRangeStop.HasValue && iterationRangeStop < totalIterationCount)
								totalIterationCount = iterationRangeStop;
						}

						// - Should we skip ahead?
						if (iterationCount == 2 && iterationRangeStart.HasValue &&
							iterationCount < iterationRangeStart)
						{
							for (; iterationCount < iterationRangeStart; iterationCount++)
								mutationStrategy.next();
						}

						try
						{
							// TODO: Iteration Starting
						}
						catch (RedoTestException e)
						{
							throw e;
						}
						catch (SoftException se)
						{
							// We should just eat SoftExceptions.
							// They indicate we should move to the next
							// iteration.
						}
						catch (PathException pe)
						{
							// We should just eat PathException.
							// They indicate we should move to the next
							// iteration.
						}

						// TODO: Pause for run.waitTime

						// TODO: Check for agent faults

						// TODO: Check for agent stop signal

						// Increment to next test
						mutationStrategy.next();

						redoCount = 0;
					}
					catch (RedoIterationException rte)
					{
						// Repeat the same iteration unless
						// we have already retried 3 times.

						if (redoCount >= 3)
							throw new PeachException(rte.Message);

						redoCount++;
						iterationCount--;
					}
				}

				//   - 
			}
			catch (MutatorCompleted mc)
			{
				// Ignore, signals end of fuzzing run
			}
			// TODO: Catch keyboard interrupt
			catch (Exception e)
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
		public delegate void DebugEventHandler(DebugLevel level, RunContext context, string from, string msg);
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
		public Dom.Dom dom = null;
		public Run run = null;
		public Test test = null;

		public List<Agent.Agent> agents = new List<Agent.Agent>();

		/// <summary>
		/// Controls if we continue fuzzing or exit
		/// after current iteration.  This can be used
		/// by UI code to stop Peach.
		/// </summary>
		public bool continueFuzzing = true;
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
		public uint? skipToIteration;

		/// <summary>
		/// Enable or disable debugging output
		/// </summary>
		public bool debug = false;

		/// <summary>
		/// Fuzzing strategy to use
		/// </summary>
		public MutationStrategy strategy = null;

		/// <summary>
		/// Name of run to perform
		/// </summary>
		public string runName = "DefaultRun";
	}
}

// end
