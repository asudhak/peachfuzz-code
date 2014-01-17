using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Peach.Core.Dom
{
	[Serializable]
	public class ActionParameter : ActionData
	{
		public ActionParameter(string name)
		{
			this.name = name;
		}

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
		[XmlAttribute]
		[DefaultValue(Type.In)]
		public Type type { get; set; }

		/// <summary>
		/// Currently unused.  Exists for schema generation.
		/// </summary>
		[XmlElement]
		[DefaultValue(null)]
		public List<Peach.Core.Xsd.DataRef> Data { get; set; }

		/// <summary>
		/// Full input name of this parameter.
		/// 'Out' parameters are input
		/// </summary>
		public override string inputName { get { return base.inputName + ".Out"; } }

		/// <summary>
		/// Full output name of this parameter.
		/// 'In' parameters are input
		/// </summary>
		public override string outputName { get { return base.outputName + ".In"; } }
	}
}
