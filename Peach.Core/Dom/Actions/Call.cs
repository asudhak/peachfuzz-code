using System;
using Peach.Core.IO;
using Peach.Core.Cracker;
using System.Collections.Generic;
using System.Linq;

namespace Peach.Core.Dom.Actions
{
	[Action("Call")]
	public class Call : Action
	{
		public Call()
		{
			parameters = new NamedCollection<ActionParameter>("Param");
		}

		/// <summary>
		/// Method to call
		/// </summary>
		public string method { get; set; }

		/// <summary>
		/// Array of parameters for a method call
		/// </summary>
		public NamedCollection<ActionParameter> parameters { get; private set; }

		/// <summary>
		/// Action result for a method call
		/// </summary>
		public ActionData result { get; set; }

		public override IEnumerable<ActionData> allData
		{
			get
			{
				foreach (var item in parameters)
					yield return item;

				if (result != null)
					yield return result;

				yield break;
			}
		}

		public override IEnumerable<ActionData> inputData
		{
			get
			{
				// inputData is used for cracking
				// Out and InOut params to a method
				// are data inputs

				foreach (var item in parameters)
				{
					if (item.type != ActionParameter.Type.In)
						yield return item;
				}

				if (result != null)
					yield return result;

				yield break;
			}
		}

		public override IEnumerable<ActionData> outputData
		{
			get
			{
				// outputData is used for fuzzing
				// In and InOut params to a method
				// are data outputs
				return parameters.Where(p => p.type != ActionParameter.Type.Out);
			}
		}

		protected override void OnRun(Publisher publisher, RunContext context)
		{
			publisher.start();

			Variant ret = null;

			// Are we sending to Agents?
			if (this.publisher == "Peach.Agent")
				ret = context.agentManager.Message("Action.Call", new Variant(method));
			else
				ret = publisher.call(method, parameters.ToList());

			if (result != null && ret != null)
			{
				BitStream data;

				try
				{
					data = (BitStream)ret;
				}
				catch (NotSupportedException)
				{
					throw new PeachException("Error, unable to convert result from method '" + method + "' to a BitStream");
				}

				try
				{
					var cracker = new DataCracker();
					cracker.CrackData(result.dataModel, data);
				}
				catch (CrackingFailure ex)
				{
					throw new SoftException(ex);
				}
			}
		}
	}
}
