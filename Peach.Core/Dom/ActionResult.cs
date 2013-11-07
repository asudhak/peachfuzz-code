using System;

namespace Peach.Core.Dom
{
	[Serializable]
	public class ActionResult : ActionData
	{
		/// <summary>
		/// Full input name of this data.
		/// </summary>
		public override string inputName { get { return base.inputName + ".Result"; } }

		/// <summary>
		/// Full output name of this data.
		/// </summary>
		public override string outputName { get { return base.outputName + ".Result"; } }
	}
}
