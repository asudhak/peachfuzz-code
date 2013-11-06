using System;
using Peach.Core.Cracker;
using Peach.Core.IO;
using System.Collections.Generic;

namespace Peach.Core.Dom.Actions
{
	[Action("Input")]
	public class Input : Action
	{
		public ActionData data { get; set; }

		public override IEnumerable<ActionData> allData
		{
			get
			{
				yield return data;
			}
		}

		public override IEnumerable<ActionData> inputData
		{
			get
			{
				yield return data;
			}
		}

		protected override void OnRun(Publisher publisher, RunContext context)
		{
			publisher.start();
			publisher.open();
			publisher.input();

			try
			{
				var cracker = new DataCracker();
				cracker.CrackData(data.dataModel, new BitStream(publisher));
			}
			catch (CrackingFailure ex)
			{
				throw new SoftException(ex);
			}
		}
	}
}
