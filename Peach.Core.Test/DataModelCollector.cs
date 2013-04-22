using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Peach.Core.IO;

namespace Peach.Core.Test
{
	public class DataModelCollector
	{
		protected List<Variant> mutations = null;
		protected List<BitStream> values = null;
		protected List<Dom.DataModel> dataModels = null;
		protected List<Dom.DataModel> mutatedDataModels = null;
		protected List<Dom.Action> actions = null;
		protected List<string> strategies = null;
		protected List<string> iterStrategies = null;
		protected List<string> allStrategies = null;
		protected bool cloneActions = false;

		[SetUp]
		public void SetUp()
		{
			cloneActions = false;
			ResetContainers();
			Dom.Action.Finished += new Dom.ActionFinishedEventHandler(Action_Finished);
			Peach.Core.MutationStrategy.Mutating += new MutationStrategy.MutationEventHandler(MutationStrategy_Mutating);
		}

		[TearDown]
		public void TearDown()
		{
			Dom.Action.Finished -= Action_Finished;
			Peach.Core.MutationStrategy.Mutating -= MutationStrategy_Mutating;
		}

		protected void ResetContainers()
		{
			values = new List<BitStream>();
			mutations = new List<Variant>();
			actions = new List<Dom.Action>();
			dataModels = new List<Dom.DataModel>();
			mutatedDataModels = new List<Dom.DataModel>();
			strategies = new List<string>();
			allStrategies = new List<string>();
			iterStrategies = new List<string>();
		}

		protected void Action_Finished(Dom.Action action)
		{
			var dom = action.parent.parent.parent as Dom.Dom;

			if (action.dataModel == null)
			{
				var models = action.parameters.Select(a => a.dataModel).Where(a => a != null);
				if (!models.Any())
					return;

				foreach (var model in models)
				{
					SaveDataModel(dom, model);
				}
			}
			else
			{
				SaveDataModel(dom, action.dataModel);
			}

			if (cloneActions)
				actions.Add(ObjectCopier.Clone(action));
			else
				actions.Add(action);
		}

		void SaveDataModel(Dom.Dom dom, Dom.DataModel model)
		{
			// Collect mutated values only after the first run
			if (!dom.context.controlIteration)
			{
				mutations.Add(model.Count > 0 ? model[0].InternalValue : null);
				mutatedDataModels.Add(model);
			}

			// Collect transformed values, actions and dataModels always
			values.Add(model.Count > 0 ? model[0].Value : null);
			dataModels.Add(model);
		}

		void MutationStrategy_Mutating(string elementName, string mutatorName)
		{
			int len = strategies.Count;
			string item = mutatorName + " | " + elementName;
			allStrategies.Add(item);
			if (len == 0 || strategies[len - 1] != item)
				strategies.Add(item);

			while (iterStrategies.Count < (actions.Count + 1))
				iterStrategies.Add("");

			if (iterStrategies[actions.Count].Length > 0)
				iterStrategies[actions.Count] += " ; ";

			iterStrategies[actions.Count] += item;
		}
	}
}
