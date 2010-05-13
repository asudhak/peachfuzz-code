
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
using PeachCore.Dom;

namespace PeachCore
{
	public delegate void StateModelStartingEventHandler(StateModel model);
	public delegate void StateModelFinishedEventHandler(StateModel model);

	public class StateModel
	{
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

		public void Run()
		{
			try
			{
				OnStarting();

				_initialState.Run();
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

		public void Run()
		{
			try
			{
				OnStarting();
				foreach (Action action in actions)
				{
					action.Run();
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

		public void Run()
		{
			try
			{
				OnStarting();

				switch (type)
				{
					case ActionType.Start:
						_publisherStarted = true;
						_publisher.start(this);
						break;
					case ActionType.Stop:
						_publisherStarted = false;
						_publisher.stop(this);
						break;
					case ActionType.Open:
					case ActionType.Connect:
						if (!_publisherStarted)
							_publisher.start(this);

						_publisher.open(this);
						break;
					case ActionType.Close:
						if (!_publisherStarted)
							_publisher.start(this);

						_publisher.close(this);
						break;

					case ActionType.Input:
						if (!_publisherStarted)
							_publisher.start(this);
						if (!_publisherOpen)
							_publisher.open(this);

						handleInput();
						break;
					case ActionType.Output:
						if (!_publisherStarted)
							_publisher.start(this);
						if (!_publisherOpen)
							_publisher.open(this);

						handleOutput();
						break;

					case ActionType.Call:
						if (!_publisherStarted)
							_publisher.start(this);
						if (!_publisherOpen)
							_publisher.open(this);

						handleCall();
						break;
					case ActionType.GetProperty:
						if (!_publisherStarted)
							_publisher.start(this);
						if (!_publisherOpen)
							_publisher.open(this);

						handleGetProperty();
						break;
					case ActionType.SetProperty:
						if (!_publisherStarted)
							_publisher.start(this);
						if (!_publisherOpen)
							_publisher.open(this);

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
