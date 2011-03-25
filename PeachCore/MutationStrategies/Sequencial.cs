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
						{
							_mutators.Add(t);
						}
					}
				}
			}
		}

		/// <summary>
		/// Call supportedDataElement method on Mutator type.
		/// </summary>
		/// <param name="mutator"></param>
		/// <param name="elem"></param>
		/// <returns>Returns true or false</returns>
		bool SupportedDataElement(Type mutator, DataElement elem)
		{
			MethodInfo supportedDataElement = mutator.GetMethod("supportedDataElement");

			object [] args = new object[1];
			args[0] = elem;

			return (bool)supportedDataElement.Invoke(null, args);
		}

		Mutator GetMutatorInstance(Type t)
		{
			return (Mutator)t.GetConstructor(new Type[] { }).Invoke(new object[] { });
		}

		void RecursevlyGetElements(DataElementContainer d, List<DataElement> all)
		{
			foreach(DataElement elem in d)
			{
				all.Add(elem);

				if(elem is DataElementContainer)
				{
					RecursevlyGetElements(elem as DataElementContainer, all);
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

				return;
			}

			if (action.dataModel != null)
			{
				List<DataElement> allElements = new List<DataElement>();
				RecursevlyGetElements(action.dataModel as DataElementContainer, allElements);
				foreach(DataElement elem in allElements)
				{
					List<Mutator> elemMutators = new List<Mutator>();

					foreach (Type t in _mutators)
					{
						if (SupportedDataElement(t, elem))
							elemMutators.Add(GetMutatorInstance(t));
					}

					_stuffs[elem] = elemMutators;
				}
			}
			
			// TODO Support parameters
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
						{
							throw new MutatorCompleted();
						}

						_mutatorEnumerator = _stuffs[_dataElementEnumerator.Current].GetEnumerator();
					}
				}
			}
			else
			{
				enumeratorsInitialized = true;
				_dataElementEnumerator = _stuffs.Keys.GetEnumerator();
				_dataElementEnumerator.MoveNext();
				_mutatorEnumerator = _stuffs[_dataElementEnumerator.Current].GetEnumerator();
				_mutatorEnumerator.MoveNext();
			}
		}
	}
}

// end
