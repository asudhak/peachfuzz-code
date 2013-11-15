using System;
using System.Collections.Generic;

namespace Peach.Core.Dom.Actions
{
	[Action("SetProperty")]
	public class SetProperty : Action
	{
		/// <summary>
		/// Property to operate on
		/// </summary>
		public string property { get; set; }

		/// <summary>
		/// Data model containing value of property
		/// </summary>
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

			var value = data.dataModel.InternalValue;
			publisher.setProperty(property, value);
		}
	}
}
