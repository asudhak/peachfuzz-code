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
		protected bool firstRun = false;
		protected List<Variant> mutations = null;
		protected List<BitStream> values = null;
		protected List<Dom.DataModel> dataModels = null;
		protected List<Dom.Action> actions = null;
		protected List<string> strategies = null;

		[SetUp]
		public void SetUp()
		{
			firstRun = false;
			values = new List<BitStream>();
			mutations = new List<Variant>();
			actions = new List<Dom.Action>();
			dataModels = new List<Dom.DataModel>();
			strategies = new List<string>();
			Dom.Action.Finished += new Dom.ActionFinishedEventHandler(Action_Finished);
			Peach.Core.MutationStrategy.Iterating += new MutationStrategy.IterationEventHandler(MutationStrategy_Iterating);
		}

		[TearDown]
		public void TearDown()
		{
			Dom.Action.Finished -= Action_Finished;
			Peach.Core.MutationStrategy.Iterating -= MutationStrategy_Iterating;
		}

		protected void Action_Finished(Dom.Action action)
		{
			// Collect mutated values only after the first run
			if (firstRun)
				mutations.Add(action.dataModel[0].InternalValue);
			else
				firstRun = true;

			// Collect transformed values, actions and dataModels always
			values.Add(action.dataModel[0].Value);
			actions.Add(action);
			dataModels.Add(action.dataModel);
		}

		void MutationStrategy_Iterating(string elementName, string mutatorName)
		{
			int len = strategies.Count;
			string item = mutatorName + " | " + elementName;
			if (len == 0 || strategies[len - 1] != item)
				strategies.Add(item);
		}
	}
}
