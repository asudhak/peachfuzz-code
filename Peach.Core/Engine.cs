
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
		public Test test = null;

		#region Events

		public delegate void TestStartingEventHandler(RunContext context);
		public delegate void IterationStartingEventHandler(RunContext context, uint currentIteration, uint? totalIterations);
		public delegate void IterationFinishedEventHandler(RunContext context, uint currentIteration);
		public delegate void FaultEventHandler(RunContext context, uint currentIteration, StateModel stateModel, Dictionary<AgentClient, Hashtable> faultData);
		public delegate void TestFinishedEventHandler(RunContext context);
		public delegate void TestErrorEventHandler(RunContext context, Exception e);
		public delegate void HaveCountEventHandler(RunContext context, uint totalIterations);

		/// <summary>
		/// Fired when a Test is starting.  This could be fired
		/// multiple times after the RunStarting event if the Run
		/// contains multiple Tests.
		/// </summary>
		public event TestStartingEventHandler TestStarting;
		/// <summary>
		/// Fired at the start of each iteration.  This event will
		/// be fired often.
		/// </summary>
		public event IterationStartingEventHandler IterationStarting;
		/// <summary>
		/// Fired at end of each iteration.  This event will be fired often.
		/// </summary>
		public event IterationFinishedEventHandler IterationFinished;
		/// <summary>
		/// Fired when a Fault is detected.
		/// </summary>
		public event FaultEventHandler Fault;
		/// <summary>
		/// Fired when a Test is finished.
		/// </summary>
		public event TestFinishedEventHandler TestFinished;
		/// <summary>
		/// Fired when an error occurs during a Test.
		/// </summary>
		public event TestErrorEventHandler TestError;
		/// <summary>
		/// FIred when we know the count of iterations the Test will take.
		/// </summary>
		public event HaveCountEventHandler HaveCount;

		public void OnTestStarting(RunContext context)
		{
			if (TestStarting != null)
				TestStarting(context);
		}
		public void OnIterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			if (IterationStarting != null)
				IterationStarting(context, currentIteration, totalIterations);
		}
		public void OnIterationFinished(RunContext context, uint currentIteration)
		{
			if (IterationFinished != null)
				IterationFinished(context, currentIteration);
		}
		public void OnFault(RunContext context, uint currentIteration, StateModel stateModel, Dictionary<AgentClient, Hashtable> faultData)
		{
			if (Fault != null)
				Fault(context, currentIteration, stateModel, faultData);
		}
		public void OnTestFinished(RunContext context)
		{
			if (TestFinished != null)
				TestFinished(context);
		}
		public void OnTestError(RunContext context, Exception e)
		{
			if (TestError != null)
				TestError(context, e);
		}
		public void OnHaveCount(RunContext context, uint totalIterations)
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

			Test test = null;

			try
			{
				test = dom.tests[config.runName];
			}
			catch
			{
				throw new PeachException("Unable to locate test named '" + config.runName + "'.");
			}

			startFuzzing(dom, test, config);
		}

		public void startFuzzing(Dom.Dom dom, Test test, RunConfiguration config)
		{
			try
			{
				if (dom == null)
					throw new ArgumentNullException("dom parameter is null");
				if (test == null)
					throw new ArgumentNullException("test parameter is null");
				if (config == null)
					throw new ArgumentNullException("config paremeter is null");

				context = new RunContext();
				context.config = config;
				context.dom = dom;
				context.test = test;

				// Initialize any watchers and loggers
                if (watcher != null)
				    watcher.Initialize(this, context);

				if(context.test.logger != null)
					context.test.logger.Initialize(this, context);

				runTest(context.dom, context.test, context);
			}
			finally
			{
                if (watcher != null)
				    watcher.Finalize(this, context);
			}
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

			runTest(context.dom, context.test, context);
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
				foreach (Dom.Agent agent in test.agents.Values)
				{
					// Only use agent if on correct platform
					if(agent.platform == Platform.OS.unknown || agent.platform == Platform.GetOS())
						context.agentManager.AgentConnect(agent);
				}

				// Get mutation strategy
				MutationStrategy mutationStrategy = test.strategy;
				mutationStrategy.Initialize(context, this);

				uint iterationCount = 0;
				uint iterationStart = 0;
				uint iterationStop = Int32.MaxValue;
				uint? iterationTotal = null;

				uint redoCount = 0;

				if (context.config.range)
				{
					context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
						"context.config.range == true, start: " +
						context.config.rangeStart +
						", stop: " +
						context.config.rangeStop);

					iterationStart = context.config.rangeStart;
					iterationStop = context.config.rangeStop;
				}
				else if (context.config.singleIteration)
				{
					context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
						"context.config.singleIteration == true");

					iterationStop = 1;
				}
				else if (context.config.skipToIteration > 0)
				{
					context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
						"context.config.skipToIteration == " + 
						context.config.skipToIteration);

					iterationStart = context.config.skipToIteration;
				}

				context.agentManager.SessionStarting();

				while (iterationCount < iterationStop && context.continueFuzzing)
				{
					try
					{
						mutationStrategy.Iteration = iterationCount;

						try
						{
							if (IterationStarting != null)
								IterationStarting(context, iterationCount, iterationTotal);

							// TODO - Handle bool for is reproduction
							context.agentManager.IterationStarting(iterationCount, false);

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
						catch (System.OutOfMemoryException)
						{
							context.DebugMessage(DebugLevel.Warning, "Engine::runTest", 
								"Warning: Iteration ended due to out of memory exception.  Continuing to next iteration.");
						}
						finally
						{
							if (IterationFinished != null)
								IterationFinished(context, iterationCount);
						}

						// TODO: Pause for run.waitTime

						if (context.agentManager.DetectedFault())
						{
							context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
								"detected fault on iteration " + iterationCount);

							var monitorData = context.agentManager.GetMonitorData();
							
							// TODO get state model data

							OnFault(context, iterationCount, test.stateModel, monitorData);
						}

						// TODO: Check for agent stop signal
						if (context.agentManager.MustStop())
						{
							context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
								"agents say we must stop!");

							throw new PeachException("Error, agent monitor stopped run!");
						}

						// The 0th iteration is magical and needs to always run once so we can
						// figure out how many iterations are actually available
						if (iterationCount == 0)
						{
							if (context.config.countOnly)
							{
								OnHaveCount(context, mutationStrategy.Count);
								break;
							}

							iterationTotal = mutationStrategy.Count;
							if (iterationTotal < iterationStop)
								iterationStop = iterationTotal.Value;

							if (iterationStart > 0)
								iterationCount = (iterationStart - 1);
						}

						++iterationCount;

						redoCount = 0;
					}
					catch (ReplayTestException)
					{
						context.DebugMessage(DebugLevel.DebugNormal, "Engine::runTest",
							"replaying iteration " + iterationCount);
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
					}
				}
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
				foreach (Publisher publisher in context.test.publishers.Values)
				{
					try
					{
						publisher.stop(null);
					}
					catch
					{
					}
				}

				context.agentManager.SessionFinished();
				context.agentManager.StopAllMonitors();
				OnTestFinished(context);

				context.test = null;

				test.strategy.Finalize(context, this);
			}
		}
	}

	public class RedoTestException : Exception
	{
	}

	public class ReplayTestException : Exception
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

		public Random random = new Random();
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
		public uint skipToIteration;

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
		public string runName = "Default";

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
