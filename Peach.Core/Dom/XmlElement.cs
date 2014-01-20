
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
using Peach.Core.IO;

namespace Peach.Core.Dom
{
	public class PeachXmlDoc
	{
		public PeachXmlDoc()
		{
		}

		public XmlDocument doc = new XmlDocument();
		public Dictionary<string, Variant> values = new Dictionary<string, Variant>();
	}

	[DataElement("XmlElement")]
	[PitParsable("XmlElement")]
	[Parameter("name", typeof(string), "Name of element", "")]
	[Parameter("attributeName", typeof(string), "Name of XML element")]
	[Parameter("ns", typeof(string), "XML Namespace", "")]
	[Parameter("length", typeof(uint?), "Length in data element", "")]
	[Parameter("lengthType", typeof(LengthType), "Units of the length attribute", "bytes")]
	[Parameter("mutable", typeof(bool), "Is element mutable", "false")]
	[Parameter("constraint", typeof(string), "Scripting expression that evaluates to true or false", "")]
	[Parameter("minOccurs", typeof(int), "Minimum occurances", "1")]
	[Parameter("maxOccurs", typeof(int), "Maximum occurances", "1")]
	[Parameter("occurs", typeof(int), "Actual occurances", "1")]
	[Serializable]
	public class XmlElement : DataElementContainer
	{
		string _elementName = null;
		string _ns = null;

		public XmlElement()
		{
		}

		public XmlElement(string name)
			: base(name)
		{
		}

		public static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			if (node.Name != "XmlElement")
				return null;

			var xmlElement = DataElement.Generate<XmlElement>(node);

			xmlElement.elementName = node.getAttrString("elementName");

			if (node.hasAttr("ns"))
				xmlElement.ns = node.getAttrString("ns");

			context.handleCommonDataElementAttributes(node, xmlElement);
			context.handleCommonDataElementChildren(node, xmlElement);
			context.handleDataElementContainer(node, xmlElement);

			return xmlElement;
		}

		/// <summary>
		/// XML Element tag name
		/// </summary>
		public virtual string elementName
		{
			get { return _elementName; }
			set
			{
				_elementName = value;
				// DefaultValue isn't used internally, but this makes the Validator show helpful text
				_defaultValue = new Variant("<{0}> Element".Fmt(value));
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

		public virtual XmlNode GenerateXmlNode(PeachXmlDoc doc, XmlNode parent)
		{
			XmlNode xmlNode = doc.doc.CreateElement(elementName, ns);

			foreach (DataElement child in this)
			{
				if (child is XmlAttribute)
				{
					XmlAttribute attrib = child as XmlAttribute;
					if (child.mutationFlags.HasFlag(MutateOverride.TypeTransform))
					{
						// Happend when data element is duplicated.  Duplicate attributes are invalid so ignore.
						continue;
					}

					if (attrib.Count > 0)
					{
						xmlNode.Attributes.Append(attrib.GenerateXmlAttribute(doc, xmlNode));
					}
				}
				else if (child is Number)
				{
					xmlNode.InnerText = (string)child.InternalValue;
				}
				else if (child is XmlElement && !child.mutationFlags.HasFlag(MutateOverride.TypeTransform))
				{
					xmlNode.AppendChild(((XmlElement)child).GenerateXmlNode(doc, xmlNode));
				}
				else
				{
					var key = "|||" + child.fullName + "|||";
					var text = doc.doc.CreateTextNode(key);
					xmlNode.AppendChild(text);
					doc.values.Add(key, new Variant(child.Value));
				}
			}

			return xmlNode;
		}

		protected override Variant GenerateInternalValue()
		{
			if (mutationFlags.HasFlag(MutateOverride.TypeTransform))
				return MutatedValue;

			PeachXmlDoc doc = new PeachXmlDoc();
			doc.doc.AppendChild(GenerateXmlNode(doc, null));
			string template = doc.doc.OuterXml;
			string[] parts = template.Split(new string[]{"|||"}, StringSplitOptions.RemoveEmptyEntries);

			BitStream bs = new BitStream();

			foreach (string item in parts)
			{
				BitwiseStream toWrite = null;
				Variant var = null;
				string key = "|||" + item + "|||";
				if (doc.values.TryGetValue(key, out var))
				{
					var type = var.GetVariantType();

					if (type == Variant.VariantType.BitStream)
						toWrite = (BitwiseStream)var;
					else if (type == Variant.VariantType.ByteString)
						toWrite = new BitStream((byte[])var);
					else
						toWrite = new BitStream(Encoding.ASCII.GetRawBytes((string)var));
				}
				else
				{
					toWrite = new BitStream(Encoding.ASCII.GetRawBytes(item));
				}
				toWrite.SeekBits(0, System.IO.SeekOrigin.Begin);
				toWrite.CopyTo(bs);
			}

			return new Variant(bs);
		}

		protected override BitwiseStream InternalValueToBitStream()
		{
			if (mutationFlags.HasFlag(MutateOverride.TypeTransform) && MutatedValue != null)
				return (BitwiseStream)MutatedValue;

			return (BitwiseStream)InternalValue;
		}
	}
}

// end
