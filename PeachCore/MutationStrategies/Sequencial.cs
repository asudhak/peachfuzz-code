using System;
using System.Collections.Generic;
using System.Text;
using PeachCore.Dom;
using System.Reflection;

namespace PeachCore.MutationStrategies
{
	[DefaultMutationStrategy]
	[MutationStrategy("Sequencial")]
	public class Sequencial : MutationStrategy
	{
		Dictionary<DataElement, List<Mutator>> _stuffs = new Dictionary<DataElement, List<Mutator>>();
		List<Type> _mutators = new List<Type>();
		bool recording = true;

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
			Action.Starting += new ActionStartingEventHandler(Action_Starting);

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
			args[1] = elem;

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

		void Action_Starting(Action action)
		{
			if (!recording)
				return;

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
			get { throw new NotImplementedException(); }
		}

		public override Mutator currentMutator()
		{
			throw new NotImplementedException();
		}

		public override void next()
		{
			throw new MutatorCompleted();
		}
	}
}

// end
