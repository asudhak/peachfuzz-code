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
