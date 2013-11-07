using System;

namespace Peach.Core.Dom
{
	[Serializable]
	public class ActionParameter : ActionData
	{
		/// <summary>
		/// Type of parameter used when calling a method.
		/// 'In' means output the data on call
		/// 'Out' means input the data after the call
		/// 'InOut' means the data is output on call and input afterwards
		/// </summary>
		public enum Type { In, Out, InOut };

		/// <summary>
		/// The type of this parameter.
		/// </summary>
		public Type type { get; set; }

		/// <summary>
		/// Full input name of this parameter.
		/// 'Out' parameters are input
		/// </summary>
		public override string inputName { get { return base.outputName + ".Out"; } }

		/// <summary>
		/// Full output name of this parameter.
		/// 'In' parameters are input
		/// </summary>
		public override string outputName { get { return base.outputName + ".In"; } }
	}
}
