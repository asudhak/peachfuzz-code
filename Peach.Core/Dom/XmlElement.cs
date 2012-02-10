
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using Peach;
using System.Xml;

namespace Peach.Core.Dom
{
	[DataElement("XmlElement")]
	[DataElementChildSupported(DataElementTypes.Any)]
	[DataElementRelationSupported(DataElementRelations.Any)]
	[Parameter("name", typeof(string), "Name of element", false)]
	[Parameter("ns", typeof(string), "XML Namespace", false)]
	[Parameter("elementName", typeof(string), "XML Element Name", true)]
	[Serializable]
	public class XmlElement : DataElementContainer
	{
		protected string elementName = null;
		protected string ns = null;

		public XmlElement()
		{
		}

		public XmlElement(string name) : base()
		{
			this.name = name;
		}

		public virtual XmlNode GenerateXmlNode(XmlDocument doc, XmlNode parent)
		{
			XmlNode xmlNode = doc.CreateElement(elementName, ns);

			foreach (DataElement child in this)
			{
				if (child is XmlAttribute)
				{
					XmlAttribute attrib = child as XmlAttribute;
					xmlNode.Attributes.Append(attrib.GenerateXmlAttribute(doc, xmlNode));
				}
				else if (child is String)
				{
					xmlNode.Value = (string)child.InternalValue;
				}
				else if (child is XmlElement)
				{
					xmlNode.AppendChild(((XmlElement)child).GenerateXmlNode(doc, xmlNode));
				}
				else
				{
					throw new PeachException("Error, XmlElements can only contain XmlElement, XmlAttribute, and a single String element.");
				}
			}

			return xmlNode;
		}

		public override Variant GenerateInternalValue()
		{
			XmlDocument doc = new XmlDocument();
			doc.AppendChild(GenerateXmlNode(doc, null));
			return new Variant(doc.OuterXml);
		}
	}
}

// end
