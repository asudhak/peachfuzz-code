
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
using System.Text;

namespace PeachCore
{
	/// <summary>
	/// Watches the Peach Engine events.  This is how to 
	/// add a UI or logging.
	/// </summary>
	public abstract class Watcher
	{
		public Watcher()
		{
			Engine.RunStarting += new Engine.RunStartingEventHandler(Engine_RunStarting);
			Engine.RunFinished += new Engine.RunFinishedEventHandler(Engine_RunFinished);
			Engine.RunError += new Engine.RunErrorEventHandler(Engine_RunError);
			Engine.TestStarting += new Engine.TestStartingEventHandler(Engine_TestStarting);
			Engine.TestFinished += new Engine.TestFinishedEventHandler(Engine_TestFinished);
			Engine.TestError += new Engine.TestErrorEventHandler(Engine_TestError);
			Engine.IterationStarting += new Engine.IterationStartingEventHandler(Engine_IterationStarting);
			Engine.IterationFinished += new Engine.IterationFinishedEventHandler(Engine_IterationFinished);
			Engine.Fault += new Engine.FaultEventHandler(Engine_Fault);
			RunContext.Debug += new RunContext.DebugEventHandler(RunContext_Debug);
			StateModel.Starting += new StateModelStartingEventHandler(StateModel_Starting);
			StateModel.Finished += new StateModelFinishedEventHandler(StateModel_Finished);
			State.Starting += new StateStartingEventHandler(State_Starting);
			State.Finished += new StateFinishedEventHandler(State_Finished);
			State.ChangingState += new StateChangingStateEventHandler(State_ChangingState);
			Action.Starting += new ActionStartingEventHandler(Action_Starting);
			Action.Finished += new ActionFinishedEventHandler(Action_Finished);
		}

		protected virtual void Action_Finished(Action action)
		{
		}

		protected virtual void Action_Starting(Action action)
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

		protected virtual void RunContext_Debug(DebugLevel level, RunContext context, string from, string msg)
		{
		}

		protected virtual void Engine_Fault(RunContext context, uint currentIteration, object[] stateModelData, object[] faultData)
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
