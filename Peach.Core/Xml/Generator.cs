
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
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Xml;

// TODO - Consume sample XML files to determin attribute and node data types
// TODO - Once we know data types, use Peach engine to produce values as well

namespace Peach.Core.Xml
{
	/// <summary>
	/// Generate XML documents based on Element definitions.
	/// </summary>
	public class Generator
	{
		public Dictionary<string, Element> elements;
		public Element rootElement;
		public Random random = new Random((uint)Environment.TickCount);
		public XmlDocument doc;

		/// <summary>
		/// Track depth to prevent infinit recurtion
		/// </summary>
		protected int _GenerateXmlNode_Depth = 0;

		public Generator(Element rootElement, Dictionary<string, Element> elements)
		{
			this.rootElement = rootElement;
			this.elements = elements;
		}

		public Generator(Element rootElement, Dictionary<string, Element> elements, uint randomSeed)
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
			XmlNode root = GenerateXmlNode(rootElement);
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
			try
			{
				_GenerateXmlNode_Depth++;

				XmlNode node = doc.CreateElement(element.name);

				// Create attributes
				GenerateXmlAttributes(element, node);

				// Create children
				if (element.relation != null && _GenerateXmlNode_Depth < 1024)
					HandleElementRelation(element, node, element.relation);

				return node;
			}
			finally
			{
				_GenerateXmlNode_Depth--;
			}
		}

		protected void HandleElementRelation(Element element, XmlNode node, ElementRelation relation)
		{
			switch (relation.type)
			{
			case ElementRelationType.And:
				HandleAnd(element, node, relation);
				break;
			case ElementRelationType.Or:
				HandleOr(element, node, relation);
				break;

			case ElementRelationType.One:
				node.AppendChild(GenerateXmlNode(relation.element));
				break;
			case ElementRelationType.OneOrMore:
				HandleOneOrMore(element, node, relation);
				break;
			case ElementRelationType.ZeroOrMore:
				HandleZeroOrMore(element, node, relation);
				break;
			case ElementRelationType.ZeroOrOne:
				HandleZeroOrOne(element, node, relation);
				break;

			case ElementRelationType.PCDATA:
				if (element.defaultValues.Count > 0)
					node.InnerText = element.defaultValues[0];
				else
					node.InnerText = "Peach";
				
				break;

			default:
				throw new NotImplementedException("Relation type '" + relation.type.ToString() + "' not supported yet.");
			}
		}

		protected void HandleZeroOrMore(Element element, XmlNode node, ElementRelation relation)
		{
			// TODO - Improve how we select number of nodes to generate!
			int cnt = 0;
			switch (random.Next(1))
			{
				case 0:
					cnt = random.Next(10);
					break;
				case 1:
					cnt = random.Next(100);
					break;
			}

			if (relation.relations.Count > 1)
				throw new ApplicationException("Relations larger than expected!");

			for (int i = 0; i < cnt; i++)
				HandleElementRelation(element, node, relation.relations[0]);
		}

		protected void HandleZeroOrOne(Element element, XmlNode node, ElementRelation relation)
		{
			if (random.Next(1) == 0)
				return;

			if (relation.element != null)
				node.AppendChild(GenerateXmlNode(relation.element));
			else
			{
				if (relation.relations.Count > 1)
					throw new ApplicationException("Relations larger than expected!");

				HandleElementRelation(element, node, relation.relations[0]);
			}
		}

		protected void HandleOneOrMore(Element element, XmlNode node, ElementRelation relation)
		{
			// TODO - Improve how we select number of nodes to generate!
			int cnt = 0;
			switch (random.Next(1))
			{
				case 0:
					cnt = random.Next(10);
					break;
				case 1:
					cnt = random.Next(100);
					break;
			}

			if (cnt == 0)
				cnt = 1;

			if (relation.relations.Count > 1)
				throw new ApplicationException("Relations larger than expected!");

			for (int i = 0; i < cnt; i++)
				if (relation.element == null)
					HandleElementRelation(element, node, relation.relations[0]);
				else
					node.AppendChild(GenerateXmlNode(relation.element));
		}

		protected void HandleOr(Element element, XmlNode node, ElementRelation relation)
		{
			int pick = random.Next(relation.relations.Count-1);
			HandleElementRelation(element, node, relation.relations[pick]);
		}

		protected void HandleAnd(Element element, XmlNode node, ElementRelation relation)
		{
			foreach (ElementRelation r in relation.relations)
				HandleElementRelation(element, node, r);
		}

		/// <summary>
		/// Generate the attributes for a new XmlNode based on
		/// the Element definition.
		/// </summary>
		/// <param name="element">Element definition</param>
		/// <param name="node">XmlNode to add attributes to</param>
		public void GenerateXmlAttributes(Element element, XmlNode node)
		{
			foreach (Attribute attrib in element.attributes.Values)
			{
				if (!attrib.required)
					if (random.Next(1) == 0)
						continue;

				if(attrib.implied)
					if (random.Next(1) == 0)
						continue;

				XmlAttribute xmlAttrib = doc.CreateAttribute(attrib.name);

				if (attrib.type == AttributeType.Enum)
					xmlAttrib.InnerText = attrib.enumValues[random.Next(attrib.enumValues.Count - 1)];
				else if (attrib.defaultValues.Count > 0)
					xmlAttrib.InnerText = attrib.defaultValues[random.Next(attrib.defaultValues.Count - 1)];
				else
					xmlAttrib.InnerText = "Peach";

				node.Attributes.Append(xmlAttrib);
			}
		}
	}
}
