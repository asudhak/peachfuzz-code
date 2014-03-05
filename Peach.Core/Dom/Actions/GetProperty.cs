using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Peach.Core.Dom.Actions
{
	[Action("GetProperty")]
	public class GetProperty : Action
	{
		/// <summary>
		/// Property to operate on
		/// </summary>
		[XmlAttribute]
		[DefaultValue(null)]
		public string property { get; set; }

		/// <summary>
		/// Data model to populate with property value
		/// </summary>
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

			var result = publisher.getProperty(property);
			data.dataModel.DefaultValue = result;
		}
	}
}
