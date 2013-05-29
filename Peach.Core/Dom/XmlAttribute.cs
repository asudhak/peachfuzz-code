
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Peach.Core.Analyzers;
using Peach;

namespace Peach.Core.Dom
{
	[DataElement("XmlAttribute")]
	[PitParsable("XmlAttribute")]
	[DataElementChildSupported(DataElementTypes.NonDataElements)]
	[Parameter("name", typeof(string), "Name of element", "")]
	[Parameter("attributeName", typeof(string), "Name of XML attribute")]
	[Parameter("ns", typeof(string), "XML Namespace", "")]
	[Parameter("length", typeof(uint?), "Length in data element", "")]
	[Parameter("lengthType", typeof(LengthType), "Units of the length attribute", "bytes")]
	[Parameter("mutable", typeof(bool), "Is element mutable", "false")]
	[Parameter("constraint", typeof(string), "Scripting expression that evaluates to true or false", "")]
	[Parameter("minOccurs", typeof(int), "Minimum occurances", "1")]
	[Parameter("maxOccurs", typeof(int), "Maximum occurances", "1")]
	[Parameter("occurs", typeof(int), "Actual occurances", "1")]
	[Serializable]
	public class XmlAttribute : DataElementContainer
	{
		string _attributeName = null;
		string _ns = null;

		public XmlAttribute()
		{
		}

		public XmlAttribute(string name)
			: base(name)
		{
		}

		public static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			if (node.Name != "XmlAttribute" || !(parent is XmlElement))
				return null;

			var xmlAttribute = DataElement.Generate<XmlAttribute>(node);

			xmlAttribute.attributeName = node.getAttrString("attributeName");

			if (node.hasAttr("ns"))
				xmlAttribute.ns = node.getAttrString("ns");

			context.handleCommonDataElementAttributes(node, xmlAttribute);
			context.handleCommonDataElementChildren(node, xmlAttribute);
			context.handleDataElementContainer(node, xmlAttribute);

			return xmlAttribute;
		}

		/// <summary>
		/// XML attribute name
		/// </summary>
		public virtual string attributeName
		{
			get { return _attributeName; }
			set
			{
				_attributeName = value;
				// DefaultValue isn't used internally, but this makes the Validator show helpful text
				_defaultValue = new Variant("'{0}' Attribute".Fmt(value));
				Invalidate();
			}
		}

		/// <summary>
		/// XML Namespace for element
		/// </summary>
		public virtual string ns
		{
			get { return _ns; }
			set
			{
				_ns = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Generate a System.Xml.XmlAttribute instance and populate with
		/// correct information.
		/// </summary>
		/// <param name="doc">XmlDocument this attribute will be part of.</param>
		/// <param name="parent">The parent XmlNode</param>
		/// <returns>Returns a valid instance of an XmlAttribute.</returns>
		public virtual System.Xml.XmlAttribute GenerateXmlAttribute(PeachXmlDoc doc, XmlNode parent)
		{
			System.Diagnostics.Debug.Assert(Count > 0);
			var elem = this[0];
			var xmlAttrib = doc.doc.CreateAttribute(attributeName, ns);
			xmlAttrib.Value = "|||" + elem.fullName + "|||";
			doc.values.Add(xmlAttrib.Value, elem.InternalValue);
			return xmlAttrib;
		}

		protected override Variant GenerateInternalValue()
		{
			return null;
		}
	}
}

// end
