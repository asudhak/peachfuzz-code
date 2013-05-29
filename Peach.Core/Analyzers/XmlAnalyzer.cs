
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
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Analyzers
{
	[Analyzer("Xml", true)]
	[Analyzer("XmlAnalyzer")]
	[Analyzer("xml.XmlAnalyzer")]
	[Serializable]
	public class XmlAnalyzer : Analyzer
	{
		static XmlAnalyzer()
		{
			supportParser = false;
			supportDataElement = true;
			supportCommandLine = false;
			supportTopLevel = false;
		}

		public XmlAnalyzer()
		{
		}

		public XmlAnalyzer(Dictionary<string, Variant> args)
		{
		}

		public override void asDataElement(DataElement parent, object dataBuffer)
		{
			if (!(parent is Dom.String))
				throw new PeachException("Error, XmlAnalyzer analyzer only operates on String elements!");

			var strElement = parent as Dom.String;

			if (string.IsNullOrEmpty((string)strElement.InternalValue))
				return;

			var doc = new XmlDocument();
			try
			{
				doc.LoadXml((string)strElement.InternalValue);
			}
			catch (Exception ex)
			{
				throw new PeachException("Errorm XmlAnalyzer failed to analyze element '" + parent.name + "'.  " + ex.Message, ex);
			}

			Dom.XmlElement xmlElement = null;

			foreach(XmlNode node in doc.ChildNodes)
			{
				if (node is XmlDeclaration || node is XmlComment)
					continue;

				if (node.Name.StartsWith("#"))
					continue;

				xmlElement = handleXmlNode(node, parent.name);
			}

			xmlElement.parent = parent.parent;

			parent.parent[parent.name] = xmlElement;
		}

		protected Dom.XmlElement handleXmlNode(XmlNode node, string name)
		{
			Dom.XmlElement elem = null;

			if (name != null)
				elem = new Dom.XmlElement(name);
			else
				elem = new Dom.XmlElement();

			elem.elementName = node.Name;
			elem.ns = node.NamespaceURI;

			foreach (System.Xml.XmlAttribute attrib in node.Attributes)
			{
				elem.Add(handleXmlAttribute(attrib));
			}

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "#text")
				{
					var str = new Dom.String();
					str.DefaultValue = new Variant(child.Value);
					elem.Add(str);
				}
				else if (!child.Name.StartsWith("#"))
				{
					elem.Add(handleXmlNode(child, null));
				}
			}

			return elem;
		}

		protected Dom.XmlAttribute handleXmlAttribute(System.Xml.XmlAttribute attrib)
		{
			var xmlAttrib = new Dom.XmlAttribute();
			xmlAttrib.attributeName = attrib.Name;
			xmlAttrib.ns = attrib.NamespaceURI;

			var strValue = new Dom.String();
			strValue.DefaultValue = new Variant(attrib.Value);

			xmlAttrib.Add(strValue);

			return xmlAttrib;
		}
	}
}

// end
