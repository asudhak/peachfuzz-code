
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
using System.Reflection;
using Peach.Core.Agent;
using Peach.Core.Dom;
using System.Threading;

namespace Peach.Core
{
	/// <summary>
	/// The main Peach fuzzing engine!
	/// </summary>
	public class Engine
	{
		public Watcher watcher = null;
		public RunContext context = null;
		public RunConfiguration config = null;
		public Dom.Dom dom = null;
		public Run run = null;
		public Test test = null;

		#region Events

		public delegate void RunStartingEventHandler(RunContext context);
		public delegate void RunFinishedEventHandler(RunContext context);
		public delegate void RunErrorEventHandler(RunContext context, Exception e);
		public delegate void TestStartingEventHandler(RunContext context);
		public delegate void IterationStartingEventHandler(RunContext context, uint currentIteration, uint? totalIterations);
		public delegate void IterationFinishedEventHandler(RunContext context, uint currentIteration);
		public delegate void FaultEventHandler(RunContext context, uint currentIteration, Dictionary<string, Variant> stateModelData, Dictionary<AgentClient, Hashtable> faultData);
		public delegate void TestFinishedEventHandler(RunContext context);
		public delegate void TestErrorEventHandler(RunContext context, Exception e);
		public delegate void HaveCountEventHandler(RunContext context, uint totalIterations);

		/// <summary>
		/// Fired when a Run is starting
		/// </summary>
		public static event RunStartingEventHandler RunStarting;
		/// <summary>
		/// Fired when a Run is finished
		/// </summary>
		public static event RunFinishedEventHandler RunFinished;
		/// <summary>
		/// Fired when an error is detected during run.
		/// </summary>
		public static event RunErrorEventHandler RunError;
		/// <summary>
		/// Fired when a Test is starting.  This could be fired
		/// multiple times after the RunStarting event if the Run
		/// contains multiple Tests.
		/// </summary>
		public static event TestStartingEventHandler TestStarting;
		/// <summary>
		/// Fired at the start of each iteration.  This event will
		/// be fired often.
		/// </summary>
		public static event IterationStartingEventHandler IterationStarting;
		/// <summary>
		/// Fired at end of each iteration.  This event will be fired often.
		/// </summary>
		public static event IterationFinishedEventHandler IterationFinished;
		/// <summary>
		/// Fired when a Fault is detected.
		/// </summary>
		public static event FaultEventHandler Fault;
		/// <summary>
		/// Fired when a Test is finished.
		/// </summary>
		public static event TestFinishedEventHandler TestFinished;
		/// <summary>
		/// Fired when an error occurs during a Test.
		/// </summary>
		public static event TestErrorEventHandler TestError;
		/// <summary>
		/// FIred when we know the count of iterations the Test will take.
		/// </summary>
		public static event HaveCountEventHandler HaveCount;

		public static void OnRunStarting(RunContext context)
		{
			if (RunStarting != null)
				RunStarting(context);
		}
		public static void OnRunFinished(RunContext context)
		{
			if (RunFinished != null)
				RunFinished(context);
		}
		public static void OnRunError(RunContext context, Exception e)
		{
			if (RunError != null)
				RunError(context, e);
		}
		public static void OnTestStarting(RunContext context)
		{
			if (TestStarting != null)
				TestStarting(context);
		}
		public static void OnIterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			if (IterationStarting != null)
				IterationStarting(context, currentIteration, totalIterations);
		}
		public static void OnIterationFinished(RunContext context, uint currentIteration)
		{
			if (IterationFinished != null)
				IterationFinished(context, currentIteration);
		}
		public static void OnFault(RunContext context, uint currentIteration, Dictionary<string, Variant> stateModelData, Dictionary<AgentClient, Hashtable> faultData)
		{
			if (Fault != null)
				Fault(context, currentIteration, stateModelData, faultData);
		}
		public static void OnTestFinished(RunContext context)
		{
			if (TestFinished != null)
				TestFinished(context);
		}
		public static void OnTestError(RunContext context, Exception e)
		{
			if (TestError != null)
				TestError(context, e);
		}
		public static void OnHaveCount(RunContext context, uint totalIterations)
		{
			if (HaveCount != null)
				HaveCount(context, totalIterations);
		}

		#endregion

		public Engine(Watcher watcher)
		{
			this.watcher = watcher;
		}

		/// <summary>
		/// Run the default fuzzing run in the specified dom.
		/// </summary>
		/// <param name="dom"></param>
		public void startFuzzing(Dom.Dom dom, RunConfiguration config)
		{
			if (dom == null)
				throw new ArgumentNullException("dom parameter is null");
			if (config == null)
				throw new ArgumentNullException("config paremeter is null");

			Run run = null;

			try
			{
				run = dom.runs[config.runName];
			}
			catch
			{
				throw new PeachException("Unable to locate run named '" + config.runName + "'.");
			}

			startFuzzing(dom, run, config);
		}

		public void startFuzzing(Dom.Dom dom, Run run, RunConfiguration config)
		{
			if (dom == null)
				throw new ArgumentNullException("dom parameter is null");
			if (run == null)
				throw new ArgumentNullException("run parameter is null");
			if (config == null)
				throw new ArgumentNullException("config paremeter is null");

			context = new RunContext();
			context.config = config;
			context.dom = dom;
			context.run = run;            

			// TODO: Start up agents!

			runRun(context);
		}

		/// <summary>
		/// Start fuzzing using a RunContext object to provide
		/// needed configuration.  This allows the caller to pre-configure
		/// any Agents prior to calling the fuzzing engine.
		/// </summary>
		/// <param name="context">Fuzzing configuration</param>
		public void startFuzzing(RunContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context parameter is null");

			runRun(context);
		}

