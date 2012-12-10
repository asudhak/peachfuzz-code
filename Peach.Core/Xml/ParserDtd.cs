
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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Peach.Core.Xml
{
	/// <summary>
	/// DTD Xml Schema parser.  Converts a DTD specification into
	/// an element constraint graph that can be used to generate XML.
	/// </summary>
	public class ParserDtd : Parser
	{
		Regex reElement = new Regex(@"<!ELEMENT\s+([^\s]+)\s+([^>]*)\s*>",
			RegexOptions.Singleline);
		Regex reElementEmpty = new Regex(@"\s*EMPTY\s*",
			RegexOptions.Singleline);
		Regex reElementAny = new Regex(@"\s*ANY\s*",
			RegexOptions.Singleline);
		Regex reElementElementContext = new Regex(@"\s*(\(.*\)[?*+]?)\s*",
			RegexOptions.Singleline);
		Regex reElementCategory = new Regex(@"\s*([^#\s()]+)\s*",
			RegexOptions.Singleline);

		Regex reEntity = new Regex(@"<!ENTITY\s*%?\s*([^\s]+)\s+(?:PUBLIC\s+)?(" + "\"[^>]*\")\\s*>",
			RegexOptions.Singleline);

		Regex reAttributeList = new Regex(@"<!ATTLIST\s+([^\s]+)\s+([^>]+)\s*>",
			RegexOptions.Singleline);
		Regex reAttribute1 = new Regex(@"^\s*([^\s]+)\s+([^\s]+|\([^\)]+\))\s+#(FIXED|IMPLIED|REQUIRED)\s+'(.*)'\s*$", RegexOptions.Multiline);
		Regex reAttribute2 = new Regex(@"^\s*([^\s]+)\s+([^\s]+|\([^\)]+\))\s+#(FIXED|IMPLIED|REQUIRED)\s*$", RegexOptions.Multiline);
		Regex reAttribute3 = new Regex(@"^\s*([^\s]+)\s+([^\s]+|\([^\)]+\))\s+'(.*)'\s*$", RegexOptions.Multiline);
		Regex reAttributeEnum = new Regex(@"\s*\(([^)]*)\)\s*");
		Regex reAttributeEnumValues = new Regex(@"\b([^\s|]+)\b");

		public override void parse(TextReader reader)
		{
			Console.WriteLine("DTD Parser Starting...");

			string data = reader.ReadToEnd();
			data = removeComments(data);

			foreach (Match entity in reEntity.Matches(data))
				handleEntity(entity);

			foreach (Match element in reElement.Matches(data))
				handleElement(element);

			foreach (Match attributeList in reAttributeList.Matches(data))
				handleAttributeList(attributeList);

			// Second pass to resolve data portion of elements
			foreach (Element el in elements.Values)
				handleElementPass2(el);

			Console.WriteLine("DTD Parser finished...");
		}

		protected void finishAttribute(string name, string attribName, string attribType,
			string attribValue, string attribModifier)
		{
			bool isImplied = false;
			bool isRequired = false;

			if (attribModifier != null)
			{
				switch (attribModifier.ToLower())
				{
					case "fixed":
						break;
					case "implied":
						isImplied = true;
						break;
					case "required":
						isRequired = true;
						break;
				}
			}

			Console.WriteLine("\t'" + attribName + "' is '" + attribType + "'.");

			Attribute attribute = new Attribute();
			attribute.name = attribName;
			attribute.implied = isImplied;
			attribute.required = isRequired;
			attribute.value = attribValue;

			switch (attribType.ToLower())
			{
				case "cdata":
					attribute.type = AttributeType.CDATA;
					break;
				case "id":
					attribute.type = AttributeType.ID;
					break;
				case "idref":
					attribute.type = AttributeType.IDREF;
					break;
				case "IDREFS":
					attribute.type = AttributeType.IDREFS;
					break;
				case "nmtoken":
					attribute.type = AttributeType.NMTOKEN;
					break;
				case "nmtokens":
					attribute.type = AttributeType.NMTOKENS;
					break;
				case "ENTITY":
					attribute.type = AttributeType.ENTITY;
					break;
				case "ENTITIES":
					attribute.type = AttributeType.ENTITIES;
					break;
				case "NOTATION":
					attribute.type = AttributeType.NOTATION;
					break;

				default:
					// must be enum
					if (!reAttributeEnum.IsMatch(attribType))
						throw new ApplicationException("Failed to parse attribute type");

					Match match = reAttributeEnum.Match(attribType);
					foreach (Match values in reAttributeEnumValues.Matches(match.Groups[1].Value))
					{
						attribute.enumValues.Add(values.Groups[1].Value);
					}

					break;
			}

			elements[name].attributes[attribName] = attribute;
		}

		protected void handleAttributeList(Match match)
		{
			string attribName = null;
			string attribType = null;
			string attribValue = null;

			string name = Entity.ResolveEntities(match.Groups[1].Value);
			string data = Entity.ResolveEntities(match.Groups[2].Value);

			Console.WriteLine("Found attribute list for element '" + name + "'.");

			foreach (Match attrib in reAttribute1.Matches(data))
			{
				attribName = attrib.Groups[1].Value;
				attribType = attrib.Groups[2].Value;
				attribValue = attrib.Groups[4].Value;

				finishAttribute(name, attribName, attribType, attribValue, attrib.Groups[3].Value);
			}
			foreach (Match attrib in reAttribute2.Matches(data))
			{
				attribName = attrib.Groups[1].Value;
				attribType = attrib.Groups[2].Value;
				attribValue = null;

				finishAttribute(name, attribName, attribType, attribValue, attrib.Groups[3].Value);
			}
			foreach (Match attrib in reAttribute3.Matches(data))
			{
				attribName = attrib.Groups[1].Value;
				attribType = attrib.Groups[2].Value;
				attribValue = attrib.Groups[3].Value;

				finishAttribute(name, attribName, attribType, attribValue, null);
			}
		}

		protected string removeComments(string data)
		{
			while (true)
			{
				int startIndex = data.IndexOf("<!--");
				if (startIndex == -1)
					break;

				int endIndex = data.IndexOf("-->");
				if (endIndex == -1)
					break;

				data = data.Remove(startIndex, (endIndex - startIndex) + 3);
			}

			return data;
		}

		protected void handleEntity(Match match)
		{
			string name = match.Groups[1].Value;
			string data = match.Groups[2].Value;
			string value = "";
			Regex reString = new Regex("\"([^\"]*)\"", RegexOptions.Singleline);

			foreach (Match part in reString.Matches(data))
				value += part.Groups[1].Value;

			Console.WriteLine("Found entity '" + name + "'.");

			Entity.entities[name] = new Entity(name, value);
		}

		protected void handleElement(Match match)
		{
			string name = null;
			string data = null;

			name = Entity.ResolveEntities(match.Groups[1].Value);
			data = Entity.ResolveEntities(match.Groups[2].Value);

			Element element = new Element();
			element.name = name;
			elements[name] = element;

			elementData[element] = data;

			Console.WriteLine("Found element '" + name + "'.");
		}

		protected void handleElementPass2(Element element)
		{
			string data = elementData[element];

			if (reElementAny.IsMatch(data))
			{
				element.isAny = true;
			}
			else if (reElementElementContext.IsMatch(data))
			{
				int pos = 1;

				data = data.Replace("\n", "").Replace("\r", "");

				element.relation = handleElementDataBlock(data, ref pos);
			}
			else if (reElementCategory.IsMatch(data))
			{
			}
			else if (reElementEmpty.IsMatch(data))
			{
				element.isEmpty = true;
			}
		}

		protected ElementRelation handleElementDataBlock(string data, ref int pos)
		{
			ElementRelation relation;
			char token;

			int indexOfComma = data.IndexOf(',', pos);
			int indexOfPipe = data.IndexOf('|', pos);
			int indexOfParenClose = data.IndexOf(')', pos);

			if ((indexOfComma < indexOfPipe && indexOfComma != -1) || indexOfPipe == -1)
			{
				token = ',';
				relation = new ElementRelation(ElementRelationType.And);
			}
			else
			{
				token = '|';
				relation = new ElementRelation(ElementRelationType.Or);
			}

			if (indexOfParenClose == -1)
				throw new ApplicationException("Error, didn't find closing paren!");

			int indexOfToken = data.IndexOf(token, pos);
			if (indexOfParenClose < indexOfToken && data.IndexOf(token, pos) != -1)
				relation = null;

			string name = null;
			char c;
			ElementRelation r = null;

			for (; pos < data.Length; pos++)
			{
				c = data[pos];

				if (c == ',' || c == '|' || c == ')')
				{
					if (r == null)
						r = new ElementRelation(ElementRelationType.One);

					if (name != null)
					{
						if (name == "#PCDATA")
							r.type = ElementRelationType.PCDATA;
						else
							r.element = elements[name];
					}

					if (relation == null)
						relation = r;
					else
						relation.relations.Add(r);

					if (c == ')')
					{
						// We shouldn't have Or/And with a single
						// relation as child.  If we do, remove usless
						// abstraction.
						if ((relation.type == ElementRelationType.Or ||
							relation.type == ElementRelationType.And) &&
							relation.relations.Count == 1)
						{
							relation = relation.relations[0];
						}

						// Look ahead for ?, +, *
						if ((pos + 1) < data.Length)
						{
							pos++;
							char nextC = data[pos];
							switch (nextC)
							{
								case '?':
									r = new ElementRelation(ElementRelationType.ZeroOrOne);
									r.relations.Add(relation);
									relation = r;
									break;
								case '+':
									r = new ElementRelation(ElementRelationType.OneOrMore);
									r.relations.Add(relation);
									relation = r;
									break;
								case '*':
									r = new ElementRelation(ElementRelationType.ZeroOrMore);
									r.relations.Add(relation);
									relation = r;
									break;

								default:
									// Backup if we don't locate
									// a parsable character.
									pos--;
									break;
							}
						}

						return relation;
					}

					r = null;
					name = null;
				}
				else if (c == '+')
				{
					ElementRelation oldR = r;
					r = new ElementRelation(ElementRelationType.OneOrMore);

					if (oldR != null)
						r.relations.Add(r);
				}
				else if (c == '*')
				{
					ElementRelation oldR = r;
					r = new ElementRelation(ElementRelationType.ZeroOrMore);

					if (oldR != null)
						r.relations.Add(r);
				}
				else if (c == '?')
				{
					ElementRelation oldR = r;
					r = new ElementRelation(ElementRelationType.ZeroOrOne);

					if (oldR != null)
						r.relations.Add(r);
				}
				else if (c == '(')
				{
					pos++;
					r = handleElementDataBlock(data, ref pos);
				}

				else if (char.IsWhiteSpace(c))
					continue;

				else
				{
					if (c == ',')
						Debugger.Break();

					if (name == null)
						name = c.ToString();
					else
						name += c;
				}
			}

			throw new ApplicationException("Whoops, we shouldn't be here!");
		}
	}
}
