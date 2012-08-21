
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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Peach.Core.IO;
using Peach.Core.Dom;
using Peach.Core.Cracker;

using NLog;

/*
 * If not 1st iteration, pick fandom data model to change
 * 
 */
namespace Peach.Core.MutationStrategies
{
	[MutationStrategy("Random")]
	[MutationStrategy("RandomStrategy")]
	[Parameter("SwitchCount", typeof(int), "Number of iterations to perform per-mutator befor switching. (default is 200)", false)]
	[Parameter("Seed", typeof(int), "Random number seed. (default is none)", false)]
	[Parameter("MaxFieldsToMutate", typeof(int), "Maximum fields to mutate at once (default is 7).", false)]
	public class RandomStrategy : MutationStrategy
	{
		protected class Iterations : Dictionary<string, List<Mutator>> { }
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		List<Type> _mutators;
		Iterations _iterations;
		SortedSet<string> _dataModels;
		string _targetDataModel;
		uint _iteration;

		/// <summary>
		/// How often to switch files.
		/// </summary>
		int switchCount = 200;

		/// <summary>
		/// Maximum number of fields to mutate at once.
		/// </summary>
		int maxFieldsToMutate = 7;

		public RandomStrategy(Dictionary<string, Variant> args)
			: base(args)
		{
			if (args.ContainsKey("SwitchCount"))
				switchCount = int.Parse((string)args["SwitchCount"]);
			if (args.ContainsKey("Seed"))
				_seed = int.Parse((string)args["Seed"]);
			if (args.ContainsKey("MaxFieldsToMutate"))
				maxFieldsToMutate = int.Parse((string)args["MaxFieldsToMutate"]);

			if (_seed == 0)
				_seed = DateTime.Now.GetHashCode();

			// Initalize our state by entering iteration 0
			Iteration = 0;
		}

		public override void Initialize(RunContext context, Engine engine)
		{
			base.Initialize(context, engine);

			Core.Dom.Action.Starting += new ActionStartingEventHandler(Action_Starting);
			_mutators = new List<Type>();
			_mutators.AddRange(EnumerateValidMutators());
		}

		public override void Finalize(RunContext context, Engine engine)
		{
			base.Finalize(context, engine);

			Core.Dom.Action.Starting -= Action_Starting;
		}

		public override uint Iteration
		{
			get
			{
				return _iteration;
			}
			set
			{
				_iteration = value;
				_targetDataModel = null;
				SeedRandom();

				if (_iteration == 0)
				{
					_iterations = new Iterations();
					_dataModels = new SortedSet<string>();
				}
			}
		}

		void Action_Starting(Core.Dom.Action action)
		{
			if (((int)(_iteration + 1) & switchCount) == 0)
				SwitchDataSet(action);

			if (_iteration > 0)
				MutateDataModel(action);
			else
				RecordDataModel(action);
		}

		private void SwitchDataSet(Dom.Action action)
		{
			if (action.dataSet == null)
				return;

			if (action.dataSet.Datas.Count <= 1 && action.dataSet.Datas[0].Files.Count <= 1)
				return;

			for (int i = 0; i < 5; ++i)
			{
				Data data = Random.Choice(action.dataSet.Datas);
				string fileName = null;

				if (data.DataType == DataType.Files)
					fileName = Random.Choice(data.Files);
				else if (data.DataType == DataType.File)
					fileName = data.FileName;
				else if (data.DataType == DataType.Fields)
					throw new NotImplementedException();

				if (fileName == null)
					throw new PeachException("No filename specified in data set.");

				byte[] fileBytes = null;

				try
				{
					fileBytes = File.ReadAllBytes(fileName);
				}
				catch
				{
					continue;
				}

				DataCracker cracker = new DataCracker();
				cracker.CrackData(action.dataModel, new BitStream(fileBytes));

				// Generate all values;
				var ret = action.dataModel.Value;
				System.Diagnostics.Debug.Assert(ret != null);

				// Store copy of new origional data model
				action.origionalDataModel = ObjectCopier.Clone<DataModel>(action.dataModel);

				// Refresh the mutators
				// TODO: Why are we doing this? Does cracking a data model change the number & names of elements?
				_dataModels.Remove(action.dataModel.fullName);
				RecordDataModel(action);

				return;
			}

			throw new PeachException("Error, RandomStrategy was unable to load data 5 times in" +
			                         "a row for model \"" + action.dataModel.fullName + "\"");
		}

		private void GatherMutators(DataElementContainer cont)
		{
			List<DataElement> allElements = new List<DataElement>();
			RecursevlyGetElements(cont, allElements);
			foreach (DataElement elem in allElements)
			{
				var elementName = elem.fullName;
				List<Mutator> mutators = new List<Mutator>();

				foreach (Type t in _mutators)
				{
					// can add specific mutators here
					if (SupportedDataElement(t, elem))
					{
						var mutator = GetMutatorInstance(t, elem);
						mutators.Add(mutator);
					}
				}

				_iterations[elementName] = mutators;
			}
		}

		private void RecordDataModel(Core.Dom.Action action)
		{
			// ParseDataModel should only be called during iteration 0
			System.Diagnostics.Debug.Assert(_iteration == 0);

			if (action.dataModel != null)
			{
				if (_dataModels.Add(action.dataModel.fullName))
					GatherMutators(action.dataModel as DataElementContainer);
			}
			else if (action.parameters != null && action.parameters.Count > 0)
			{
				foreach (ActionParameter param in action.parameters)
				{
					if (param.dataModel != null)
						if (_dataModels.Add(param.dataModel.fullName))
							GatherMutators(param.dataModel as DataElementContainer);
				}
			}
		}

		private void ApplyMutation(DataModel dataModel)
		{
			List<DataElement> allElements = new List<DataElement>();
			foreach (var item in dataModel.EnumerateAllElements())
			{
				if (item.isMutable)
					allElements.Add(item);
			}

			// Random.Next() Doesn't include max and we want it to
			DataElement[] toMutate = Random.Sample(allElements, Random.Next(1, maxFieldsToMutate + 1));
			foreach (var item in toMutate)
			{
				Mutator mutator = Random.Choice(_iterations[item.fullName]);
				OnMutating(item.fullName, mutator.name);
				logger.Debug("Action_Starting: Fuzzing: " + item.fullName);
				logger.Debug("Action_Starting: Mutator: " + mutator.name);
				mutator.randomMutation(item);
			}
		}

		private void MutateDataModel(Core.Dom.Action action)
		{
			// MutateDataModel should only be called after ParseDataModel
			System.Diagnostics.Debug.Assert(_iteration > 0);

			if (_targetDataModel == null)
				_targetDataModel = Random.Choice(_dataModels);

			if (action.dataModel != null && action.dataModel.fullName == _targetDataModel)
				ApplyMutation(action.dataModel);

			// TODO: Why don't we mutate the action.parameters data model?
		}

		public override uint Count
		{
			get
			{
				return uint.MaxValue;
			}
		}
	}
}

// end
