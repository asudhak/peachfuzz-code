
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
	[Parameter("name", typeof(string), "Name of element", false)]
	[Parameter("attributeName", typeof(string), "Name of attribute", true)]
	[Parameter("ns", typeof(string), "XML Namespace", false)]
	[Serializable]
	public class XmlAttribute : DataElementContainer
	{
		string _attributeName = null;
		string _ns = null;

		public static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			if (node.Name != "XmlAttribute" || !(parent is XmlElement))
				return null;

			var xmlAttribute = DataElement.Generate<XmlAttribute>(node);

			xmlAttribute.attributeName = node.getAttribute("attributeName");
			xmlAttribute.ns = node.getAttribute("ns");

			if (xmlAttribute.attributeName == null)
				throw new PeachException("Error, attributeName is a required attribute for XmlAttribute: " + xmlAttribute.name);

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
		public virtual System.Xml.XmlAttribute GenerateXmlAttribute(XmlDocument doc, XmlNode parent)
		{
			var xmlAttrib = doc.CreateAttribute(attributeName, ns);
			xmlAttrib.Value = (string)this[0].InternalValue;
			return xmlAttrib;
		}

    public override object GetParameter(string parameterName)
    {
      switch (parameterName)
      {
        case "name":
          return this.name;
        case "attributeName":
          return this.attributeName;
        case "ns":
          return this.ns;
        default:
          throw new PeachException(System.String.Format("Parameter '{0}' does not exist in Peach.Core.Dom.XmlAttribute", parameterName));
      }
    }
	}
}

// end
