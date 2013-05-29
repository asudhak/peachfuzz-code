
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
using Peach.Core.Agent;

namespace Peach.Core
{
	/// <summary>
	/// Watches the Peach Engine events.  This is how to 
	/// add a UI or logging.
	/// </summary>
	[Serializable]
	public abstract class Watcher
	{
		public Watcher()
		{
		}

		public void Initialize(Engine engine, RunContext context)
		{
			engine.TestStarting += new Engine.TestStartingEventHandler(Engine_TestStarting);
			engine.TestFinished += new Engine.TestFinishedEventHandler(Engine_TestFinished);
			engine.TestError += new Engine.TestErrorEventHandler(Engine_TestError);
			engine.IterationStarting += new Engine.IterationStartingEventHandler(Engine_IterationStarting);
			engine.IterationFinished += new Engine.IterationFinishedEventHandler(Engine_IterationFinished);
			engine.Fault += new Engine.FaultEventHandler(Engine_Fault);
			engine.ReproFault += new Engine.ReproFaultEventHandler(Engine_ReproFault);
			engine.ReproFailed += new Engine.ReproFailedEventHandler(Engine_ReproFailed);
			engine.HaveCount += new Engine.HaveCountEventHandler(Engine_HaveCount);
			engine.HaveParallel += new Engine.HaveParallelEventHandler(Engine_HaveParallel);

			MutationStrategy.Mutating += new MutationStrategy.MutationEventHandler(MutationStrategy_Mutating);
			StateModel.Starting += new StateModelStartingEventHandler(StateModel_Starting);
			StateModel.Finished += new StateModelFinishedEventHandler(StateModel_Finished);
			State.Starting += new StateStartingEventHandler(State_Starting);
			State.Finished += new StateFinishedEventHandler(State_Finished);
			State.ChangingState += new StateChangingStateEventHandler(State_ChangingState);
			Core.Dom.Action.Starting += new ActionStartingEventHandler(Action_Starting);
			Core.Dom.Action.Finished += new ActionFinishedEventHandler(Action_Finished);
		}

		public void Finalize(Engine engine, RunContext context)
		{
			MutationStrategy.Mutating -= MutationStrategy_Mutating;
			StateModel.Starting -= StateModel_Starting;
			StateModel.Finished -= StateModel_Finished;
			State.Starting -= State_Starting;
			State.Finished -= State_Finished;
			State.ChangingState -= State_ChangingState;
			Core.Dom.Action.Starting -= Action_Starting;
			Core.Dom.Action.Finished -= Action_Finished;
		}

		protected virtual void MutationStrategy_Mutating(string elementName, string mutatorName)
		{
		}

		protected virtual void Engine_HaveCount(RunContext context, uint totalIterations)
		{
		}

		protected virtual void Engine_HaveParallel(RunContext context, uint startIteration, uint stopIteration)
		{
		}

		protected virtual void Action_Finished(Core.Dom.Action action)
		{
		}

		protected virtual void Action_Starting(Core.Dom.Action action)
		{
		}

		protected virtual void State_ChangingState(State state, State toState)
		{
		}

		protected virtual void State_Finished(State state)
		{
		}

		protected virtual void State_Starting(State state)
		{
		}

		protected virtual void StateModel_Finished(StateModel model)
		{
		}

		protected virtual void StateModel_Starting(StateModel model)
		{
		}

		protected virtual void Engine_ReproFailed(RunContext context, uint currentIteration)
		{
		}

		protected virtual void Engine_ReproFault(RunContext context, uint currentIteration, StateModel stateModel, Fault[] faultData)
		{
		}

		protected virtual void Engine_Fault(RunContext context, uint currentIteration, StateModel stateModel, Fault[] faultData)
		{
		}

		protected virtual void Engine_IterationFinished(RunContext context, uint currentIteration)
		{
		}

		protected virtual void Engine_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
		}

		protected virtual void Engine_TestError(RunContext context, Exception e)
		{
		}

		protected virtual void Engine_TestFinished(RunContext context)
		{
		}

		protected virtual void Engine_TestStarting(RunContext context)
		{
		}

		protected virtual void Engine_RunError(RunContext context, Exception e)
		{
		}

		protected virtual void Engine_RunFinished(RunContext context)
		{
		}

		protected virtual void Engine_RunStarting(RunContext context)
		{
		}
	}
}

// end
