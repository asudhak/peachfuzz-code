
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

using NLog;

namespace Peach.Core
{
	/// <summary>
	/// The main Peach fuzzing engine!
	/// </summary>
	[Serializable]
	public class Engine
	{
		[NonSerialized]
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		[NonSerialized]
		public Watcher watcher = null;
		public RunContext context = null;
		public Dom.Dom dom = null;
		public Test test = null;

		#region Events

		public delegate void TestStartingEventHandler(RunContext context);
		public delegate void IterationStartingEventHandler(RunContext context, uint currentIteration, uint? totalIterations);
		public delegate void IterationFinishedEventHandler(RunContext context, uint currentIteration);
		public delegate void FaultEventHandler(RunContext context, uint currentIteration, StateModel stateModel, Fault[] faultData);
		public delegate void ReproFaultEventHandler(RunContext context, uint currentIteration, StateModel stateModel, Fault[] faultData);
		public delegate void ReproFailedEventHandler(RunContext context, uint currentIteration);
		public delegate void TestFinishedEventHandler(RunContext context);
		public delegate void TestErrorEventHandler(RunContext context, Exception e);
		public delegate void HaveCountEventHandler(RunContext context, uint totalIterations);
		public delegate void HaveParallelEventHandler(RunContext context, uint startIteration, uint stopIteration);

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
		/// Fired when a Fault is detected and the engine starts retrying to reproduce it.
		/// </summary>
		public event ReproFaultEventHandler ReproFault;
		/// <summary>
		/// Fired when a Fault is is unable to be reproduced
		/// </summary>
		public event ReproFailedEventHandler ReproFailed;
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
		/// Fired when we know the count of iterations the Test will take.
		/// </summary>
		public event HaveCountEventHandler HaveCount;
		/// <summary>
		/// Fired when we know the range of iterations the parallel Test will take.
		/// </summary>
		public event HaveParallelEventHandler HaveParallel;

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
		public void OnFault(RunContext context, uint currentIteration, StateModel stateModel, Fault[] faultData)
		{
			logger.Debug(">> OnFault");

			if (Fault != null)
				Fault(context, currentIteration, stateModel, faultData);

			logger.Debug("<< OnFault");
		}
		public void OnReproFault(RunContext context, uint currentIteration, StateModel stateModel, Fault[] faultData)
		{
			if (ReproFault != null)
				ReproFault(context, currentIteration, stateModel, faultData);
		}
		public void OnReproFailed(RunContext context, uint currentIteration)
		{
			if (ReproFailed != null)
				ReproFailed(context, currentIteration);
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
		public void OnHaveParallel(RunContext context, uint startIteration, uint stopIteration)
		{
			if (HaveParallel != null)
				HaveParallel(context, startIteration, stopIteration);
		}

		#endregion

		public Engine(Watcher watcher)
		{
			this.watcher = watcher;
			context = new RunContext();
			context.engine = this;
		}

		/// <summary>
		/// Run the default fuzzing run in the specified dom.
		/// </summary>
		/// <param name="dom"></param>
		/// <param name="config"></param>
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
			catch (Exception ex)
			{
				throw new PeachException("Unable to locate test named '" + config.runName + "'.", ex);
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

				context.config = config;
				context.dom = dom;
				context.test = test;

				dom.context = context;

				// Initialize any watchers and loggers
				if (watcher != null)
					watcher.Initialize(this, context);

				foreach (var logger in context.test.loggers)
					logger.Initialize(this, context);

				runTest(context.dom, context.test, context);
			}
			finally
			{
				if (context.test != null)
					foreach (var logger in context.test.loggers)
						logger.Finalize(this, context);

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
				context.test.strategy.Context = context;
				context.test.strategy.Engine = this;
				context.agentManager = new AgentManager(context);
				context.reproducingFault = false;
				context.reproducingIterationJumpCount = 1;

				// Get mutation strategy
				MutationStrategy mutationStrategy = test.strategy;
				mutationStrategy.Initialize(context, this);

				uint iterationStart = 1;
				uint iterationStop = uint.MaxValue;
				uint? iterationTotal = null;
				uint lastControlIteration = 0;

				uint redoCount = 0;

				if (context.config.parallel && !mutationStrategy.IsDeterministic)
					throw new NotSupportedException("parallel is not supported when a non-deterministic mutation strategy is used");

				if (context.config.range)
				{
					if (context.config.parallel)
						throw new NotSupportedException("range is not supported when parallel is used");

					logger.Debug("runTest: context.config.range == true, start: " +
						context.config.rangeStart + ", stop: " + context.config.rangeStop);

					iterationStart = context.config.rangeStart;
					iterationStop = context.config.rangeStop;
				}
				else if (context.config.skipToIteration > 1)
				{
					logger.Debug("runTest: context.config.skipToIteration == " +
						context.config.skipToIteration);

					iterationStart = context.config.skipToIteration;
				}

				iterationStart = Math.Max(1, iterationStart);

				uint lastReproFault = iterationStart - 1;
				uint iterationCount = iterationStart;
				bool firstRun = true;

				// First iteration is always a control/recording iteration
				context.controlIteration = true;
				context.controlRecordingIteration = true;

				OnTestStarting(context);

				// Start agents
				foreach (Dom.Agent agent in test.agents.Values)
				{
					// Only use agent if on correct platform
					if ((agent.platform & Platform.GetOS()) != Platform.OS.None)
					{
						context.agentManager.AgentConnect(agent);
						context.agentManager.GetAgent(agent.name).SessionStarting();

						// Note: We want to perfrom SessionStarting on each agent
						//       in turn.  We do this incase the first agent starts
						//       a virtual machine that contains the second agent.
					}
				}

				while ((firstRun || iterationCount <= iterationStop) && context.continueFuzzing)
				{
					firstRun = false;

					// Clear out or iteration based state store
					context.iterationStateStore.Clear();

					// Should we perform a control iteration?
					if (test.controlIterationEvery > 0 && !context.reproducingFault)
					{
						if (iterationCount % test.controlIterationEvery == 0 && lastControlIteration != iterationCount)
							context.controlIteration = true;
					}

					try
					{
						// Must set iteration 1st as strategy could enable control/record bools
						mutationStrategy.Iteration = iterationCount;

						if (context.controlIteration && context.controlRecordingIteration)
						{
							context.controlRecordingActionsExecuted.Clear();
							context.controlRecordingStatesExecuted.Clear();
						}

						context.controlActionsExecuted.Clear();
						context.controlStatesExecuted.Clear();


						if (context.config.singleIteration && !context.controlIteration && iterationCount == 1)
						{
							logger.Debug("runTest: context.config.singleIteration == true");
							break;
						}

						// Make sure we are not hanging on to old faults.
						context.faults.Clear();

						try
						{
							if (IterationStarting != null)
								IterationStarting(context, iterationCount, iterationTotal.HasValue ? iterationStop : iterationTotal);

							if (context.controlIteration)
							{
								if (context.controlRecordingIteration)
									logger.Debug("runTest: Performing recording iteration.");
								else
									logger.Debug("runTest: Performing control iteration.");
							}

							context.agentManager.IterationStarting(iterationCount, context.reproducingFault);

							test.stateModel.Run(context);
						}
						catch (SoftException se)
						{
							// We should just eat SoftExceptions.
							// They indicate we should move to the next
							// iteration.

							if (context.controlIteration)
							{
								logger.Debug("runTest: SoftException on control iteration");
								if (se.InnerException != null)
									throw new PeachException(se.InnerException.Message, se);
								throw new PeachException(se.Message, se);
							}

							logger.Debug("runTest: SoftException, skipping to next iteration");
						}
						catch (PathException)
						{
							// We should just eat PathException.
							// They indicate we should move to the next
							// iteration.

							logger.Debug("runTest: PathException, skipping to next iteration");
						}
						catch (System.OutOfMemoryException ex)
						{
							logger.Debug(ex.Message);
							logger.Debug(ex.StackTrace);
							logger.Debug("runTest: " +
								"Warning: Iteration ended due to out of memory exception.  Continuing to next iteration.");

							throw new SoftException("Out of memory");
						}
						finally
						{
							context.agentManager.IterationFinished();

							if (IterationFinished != null)
								IterationFinished(context, iterationCount);

							// If this was a control iteration, verify it againt our origional
							// recording.
							if (context.controlRecordingIteration == false && 
								context.controlIteration &&
								!test.nonDeterministicActions)
							{
								if (context.controlRecordingActionsExecuted.Count != context.controlActionsExecuted.Count)
								{
									string description = string.Format(@"The Peach control iteration performed failed
to execute same as initial control.  Number of actions is different. {0} != {1}",
										context.controlRecordingActionsExecuted.Count,
										context.controlActionsExecuted.Count);

									logger.Debug(description);
									OnControlFault(context, iterationCount, description);
								}
								else if (context.controlRecordingStatesExecuted.Count != context.controlStatesExecuted.Count)
								{
									string description = string.Format(@"The Peach control iteration performed failed
to execute same as initial control.  Number of states is different. {0} != {1}",
										context.controlRecordingStatesExecuted.Count,
										context.controlStatesExecuted.Count);

									logger.Debug(description);
									OnControlFault(context, iterationCount, description);
								}

								if (context.faults.Count == 0)
								{
									foreach (Dom.Action action in context.controlRecordingActionsExecuted)
									{
										if (!context.controlActionsExecuted.Contains(action))
										{
											string description = @"The Peach control iteration performed failed
to execute same as initial control.  Action " + action.name + " was not performed.";

											logger.Debug(description);
											OnControlFault(context, iterationCount, description);
										}
									}
								}

								if (context.faults.Count == 0)
								{
									foreach (Dom.State state in context.controlRecordingStatesExecuted)
									{
										if (!context.controlStatesExecuted.Contains(state))
										{
											string description = @"The Peach control iteration performed failed
to execute same as initial control.  State " + state.name + "was not performed.";

											logger.Debug(description);
											OnControlFault(context, iterationCount, description);
										}
									}
								}
							}
						}

						// User can specify a time to wait between iterations
						// we can use that time to better detect faults
						if (context.test.waitTime > 0)
							Thread.Sleep((int)(context.test.waitTime * 1000));

						if (context.reproducingFault)
						{
							// User can specify a time to wait between iterations
							// when reproducing faults.
							if (context.test.faultWaitTime > 0)
								Thread.Sleep((int)(context.test.faultWaitTime * 1000));
						}

						// Collect any faults that were found
						context.OnCollectFaults();

						if (context.faults.Count > 0)
						{
							logger.Debug("runTest: detected fault on iteration " + iterationCount);

							foreach (Fault fault in context.faults)
							{
								fault.iteration = iterationCount;
								fault.controlIteration = context.controlIteration;
								fault.controlRecordingIteration = context.controlRecordingIteration;
							}

							if (context.reproducingFault || !test.replayEnabled)
								OnFault(context, iterationCount, test.stateModel, context.faults.ToArray());
							else
								OnReproFault(context, iterationCount, test.stateModel, context.faults.ToArray());

							if (context.controlIteration && (!test.replayEnabled || context.reproducingFault))
							{
								logger.Debug("runTest: Fault detected on control iteration");
								throw new PeachException("Fault detected on control iteration.");
							}

							if (context.reproducingFault)
							{
								lastReproFault = iterationCount;

								// If we have moved less than 20 iterations, start fuzzing
								// from here thinking we may have not really performed the
								// next few iterations.

								// Otherwise skip forward to were we left off.

								if (context.reproducingInitialIteration - iterationCount > 20)
								{
									iterationCount = (uint)context.reproducingInitialIteration;
								}

								context.reproducingFault = false;
								context.reproducingIterationJumpCount = 1;

								logger.Debug("runTest: Reproduced fault, continuing fuzzing at iteration " + iterationCount);
							}
							else if (test.replayEnabled)
							{
								logger.Debug("runTest: Attempting to reproduce fault.");

								context.reproducingFault = true;
								context.reproducingInitialIteration = iterationCount;
								context.reproducingIterationJumpCount = 1;

								// User can specify a time to wait between iterations
								// we can use that time to better detect faults
								if (context.test.waitTime > 0)
									Thread.Sleep((int)(context.test.waitTime * 1000));

								// User can specify a time to wait between iterations
								// when reproducing faults.
								if (context.test.faultWaitTime > 0)
									Thread.Sleep((int)(context.test.faultWaitTime * 1000));

								logger.Debug("runTest: replaying iteration " + iterationCount);
								continue;
							}
						}
						else if (context.reproducingFault)
						{
							uint maxJump = context.reproducingInitialIteration - lastReproFault - 1;

							if (context.reproducingIterationJumpCount >= (maxJump * 2) || context.reproducingIterationJumpCount > context.reproducingMaxBacksearch)
							{
								logger.Debug("runTest: Giving up reproducing fault, reached max backsearch.");

								context.reproducingFault = false;
								iterationCount = context.reproducingInitialIteration;

								OnReproFailed(context, iterationCount);
							}
							else
							{
								uint delta = Math.Min(maxJump, context.reproducingIterationJumpCount);
								iterationCount = (uint)context.reproducingInitialIteration - delta - 1;

								logger.Debug("runTest: " +
									"Moving backwards " + delta + " iterations to reproduce fault.");
							}

							// Make next jump larger
							context.reproducingIterationJumpCount *= context.reproducingSkipMultiple;
						}

						if (context.agentManager.MustStop())
						{
							logger.Debug("runTest: agents say we must stop!");

							throw new PeachException("Error, agent monitor stopped run!");
						}

						// Update our totals and stop based on new count
						if (context.controlIteration && context.controlRecordingIteration && !iterationTotal.HasValue)
						{
							if (context.config.countOnly)
							{
								OnHaveCount(context, mutationStrategy.Count);
								break;
							}

							iterationTotal = mutationStrategy.Count;
							if (iterationTotal < iterationStop)
								iterationStop = iterationTotal.Value;

							if (context.config.parallel)
							{
								if (iterationTotal < context.config.parallelTotal)
									throw new PeachException(string.Format("Error, {1} parallel machines is greater than the {0} total iterations.", iterationTotal, context.config.parallelTotal));

								var range = Utilities.SliceRange(1, iterationStop, context.config.parallelNum, context.config.parallelTotal);

								iterationStart = range.Item1;
								iterationStop = range.Item2;

								OnHaveParallel(context, iterationStart, iterationStop);

								if (context.config.skipToIteration > iterationStart)
									iterationStart = context.config.skipToIteration;

								iterationCount = iterationStart;
							}
						}

						// Don't increment the iteration count if we are on a 
						// control iteration
						if (!context.controlIteration)
							++iterationCount;

						redoCount = 0;
					}
					catch (RedoIterationException rte)
					{
						logger.Debug("runTest: redoing test iteration for the " + redoCount + " time.");

						// Repeat the same iteration unless
						// we have already retried 3 times.

						if (redoCount >= 3)
							throw new PeachException(rte.Message, rte);

						redoCount++;
					}
					finally
					{
						if (!context.reproducingFault)
						{
							if (context.controlIteration)
								lastControlIteration = iterationCount;

							context.controlIteration = false;
							context.controlRecordingIteration = false;
						}
					}
				}
			}
			catch (MutatorCompleted)
			{
				// Ignore, signals end of fuzzing run
				logger.Debug("runTest: MutatorCompleted exception, ending fuzzing");
			}
			finally
			{
				foreach (Publisher publisher in context.test.publishers.Values)
				{
					try
					{
						publisher.stop();
					}
					catch
					{
					}
				}

				context.agentManager.SessionFinished();
				context.agentManager.StopAllMonitors();
				context.agentManager.Shutdown();
				OnTestFinished(context);

				context.test = null;

				test.strategy.Finalize(context, this);
			}
		}

		private void OnControlFault(RunContext context, uint iterationCount, string description)
		{
			// Don't tell the engine to stop, let the replay logic determine what to do
			// If a fault is detected or reproduced on a control iteration the engine
			// will automatically stop.

			Fault fault = new Fault();
			fault.detectionSource = "PeachControlIteration";
			fault.iteration = iterationCount;
			fault.controlIteration = context.controlIteration;
			fault.controlRecordingIteration = context.controlRecordingIteration;
			fault.title = "Peach Control Iteration Failed";
			fault.description = description;
			fault.folderName = "ControlIteration";
			fault.type = FaultType.Fault;
			context.faults.Add(fault);
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
}

// end
