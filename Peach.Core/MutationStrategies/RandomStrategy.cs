
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

namespace Peach.Core.MutationStrategies
{
    public delegate void RandomStrategyIterationEventHandler(string elementName, string mutatorName);

	[MutationStrategy("Random")]
	[MutationStrategy("RandomStrategy")]
	[Parameter("SwitchCount", typeof(int), "Number of iterations to perform per-mutator befor switching. (default is 200)", false)]
	[Parameter("Seed", typeof(int), "Random number seed. (default is none)", false)]
	[Parameter("MaxFieldsToMutate", typeof(int), "Maximum fields to mutate at once (default is 7).", false)]
	public class RandomStrategy : MutationStrategy
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
        public static event RandomStrategyIterationEventHandler Iterating;

		/// <summary>
		/// DataElement's fullname to list of mutators
		/// </summary>
		Dictionary<string, List<Mutator>> dataElementMutators = new Dictionary<string, List<Mutator>>();

		List<Type> _mutators = new List<Type>();

		/// <summary>
		/// Is this the first iteration for a specific data set?
		/// </summary>
		bool isFirstIteration = true;

		/// <summary>
		/// How often to switch files.
		/// </summary>
		int switchCount = 200;

		/// <summary>
		/// Random SEED
		/// </summary>
		int randomSeed = 0;

		/// <summary>
		/// Maximum number of fields to mutate at once.
		/// </summary>
		int maxFieldsToMutate = 7;

		int iterationCount = 0;

		/// <summary>
		/// Collection of data models.  Fullname is key.
		/// </summary>
		Dictionary<string, DataModel> dataModels = new Dictionary<string, DataModel>();

		/// <summary>
		/// DataModel's fullName selected for change.
		/// </summary>
		string dataModelToChange = null;

		/// <summary>
		/// Random number generator.
		/// </summary>
		Random _random = null;

		public override Random random
		{
			get { return _random; }
		}

		public override int IterationCount
		{
			get { return iterationCount; }
		}

		static bool eventsRegistered = false;

		public RandomStrategy(Dictionary<string, Variant> args)
			: base(args)
		{
			if (args.ContainsKey("SwitchCount"))
				switchCount = int.Parse((string)args["SwitchCount"]);
			if (args.ContainsKey("Seed"))
				randomSeed = int.Parse((string)args["Seed"]);
			if (args.ContainsKey("MaxFieldsToMutate"))
				maxFieldsToMutate = int.Parse((string)args["MaxFieldsToMutate"]);

			if (randomSeed == 0)
				randomSeed = DateTime.Now.GetHashCode();

			_random = new Random(randomSeed + iterationCount);
		}

		public override void Initialize(RunContext context, Engine engine)
		{
			base.Initialize(context, engine);

			Core.Dom.Action.Starting += new ActionStartingEventHandler(Action_Starting);
			_context = context;

			eventsRegistered = true;
			engine.IterationStarting += new Engine.IterationStartingEventHandler(Engine_IterationStarting);
			engine.IterationFinished += new Engine.IterationFinishedEventHandler(Engine_IterationFinished);

			_mutators.AddRange(EnumerateValidMutators());
		}

		public override void Finalize(RunContext context, Engine engine)
		{
			base.Finalize(context, engine);

			Core.Dom.Action.Starting -= Action_Starting;
			engine.IterationStarting -= Engine_IterationStarting;
			engine.IterationFinished -= Engine_IterationFinished;
		}

		void Engine_IterationFinished(RunContext context, uint currentIteration)
		{
			isFirstIteration = false;
			dataModelToChange = null;
		}

