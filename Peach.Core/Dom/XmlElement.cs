
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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Peach.Core.Analyzers;
using Peach.Core.IO;
using NLog;

namespace Peach.Core.Dom
{
	[DataElement("XmlElement")]
	[PitParsable("XmlElement")]
	[Parameter("name", typeof(string), "Name of element", "")]
	[Parameter("elementName", typeof(string), "Name of XML element")]
	[Parameter("ns", typeof(string), "XML Namespace", "")]
	[Parameter("length", typeof(uint?), "Length in data element", "")]
	[Parameter("lengthType", typeof(LengthType), "Units of the length attribute", "bytes")]
	[Parameter("mutable", typeof(bool), "Is element mutable", "true")]
	[Parameter("constraint", typeof(string), "Scripting expression that evaluates to true or false", "")]
	[Parameter("minOccurs", typeof(int), "Minimum occurances", "1")]
	[Parameter("maxOccurs", typeof(int), "Maximum occurances", "1")]
	[Parameter("occurs", typeof(int), "Actual occurances", "1")]
	[Serializable]
	public class XmlElement : DataElementContainer
	{
		protected static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		string _elementName = null;
		string _ns = null;

		public string version { get; set; }
		public string encoding { get; set; }
		public string standalone { get; set; }

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

		protected static string ElemToStr(DataElement elem)
		{
			var iv = elem.InternalValue;
			if (iv.GetVariantType() != Variant.VariantType.BitStream)
				return (string)iv;

			var bs = elem.Value;
			var ret = new BitReader(bs).ReadString(Encoding.ISOLatin1);
			bs.Seek(0, System.IO.SeekOrigin.Begin);
			return ret;
		}

		protected static string ContToStr(DataElementContainer cont)
		{
			var sb = new StringBuilder();
			foreach (var item in cont)
				sb.Append(ElemToStr(item));
			return sb.ToString();
		}

		protected void GenXmlNode(XmlDocument doc, XmlNode parent)
		{
			var node = doc.CreateElement(elementName, ns);
			parent.AppendChild(node);

			foreach (var child in this)
			{
				var asAttr = child as XmlAttribute;
				if (asAttr != null && asAttr.attributeName != null)
				{
					var asStr = ContToStr(asAttr);

					// If the attribute is xmlns and the value is empty
					// we can't add it to the document w/o getting an
					// ArgumentException when generating the final xml
					if (asAttr.attributeName.Split(new[] { ':' })[0] == "xmlns" && string.IsNullOrEmpty(asStr))
						continue;

					var attr = doc.CreateAttribute(asAttr.attributeName, asAttr.ns);
					attr.Value = asStr;
					node.Attributes.Append(attr);
					continue;
				}

				var asElem = child as XmlElement;
				if (asElem != null && !asElem.mutationFlags.HasFlag(MutateOverride.TypeTransform))
				{
					asElem.GenXmlNode(doc, node);
					continue;
				}

				var text = doc.CreateTextNode(ElemToStr(child));
				node.AppendChild(text);
			}
		}

		protected override Variant GenerateInternalValue()
		{
			if (mutationFlags.HasFlag(MutateOverride.TypeTransform))
				return MutatedValue;

			var doc = new XmlDocument();

			if (!string.IsNullOrEmpty(version) || !string.IsNullOrEmpty(encoding) || !string.IsNullOrEmpty(standalone))
			{
				var decl = doc.CreateXmlDeclaration(version, encoding, standalone);
				doc.AppendChild(decl);
			}

			GenXmlNode(doc, doc);

			try
			{
				return new Variant(doc.OuterXml);
			}
			catch (Exception ex)
			{
				throw new SoftException(ex);
			}
		}

		protected override BitwiseStream InternalValueToBitStream()
		{
			if (mutationFlags.HasFlag(MutateOverride.TypeTransform) && MutatedValue != null)
				return (BitwiseStream)MutatedValue;

			var enc = Encoding.GetEncoding(encoding ?? "utf-8");
			var ret = new BitStream(enc.GetRawBytes((string)InternalValue));
			return ret;
		}
	}
}

// end