		protected void runRun(RunContext context)
		{
			if (context.run == null)
				throw new ArgumentNullException("context.run is null");
			if (context.dom == null)
				throw new ArgumentNullException("context.dom is null");
			if (context.config == null)
				throw new ArgumentNullException("context.config is null");

			try
			{
				Dom.Dom dom = context.dom;
				Run run = context.run;
				context.test = null;

				OnRunStarting(context);

				foreach (Test test in run.tests.Values)
				{
					context.test = test;
					runTest(dom, test, context);
				}
			}
			//catch (Exception e)
			//{
			//	OnRunError(context, e);
			//}
			finally
			{
				OnRunFinished(context);

				context.run = null;
			}
		}

		/// <summary>
		/// Run a test case.  Contains main fuzzing loop.
		/// </summary>
		/// <param name="dom"></param>
		/// <param name="test"></param>
		/// <param name="context"></param>
		protected void runTest(Dom.Dom dom, Test test, RunContext context)
		{
			try
			{
				context.test = test;
				context.agentManager = new AgentManager();
				OnTestStarting(context);

				// Start agents
				foreach(Dom.Agent agent in test.agents.Values)
					context.agentManager.AgentConnect(agent);

				// Get mutation strategy
				MutationStrategy mutationStrategy = test.strategy;
				mutationStrategy.Initialize(context, this);

				uint iterationCount = 0;
				uint? totalIterationCount = null;

				uint? iterationRangeStart = null;
				uint? iterationRangeStop = null;

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
				else if (context.config.singleIteration)
				{
					context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
						"context.config.singleIteration == true");
					
					iterationRangeStop = 1;
				}
				else if (context.config.skipToIteration != null)
				{
					context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
						"context.config.skipToIteration == " + 
						context.config.skipToIteration);
					
					iterationRangeStart = context.config.skipToIteration;
				}

				context.agentManager.SessionStarting();

				while (context.continueFuzzing)
				{
					try
					{
						iterationCount++;

						// - Did we finish our test range?
						if (iterationRangeStop != null && iterationCount >= (iterationRangeStop+1))
							break;

						// - Get total count?
						if (iterationCount == 2 && totalIterationCount == null)
						{
							totalIterationCount = mutationStrategy.count;

							if (iterationRangeStop != null && iterationRangeStop < totalIterationCount)
								totalIterationCount = iterationRangeStop;
						}

						// - Just getting count?
						if (context.config.countOnly && iterationCount > 1 && totalIterationCount != null)
						{
							OnHaveCount(context, (uint)totalIterationCount);
							break;
						}

						// - Should we skip ahead?
						if (iterationCount == 2 && iterationRangeStart != null &&
							iterationCount < iterationRangeStart)
						{
							// TODO - We need an event for this!
							for (; iterationCount < iterationRangeStart; iterationCount++)
								mutationStrategy.next();
						}

						try
						{
							if (Engine.IterationStarting != null)
								Engine.IterationStarting(context, iterationCount, totalIterationCount);

							// TODO - Handle bool for is reproduction
							context.agentManager.IterationStarting((int)iterationCount, false);

							test.stateModel.Run(context);

							context.agentManager.IterationFinished();

							//Thread.Sleep(1000 * 5);
						}
						catch (RedoTestException e)
						{
							throw e;
						}
						catch (SoftException)
						{
							// We should just eat SoftExceptions.
							// They indicate we should move to the next
							// iteration.

							context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
								"SoftException, skipping to next iteration");
						}
						catch (PathException)
						{
							// We should just eat PathException.
							// They indicate we should move to the next
							// iteration.

							context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
								"PathException, skipping to next iteration");
						}
						finally
						{
							if (Engine.IterationFinished != null)
								Engine.IterationFinished(context, iterationCount);
						}

						// TODO: Pause for run.waitTime

						if (context.agentManager.DetectedFault())
						{
							context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
								"detected fault on iteration " + iterationCount);

							var monitorData = context.agentManager.GetMonitorData();
							
							// TODO get state model data

							OnFault(context, iterationCount, null, monitorData);
						}

						// TODO: Check for agent stop signal
						if (context.agentManager.MustStop())
						{
							context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
								"agents say we must stop!");

							throw new PeachException("Error, agent monitor stopped run!");
						}

						// Increment to next test
						mutationStrategy.next();

						redoCount = 0;
					}
					catch (RedoIterationException rte)
					{
						context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
							"redoing test iteration for the " + redoCount + " time.");

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
			catch (MutatorCompleted)
			{
				// Ignore, signals end of fuzzing run
				context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
					"MutatorCompleted exception, ending fuzzing");
			}
			// TODO: Catch keyboard interrupt
			//catch (Exception e)
			//{
			//    OnTestError(context, e);
			//    throw e;
			//}
			finally
			{
				context.agentManager.SessionFinished();
				OnTestFinished(context);

				context.test = null;
			}
		}
	}

	public class RedoTestException : Exception
	{
	}

	public enum DebugLevel
	{
		Critical,
		Warning,
		DebugNormal,
		DebugVerbose,
		DebugSuperVerbose
	}

	[Serializable]
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

		public Random random = new Random();
		public RunConfiguration config = null;
		public Dom.Dom dom = null;
		public Run run = null;
		public Test test = null;
		public AgentManager agentManager = null;

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

		/// <summary>
		/// Name of PIT file (used by logger)
		/// </summary>
		public string pitFile = null;

		/// <summary>
		/// Command line if any (used by logger)
		/// </summary>
		public string commandLine = null;

		/// <summary>
		/// Date and time of run (used by logger)
		/// </summary>
		public DateTime runDateTime = DateTime.Now;

		/// <summary>
		/// Peach version currently running (used by logger)
		/// </summary>
		public string version
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}
	}
}

// end