		void Engine_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			if (!isFirstIteration)
			{
				// Select the data model to change
				dataModelToChange = random.Choice<DataModel>(dataModels.Values).fullName;
			}
		}

		void Action_Starting(Core.Dom.Action action)
		{
			if (action.dataSet != null && 
				(action.dataSet.Datas.Count > 1 || action.dataSet.Datas[0].Files.Count>1) && 
				iterationCount % switchCount == 0)
			{
				// Time to switch the data!
				// We will try 5 times to load some data then error out.
				int tryCount = 0;
				while (true)
				{
					try
					{
						Data data = random.Choice<Data>(action.dataSet.Datas);
						string fileName = null;

						if (data.DataType == DataType.Files)
							fileName = random.Choice<string>(data.Files);

						else if (data.DataType == DataType.File)
							fileName = data.FileName;

						if (fileName != null)
						{
							DataCracker cracker = new DataCracker();
							cracker.CrackData(action.dataModel, new BitStream(File.ReadAllBytes(fileName)));

							// Generate all values
							var ret = action.dataModel.Value;

							// Store copy of new origional data model
							action.origionalDataModel = ObjectCopier.Clone<DataModel>(action.dataModel);
						}
						else if (data.DataType == DataType.Fields)
						{
							// TODO - Implement data fields method of setting values.
							throw new NotImplementedException();
						}
						else
							throw new ApplicationException("Hrm, we shouldn't be here!");

						isFirstIteration = true;
						dataModels.Remove(action.dataModel.fullName);
						tryCount = 0;
						break;
					}
					catch
					{
						tryCount++;
						if (tryCount > 5)
							throw new PeachException("Error, RandomStrategy was unable to load data 5 times in a row for model \"" + 
								action.dataModel.fullName + "\"");
					}
				}
			}

			// Get all the fields and there corresponding mutators
			if (isFirstIteration && action.dataModel != null && !dataModels.ContainsKey(action.dataModel.fullName))
			{
				List<DataElement> allElements = new List<DataElement>();
				RecursevlyGetElements(action.dataModel as DataElementContainer, allElements);
				foreach (DataElement elem in allElements)
				{
					List<Mutator> elemMutators = new List<Mutator>();

					foreach (Type t in _mutators)
					{
						if (SupportedDataElement(t, elem))
							elemMutators.Add(GetMutatorInstance(t, elem));
					}

					dataElementMutators[elem.fullName] = elemMutators;
				}

				dataModels[action.dataModel.fullName] = action.dataModel;
			}
			else if (isFirstIteration && action.parameters.Count > 0)
			{
				foreach (ActionParameter param in action.parameters)
				{
					if (dataModels.ContainsKey(param.dataModel.fullName))
						continue;

					List<DataElement> allElements = new List<DataElement>();
					RecursevlyGetElements(param.dataModel as DataElementContainer, allElements);
					foreach (DataElement elem in allElements)
					{
						List<Mutator> elemMutators = new List<Mutator>();

						foreach (Type t in _mutators)
						{
							if (SupportedDataElement(t, elem))
								elemMutators.Add(GetMutatorInstance(t, elem));
						}

						dataElementMutators[elem.fullName] = elemMutators;
					}

					dataModels[param.dataModel.fullName] = param.dataModel;
				}
			}
			else if (action.dataModel != null && dataModelToChange == action.dataModel.fullName)
			{
				List<DataElement> elements = new List<DataElement>();
				foreach (DataElement elem in action.dataModel.EnumerateAllElements())
				{
					if (elem.isMutable)
						elements.Add(elem);
				}

				DataElement[] elementsToMutate = random.Sample<DataElement>(elements, random.Next(1, maxFieldsToMutate));

				// TODO - Report which elements are mutating!

				foreach (DataElement elem in elementsToMutate)
				{
					Mutator mutator = random.Choice<Mutator>(dataElementMutators[elem.fullName]);

					logger.Info("Action_Starting: Fuzzing: " + elem.fullName);
					logger.Info("Action_Starting: Mutator: " + mutator.name);

                    if (Iterating != null)
                        Iterating(elem.fullName, mutator.name);

					try
					{
						mutator.randomMutation(elem);
					}
					catch (OutOfMemoryException)
					{
						logger.Debug("Mutator caused out of memory exception, Ignoring!");
					}
				}
			}
		}

		public override uint count
		{
			get
			{
				return Int32.MaxValue;
			}
		}

		public override Mutator currentMutator()
		{
			return null;
		}

		public override void next()
		{
			iterationCount++;
			_random = new Random(randomSeed + iterationCount);
		}
	}
}

// end
