
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
using System.Linq;

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
	[DefaultMutationStrategy]
	[MutationStrategy("Random", true)]
	[MutationStrategy("RandomStrategy")]
	[Parameter("SwitchCount", typeof(int), "Number of iterations to perform per-mutator befor switching.", "200")]
	[Parameter("MaxFieldsToMutate", typeof(int), "Maximum fields to mutate at once.", "6")]
	public class RandomStrategy : MutationStrategy
	{
		class DataSetTracker
		{
			public List<Data> options = new List<Data>();
			public uint iteration = 1;
		};

		protected class ElementId : Tuple<string, string>
		{
			public ElementId(string modelName, string elementName)
				: base(modelName, elementName)
			{
			}

			public string ModelName { get { return Item1; } }
			public string ElementName { get { return Item2; } }
		}

		protected class Iterations : OrderedDictionary<ElementId, List<Mutator>> { }
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		OrderedDictionary<string, DataSetTracker> _dataSets;
		List<Type> _mutators;
		Iterations _iterations;
		KeyValuePair<ElementId, List<Mutator>>[] _mutations;

		/// <summary>
		/// container also contains states if we have mutations
		/// we can apply to them.  State names are prefixed with "STATE_" to avoid
		/// conflicting with data model names.
		/// Use a list to maintain the order this strategy learns about data models
		/// </summary>
		uint _iteration;
		Random _randomDataSet;
		uint _lastIteration = 1;

		/// <summary>
		/// How often to switch files.
		/// </summary>
		int switchCount = 200;

		/// <summary>
		/// Maximum number of fields to mutate at once.
		/// </summary>
		int maxFieldsToMutate = 6;

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

			Core.Dom.Action.Starting += new ActionStartingEventHandler(Action_Starting);
			Core.Dom.State.Starting += new StateStartingEventHandler(State_Starting);
			engine.IterationStarting += new Engine.IterationStartingEventHandler(engine_IterationStarting);
			_mutators = new List<Type>();
			_mutators.AddRange(EnumerateValidMutators());
		}

		void engine_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			if (context.controlIteration && context.controlRecordingIteration)
			{
				_iterations = new Iterations();
				_dataSets = new OrderedDictionary<string, DataSetTracker>();
				_mutations = null;
			}
			else
			{
				// Random.Next() Doesn't include max and we want it to
				var fieldsToMutate = Random.Next(1, maxFieldsToMutate + 1);

				_mutations = Random.Sample(_iterations, fieldsToMutate);
			}
		}

		public override void Finalize(RunContext context, Engine engine)
		{
			base.Finalize(context, engine);

			Core.Dom.Action.Starting -= Action_Starting;
			Core.Dom.State.Starting -= State_Starting;
			engine.IterationStarting -= engine_IterationStarting;
		}

		private uint GetSwitchIteration()
		{
			// Returns the iteration we should switch our dataSet based off our
			// current iteration. For example, if switchCount is 10, this function
			// will return 1, 11, 21, 31, 41, 51, etc.
			uint ret = _iteration - ((_iteration - 1) % (uint)switchCount);
			return ret;
		}

		public override bool IsDeterministic
		{
			get
			{
				return false;
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
				_iteration = value;
				SeedRandom();

				if (_iteration == GetSwitchIteration() && _lastIteration != _iteration)
					_randomDataSet = null;

				if (_randomDataSet == null)
				{
					logger.Debug("Iteration: Switch iteration, setting controlIteration and controlRecordingIteration.");

					_randomDataSet = new Random(this.Seed + GetSwitchIteration());

					_context.controlIteration = true;
					_context.controlRecordingIteration = true;
					_lastIteration = _iteration;
				}

				_mutations = null;
			}
		}

		void Action_Starting(Core.Dom.Action action)
		{
			// Is this a supported action?
			if (!(action.type == ActionType.Output || action.type == ActionType.SetProperty || action.type == ActionType.Call))
				return;

			if (_context.controlIteration && _context.controlRecordingIteration)
			{
				RecordDataSet(action);
				SyncDataSet(action);
				RecordDataModel(action);
			}
			else if (!_context.controlIteration)
			{
				MutateDataModel(action);
			}
		}

		void State_Starting(State state)
		{
			if (!_context.controlIteration || !_context.controlRecordingIteration)
				return;

			var key = new ElementId("STATE_" + state.name, null);

			if (_iterations.ContainsKey(key))
				return;

			List<Mutator> mutators = new List<Mutator>();

			foreach (Type t in _mutators)
			{
				// can add specific mutators here
				if (SupportedState(t, state))
				{
					var mutator = GetMutatorInstance(t, state);
					mutators.Add(mutator);
				}
			}

			if (mutators.Count > 0)
				_iterations.Add(key, mutators);
		}

		private DataModel ApplyFileData(Dom.Action action, Data data)
		{
			byte[] fileBytes = null;

			for (int i = 0; i < 5 && fileBytes == null; ++i)
			{
				try
				{
					fileBytes = File.ReadAllBytes(data.FileName);
				}
				catch (Exception ex)
				{
					logger.Debug("Failed to open '{0}'. {1}", data.FileName, ex.Message);
				}
			}

			if (fileBytes == null)
				throw new CrackingFailure(null, null);

			// Note: We need to find the origional data model to use.  Re-using
			// a data model that has been cracked into will fail in odd ways.
			var dataModel = GetNewDataModel(action);

			// Crack the file
			DataCracker cracker = new DataCracker();
			cracker.CrackData(dataModel, new BitStream(fileBytes));

			return dataModel;
		}

		private DataModel AppleFieldData(Dom.Action action, Data data)
		{
			// Note: We need to find the origional data model to use.  Re-using
			// a data model that has been cracked into will fail in odd ways.
			var dataModel = GetNewDataModel(action);

			// Apply the fields
			data.ApplyFields(dataModel);

			return dataModel;
		}

		private DataModel GetNewDataModel(Dom.Action action)
		{
			var referenceName = action.dataModel.referenceName;
			if (referenceName == null)
				referenceName = action.dataModel.name;

			var sm = action.parent.parent;
			Dom.Dom dom = _context.dom;

			int i = sm.name.IndexOf(':');
			if (i > -1)
			{
				string prefix = sm.name.Substring(0, i);

				Dom.Dom other;
				if (!_context.dom.ns.TryGetValue(prefix, out other))
					throw new PeachException("Unable to locate namespace '" + prefix + "' in state model '" + sm.name + "'.");

				dom = other;
			}

			// Need to take namespaces into account when searching for the model
			var baseModel = dom.getRef<DataModel>(referenceName, a => a.dataModels);

			var dataModel = baseModel.Clone() as DataModel;
			dataModel.isReference = true;
			dataModel.referenceName = referenceName;

			return dataModel;
		}

		private void SyncDataSet(Dom.Action action)
		{
			System.Diagnostics.Debug.Assert(_iteration != 0);

			// Only sync <Data> elements if the action has a data model
			if (action.dataModel == null)
				return;

			string key = GetDataModelName(action);
			DataSetTracker val = null;
			if (!_dataSets.TryGetValue(key, out val))
				return;

			// If the last switch was within the current iteration range then we don't have to switch.
			uint switchIteration = GetSwitchIteration();
			if (switchIteration == val.iteration)
				return;

			// Don't switch files if we are only using a single file :)
			if (val.options.Count < 2)
				return;

			DataModel dataModel = null;

			// Some of our sample files may not crack.  Loop through them until we
			// find a good sample file.
			while (val.options.Count > 0 && dataModel == null)
			{
				Data option = _randomDataSet.Choice(val.options);

				if (option.DataType == DataType.File)
				{
					try
					{
						dataModel = ApplyFileData(action, option);
					}
					catch (CrackingFailure)
					{
						logger.Debug("Removing " + option.FileName + " from sample list.  Unable to crack.");
						val.options.Remove(option);
					}
				}
				else if (option.DataType == DataType.Fields)
				{
					try
					{
						dataModel = AppleFieldData(action, option);
					}
					catch (PeachException)
					{
						logger.Debug("Removing " + option.name + " from sample list.  Unable to apply fields.");
						val.options.Remove(option);
					}
				}
			}

			if (dataModel == null)
				throw new PeachException("Error, RandomStrategy was unable to load data for model \"" + action.dataModel.fullName + "\"");

			// Set new data model
			action.dataModel = dataModel;

			// Generate all values;
			var ret = action.dataModel.Value;
			System.Diagnostics.Debug.Assert(ret != null);

			// Store copy of new origional data model
			action.origionalDataModel = action.dataModel.Clone() as DataModel;

			// Save our current state
			val.iteration = switchIteration;
		}

		private void GatherMutators(string modelName, DataElementContainer cont)
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
					_iterations.Add(new ElementId(modelName, elementName), mutators);
			}
		}

		private void RecordDataModel(Core.Dom.Action action)
		{
			if (action.dataModel != null)
			{
				string modelName = GetDataModelName(action);
				GatherMutators(modelName, action.dataModel);
			}
			else if (action.parameters != null)
			{
				foreach (ActionParameter param in action.parameters)
				{
					if (param.dataModel != null)
					{
						string modelName = GetDataModelName(action, param);
						GatherMutators(modelName, param.dataModel);
					}
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
							val.options.Add(item);
							break;
						case DataType.Files:
							val.options.AddRange(item.Files.Select(a => new Data() { DataType = DataType.File, FileName = a }));
							break;
						case DataType.Fields:
							val.options.Add(item);
							break;
						default:
							throw new PeachException("Unexpected DataType: " + item.DataType.ToString());
					}
				}

				if (val.options.Count > 0)
				{
					// Need to properly support more than one action that are unnamed
					string key = GetDataModelName(action);
					System.Diagnostics.Debug.Assert(!_dataSets.ContainsKey(key));
					_dataSets.Add(key, val);
				}
			}

		}

		private void ApplyMutation(string modelName, DataModel dataModel)
		{
			foreach (var item in _mutations)
			{
				if (item.Key.ModelName != modelName)
					continue;

				var elem = dataModel.find(item.Key.ElementName);
				if (elem != null)
				{
					Mutator mutator = Random.Choice(item.Value);
					OnMutating(item.Key.ElementName, mutator.name);
					logger.Debug("Action_Starting: Fuzzing: " + item.Key.ElementName);
					logger.Debug("Action_Starting: Mutator: " + mutator.name);
					mutator.randomMutation(elem);
				}
				else
				{
					logger.Debug("Action_Starting: Skipping Fuzzing: " + item.Key.ElementName);
				}
			}
		}

		private void MutateDataModel(Core.Dom.Action action)
		{
			// MutateDataModel should only be called after ParseDataModel
			System.Diagnostics.Debug.Assert(_iteration > 0);

			if (action.dataModel != null)
			{
				string modelName = GetDataModelName(action);
				ApplyMutation(modelName, action.dataModel);
			}
			else if (action.parameters != null)
			{
				foreach (var param in action.parameters)
				{
					if (param.dataModel != null)
					{
						string modelName = GetDataModelName(action, param);
						ApplyMutation(modelName, param.dataModel);
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

			string name = "STATE_" + state.name;

			foreach (var item in _mutations)
			{
				if (item.Key.ModelName != name)
					continue;

				Mutator mutator = Random.Choice(item.Value);
				OnMutating(state.name, mutator.name);

				logger.Debug("MutateChangingState: Fuzzing state change: " + state.name);
				logger.Debug("MutateChangingState: Mutator: " + mutator.name);

				return mutator.changeState(state);
			}

			return state;
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
