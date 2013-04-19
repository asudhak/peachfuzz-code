
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
using Peach.Core.Dom;
using System.Reflection;

using NLog;

namespace Peach.Core.MutationStrategies
{
	[MutationStrategy("Sequential", true)]
	[Serializable]
	public class Sequential : MutationStrategy
	{
		protected class Iterations : List<Tuple<string, Mutator, string>> { }

		[NonSerialized]
		protected static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		[NonSerialized]
		protected IEnumerator<Tuple<string, Mutator, string>> _enumerator;

		[NonSerialized]
		protected Iterations _iterations = new Iterations();

		private List<Type> _mutators = null;
		private uint _count = 1;
		private uint _iteration = 1;

		public Sequential(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		public override void Initialize(RunContext context, Engine engine)
		{
			base.Initialize(context, engine);

			// Force seed to always be the same
			context.config.randomSeed = 31337;

			Core.Dom.Action.Starting += new ActionStartingEventHandler(Action_Starting);
			Core.Dom.State.Starting += new StateStartingEventHandler(State_Starting);
			context.engine.IterationFinished += new Engine.IterationFinishedEventHandler(engine_IterationFinished);
			context.engine.IterationStarting += new Engine.IterationStartingEventHandler(engine_IterationStarting);
			_mutators = new List<Type>();
			_mutators.AddRange(EnumerateValidMutators());
		}

		void engine_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			if (context.controlIteration && context.controlRecordingIteration)
			{
				// Starting to record
				_iterations = new Iterations();
				_count = 0;
			}
		}

		void engine_IterationFinished(RunContext context, uint currentIteration)
		{
			// If we were recording, end of iteration is end of recording
			if(context.controlIteration && context.controlRecordingIteration)
				OnDataModelRecorded();	
		}

		public override void Finalize(RunContext context, Engine engine)
		{
			base.Finalize(context, engine);

			Core.Dom.Action.Starting -= Action_Starting;
			Core.Dom.State.Starting -= State_Starting;
			context.engine.IterationStarting -= engine_IterationStarting;
			context.engine.IterationFinished -= engine_IterationFinished;
		}

		protected virtual void OnDataModelRecorded()
		{
		}

		public override bool IsDeterministic
		{
			get
			{
				return true;
			}
		}

		public override uint Iteration
		{
			get
			{
				return _iteration;
			}
			set
			{
				SetIteration(value);
				SeedRandom();
			}
		}

		private void SetIteration(uint value)
		{
			System.Diagnostics.Debug.Assert(value > 0);

			if (_context.controlIteration && _context.controlRecordingIteration)
			{
				return;
			}

			if (_iteration == 1 || value < _iteration)
			{
				_iteration = 1;
				_enumerator = _iterations.GetEnumerator();
				_enumerator.MoveNext();
				_enumerator.Current.Item2.mutation = 0;
			}

			uint needed = value - _iteration;

			if (needed == 0)
				return;

			while (true)
			{
				var mutator = _enumerator.Current.Item2;
				uint remain = (uint)mutator.count - mutator.mutation;

				if (remain > needed)
				{
					mutator.mutation += needed;
					_iteration = value;
					return;
				}

				needed -= remain;
				_enumerator.MoveNext();
				_enumerator.Current.Item2.mutation = 0;
			}
		}

		private void Action_Starting(Core.Dom.Action action)
		{
			// Is this a supported action?
			if (!(action.type == ActionType.Output || action.type == ActionType.SetProperty || action.type == ActionType.Call))
				return;

			if (!_context.controlIteration)
				MutateDataModel(action);

			else if(_context.controlIteration && _context.controlRecordingIteration)
				RecordDataModel(action);
		}

		void State_Starting(State state)
		{
			if (_context.controlIteration && _context.controlRecordingIteration)
			{
				foreach (Type t in _mutators)
				{
					// can add specific mutators here
					if (SupportedState(t, state))
					{
						var mutator = GetMutatorInstance(t, state);
						_iterations.Add(new Tuple<string, Mutator, string>("STATE_"+state.name, mutator, null));
						_count += (uint)mutator.count;
					}
				}
			}
		}


		// Recursivly walk all DataElements in a container.
		// Add the element and accumulate any supported mutators.
		private void GatherMutators(string modelName, DataElementContainer cont)
		{
			List<DataElement> allElements = new List<DataElement>();
			RecursevlyGetElements(cont, allElements);
			foreach (DataElement elem in allElements)
			{
				var elementName = elem.fullName;

				foreach (Type t in _mutators)
				{
					// can add specific mutators here
					if (SupportedDataElement(t, elem))
					{
						var mutator = GetMutatorInstance(t, elem);
						_iterations.Add(new Tuple<string, Mutator, string>(elementName, mutator, modelName));
						_count += (uint)mutator.count;
					}
				}
			}
		}

		private void RecordDataModel(Core.Dom.Action action)
		{
			// ParseDataModel should only be called during iteration 0
			System.Diagnostics.Debug.Assert(_context.controlIteration && _context.controlRecordingIteration);

			if (action.dataModel != null)
			{
				string modelName = GetDataModelName(action);
				GatherMutators(modelName, action.dataModel as DataElementContainer);
			}
			else if (action.parameters != null && action.parameters.Count > 0)
			{
				foreach (ActionParameter param in action.parameters)
				{
					if (param.dataModel != null)
					{
						string modelName = GetDataModelName(action, param);
						GatherMutators(modelName, param.dataModel as DataElementContainer);
					}
				}
			}
		}

		/// <summary>
		/// Allows mutation strategy to affect state change.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public override State MutateChangingState(State state)
		{
			if (_context.controlIteration)
				return state;

			if ("STATE_" + state.name == _enumerator.Current.Item1)
			{
				OnMutating(state.name, _enumerator.Current.Item2.name);
				logger.Debug("MutateChangingState: Fuzzing state change: " + state.name);
				logger.Debug("MutateChangingState: Mutator: " + _enumerator.Current.Item2.name);
				return _enumerator.Current.Item2.changeState(state);
			}

			return state;
		}

		private void ApplyMutation(string modelName, DataModel dataModel)
		{
			// Ensure we are on the right model
			if (_enumerator.Current.Item3 != modelName)
				return;

			var fullName = _enumerator.Current.Item1;
			var dataElement = dataModel.find(fullName);

			if (dataElement != null)
			{
				var mutator = _enumerator.Current.Item2;
				OnMutating(fullName, mutator.name);
				logger.Debug("ApplyMutation: Fuzzing: " + fullName);
				logger.Debug("ApplyMutation: Mutator: " + mutator.name);
				mutator.sequentialMutation(dataElement);
			}
		}

		private void MutateDataModel(Core.Dom.Action action)
		{
			// MutateDataModel should only be called after ParseDataModel
			System.Diagnostics.Debug.Assert(_count >= 1);
			System.Diagnostics.Debug.Assert(_iteration > 0);
			System.Diagnostics.Debug.Assert(!_context.controlIteration);

			if (action.dataModel != null)
			{
				string modelName = GetDataModelName(action);
				ApplyMutation(modelName, action.dataModel);
			}
			else if (action.parameters != null && action.parameters.Count > 0)
			{
				foreach (ActionParameter param in action.parameters)
				{
					if (param.dataModel != null)
					{
						string modelName = GetDataModelName(action, param);
						ApplyMutation(modelName, param.dataModel);
					}
				}
			}
		}

		public override uint Count
		{
			get
			{
				return _count;
			}
		}
	}
}

// end
