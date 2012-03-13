
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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Peach.Core;
using NLog;

namespace Peach.Core.Dom
{
	public delegate void StateStartingEventHandler(State state);
	public delegate void StateFinishedEventHandler(State state);
	public delegate void StateChangingStateEventHandler(State state, State toState);

	public class State : INamed
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		public string _name = "Unknown State";
		public List<Action> actions = new List<Action>();

		public StateModel parent = null;

		/// <summary>
		/// State is starting to execute.
		/// </summary>
		public static event StateStartingEventHandler Starting;
		/// <summary>
		/// State has finished executing.
		/// </summary>
		public static event StateFinishedEventHandler Finished;
		/// <summary>
		/// Changing to another state.
		/// </summary>
		public static event StateChangingStateEventHandler ChangingState;

		public string name
		{
			get { return _name; }
			set { _name = value; }
		}

		protected virtual void OnStarting()
		{
			if (Starting != null)
				Starting(this);
		}

		protected virtual void OnFinished()
		{
			if (Finished != null)
				Finished(this);
		}

		protected virtual void OnChanging(State toState)
		{
			if (ChangingState != null)
				ChangingState(this, toState);
		}

		public void Run(RunContext context)
		{
			try
			{
				OnStarting();
				foreach (Action action in actions)
				{
					action.Run(context);
				}
			}
			catch (ActionChangeStateException e)
			{
				OnChanging(e.changeToState);
			}
			finally
			{
				OnFinished();
			}
		}
	}
}

// END
