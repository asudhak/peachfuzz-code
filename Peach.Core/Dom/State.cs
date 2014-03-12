
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
using System.Xml;
using System.Xml.Serialization;

using Peach.Core;

using NLog;
using System.ComponentModel;

namespace Peach.Core.Dom
{
	public delegate void StateStartingEventHandler(State state);
	public delegate void StateFinishedEventHandler(State state);
	public delegate void StateChangingStateEventHandler(State state, State toState);

	/// <summary>
	/// The State element defines a sequence of Actions to perform.  Actions can cause a 
	/// change to another State.  Such changes can occur dynamically based on content received or sent
	/// by attaching python expressions to actions via the onStart/onComplete/when attributes.
	/// </summary>
	public class State : INamed
	{
		protected Dictionary<string, object> scope = new Dictionary<string, object>();

		public State()
		{
			actions = new NamedCollection<Action>();
		}

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

		/// <summary>
		/// The name of this state.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(null)]
		public string name { get; set; }

		/// <summary>
		/// Expression to run when action is starting
		/// </summary>
		[XmlAttribute]
		[DefaultValue(null)]
		public string onStart { get; set; }

		/// <summary>
		/// Expression to run when action is completed
		/// </summary>
		[XmlAttribute]
		[DefaultValue(null)]
		public string onComplete { get; set; }

		/// <summary>
		/// The actions contained in this state.
		/// </summary>
		[PluginElement("type", typeof(Action), Combine = true)]
		[DefaultValue(null)]
		public NamedCollection<Action> actions { get; set; }

		/// <summary>
		/// The state model that owns this state.
		/// </summary>
		public StateModel parent { get; set; }

		/// <summary>
		/// Has the state started?
		/// </summary>
		public bool started { get; private set; }

		/// <summary>
		/// Has the start completed?
		/// </summary>
		public bool finished { get; private set; }

		/// <summary>
		/// Has an error occurred?
		/// </summary>
		public bool error { get; private set; }

		/// <summary>
		/// How many times has this state run
		/// </summary>
		public uint runCount { get; private set; }

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

		public virtual void OnChanging(State toState)
		{
			if (ChangingState != null)
				ChangingState(this, toState);
		}

		protected virtual void RunScript(string expr)
		{
			if (!string.IsNullOrEmpty(expr))
			{
				Scripting.Exec(expr, scope);
			}
		}

		public void Run(RunContext context)
		{
			try
			{
				// Setup scope for any scripting expressions
				scope["context"] = context;
				scope["Context"] = context;
				scope["state"] = this;
				scope["State"] = this;
				scope["StateModel"] = parent;
				scope["stateModel"] = parent;
				scope["Test"] = parent.parent;
				scope["test"] = parent.parent;
				scope["self"] = this;

				if (context.controlIteration && context.controlRecordingIteration)
					context.controlRecordingStatesExecuted.Add(this);
				else if (context.controlIteration)
					context.controlStatesExecuted.Add(this);

				started = true;
				finished = false;
				error = false;

				if (++runCount > 1)
					UpdateToOriginalDataModel(runCount);

				OnStarting();

				RunScript(onStart);

				foreach (Action action in actions)
					action.Run(context);

				// onComplete script run from finally.
			}
			catch(ActionChangeStateException)
			{
				// this is not an error
				throw;
			}
			catch
			{
				error = true;
				throw;
			}
			finally
			{
				finished = true;

				RunScript(onComplete);
				OnFinished();
			}
		}

		public void UpdateToOriginalDataModel()
		{
			UpdateToOriginalDataModel(0);
		}

		private void UpdateToOriginalDataModel(uint runCount)
		{
			this.runCount = runCount;

			foreach (var action in actions)
				action.UpdateToOriginalDataModel();
		}
	}
}

// END
