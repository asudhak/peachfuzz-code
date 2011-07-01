
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
using System.Threading;
using Peach.Core;
using NLog;

namespace Peach.Core.Dom
{
	public delegate void StateModelStartingEventHandler(StateModel model);
	public delegate void StateModelFinishedEventHandler(StateModel model);

	//[Serializable]
	public class StateModel
	{
		NLog.Logger logger = LogManager.GetLogger("Peach.Core.Dom.StateModel");

		public string name = null;
		public object parent;
		protected State _initialState = null;

		public List<State> states = new List<State>();

		public State initialState
		{
			get
			{
				return _initialState;
			}

			set
			{
				_initialState = value;
			}
		}

		/// <summary>
		/// StateModel is starting to execute.
		/// </summary>
		public static event StateModelStartingEventHandler Starting;
		/// <summary>
		/// StateModel has finished executing.
		/// </summary>
		public static event StateModelFinishedEventHandler Finished;

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

		public void Run(RunContext context)
		{
			try
			{
				OnStarting();

				_initialState.Run(context);
			}
			finally
			{
				OnFinished();
			}
		}
	}

	public delegate void StateStartingEventHandler(State state);
	public delegate void StateFinishedEventHandler(State state);
	public delegate void StateChangingStateEventHandler(State state, State toState);

	public class State
	{
		NLog.Logger logger = LogManager.GetLogger("Peach.Core.Dom.State");
		public string name = "Unknown State";
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
			catch(ActionChangeStateException e)
			{
				OnChanging(e.changeToState);
			}
			finally
			{
				OnFinished();
			}
		}
	}

	/// <summary>
	/// Action types
	/// </summary>
	public enum ActionType
	{
		Unknown,
		
		Start,
		Stop,

		Accept,
		Connect,
		Open,
		Close,

		Input,
		Output,

		Call,
		SetProperty,
		GetProperty,

		ChangeState,
		Slurp
	}

	public delegate void ActionStartingEventHandler(Action action);
	public delegate void ActionFinishedEventHandler(Action action);

	/// <summary>
	/// Performs an Action such as sending output,
	/// calling a method, etc.
	/// </summary>
	public class Action
	{
		NLog.Logger logger = LogManager.GetLogger("Peach.Core.Dom.Action");
		public string name = "Unknown Action";
		public ActionType type = ActionType.Unknown;

		public State parent = null;

		protected DataModel _dataModel;
		protected DataModel _origionalDataModel;
		protected Data _data;

		protected string _publisher = null;
		protected string _when = null;
		protected string _onStart = null;
		protected string _onComplete = null;
		protected string _ref = null;
		protected string _method = null;
		protected string _property = null;
		protected string _value = null;
		protected string _setXpath = null;
		protected string _valueXpath = null;

		public Data data
		{
			get { return _data; }
			set { _data = value; }
		}

		public DataModel dataModel
		{
			get { return _dataModel; }
			set
			{
				if (_origionalDataModel == null)
				{
					_origionalDataModel = value;
					
					// Get the value to optimize next generation based on invalidation
					object tmp = _origionalDataModel.Value;
				}

				_dataModel = value;
			}
		}

		public DataModel origionalDataModel
		{
			get { return _origionalDataModel; }
			set { _origionalDataModel = value; }
		}

		public string value
		{
			get { return _value; }
			set { _value = value; }
		}

		public string setXpath
		{
			get { return _setXpath; }
			set { _setXpath = value; }
		}

		public string valueXpath
		{
			get { return _valueXpath; }
			set { _valueXpath = value; }
		}

		public string publisher
		{
			get { return _publisher; }
			set { _publisher = value; }
		}

		public string when
		{
			get { return _when; }
			set { _when = value; }
		}

		public string onStart
		{
			get { return _onStart; }
			set { _onStart = value; }
		}

		public string onComplete
		{
			get { return _onComplete; }
			set { _onComplete = value; }
		}

		public string refDataModel
		{
			get { return _ref; }
			set { _ref = value; }
		}

		public string method
		{
			get { return _method; }
			set { _method = value; }
		}

		public string property
		{
			get { return _property; }
			set { _property = value; }
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

		public void Run(RunContext context)
		{
			logger.Trace("Run({0}): {1}", name, type);

			try
			{
				// TODO: Locate publisher by name
				//       or get default.

				Publisher publisher = null;
				if (this.publisher != null && this.publisher != "Peach.Agent")
				{
					publisher = context.test.publishers[this.publisher];
				}
				else
				{
					publisher = context.test.publishers[0];
				}

				OnStarting();

				switch (type)
				{
					case ActionType.Start:
						publisher.start(this);
						break;
					case ActionType.Stop:
						publisher.stop(this);
						break;
					case ActionType.Open:
					case ActionType.Connect:
						publisher.open(this);
						break;
					case ActionType.Close:
						publisher.close(this);
						break;

					case ActionType.Input:
						handleInput();
						break;
					case ActionType.Output:
						publisher.output(this, new Variant(this.dataModel.Value));
						break;

					case ActionType.Call:
						handleCall(context);
						break;
					case ActionType.GetProperty:
						handleGetProperty();
						break;
					case ActionType.SetProperty:
						handleSetProperty();
						break;

					case ActionType.ChangeState:
						handleChangeState();
						break;
					case ActionType.Slurp:
						handleSlurp();
						break;

					default:
						throw new ApplicationException("Error, Action.Run fell into unknown Action type handler!");
				}
			}
			finally
			{
				OnFinished();
			}
		}

		protected void handleInput()
		{
		}

		protected void handleOutput()
		{
		}

		protected void handleCall(RunContext context)
		{
			// Are we sending to Agents?
			if (this.publisher == "Peach.Agent")
			{
				context.agentManager.Message("Action.Call", new Variant(this.method));

				Variant ret = new Variant(0);
				DateTime start = DateTime.Now;

				while (true)
				{
					ret = context.agentManager.Message("Action.Call.IsRunning", new Variant(this.method));
					if (((int)ret) == 0)
						break;

					// TODO - Expose 10 as the timeout
					if (DateTime.Now.Subtract(start).Seconds > 10)
						break;

					Thread.Sleep(200);
				}

				return;
			}

			throw new NotImplementedException("TODO");
		}

		protected void handleGetProperty()
		{
		}

		protected void handleSetProperty()
		{
		}

		protected void handleChangeState()
		{
		}

		protected void handleSlurp()
		{
		}
	}

	public enum ActionParameterType
	{
		In,
		Out,
		InOut
	}

	public class ActionParameter
	{
		public ActionParameterType type;
		public DataElement dataModel;
		public object data;
	}

	public class ActionResult
	{
		DataElement dataModel;
	}

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
