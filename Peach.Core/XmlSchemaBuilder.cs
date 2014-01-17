using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Peach.Core.Xsd
{
	#region Dom Elements

	/// <summary>
	/// Root element of a Peach XML DDL document.
	/// </summary>
	[XmlRoot(ElementName = "Peach", Namespace = Dom.TargetNamespace)]
	public class Dom
	{
		/// <summary>
		/// Namespace used by peach.
		/// </summary>
		public const string TargetNamespace = "http://peachfuzzer.com/2012/Peach";

		/// <summary>
		/// Version of this XML file.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(null)]
		public string version { get; set; }

		/// <summary>
		/// Author of this XML file.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(null)]
		public string author { get; set; }

		/// <summary>
		/// Description of this XML file.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(null)]
		public string description { get; set; }

		[XmlElement]
		[DefaultValue(null)]
		public List<Include> Include { get; set; }

		[XmlElement]
		[DefaultValue(null)]
		public List<Import> Import { get; set; }

		[XmlElement]
		[DefaultValue(null)]
		public List<Require> Require { get; set; }

		[XmlElement]
		[DefaultValue(null)]
		public List<PythonPath> PythonPath { get; set; }

		[XmlElement]
		[DefaultValue(null)]
		public List<RubyPath> RubyPath { get; set; }

		[XmlElement]
		[DefaultValue(null)]
		public List<Python> Python { get; set; }

		[XmlElement]
		[DefaultValue(null)]
		public List<Ruby> Ruby { get; set; }

		[XmlElement]
		[DefaultValue(null)]
		public Defaults Defaults { get; set; }

		[XmlElement("Data")]
		public List<Data> Datas { get; set; }

		[XmlElement("DataModel")]
		public List<DataModel> DataModels { get; set; }

		[XmlElement("Ocl")]
		public List<Ocl> Ocls { get; set; }

		[XmlElement("StateModel")]
		public NamedCollection<Peach.Core.Dom.StateModel> StateModels { get; set; }

		[XmlElement("Agent")]
		public NamedCollection<Peach.Core.Dom.Agent> Agents { get; set; }

		[XmlElement("Test")]
		public NamedCollection<Peach.Core.Dom.Test> Tests { get; set; }

		/*
		 * DataModel
		 * Analyzer (Top Level)
		 */
	}

	public class DataModel
	{
	}

	/// <summary>
	/// Provide OCL script inside of cdata.
	/// </summary>
	public class Ocl
	{
		/// <summary>
		/// Only use these OCL Contexts on Control or Record iterations.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(false)]
		public bool controlOnly { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

	/// <summary>
	/// Provide an OCL script or reference existing context.
	/// </summary>
	public class OclRef
	{
		/// <summary>
		/// Reference existing OCL Context.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(null)]
		public string context { get; set; }

	}

	/// <summary>
	/// Imports other Peach XML files into a namespace.
	/// This allows reusing existing templates from other Peach XML files.
	/// </summary>
	public class Include
	{
		/// <summary>
		/// The namespace prefix. One or more alphanumeric characters. Must not include a period.
		/// </summary>
		[XmlAttribute]
		public string ns { get; set; }

		/// <summary>
		/// URL of file to include. For files say "file:path/to/file".
		/// </summary>
		[XmlAttribute]
		public string src { get; set; }
	}

	/// <summary>
	/// Import a python file into the current context.
	/// This allows referencing generators and methods in external python files.
	/// Synonymous with saying "import xyz".
	/// </summary>
	public class Import
	{
		/// <summary>
		/// Just like the python "import xyz" syntax.
		/// </summary>
		[XmlAttribute]
		public string import { get; set; }
	}

	/// <summary>
	/// Import a ruby file into the current context.
	/// This allows referencing generators and methods in external ruby files.
	/// Synonymous with saying "require xyz".
	/// </summary>
	public class Require
	{
		/// <summary>
		/// Just like the ruby "require xyz" syntax.
		/// </summary>
		[XmlAttribute]
		public string require { get; set; }
	}

	/// <summary>
	/// Includes an additional path for python module resolution.
	/// </summary>
	public class PythonPath
	{
		/// <summary>
		/// Include this path when resolving python modules.
		/// </summary>
		[XmlAttribute]
		public string path { get; set; }
	}

	/// <summary>
	/// Includes an additional path for ruby module resolution.
	/// </summary>
	public class RubyPath
	{
		/// <summary>
		/// Include this path when resolving ruby modules.
		/// </summary>
		[XmlAttribute]
		public string path { get; set; }
	}

	/// <summary>
	/// This element allows for running Python code.
	/// This is useful to call any initialization methods for code that is later used.
	/// This is an advanced element.
	/// </summary>
	public class Python
	{
		/// <summary>
		/// Python code to run.
		/// </summary>
		[XmlAttribute]
		public string code { get; set; }
	}

	/// <summary>
	/// This element allows for running Ruby code.
	/// This is useful to call any initialization methods for code that is later used.
	/// This is an advanced element.
	/// </summary>
	public class Ruby
	{
		/// <summary>
		/// Ruby code to run.
		/// </summary>
		[XmlAttribute]
		public string code { get; set; }
	}

	/// <summary>
	/// Controls the default values of attributes for number elements.
	/// </summary>
	public class NumberDefaults
	{
		/// <summary>
		/// Specifies the byte order of the number.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(Peach.Core.Dom.EndianType.Little)]
		public Peach.Core.Dom.EndianType endian { get; set; }

		/// <summary>
		/// Specifies if the the number signed.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(true)]
		public bool signed { get; set; }

		/// <summary>
		/// Specifies the format of the value attribute.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(Peach.Core.Dom.ValueType.String)]
		public Peach.Core.Dom.ValueType valueType { get; set; }
	}

	/// <summary>
	/// Controls the default values of attributes for string elements.
	/// </summary>
	public class StringDefaults
	{
		/// <summary>
		/// Specifies the character encoding of the string.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(Peach.Core.Dom.StringType.ascii)]
		public Peach.Core.Dom.StringType type { get; set; }

		/// <summary>
		/// Specifies if the string is null terminated.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(false)]
		public bool nullTerminated { get; set; }

		/// <summary>
		/// Specify the character to bad the string with if it's length if less then
		/// specified in the length attribute. Only valid when the length attribute is also
		/// specified.  This field will accept python escape sequences
		/// such as \xNN, \r, \n, etc.
		/// </summary>
		[XmlAttribute]
		[DefaultValue("\\x00")]
		public char padCharacter { get; set; }

		/// <summary>
		/// Specifies the units of the length attribute.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(Peach.Core.Dom.LengthType.Bytes)]
		public Peach.Core.Dom.LengthType lengthType { get; set; }

		/// <summary>
		/// Specify the format of the value attribute.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(Peach.Core.Dom.ValueType.String)]
		public Peach.Core.Dom.ValueType valueType { get; set; }
	}

	/// <summary>
	/// Controls the default values of attributes for flags elements.
	/// </summary>
	public class FlagsDefaults
	{
		/// <summary>
		/// Specifies the byte order of the flag set.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(Peach.Core.Dom.EndianType.Little)]
		public Peach.Core.Dom.EndianType endian { get; set; }

		/// <summary>
		/// Specifies the length in bits of the flag set.
		/// </summary>
		[XmlAttribute]
		public uint size { get; set; }
	}

	/// <summary>
	/// Controls the default values of attributes for blob elements.
	/// </summary>
	public class BlobDefaults
	{
		/// <summary>
		/// Specifies the units of the length attribute.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(Peach.Core.Dom.LengthType.Bytes)]
		public Peach.Core.Dom.LengthType lengthType { get; set; }

		/// <summary>
		/// Specifies the format of the value attribute.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(Peach.Core.Dom.ValueType.String)]
		public Peach.Core.Dom.ValueType valueType { get; set; }
	}

	/// <summary>
	/// This element allow setting default values for data elements.
	/// </summary>
	public class Defaults
	{
		[XmlElement]
		[DefaultValue(null)]
		public NumberDefaults Number { get; set; }

		[XmlElement]
		[DefaultValue(null)]
		public StringDefaults String { get; set; }

		[XmlElement]
		[DefaultValue(null)]
		public FlagsDefaults Flags { get; set; }

		[XmlElement]
		[DefaultValue(null)]
		public BlobDefaults Blob { get; set; }
	}

	/// <summary>
	/// Param elements provide parameters for the parent element.
	/// </summary>
	public class PluginParam
	{
		/// <summary>
		/// Name of the parameter.
		/// </summary>
		[XmlAttribute]
		public string name { get; set; }

		/// <summary>
		/// Value of the parameter.
		/// </summary>
		[XmlAttribute]
		public string value { get; set; }
	}

	/// <summary>
	/// Specifies a set of default data values for a template.
	/// </summary>
	public class Data
	{
		/// <summary>
		/// Specifies a value for a field in a template.
		/// </summary>
		public class Field
		{
			/// <summary>
			/// Name of field to specify a default value for.
			/// Format of name is: "Element" or "Block.Block.Element".
			/// </summary>
			[XmlAttribute]
			public string name { get; set; }

			/// <summary>
			/// Default value for template field.
			/// </summary>
			[XmlAttribute]
			public string value { get; set; }

			/// <summary>
			/// Format of value attribute.
			/// </summary>
			[XmlAttribute]
			[DefaultValue(Peach.Core.Dom.ValueType.String)]
			public Peach.Core.Dom.ValueType valueType { get; set; }

		}

		/// <summary>
		/// Name of the data template.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(null)]
		public string name { get; set; }

		/// <summary>
		/// Use contents of file to populate data model.
		/// Peach will try and crack the file  based on the data model.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(null)]
		public string fileName { get; set; }

		[XmlElement("Field")]
		[DefaultValue(null)]
		public List<Field> Fields { get; set; }
	}

	/// <summary>
	/// Specifies a set of default data values for a template.
	/// </summary>
	public class DataRef : Data
	{
		/// <summary>
		/// Name of other data template to reference.
		/// </summary>
		[XmlAttribute("ref")]
		[DefaultValue(null)]
		public string refData { get; set; }
	}

	#endregion

	#region XmlDocFetcher

	internal static class XmlDocFetcher
	{
		static Dictionary<Assembly, XPathDocument> Cache = new Dictionary<Assembly, XPathDocument>();
		static Regex Whitespace = new Regex(@"\r\n\s+", RegexOptions.Compiled);

		public static string GetSummary(Type type)
		{
			return Select(type, "T", "", "");
		}

		public static string GetSummary(PropertyInfo pi)
		{
			return Select(pi.DeclaringType, "P", pi.Name, "");
		}

		public static string GetSummary(FieldInfo fi)
		{
			return Select(fi.DeclaringType, "F", fi.Name, "");
		}

		public static string GetSummary(MethodInfo mi)
		{
			var type = mi.DeclaringType;

			var parameters = mi.GetParameters().Select(p => p.ParameterType.FullName);
			var args = string.Join(",", parameters);
			if (!string.IsNullOrEmpty(args))
				args = string.Format("({0})", args);

			return Select(type, "M", mi.Name, args);
		}

		static string Select(Type type, string prefix, string suffix, string args)
		{
			var doc = GetXmlDoc(type.Assembly);
			if (doc == null)
				return null;

			var name = string.IsNullOrEmpty(suffix) ? type.FullName : string.Join(".", type.FullName, suffix);
			var query = string.Format("/doc/members/member[@name='{0}:{1}{2}']/summary", prefix, name, args);

			var navi = doc.CreateNavigator();
			var iter = navi.Select(query);

			if (!iter.MoveNext())
				return null; // No values

			var elem = iter.Current;

			if (iter.MoveNext())
				throw new NotSupportedException(); // Multiple matches!

			var ret = Whitespace.Replace(elem.Value, "\r\n");
			return ret.Trim();
		}

		static XPathDocument GetXmlDoc(Assembly asm)
		{
			XPathDocument doc;

			if (!Cache.TryGetValue(asm, out doc))
			{
				var file = Path.ChangeExtension(asm.CodeBase, ".xml");
				doc = new XPathDocument(file);
				Cache.Add(asm, doc);
			}

			return doc;
		}
	}

	#endregion

	#region Extension Methods

	internal static class Extensions
	{
		public static XmlNode[] ToNodeArray(this string text)
		{
			XmlDocument doc = new XmlDocument();
			return new XmlNode[1] { doc.CreateTextNode(text) };
		}

		public static void SetText(this XmlSchemaDocumentation doc, string text)
		{
			doc.Markup = text.ToNodeArray();
		}

		public static void Annotate(this XmlSchemaAnnotated item, PropertyInfo pi)
		{
			item.Annotate(XmlDocFetcher.GetSummary(pi));
		}

		public static void Annotate(this XmlSchemaAnnotated item, Type type)
		{
			item.Annotate(XmlDocFetcher.GetSummary(type));
		}

		public static void Annotate(this XmlSchemaAnnotated item, FieldInfo fi)
		{
			item.Annotate(XmlDocFetcher.GetSummary(fi));
		}

		public static void Annotate(this XmlSchemaAnnotated item, string text)
		{
			if (string.IsNullOrEmpty(text))
				return;

			var doc = new XmlSchemaDocumentation();
			doc.SetText(text);

			var anno = new XmlSchemaAnnotation();
			anno.Items.Add(doc);

			item.Annotation = anno;
		}

		/// <summary>
		/// Extension to the MemberInfo class. Return all attributes matching the specified type.
		/// </summary>
		/// <typeparam name="A">Attribute type to find.</typeparam>
		/// <param name="mi">MemberInfo in which the search should run over.</param>
		/// <returns>A generator which yields the attributes specified.</returns>
		public static IEnumerable<A> GetAttributes<A>(this MemberInfo mi)
			where A : Attribute
		{
			var attrs = System.Attribute.GetCustomAttributes(mi, typeof(A), false);
			return attrs.OfType<A>();
		}
	}

	#endregion

	public class SchemaBuilder
	{
		Dictionary<Type, XmlSchemaSimpleType> enumTypeCache;
		Dictionary<Type, XmlSchemaObject> objTypeCache;

		XmlSchema schema;

		public SchemaBuilder(Type type)
		{
			enumTypeCache = new Dictionary<Type, XmlSchemaSimpleType>();
			objTypeCache = new Dictionary<Type, XmlSchemaObject>();

			var root = type.GetAttributes<XmlRootAttribute>().First();

			schema = new XmlSchema();
			schema.TargetNamespace = root.Namespace;
			schema.ElementFormDefault = XmlSchemaForm.Qualified;

			AddElement(root.ElementName, type, null);
		}

		public XmlSchema Compile()
		{
			var schemaSet = new XmlSchemaSet();
			schemaSet.ValidationEventHandler += ValidationEventHandler;
			schemaSet.Add(schema);
			schemaSet.Compile();

			var compiled = schemaSet.Schemas().OfType<XmlSchema>().First();

			return compiled;
		}

		public static void Generate(Type type, Stream stream)
		{
			var settings = new XmlWriterSettings() { Indent = true, Encoding = System.Text.Encoding.UTF8 };
			var writer = XmlWriter.Create(stream, settings);
			var compiled = new SchemaBuilder(type).Compile();

			compiled.Write(writer);
		}

		T MakeItem<T>(string name, Type type) where T : XmlSchemaAnnotated, new()
		{
			if (type.IsGenericType)
				throw new ArgumentException();

			var item = new T();

			var mi = typeof(T).GetProperty("Name");
			mi.SetValue(item, name, null);

			item.Annotate(type);

			// Add even though we haven't filled everything out yet
			schema.Items.Add(item);

			// Cache this so we don't do this more than once
			objTypeCache.Add(type, item);

			return item;
		}

		void AddElement(string name, Type type, PluginElementAttribute pluginAttr)
		{
			var schemaElem = MakeItem<XmlSchemaElement>(name, type);

			var complexType = new XmlSchemaComplexType();

			Populate(complexType, type, pluginAttr);

			schemaElem.SchemaType = complexType;
		}

		void Populate(XmlSchemaComplexType complexType, Type type, PluginElementAttribute pluginAttr)
		{
			if (pluginAttr == null)
				PopulateComplexType(complexType, type);
			else if (pluginAttr.Combine)
				CombinePluginType(complexType, pluginAttr);
			else
				PopulatePluginType(complexType, pluginAttr);
		}

		void CombinePluginType(XmlSchemaComplexType complexType, PluginElementAttribute pluginAttr)
		{
			var addedAttrs = new Dictionary<string, XmlSchemaAttribute>();
			var addedElems = new Dictionary<string, XmlSchemaElement>();

			var schemaParticle = new XmlSchemaChoice();
			schemaParticle.MinOccursString = "0";
			schemaParticle.MaxOccursString = "unbounded";

			var restrictEnum = new XmlSchemaSimpleTypeRestriction();
			restrictEnum.BaseTypeName = new XmlQualifiedName("string", XmlSchema.Namespace);

			foreach (var item in ClassLoader.GetAllByAttribute<PluginAttribute>((t, a) => a.Type == pluginAttr.PluginType && a.IsDefault && !a.IsTest))
			{
				restrictEnum.Facets.Add(MakePluginFacet(item.Key, item.Value));

				foreach (var pi in item.Value.GetProperties())
				{
					var attrAttr = pi.GetAttributes<XmlAttributeAttribute>().FirstOrDefault();
					if (attrAttr != null)
					{
						var attr = MakeAttribute(attrAttr.AttributeName, pi);
						if (!addedAttrs.ContainsKey(attr.Name))
						{
							complexType.Attributes.Add(attr);
							addedAttrs.Add(attr.Name, attr);
						}
						continue;
					}

					var elemAttr = pi.GetAttributes<XmlElementAttribute>().FirstOrDefault();

					if (elemAttr != null)
					{
						var elems = MakeElement(elemAttr.ElementName, null, pi, null);
						foreach (var elem in elems)
						{
							var key = elem.Name ?? elem.RefName.Name;
							if (!addedElems.ContainsKey(key))
							{
								elem.MinOccursString = null;
								elem.MaxOccursString = null;
								schemaParticle.Items.Add(elem);
								addedElems.Add(key, elem);
							}
						}
					}
				}
			}

			var enumType = new XmlSchemaSimpleType();
			enumType.Content = restrictEnum;

			var typeAttr = new XmlSchemaAttribute();
			typeAttr.Name = pluginAttr.AttributeName;
			typeAttr.Use = XmlSchemaUse.Required;
			typeAttr.SchemaType = enumType;
			typeAttr.Annotate("Specify the {0} of a Peach {1}.".Fmt(
				pluginAttr.AttributeName,
				pluginAttr.PluginType.Name.ToLower()
				));

			complexType.Attributes.Add(typeAttr);

			if (schemaParticle.Items.Count > 0)
				complexType.Particle = schemaParticle;
		}

		void PopulatePluginType(XmlSchemaComplexType complexType, PluginElementAttribute pluginAttr)
		{
			if (typeof(Peach.Core.Dom.INamed).IsAssignableFrom(pluginAttr.PluginType))
			{
				var nameAttr = new XmlSchemaAttribute();
				nameAttr.Name = "name";
				nameAttr.Annotate("{0} name.".Fmt(pluginAttr.PluginType.Name));
				nameAttr.Use = XmlSchemaUse.Optional;

				complexType.Attributes.Add(nameAttr);
			}

			var typeAttr = new XmlSchemaAttribute();
			typeAttr.Name = pluginAttr.AttributeName;
			typeAttr.Use = XmlSchemaUse.Required;
			typeAttr.Annotate("Specify the class name of a Peach {0}. You can implement your own {1}s as needed.".Fmt(
				pluginAttr.PluginType.Name,
				pluginAttr.PluginType.Name.ToLower()
				));

			var restrictEnum = new XmlSchemaSimpleTypeRestriction();
			restrictEnum.BaseTypeName = new XmlQualifiedName("string", XmlSchema.Namespace);

			foreach (var item in ClassLoader.GetAllByAttribute<PluginAttribute>((t, a) => a.Type == pluginAttr.PluginType && a.IsDefault && !a.IsTest))
			{
				restrictEnum.Facets.Add(MakePluginFacet(item.Key, item.Value));
			}

			var enumType = new XmlSchemaSimpleType();
			enumType.Content = restrictEnum;

			var restrictLen = new XmlSchemaSimpleTypeRestriction();
			restrictLen.BaseTypeName = new XmlQualifiedName("string", XmlSchema.Namespace);
			restrictLen.Facets.Add(new XmlSchemaMaxLengthFacet() { Value = "1024" });

			var userType = new XmlSchemaSimpleType();
			userType.Content = restrictLen;

			var union = new XmlSchemaSimpleTypeUnion();
			union.BaseTypes.Add(userType);
			union.BaseTypes.Add(enumType);

			var schemaType = new XmlSchemaSimpleType();
			schemaType.Content = union;

			typeAttr.SchemaType = schemaType;

			complexType.Attributes.Add(typeAttr);

			if (!objTypeCache.ContainsKey(typeof(PluginParam)))
				AddElement("Param", typeof(PluginParam), null);

			var schemaElem = new XmlSchemaElement();
			schemaElem.MinOccursString = "0";
			schemaElem.MaxOccursString = "unbounded";
			schemaElem.RefName = new XmlQualifiedName("Param", schema.TargetNamespace);

			var schemaParticle = new XmlSchemaSequence();
			schemaParticle.Items.Add(schemaElem);

			complexType.Particle = schemaParticle;
		}

		private XmlSchemaObject MakePluginFacet(PluginAttribute pluginAttribute, Type type)
		{
			var facet = new XmlSchemaEnumerationFacet();
			facet.Value = pluginAttribute.Name;

			var descAttr = type.GetAttributes<DescriptionAttribute>().FirstOrDefault();
			if (descAttr != null)
				facet.Annotate(descAttr.Description);
			else
				facet.Annotate(type);

			return facet;
		}

		void PopulateComplexType(XmlSchemaComplexType complexType, Type type)
		{
			if (type.IsAbstract)
				complexType.IsAbstract = true;

			XmlSchemaGroupBase schemaParticle = new XmlSchemaSequence();

			foreach (var pi in type.GetProperties())
			{
				if (pi.DeclaringType != type)
					continue;

				var textAttr = pi.GetAttributes<XmlTextAttribute>().FirstOrDefault();
				if (textAttr != null)
				{
					complexType.IsMixed = true;
					continue;
				}

				var attrAttr = pi.GetAttributes<XmlAttributeAttribute>().FirstOrDefault();
				if (attrAttr != null)
				{
					var attr = MakeAttribute(attrAttr.AttributeName, pi);
					complexType.Attributes.Add(attr);
					continue;
				}

				var elemAttrs = pi.GetAttributes<XmlElementAttribute>();

				if (elemAttrs.Skip(1).Any())
				{
					// If there is more than 1 XmlElement attribute, make a sequence of choice of elements
					var elemChoice = new XmlSchemaChoice();

					foreach (var elemAttr in elemAttrs)
					{
						var elems = MakeElement(elemAttr.ElementName, elemAttr.Type, pi, null);

						foreach (var elem in elems)
						{
							elemChoice.MinOccursString = elem.MinOccursString;
							elemChoice.MaxOccursString = elem.MaxOccursString;
							elem.MinOccursString = "0";
							elem.MaxOccursString = "1";

							elemChoice.Items.Add(elem);
						}
					}

					schemaParticle.Items.Add(elemChoice);
				}
				else if (elemAttrs.Any())
				{
					var elemAttr = elemAttrs.First();
					var elems = MakeElement(elemAttr.ElementName, null, pi, null);

					foreach (var elem in elems)
						schemaParticle.Items.Add(elem);

					continue;
				}

				var pluginAttr = pi.GetAttributes<PluginElementAttribute>().FirstOrDefault();
				if (pluginAttr != null)
				{
					var elems = MakeElement(pluginAttr.ElementName, null, pi, pluginAttr);

					foreach (var elem in elems)
						schemaParticle.Items.Add(elem);

					continue;
				}
			}

			// See if there is a better way to do this.
			// If particle contains a single element, use xs:sequence
			// If all items are [0|1,1], use xs:all
			// Else, use xs:choice [0, unbounded]
			while (schemaParticle.Items.Count > 1)
			{
				var items = schemaParticle.Items.OfType<XmlSchemaParticle>();
				if (!items.Where(i => i.MaxOccursString != "1").Any())
				{
					// All maxOccurs are "1", check minOccurs...
					var disinct = items.Select(a => a.MinOccursString).Distinct();
					var value = disinct.FirstOrDefault();
					var moreThanOne = disinct.Skip(1).Any();

					if (!moreThanOne && (value == "0" || value == "1"))
					{
						// minOccurs is "0" or "1", convert to xs:all
						var allParticle = new XmlSchemaAll();
						allParticle.MinOccursString = value;
						allParticle.MaxOccursString = "1";

						foreach (var item in schemaParticle.Items)
							allParticle.Items.Add(item);

						schemaParticle = allParticle;

						break;
					}
				}

				var choiceParticle = new XmlSchemaChoice();
				choiceParticle.MinOccursString = "0";
				choiceParticle.MaxOccursString = "unbounded";

				foreach (var item in items)
				{
					item.MinOccursString = null;
					item.MaxOccursString = null;
					choiceParticle.Items.Add(item);
				}

				schemaParticle = choiceParticle;

				break;
			}

			if (type.BaseType != typeof(object) && type.BaseType != null)
			{
				var ext = new XmlSchemaComplexContentExtension();
				ext.BaseTypeName = new XmlQualifiedName(type.BaseType.Name, schema.TargetNamespace);

				if (schemaParticle.Items.Count > 0)
					ext.Particle = schemaParticle;

				var content = new XmlSchemaComplexContent();
				content.IsMixed = false;
				content.Content = ext;

				complexType.ContentModel = content;
			}
			else if (schemaParticle.Items.Count > 0)
			{
				complexType.Particle = schemaParticle;
			}
		}

		void AddComplexType(string name, Type type, PluginElementAttribute pluginAttr)
		{
			var complexType = MakeItem<XmlSchemaComplexType>(name, type);

			Populate(complexType, type, pluginAttr);

			if (type.BaseType != typeof(object) && type.BaseType != null)
			{
				System.Diagnostics.Debug.Assert(pluginAttr == null);
				if (!objTypeCache.ContainsKey(type.BaseType))
					AddComplexType(type.BaseType.Name, type.BaseType, null);
			}
		}

		void ValidationEventHandler(object sender, ValidationEventArgs e)
		{
			Console.WriteLine(e.Exception);
			Console.WriteLine(e.Message);
		}

		XmlQualifiedName GetSchemaType(Type type)
		{
			if (type == typeof(char))
				return new XmlQualifiedName("string", XmlSchema.Namespace);

			if (type == typeof(string))
				return new XmlQualifiedName("string", XmlSchema.Namespace);

			if (type == typeof(bool))
				return new XmlQualifiedName("boolean", XmlSchema.Namespace);

			if (type == typeof(uint))
				return new XmlQualifiedName("unsignedInt", XmlSchema.Namespace);

			if (type == typeof(int))
				return new XmlQualifiedName("unsignedInt", XmlSchema.Namespace);

			if (type == typeof(decimal))
				return new XmlQualifiedName("decimal", XmlSchema.Namespace);

			throw new NotImplementedException();
		}

		XmlSchemaAttribute MakeAttribute(string name, PropertyInfo pi)
		{
			if (string.IsNullOrEmpty(name))
				name = pi.Name;

			var defaultValue = pi.GetAttributes<DefaultValueAttribute>().FirstOrDefault();

			var attr = new XmlSchemaAttribute();
			attr.Name = name;
			attr.Annotate(pi);

			if (pi.PropertyType.IsEnum)
			{
				attr.SchemaType = GetEnumType(pi.PropertyType);
			}
			else
			{
				attr.SchemaTypeName = GetSchemaType(pi.PropertyType);
			}

			if (defaultValue != null)
			{
				attr.Use = XmlSchemaUse.Optional;

				if (defaultValue.Value != null)
				{
					var valStr = defaultValue.Value.ToString();
					var valType = defaultValue.Value.GetType();

					if (valType == typeof(bool))
					{
						valStr = XmlConvert.ToString((bool)defaultValue.Value);
					}
					else if (valType.IsEnum)
					{
						var enumType = valType.GetField(valStr);
						var enumAttr = enumType.GetAttributes<XmlEnumAttribute>().FirstOrDefault();
						if (enumAttr != null)
							valStr = enumAttr.Name;
					}

					attr.DefaultValue = valStr;
				}
				else if (pi.PropertyType.IsEnum)
				{
					var content = (XmlSchemaSimpleTypeRestriction)attr.SchemaType.Content;
					var facet = (XmlSchemaEnumerationFacet)content.Facets[0];
					attr.DefaultValue = facet.Value;
				}
			}
			else
			{
				attr.Use = XmlSchemaUse.Required;
			}

			return attr;
		}

		XmlSchemaSimpleType GetEnumType(Type type)
		{
			XmlSchemaSimpleType ret;
			if (enumTypeCache.TryGetValue(type, out ret))
				return ret;

			var content = new XmlSchemaSimpleTypeRestriction()
			{
				BaseTypeName = new XmlQualifiedName("string", XmlSchema.Namespace),
			};

			foreach (var item in type.GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				var attr = item.GetAttributes<XmlEnumAttribute>().FirstOrDefault();

				var facet = new XmlSchemaEnumerationFacet();
				facet.Value = attr != null ? attr.Name : item.Name;
				facet.Annotate(item);

				content.Facets.Add(facet);
			}

			ret = new XmlSchemaSimpleType()
			{
				Content = content,
			};

			enumTypeCache.Add(type, ret);

			return ret;
		}

		bool IsGenericCollection(Type type)
		{
			var ifaces = type.GetInterfaces();
			foreach (var iface in ifaces)
			{
				if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(ICollection<>))
					return true;
			}

			var baseType = type.BaseType;
			if (baseType == null)
				return false;

			return IsGenericCollection(baseType);
		}

		XmlSchemaElement[] MakeElement(string name, Type attrType, PropertyInfo pi, PluginElementAttribute pluginAttr)
		{
			if (string.IsNullOrEmpty(name))
				name = attrType == null ? pi.Name : attrType.Name;

			var type = pi.PropertyType;
			var defaultValue = pi.GetAttributes<DefaultValueAttribute>().FirstOrDefault();
			var isArray = type.IsArray;

			if (type.IsGenericType)
			{
				if (!IsGenericCollection(type))
					throw new NotSupportedException();

				var args = type.GetGenericArguments();
				if (args.Length != 1)
					throw new NotSupportedException();

				type = args[0];
				isArray = true;
			}

			var schemaElem = new XmlSchemaElement();
			schemaElem.MinOccursString = defaultValue != null ? "0" : "1";
			schemaElem.MaxOccursString = isArray ? "unbounded" : "1";

			if (type == typeof(DataModel))
				return MakeDataModel(schemaElem.MinOccursString, schemaElem.MaxOccursString);

			var destType = attrType ?? type;

			if (name != type.Name || type != destType)
			{
				schemaElem.Name = name;
				schemaElem.SchemaTypeName = new XmlQualifiedName(destType.Name, schema.TargetNamespace);

				if (!objTypeCache.ContainsKey(destType))
					AddComplexType(destType.Name, destType, pluginAttr);
			}
			else
			{
				schemaElem.RefName = new XmlQualifiedName(type.Name, schema.TargetNamespace);

				if (!objTypeCache.ContainsKey(type))
					AddElement(name, type, pluginAttr);
			}

			return new XmlSchemaElement[] { schemaElem };
		}

		private XmlSchemaElement[] MakeDataModel(string minOccurs, string maxOccurrs)
		{
			var ret = new List<XmlSchemaElement>();

			foreach (var item in ClassLoader.GetAllByAttribute<PitParsableAttribute>((t,a) => a.topLevel))
			{
				var name = item.Key.xmlElementName;
				var type = item.Value;

				var schemaElem = new XmlSchemaElement();
				schemaElem.MinOccursString = minOccurs;
				schemaElem.MaxOccursString = maxOccurrs;

				schemaElem.RefName = new XmlQualifiedName(name, schema.TargetNamespace);

				if (!objTypeCache.ContainsKey(type))
					AddDataModel(name, type);

				ret.Add(schemaElem);
			}

			return ret.ToArray();
		}

		void AddDataModel(string name, Type type)
		{
			var schemaElem = MakeItem<XmlSchemaElement>(name, type);

			var complexType = new XmlSchemaComplexType();

			schemaElem.SchemaType = complexType;
		}
	}
}
