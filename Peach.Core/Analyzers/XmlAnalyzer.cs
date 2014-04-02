
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
using Peach.Core.Cracker;

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

		public override void asDataElement(DataElement parent, Dictionary<DataElement, Position> positions)
		{
			var strElement = parent as Dom.String;
			if (strElement == null)
				throw new PeachException("Error, XmlAnalyzer analyzer only operates on String elements!");

			var value = (string)strElement.InternalValue;
			if (string.IsNullOrEmpty(value))
				return;

			var doc = new XmlDocument();

			try
			{
				doc.LoadXml(value);
			}
			catch (Exception ex)
			{
				throw new PeachException("Error, XmlAnalyzer failed to analyze element '" + parent.name + "'.  " + ex.Message, ex);
			}

			var elem = new Dom.XmlElement(strElement.name);

			foreach (XmlNode node in doc.ChildNodes)
			{
				handleXmlNode(elem, node, strElement.stringType);
			}

			var decl = doc.FirstChild as XmlDeclaration;
			if (decl != null)
			{
				elem.version = decl.Version;
				elem.encoding = decl.Encoding;
				elem.standalone = decl.Standalone;
			}

			parent.parent[parent.name] = elem;
		}

		protected void handleXmlNode(Dom.XmlElement elem, XmlNode node, StringType type)
		{
			if (node is XmlComment || node is XmlDeclaration)
				return;

			elem.elementName = node.Name;
			elem.ns = node.NamespaceURI;

			foreach (System.Xml.XmlAttribute attr in node.Attributes)
			{
				var strElem = makeString("value", attr.Value, type);
				var attrName = elem.UniqueName(attr.Name.Replace(':', '_'));
				var attrElem = new Dom.XmlAttribute(attrName)
				{
					attributeName = attr.Name,
					ns = attr.NamespaceURI,
				};

				attrElem.Add(strElem);
				elem.Add(attrElem);
			}

			foreach (System.Xml.XmlNode child in node.ChildNodes)
			{
				if (child.Name == "#text")
				{
					var str = makeString(child.Name, child.Value, type);
					elem.Add(str);
				}
				else if (!child.Name.StartsWith("#"))
				{
					var childName = elem.UniqueName(child.Name.Replace(':', '_'));
					var childElem = new Dom.XmlElement(childName);

					elem.Add(childElem);

					handleXmlNode(childElem, child, type);
				}
			}
		}

		private static Dom.String makeString(string name, string value, StringType type)
		{
			var str = new Dom.String(name)
			{
				stringType = type,
				DefaultValue = new Variant(value),
			};

			var hint = new Dom.Hint("Peach.TypeTransform", "false");
			str.Hints.Add(hint.Name, hint);

			return str;
		}
	}
}
