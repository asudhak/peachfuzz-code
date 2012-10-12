using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peach.Core.Analyzers;
using System.Xml;
using Peach.Core.Dom;

namespace Peach.Core
{
	/// <summary>
	/// Indicate a class implements methods required
	/// to support PIT Parsing.
	/// </summary>
	/// <remarks>
	/// Any type that is marked with this attribute must implement
	/// the following methods:
	/// 
	/// public static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
	/// 
	/// If unable to parse the current XML, just return null.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class PitParsableAttribute : Attribute
	{
		/// <summary>
		/// XML element name that corresponds to this type.
		/// </summary>
		public string xmlElementName;

		/// <summary>
		/// Indicate a class implements methods required
		/// to support PIT Parsing.
		/// </summary>
		/// <param name="xmlElementName">XML element name that corresponds to this type.</param>
		public PitParsableAttribute(string xmlElementName)
		{
			this.xmlElementName = xmlElementName;
		}
	}

	public delegate DataElement PitParserDelegate(PitParser context, XmlNode node, DataElementContainer parent);
}
