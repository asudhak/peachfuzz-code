
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
using System.Linq;

using NLog;

namespace Peach.Core.Dom
{
	/// <summary>
	/// Used to indicate a class is a valid Action and 
	/// provide it's invoking name used in the Pit XML file.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class ActionAttribute : PluginAttribute
	{
		public ActionAttribute(string name)
			: base(typeof(Action), name, true)
		{
		}
	}

	public delegate void ActionStartingEventHandler(Action action);
	public delegate void ActionFinishedEventHandler(Action action);

	/// <summary>
	/// Performs an Action such as sending output, calling a method, etc.
	/// </summary>
	[Serializable]
	public abstract class Action : INamed
	{
		protected static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		#region Common Action Properties

		/// <summary>
		/// Action was started
		/// </summary>
		public bool started { get; private set; }

		/// <summary>
		/// Action finished
		/// </summary>
		public bool finished { get; private set; }

		/// <summary>
		/// Action errored
		/// </summary>
		public bool error { get; private set; }

		#endregion

		#region Common Action Attributes

		/// <summary>
		/// Name of this action
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// Name of publisher to use
		/// </summary>
		public string publisher { get; set; }

		/// <summary>
		/// Only run action when expression is true
		/// </summary>
		public string when { get; set; }

		/// <summary>
		/// Expression to run when action is starting
		/// </summary>
		public string onStart { get; set; }

		/// <summary>
		/// Expression to run when action is completed
		/// </summary>
		public string onComplete { get; set; }

		#endregion

		/// <summary>
		/// The type of this action
		/// </summary>
		public string type
		{
			get
			{
				var attr = ClassLoader.GetAttributes<ActionAttribute>(GetType(), null).FirstOrDefault();
				return attr != null ? attr.Name : "Unknown";
			}
		}

		/// <summary>
		/// The state this action belongs to
		/// </summary>
		public State parent { get; set; }

		/// <summary>
		/// Provides backwards compatibility to the unit tests.
		/// Will be removed in the future.
		/// </summary>
		public DataModel dataModel
		{
			get
			{
				if (allData.Count() != 1)
					throw new NotSupportedException();

				return allData.First().dataModel;
			}
		}

		/// <summary>
		/// Action is starting to execute
		/// </summary>
		public static event ActionStartingEventHandler Starting;

		/// <summary>
		/// Action has finished executing
		/// </summary>
		public static event ActionFinishedEventHandler Finished;

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

		protected virtual void RunScript(string expr)
		{
			if (!string.IsNullOrEmpty(expr))
			{
				Dictionary<string, object> state = new Dictionary<string, object>();
				state["action"] = this;
				state["state"] = this.parent;
				state["self"] = this;

				Scripting.EvalExpression(expr, state);
			}
		}

		/// <summary>
		/// Update any DataModels we contain to new clones of
		/// origionalDataModel.
		/// </summary>
		/// <remarks>
		/// This should be performed in StateModel to every State/Action at
		/// start of the iteration.
		/// </remarks>
		public void UpdateToOriginalDataModel()
		{
			foreach (var item in allData)
			{
				item.UpdateToOriginalDataModel();
			}
		}

		/// <summary>
		/// Run the action on the publisher
		/// </summary>
		/// <param name="publisher"></param>
		/// <param name="context"></param>
		protected abstract void OnRun(Publisher publisher, RunContext context);

		/// <summary>
		/// All Data (DataModels &amp; DataSets) used by this action.
		/// </summary>
		public virtual IEnumerable<ActionData> allData
		{
			get
			{
				yield break;
			}
		}

		/// <summary>
		/// All Data (DataModels &amp; DataSets) used for input (cracking) by this action.
		/// </summary>
		public virtual IEnumerable<ActionData> inputData
		{
			get
			{
				yield break;
			}
		}

		/// <summary>
		/// All Data (DataModels &amp; DataSets) used for output (fuzzing) by this action.
		/// </summary>
		public virtual IEnumerable<ActionData> outputData
		{
			get
			{
				yield break;
			}
		}

		public void Run(RunContext context)
		{
			logger.Trace("Run({0}): {1}", name, GetType().Name);

			if (when != null)
			{
				Dictionary<string, object> scope = new Dictionary<string, object>();
				scope["context"] = context;
				scope["Context"] = context;
				scope["action"] = this;
				scope["Action"] = this;
				scope["state"] = parent;
				scope["State"] = parent;
				scope["StateModel"] = parent.parent;
				scope["Test"] = parent.parent.parent;
				scope["self"] = this;

				object value = Scripting.EvalExpression(when, scope);
				if (!(value is bool))
				{
					logger.Debug("Run: action '{0}' when return is not boolean, returned: {1}", name, value);
					return;
				}

				if (!(bool)value)
				{
					logger.Debug("Run: action '{0}' when returned false", name);
					return;
				}
			}

			try
			{
				Publisher publisher = null;
				if (this.publisher != null && this.publisher != "Peach.Agent")
				{
					if (!context.test.publishers.ContainsKey(this.publisher))
					{
						logger.Debug("Run: Publisher '" + this.publisher + "' not found!");
						throw new PeachException("Error, Action '" + name + "' publisher value '" + this.publisher + "' was not found!");
					}

					publisher = context.test.publishers[this.publisher];
				}
				else
				{
					publisher = context.test.publishers[0];
				}

				if (context.controlIteration && context.controlRecordingIteration)
				{
					logger.Debug("Run: Adding action to controlRecordingActionsExecuted");
					context.controlRecordingActionsExecuted.Add(this);
				}
				else if (context.controlIteration)
				{
					logger.Debug("Run: Adding action to controlActionsExecuted");
					context.controlActionsExecuted.Add(this);
				}

				started = true;
				finished = false;
				error = false;

				OnStarting();

				logger.Debug("ActionType.{0}", GetType().Name.ToString());

				RunScript(onStart);

				// Save output data
				foreach (var item in outputData)
					parent.parent.SaveData(item.outputName, item.dataModel.Value);

				OnRun(publisher, context);

				// Save input data
				foreach (var item in inputData)
					parent.parent.SaveData(item.inputName, item.dataModel.Value);

				RunScript(onComplete);

				finished = true;
			}
			catch
			{
				error = true;
				throw;
			}
			finally
			{
				finished = true;
				OnFinished();
			}
		}
	}

	[Serializable]
	public class ActionChangeStateException : Exception
	{
		public State changeToState;

		public ActionChangeStateException(State changeToState)
		{
			this.changeToState = changeToState;
		}
	}
}

// END
