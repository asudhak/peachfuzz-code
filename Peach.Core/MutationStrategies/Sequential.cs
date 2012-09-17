
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
	[MutationStrategy("Sequential")]
	[MutationStrategy("Sequencial")] // for backwards compatibility with older PITs
	public class Sequential : MutationStrategy
	{
		protected class Iterations : List<Tuple<string, Mutator>> { }

		protected static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		protected Iterations.Enumerator _enumerator;
		protected Iterations _iterations = new Iterations();

		private List<Type> _mutators = null;
		private uint _count = 1;
		private uint _iteration = 0;

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
			_mutators = new List<Type>();
			_mutators.AddRange(EnumerateValidMutators());
		}

		public override void Finalize(RunContext context, Engine engine)
		{
			base.Finalize(context, engine);

			Core.Dom.Action.Starting -= Action_Starting;
		}

		protected virtual void OnDataModelRecorded()
		{
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
			if (value == 0)
			{
				_iterations = new Iterations();
				_count = 1;
				_iteration = 0;
				return;
			}

			System.Diagnostics.Debug.Assert(value > 0);
			System.Diagnostics.Debug.Assert(value < Count);

			// When we transition out of iteration 0, signal the data model has been recorded
			if (_iteration == 0 && value > 0)
				OnDataModelRecorded();

			if (_iteration == 0 || value < _iteration)
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
			if (_iteration > 0)
				MutateDataModel(action);
			else
				RecordDataModel(action);
		}

		// Recursivly walk all DataElements in a container.
		// Add the element and accumulate any supported mutators.
		private void GatherMutators(DataElementContainer cont)
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
						_iterations.Add(new Tuple<string,Mutator>(elementName, mutator));
						_count += (uint)mutator.count;
					}
				}
			}
		}

		private void RecordDataModel(Core.Dom.Action action)
		{
			// ParseDataModel should only be called during iteration 0
			System.Diagnostics.Debug.Assert(_iteration == 0);

			if (action.dataModel != null)
			{
				GatherMutators(action.dataModel as DataElementContainer);
			}
			else if (action.parameters != null && action.parameters.Count > 0)
			{
				foreach (ActionParameter param in action.parameters)
				{
					if (param.dataModel != null)
						GatherMutators(param.dataModel as DataElementContainer);
				}
			}
		}

		private void ApplyMutation(DataModel dataModel)
		{
			var fullName = _enumerator.Current.Item1;
			var dataElement = dataModel.find(fullName);

			if (dataElement != null)
			{
				var mutator = _enumerator.Current.Item2;
				OnMutating(fullName, mutator.name);
				logger.Debug("Action_Starting: Fuzzing: " + fullName);
				logger.Debug("Action_Starting: Mutator: " + mutator.name);
				mutator.sequentialMutation(dataElement);
			}
		}

		private void MutateDataModel(Core.Dom.Action action)
		{
			// MutateDataModel should only be called after ParseDataModel
			System.Diagnostics.Debug.Assert(_count > 1);
			System.Diagnostics.Debug.Assert(_iteration > 0);

			if (action.dataModel != null)
			{
				ApplyMutation(action.dataModel);
			}
			else if (action.parameters != null && action.parameters.Count > 0)
			{
				foreach (ActionParameter param in action.parameters)
				{
					if (param.dataModel != null)
						ApplyMutation(param.dataModel);
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
