
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
using Peach.Core.Dom;
using System.Reflection;

namespace Peach.Core.MutationStrategies
{
	[DefaultMutationStrategy]
	[MutationStrategy("Sequencial")]
	public class Sequencial : MutationStrategy
	{
		Dictionary<DataElement, List<Mutator>> _stuffs = new Dictionary<DataElement, List<Mutator>>();
		List<Type> _mutators = new List<Type>();
		bool recording = true;
		int elementPosition = 0;
		int mutatorPosition = 0;
		int? _count = null;

		public Sequencial(Dictionary<string,string> args)
			: base(args)
		{
		}

		public override void Initialize(RunContext context, Engine engine)
		{
			// Setup our handlers to record first iteration
			recording = true;
			StateModel.Starting += new StateModelStartingEventHandler(StateModel_Starting);
			StateModel.Finished += new StateModelFinishedEventHandler(StateModel_Finished);
			Core.Dom.Action.Starting += new ActionStartingEventHandler(Action_Starting);
			_context = context;

			// Locate all mutators
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type t in a.GetExportedTypes())
				{
					if (!t.IsClass)
						continue;

					foreach (object attrib in t.GetCustomAttributes(true))
					{
                        if (attrib is MutatorAttribute)
						    _mutators.Add(t);
					}
				}
			}
		}

		void Action_Starting(Core.Dom.Action action)
		{
			if (!recording && enumeratorsInitialized)
			{
				string fullName = _dataElementEnumerator.Current.fullName;
				if (action.dataModel != null)
				{
					DataElement elem = action.origionalDataModel.find(fullName);
					if (elem != null)
					{
						// Clone the data model, not the internal data element
						action.dataModel = (DataModel)ObjectCopier.Clone<DataElement>(elem.getRoot());
						elem = action.dataModel.find(fullName);
						_mutatorEnumerator.Current.sequencialMutation(elem);
					}
				}
				else if(action.parameters != null && action.parameters.Count > 0)
				{
					foreach (ActionParameter param in action.parameters)
					{
						if (param.dataModel == null)
							continue;

						DataElement elem = param.origionalDataModel.find(fullName);
						if (elem != null)
						{
							// Clone the data model, not the internal data element
							param.dataModel = (DataModel)ObjectCopier.Clone<DataElement>(elem.getRoot());
							elem = param.dataModel.find(fullName);
							_mutatorEnumerator.Current.sequencialMutation(elem);
						}
					}
				}

				return;
			}

			if (action.dataModel != null)
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

					_stuffs[elem] = elemMutators;
				}
			}
			else if (action.parameters != null && action.parameters.Count > 0)
			{
				foreach (ActionParameter param in action.parameters)
				{
					if (param.dataModel == null)
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

						_stuffs[elem] = elemMutators;
					}
				}
			}
		}

		void StateModel_Finished(StateModel model)
		{
			recording = false;
		}

		void StateModel_Starting(StateModel model)
		{
			if (recording)
			{
				_stuffs.Clear();
			}
		}

		public override uint count
		{
			get
			{
				if (_count != null)
					return (uint)_count;

				_count = 1; // Always one iteration before us!
				foreach (List<Mutator> l in _stuffs.Values)
				{
					foreach (Mutator m in l)
					{
						_count += m.count;
					}
				}

				return (uint)_count;
			}
		}

		public override Mutator currentMutator()
		{
			throw new NotImplementedException();
		}

		bool enumeratorsInitialized = false;
		Dictionary<DataElement, List<Mutator>>.KeyCollection.Enumerator _dataElementEnumerator;
		List<Mutator>.Enumerator _mutatorEnumerator;

		public override void next()
		{
			if (enumeratorsInitialized)
			{
				try
				{
					_mutatorEnumerator.Current.next();
				}
				catch
				{
					while (!_mutatorEnumerator.MoveNext())
					{
						if (!_dataElementEnumerator.MoveNext())
							throw new MutatorCompleted();

						_mutatorEnumerator = _stuffs[_dataElementEnumerator.Current].GetEnumerator();
					}
				}
			}
			else
			{
				enumeratorsInitialized = true;
				_dataElementEnumerator = _stuffs.Keys.GetEnumerator();
				if (!_dataElementEnumerator.MoveNext())
					throw new MutatorCompleted();

				_mutatorEnumerator = _stuffs[_dataElementEnumerator.Current].GetEnumerator();
				while (!_mutatorEnumerator.MoveNext())
				{
					if (!_dataElementEnumerator.MoveNext())
						throw new MutatorCompleted();

					_mutatorEnumerator = _stuffs[_dataElementEnumerator.Current].GetEnumerator();
				}
			}
		}
	}
}

// end
