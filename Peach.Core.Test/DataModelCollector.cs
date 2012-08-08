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
		protected List<Dom.Action> actions = null;

		[SetUp]
		public void SetUp()
		{
			firstRun = false;
			values = new List<BitStream>();
			mutations = new List<Variant>();
			actions = new List<Dom.Action>();
			Dom.Action.Finished += new Dom.ActionFinishedEventHandler(Action_Finished);
		}

		[TearDown]
		public void TearDown()
		{
			Dom.Action.Finished -= Action_Finished;
		}

		protected void Action_Finished(Dom.Action action)
		{
			// Collect mutated values only after the first run
			if (firstRun)
				mutations.Add(action.dataModel[0].InternalValue);
			else
				firstRun = true;

			// Collect transformed values always
			values.Add(action.dataModel[0].Value);

			// Collect actions always
			actions.Add(action);
		}
	}
}
