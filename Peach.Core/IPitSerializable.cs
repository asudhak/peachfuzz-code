using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Peach.Core
{
	public interface IPitSerializable
	{
		/// <summary>
		/// Serialize into PIT XML format
		/// </summary>
		/// <param name="doc">Document object for peach pit</param>
		/// <param name="parent">Parent node (may be null)</param>
		/// <returns>Returns instance of XmlNode containing serialized data or null.</returns>
		XmlNode pitSerialize(XmlDocument doc, XmlNode parent);
	}
}
