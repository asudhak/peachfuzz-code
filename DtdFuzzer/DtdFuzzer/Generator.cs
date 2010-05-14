
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
using System.Linq;
using System.Text;
using System.Xml;

namespace DtdFuzzer
{
	/// <summary>
	/// Generate XML documents based on Element definitions.
	/// </summary>
	public class Generator
	{
		public Dictionary<string, Element> elements;
		public Element rootElement;
		public Random random = new Random();
		public XmlDocument doc;

		public Generator(Element rootElement, Dictionary<string, Element> elements)
		{
			this.rootElement = rootElement;
			this.elements = elements;
		}

		public Generator(Element rootElement, Dictionary<string, Element> elements, int randomSeed)
		{
			this.rootElement = rootElement;
			this.elements = elements;
			this.random = new Random(randomSeed);
		}

		/// <summary>
		/// Generate an XmlDocument based on the definition
		/// of the root Element provided to the constructor
		/// of this class.
		/// 
		/// Each call to GenerateXmlDocument will return a 
		/// different document.
		/// </summary>
		/// <returns>Returns XmlDocument generated from rootElement definition.</returns>
		public XmlDocument GenerateXmlDocument()
		{
			doc = new XmlDocument();
			XmlNode root = doc.CreateElement(rootElement.name);
			doc.AppendChild(root);

			return doc;
		}

		/// <summary>
		/// Generate an XmlNode based on an Element definition.
		/// Will also generate all attributes and child elements.
		/// </summary>
		/// <param name="element">Element definition to use</param>
		/// <returns>Returns XmlNode object</returns>
		public XmlNode GenerateXmlNode(Element element)
		{
			XmlNode node = doc.CreateElement(element.name);

			// Create attributes

			// Create children

			return node;
		}

		/// <summary>
		/// Generate the attributes for a new XmlNode based on
		/// the Element definition.
		/// </summary>
		/// <param name="element">Element definition</param>
		/// <param name="node">XmlNode to add attributes to</param>
		public void GenerateXmlAttributes(Element element, XmlNode node)
		{
		}
	}
}
