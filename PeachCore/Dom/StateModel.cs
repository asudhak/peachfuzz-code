
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

namespace PeachCore
{
	public delegate void StateModelStartingEventHandler(StateModel model);
	public delegate void StateModelFinishedEventHandler(StateModel model);

	public class StateModel
	{
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

		protected static virtual void OnStarting(StateModel obj)
		{
			if (Starting != null)
				Starting(obj);
		}

		protected static virtual void OnFinished(StateModel obj)
		{
			if (Finished != null)
				Finished(obj);
		}

		public void Run()
		{
			try
			{
				OnStarting(this);

				_initialState.Run();
			}
			finally
			{
				OnFinished(this);
			}
		}
	}

	public delegate void StateStartingEventHandler(State state);
	public delegate void StateFinishedEventHandler(State state);
	public delegate void StateChangingStateEventHandler(State state, State toState);

	public class State
	{
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

		protected static virtual void OnStarting(State obj)
		{
			if (Starting != null)
				Starting(obj);
		}

		protected static virtual void OnFinished(State obj)
		{
			if (Finished != null)
				Finished(obj);
		}

		protected static virtual void OnChanging(State obj, State toState)
		{
			if (Changing != null)
				Changing(obj, toState);
		}

		public void Run()
		{
			try
			{
				OnStarting(this);
				foreach (Action action in actions)
				{
					action.Run();
				}
			}
			catch(ActionChangeStateException e)
			{
				OnChanging(this, e.changeToState);
			}
			finally
			{
				OnFinished(this);
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
		public string name = "Unknown Action";
		public ActionType type = ActionType.Unknown;

		public State parent = null;

		public Publisher _publisher = null;
		public bool _publisherStarted = false;
		public bool _publisherOpen = false;

		public DataElement _dataModel;
		public object _dataSet;

		/// <summary>
		/// Action is starting to execute
		/// </summary>
		public static event ActionStartingEventHandler Starting;
		/// <summary>
		/// Action has finished executing
		/// </summary>
		public static event ActionFinishedEventHandler Finished;

		protected static virtual void OnStarting(Action obj)
		{
			if (Starting != null)
				Starting(obj);
		}

		protected static virtual void OnFinished(Action obj)
		{
			if (Finished != null)
				Finished(obj);
		}

		public void Run()
		{
			try
			{
				OnStarting(this);

				switch (type)
				{
					case ActionType.Start:
						_publisherStarted = true;
						_publisher.start();
						break;
					case ActionType.Stop:
						_publisherStarted = false;
						_publisher.stop();
						break;
					case ActionType.Open:
					case ActionType.Connect:
						if (!_publisherStarted)
							_publisher.start();

						_publisher.open();
						break;
					case ActionType.Close:
						if (!_publisherStarted)
							_publisher.start();

						_publisher.close();
						break;

					case ActionType.Input:
						if (!_publisherStarted)
							_publisher.start();
						if (!_publisherOpen)
							_publisher.open();

						handleInput();
						break;
					case ActionType.Output:
						if (!_publisherStarted)
							_publisher.start();
						if (!_publisherOpen)
							_publisher.open();

						handleOutput();
						break;

					case ActionType.Call:
						if (!_publisherStarted)
							_publisher.start();
						if (!_publisherOpen)
							_publisher.open();

						handleCall();
						break;
					case ActionType.GetProperty:
						if (!_publisherStarted)
							_publisher.start();
						if (!_publisherOpen)
							_publisher.open();

						handleGetProperty();
						break;
					case ActionType.SetProperty:
						if (!_publisherStarted)
							_publisher.start();
						if (!_publisherOpen)
							_publisher.open();

						handleSetProperty();
						break;

					case ActionType.ChangeState:
						handleChangeState();
						break;
					case ActionType.Slurp:
						handleSlurp();
						break;

					default:
						throw ApplicationException("Error, Action.Run fell into unknown Action type handler!");
				}
			}
			finally
			{
				OnFinished(this);
			}
		}

		protected void handleInput()
		{
		}

		protected void handleOutput()
		{
		}

		protected void handleCall()
		{
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
