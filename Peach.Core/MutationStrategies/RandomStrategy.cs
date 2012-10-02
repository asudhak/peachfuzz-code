
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
	[Parameter("MaxFieldsToMutate", typeof(int), "Maximum fields to mutate at once (default is 7).", false)]
	public class RandomStrategy : MutationStrategy
	{
		class DataSetTracker
		{
			public List<string> fileNames = new List<string>();
			public uint iteration = 0;
		};

		protected class Iterations : Dictionary<string, List<Mutator>> { }
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		Dictionary<string, DataSetTracker> _dataSets;
		List<Type> _mutators;
		Iterations _iterations;
		SortedSet<string> _dataModels;
		string _targetDataModel;
		uint _iteration;
		Random _randomDataSet;

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
			if (args.ContainsKey("MaxFieldsToMutate"))
				maxFieldsToMutate = int.Parse((string)args["MaxFieldsToMutate"]);
		}

		public override void Initialize(RunContext context, Engine engine)
		{
			base.Initialize(context, engine);

			// Initalize our state by entering iteration 0
			Iteration = 0;

			Core.Dom.Action.Starting += new ActionStartingEventHandler(Action_Starting);
			_mutators = new List<Type>();
			_mutators.AddRange(EnumerateValidMutators());
		}

		public override void Finalize(RunContext context, Engine engine)
		{
			base.Finalize(context, engine);

			Core.Dom.Action.Starting -= Action_Starting;
		}

		private uint GetSwitchIteration()
		{
			// Returns the iteration we should switch our dataSet based off our
			// current iteration. For example, if switchCount is 10, this function
			// will return 1, 11, 21, 31, 41, 51, etc.
			return _iteration - (_iteration % (uint)switchCount);
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

				if (_iteration == GetSwitchIteration())
					_randomDataSet = null;

				if (_iteration == 0)
				{
					_iterations = new Iterations();
					_dataModels = new SortedSet<string>();
					_dataSets = new Dictionary<string, DataSetTracker>();
				}
				else if (_randomDataSet == null)
				{
					_randomDataSet = new Random(this.Seed + GetSwitchIteration());
				}
			}
		}

		void Action_Starting(Core.Dom.Action action)
		{
			if (_iteration == 0)
			{
				RecordDataSet(action);
				RecordDataModel(action);
			}
			else
			{
				SyncDataSet(action);
				MutateDataModel(action);
			}
		}

		private void SyncDataSet(Dom.Action action)
		{
			System.Diagnostics.Debug.Assert(_iteration != 0);

			string key = action.name + " " + action.GetHashCode();
			DataSetTracker val = null;
			if (!_dataSets.TryGetValue(key, out val))
				return;

			// If the last switch was within the current iteration range then we don't have to switch.
			uint switchIteration = GetSwitchIteration();
			if (switchIteration == val.iteration)
				return;

			// Don't switch files if we are only using a single file :)
			if (val.fileNames.Count < 2)
				return;

			// Only pick the file name once so any given iteration is guranteed to be deterministic
			string fileName = _randomDataSet.Choice(val.fileNames);
			byte[] fileBytes = null;

			for (int i = 0; i < 5; ++i)
			{
				try
				{
					fileBytes = File.ReadAllBytes(fileName);
				}
				catch
				{
					continue;
				}

				// Crack the file

				// Note: We need to find the origional data model to use.  Re-using
				// a data model that has been cracked into will fail in odd ways.

				var referenceName = action.dataModel.referenceName;
				action.dataModel = _context.dom.dataModels[referenceName].Clone() as DataModel;
				action.dataModel.isReference = true;
				action.dataModel.referenceName = referenceName;

				DataCracker cracker = new DataCracker();
				cracker.CrackData(action.dataModel, new BitStream(fileBytes));

				// Generate all values;
				var ret = action.dataModel.Value;
				System.Diagnostics.Debug.Assert(ret != null);

				// Remove our old mutators
				_dataModels.Remove(action.origionalDataModel.fullName);
				List<DataElement> oldElements = new List<DataElement>();
				RecursevlyGetElements(action.origionalDataModel, oldElements);
				foreach (var item in oldElements)
					_iterations.Remove(item.fullName);

				// Store copy of new origional data model
				action.origionalDataModel = action.dataModel.Clone() as DataModel;

				// Refresh the mutators
				RecordDataModel(action);

				// Save our current state
				val.iteration = switchIteration;

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

				if (mutators.Count > 0)
					_iterations[elementName] = mutators;
			}
		}

		private void RecordDataModel(Core.Dom.Action action)
		{
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

		private void RecordDataSet(Core.Dom.Action action)
		{
			if (action.dataSet != null)
			{
				DataSetTracker val = new DataSetTracker();
				foreach (var item in action.dataSet.Datas)
				{
					switch (item.DataType)
					{
						case DataType.File:
							val.fileNames.Add(item.FileName);
							break;
						case DataType.Files:
							val.fileNames.AddRange(item.Files);
							break;
						case DataType.Fields:
							throw new NotImplementedException();
						default:
							throw new PeachException("Unexpected DataType: " + item.DataType.ToString());
					}
				}

				if (val.fileNames.Count > 0)
				{
					// Need to properly support more than one action that are unnamed
					string key = action.name + " " + action.GetHashCode();
					System.Diagnostics.Debug.Assert(!_dataSets.ContainsKey(key));
					_dataSets.Add(key, val);
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
				if (_iterations.ContainsKey(item.fullName))
				{
					Mutator mutator = Random.Choice(_iterations[item.fullName]);
					OnMutating(item.fullName, mutator.name);
					logger.Debug("Action_Starting: Fuzzing: " + item.fullName);
					logger.Debug("Action_Starting: Mutator: " + mutator.name);
					mutator.randomMutation(item);
				}
				else
				{
					logger.Debug("Action_Starting: Skipping Fuzzing: " + item.fullName);
				}
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
