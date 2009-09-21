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

		public object _publisher;

		public DataElement _dataModel;
		public object _dataSet;

		/// <summary>
		/// Action is starting to execute
		/// </summary>
		public event ActionStartingEventHandler Starting;
		/// <summary>
		/// Action has finished executing
		/// </summary>
		public event ActionFinishedEventHandler Finished;

		public void Run()
		{
			throw ApplicationException("TODO: Implement Action.Run");
		}
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
