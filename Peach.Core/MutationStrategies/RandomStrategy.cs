
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

		protected class Iterations : Dictionary<string, List<Mutator>> { }
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		Dictionary<string, DataSetTracker> _dataSets;
		List<Type> _mutators;
		Iterations _iterations;

		/// <summary>
		/// container also contains states if we have mutations
		/// we can apply to them.  State names are prefixed with "STATE_" to avoid
		/// conflicting with data model names.
		/// Use a list to maintain the order this strategy learns about data models
		/// </summary>
		List<string> _dataModels;
		string _targetDataModel;
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
				_dataModels = new List<string>();
				_dataSets = new Dictionary<string, DataSetTracker>();
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
				_lastIteration = _iteration;
				_iteration = value;
				_targetDataModel = null;
				SeedRandom();

				if (!_context.controlIteration && _iteration == GetSwitchIteration() && _lastIteration != _iteration)
					_randomDataSet = null;

				if (_randomDataSet == null)
				{
					logger.Debug("Iteration: Switch iteration, setting controlIteration and controlRecordingIteration.");

					_randomDataSet = new Random(this.Seed + GetSwitchIteration());

					_context.controlIteration = true;
					_context.controlRecordingIteration = true;
				}
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

			string name = "STATE_" + state.name;
			if (_dataModels.Exists(a => a == name))
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
			{
				_dataModels.Add(name);
				_iterations[name] = mutators;
			}
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

			var referenceName = action.dataModel.referenceName;
			if (referenceName == null)
				referenceName = action.dataModel.name;

			var dataModel = _context.dom.dataModels[referenceName].Clone() as DataModel;
			dataModel.isReference = true;
			dataModel.referenceName = referenceName;

			// Crack the file

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dataModel, new BitStream(fileBytes));

			return dataModel;
		}

		private DataModel AppleFieldData(Dom.Action action, Data data)
		{
			// Note: We need to find the origional data model to use.  Re-using
			// a data model that has been cracked into will fail in odd ways.

			var referenceName = action.dataModel.referenceName;
			if (referenceName == null)
				referenceName = action.dataModel.name;

			var dataModel = _context.dom.dataModels[referenceName].Clone() as DataModel;
			dataModel.isReference = true;
			dataModel.referenceName = referenceName;

			// Apply the fields

			data.ApplyFields(dataModel);

			return dataModel;
		}

		private void SyncDataSet(Dom.Action action)
		{
			System.Diagnostics.Debug.Assert(_iteration != 0);

			// dataModel is null on type="call"!
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

			// Remove our old mutators
			foreach(var dataModelName in GetAllDataModelNames(action))
				_dataModels.Remove(dataModelName);

			List<DataElement> oldElements = new List<DataElement>();
			RecursevlyGetElements(action.origionalDataModel, oldElements);
			foreach (var item in oldElements)
				_iterations.Remove(item.fullName);

			// Store copy of new origional data model
			action.origionalDataModel = action.dataModel.Clone() as DataModel;

			// Save our current state
			val.iteration = switchIteration;
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
				string name = GetDataModelName(action);
				if (!_dataModels.Exists(a => a == name))
				{
					_dataModels.Add(name);
					GatherMutators(action.dataModel as DataElementContainer);
				}
			}
			else if (action.parameters != null && action.parameters.Count > 0)
			{
				foreach (ActionParameter param in action.parameters)
				{
					if (param.dataModel != null)
					{
						string name = GetDataModelName(action, param);
						if (!_dataModels.Exists(a => a == name))
						{
							_dataModels.Add(name);
							GatherMutators(param.dataModel as DataElementContainer);
						}
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

		private void ApplyMutation(DataModel dataModel)
		{
			List<DataElement> allElements = new List<DataElement>();
			foreach (var item in dataModel.EnumerateAllElements())
			{
				if (item.isMutable)
					allElements.Add(item);
			}

			// Random.Next() Doesn't include max and we want it to
			var fieldsToMutate = Random.Next(1, maxFieldsToMutate + 1);
			logger.Debug("ApplyMutation: fieldsToMutate: " + fieldsToMutate + "; max: " + maxFieldsToMutate + "; available: " + allElements.Count);
			DataElement[] toMutate = Random.Sample(allElements, fieldsToMutate);
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

			if (action.dataModel != null && GetDataModelName(action) == _targetDataModel)
				ApplyMutation(action.dataModel);
			else if (action.parameters.Count != 0)
			{
				foreach (var param in action.parameters)
				{
					if (param.dataModel != null && GetDataModelName(action, param) == _targetDataModel)
						ApplyMutation(param.dataModel);
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

			if ("STATE_" + state.name == _targetDataModel)
			{
				Mutator mutator = Random.Choice(_iterations["STATE_" + state.name]);
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
