
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
using System.Text.RegularExpressions;

namespace DtdFuzzer
{
	public class Element
	{
		public string name;
		public bool isAny = false;
		public bool isEmpty = false;

		/// <summary>
		/// Key == Attribute.name, value == Attribute
		/// </summary>
		public Dictionary<string, Attribute> attributes = new Dictionary<string, Attribute>();

		public ElementRelation relation = null;
	}

	/// <summary>
	/// Element relation type
	/// </summary>
	public enum ElementRelationType
	{
		// Use relations to hold each option
		Or,
		And,

		// Check both!
		One,
		OneOrMore,
		ZeroOrOne,
		ZeroOrMore,

		// Expect no elements in list
		PCDATA
	}

	/// <summary>
	/// Capture an element relation from the DTD.
	/// </summary>
	public class ElementRelation
	{
		public ElementRelationType type;
		public List<ElementRelation> relations = new List<ElementRelation>();
		public Element element = null;

		public ElementRelation(ElementRelationType type)
		{
			this.type = type;
		}
	}

	public enum AttributeType
	{
		CDATA,
		Enum,
		ID,
		IDREF,
		IDREFS,
		NMTOKEN,
		NMTOKENS,
		ENTITY,
		ENTITIES,
		NOTATION,
		XmlValue
	}

	public class Attribute
	{
		public string name;
		public AttributeType type;
		public string value;
		public bool required = false;
		public bool implied = false;
		public List<string> enumValues = new List<string>();
	}

	public class Entity
	{
		static Entity()
		{
			entities["lt"] = new Entity("lt", "<");
			entities["gt"] = new Entity("gt", ">");
			entities["quot"] = new Entity("quot", "\"");
			entities["amp"] = new Entity("amp", "&");
			entities["apos"] = new Entity("apos", "'");
		}

		/// <summary>
		/// Uses our list of defined entities to resolve any 
		/// entity references inside of a string.
		/// </summary>
		/// <param name="data">String that may contain entities</param>
		/// <returns>Returns a string with all entities resolved.</returns>
		public static string ResolveEntities(string data)
		{
			while(reEntity.IsMatch(data))
			{
				Match match = reEntity.Match(data);
				data = reEntity.Replace(data, entities[match.Groups[1].Value].value, 1);
			}

			return data;
		}

		public Entity()
		{
		}

		public Entity(string name, string value)
		{
			this.name = name;
			this.value = value;
		}

		static Regex reEntity = new Regex(@"%([^%;]+);");
		public static Dictionary<string, Entity> entities = new Dictionary<string, Entity>();

		public string name = "";
		string _value = "";
		public string value
		{
			get
			{
				string ret = _value;

				while (reEntity.IsMatch(ret))
				{
					Match match = reEntity.Match(ret);
					ret = reEntity.Replace(ret, entities[match.Groups[1].Value].value, 1);
				}

				return ret;
			}
			set
			{
				_value = value;
			}
		}
	}
}
