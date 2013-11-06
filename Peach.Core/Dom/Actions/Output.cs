using System;
using System.Collections.Generic;

namespace Peach.Core.Dom.Actions
{
	[Action("Output")]
	public class Output : Action
	{
		public ActionData data { get; set; }

		public override IEnumerable<ActionData> allData
		{
			get
			{
				yield return data;
			}
		}

		public override IEnumerable<ActionData> outputData
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

			var value = data.dataModel.Value;
			publisher.output(value);
		}
	}
}
