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

		[XmlElement("Godel")]
		public List<Godel> Godels { get; set; }

		[XmlElement("StateModel")]
		public NamedCollection<Peach.Core.Dom.StateModel> StateModels { get; set; }

		[XmlElement("Agent")]
		public NamedCollection<Peach.Core.Dom.Agent> Agents { get; set; }

		[XmlElement("Test")]
		public NamedCollection<Peach.Core.Dom.Test> Tests { get; set; }
	}

	public class DataModel
	{
		[PluginElement("class", typeof(Peach.Core.Fixup))]
		public Peach.Core.Fixup Fixup { get; set; }

		[PluginElement("class", typeof(Peach.Core.Transformer))]
		public Peach.Core.Transformer Transformer { get; set; }

		[PluginElement("class", typeof(Peach.Core.Analyzer))]
		public Peach.Core.Analyzer Analyzer { get; set; }

		[PluginElement("type", typeof(Peach.Core.Dom.Relation), Combine = true)]
		public Peach.Core.Dom.Relation Relation { get; set; }

		[XmlElement("Hint")]
		[DefaultValue(null)]
		public List<Peach.Core.Dom.Hint> Hint { get; set; }
	}

	public class Godel
	{
		[XmlAnyAttribute]
		public XmlAttribute[] Attributes { get; set; }
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
		/// Name of other data template to reference.
		/// </summary>
		[XmlAttribute("ref")]
		[DefaultValue(null)]
		public string refData { get; set; }

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
		Dictionary<Type, XmlSchemaComplexType> objTypeCache;
		Dictionary<Type, XmlSchemaElement> objElemCache;

		XmlSchema schema;

		public SchemaBuilder(Type type)
		{
			enumTypeCache = new Dictionary<Type, XmlSchemaSimpleType>();
			objTypeCache = new Dictionary<Type, XmlSchemaComplexType>();
			objElemCache = new Dictionary<Type, XmlSchemaElement>();

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

		T MakeItem<T>(string name, Type type, Dictionary<Type, T> cache) where T : XmlSchemaAnnotated, new()
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
			cache.Add(type, item);

			return item;
		}

		void AddElement(string name, Type type, PluginElementAttribute pluginAttr)
		{
			var schemaElem = MakeItem<XmlSchemaElement>(name, type, objElemCache);

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

					var anyAttr = pi.GetAttributes<XmlAnyAttributeAttribute>().FirstOrDefault();
					if (anyAttr != null)
					{
						complexType.AnyAttribute = new XmlSchemaAnyAttribute() { ProcessContents = XmlSchemaContentProcessing.Skip };
						continue;
					}
				}

				foreach (var prop in item.Value.GetAttributes<ParameterAttribute>())
				{
					var attr = MakeAttribute(prop.name, prop);
					if (!addedAttrs.ContainsKey(attr.Name))
					{
						complexType.Attributes.Add(attr);
						addedAttrs.Add(attr.Name, attr);
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
			if (pluginAttr.Named)
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

			if (!objElemCache.ContainsKey(typeof(PluginParam)))
				AddElement("Param", typeof(PluginParam), null);


			var schemaElem = new XmlSchemaElement();
			schemaElem.RefName = new XmlQualifiedName("Param", schema.TargetNamespace);

			XmlSchemaGroupBase schemaParticle;

			if (pluginAttr.PluginType == typeof(Transformer))
			{
				schemaParticle = new XmlSchemaChoice();
				schemaParticle.MinOccursString = "0";
				schemaParticle.MaxOccursString = "unbounded";

				var transElem = new XmlSchemaElement();
				transElem.RefName = new XmlQualifiedName("Transformer", schema.TargetNamespace);
				schemaParticle.Items.Add(transElem);
			}
			else
			{
				schemaParticle = new XmlSchemaSequence();
				schemaElem.MinOccursString = "0";
				schemaElem.MaxOccursString = "unbounded";
			}

			schemaParticle.Items.Add(schemaElem);

			complexType.Particle = schemaParticle;
		}

		private XmlSchemaObject MakePluginFacet(PluginAttribute pluginAttribute, Type type)
		{
			var facet = new XmlSchemaEnumerationFacet();
			facet.Value = pluginAttribute.Name;

			// Lame: For actions, everyone expects title case (SetProperty) but
			// the schema expects camel case (setProperty).
			if (pluginAttribute.Type == typeof(Peach.Core.Dom.Action))
				facet.Value = Char.ToLowerInvariant(facet.Value[0]) + facet.Value.Substring(1);

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

				var anyAttr = pi.GetAttributes<XmlAnyAttributeAttribute>().FirstOrDefault();
				if (anyAttr != null)
				{
					complexType.AnyAttribute = new XmlSchemaAnyAttribute() { ProcessContents = XmlSchemaContentProcessing.Skip };
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

				foreach (var attr in complexType.Attributes)
					ext.Attributes.Add(attr);

				complexType.Attributes.Clear();

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
			var complexType = MakeItem<XmlSchemaComplexType>(name, type, objTypeCache);

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
			if (IsGenericType(type, typeof(Nullable<>)))
				type = type.GetGenericArguments()[0];

			if (type == typeof(char))
				return new XmlQualifiedName("string", XmlSchema.Namespace);

			if (type == typeof(string))
				return new XmlQualifiedName("string", XmlSchema.Namespace);

			if (type == typeof(bool))
				return new XmlQualifiedName("boolean", XmlSchema.Namespace);

			if (type == typeof(uint))
				return new XmlQualifiedName("unsignedInt", XmlSchema.Namespace);

			if (type == typeof(int))
				return new XmlQualifiedName("int", XmlSchema.Namespace);

			if (type == typeof(decimal))
				return new XmlQualifiedName("decimal", XmlSchema.Namespace);

			if (type == typeof(Peach.Core.Dom.DataElement))
				return new XmlQualifiedName("string", XmlSchema.Namespace);

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

		XmlSchemaAttribute MakeAttribute(string name, ParameterAttribute paramAttr)
		{
			name = paramAttr.name;

			var attr = new XmlSchemaAttribute();
			attr.Name = name;
			attr.Annotate(paramAttr.description);

			if (paramAttr.type.IsEnum)
			{
				attr.SchemaType = GetEnumType(paramAttr.type);
			}
			else
			{
				attr.SchemaTypeName = GetSchemaType(paramAttr.type);
			}

			if (!paramAttr.required)
			{
				attr.Use = XmlSchemaUse.Optional;

				if (!string.IsNullOrEmpty(paramAttr.defaultValue))
				{
					var valStr = paramAttr.defaultValue;
					var valType = paramAttr.type;

					if (valType == typeof(bool))
					{
						valStr = XmlConvert.ToString(bool.Parse(valStr));
					}

					attr.DefaultValue = valStr;
				}
				else if (paramAttr.type.IsEnum)
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

		bool IsGenericType(Type sourceType, Type targetType)
		{
			if (sourceType.IsGenericType && sourceType.GetGenericTypeDefinition() == targetType)
				return true;

			var ifaces = sourceType.GetInterfaces();
			foreach (var iface in ifaces)
			{
				if (iface.IsGenericType && iface.GetGenericTypeDefinition() == targetType)
					return true;
			}

			var baseType = sourceType.BaseType;
			if (baseType == null)
				return false;

			return IsGenericType(baseType, targetType);
		}

		bool IsGenericCollection(Type type)
		{
			return IsGenericType(type, typeof(ICollection<>));
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

				if (!objElemCache.ContainsKey(type))
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

				var elem = MakeDataElement(name, type);
				elem.MinOccursString = minOccurs;
				elem.MaxOccursString = maxOccurrs;

				ret.Add(elem);
			}

			return ret.ToArray();
		}

		XmlSchemaElement MakeDataElement(string name, Type type)
		{
			var schemaElem = new XmlSchemaElement();

			schemaElem.RefName = new XmlQualifiedName(name, schema.TargetNamespace);

			if (!objElemCache.ContainsKey(type))
				AddDataElement(name, type);

			return schemaElem;
		}

		void AddDataElement(string name, Type type)
		{
			var schemaElem = MakeItem<XmlSchemaElement>(name, type, objElemCache);

			var complexType = new XmlSchemaComplexType();

			var schemaParticle = new XmlSchemaChoice();
			schemaParticle.MinOccursString = "0";
			schemaParticle.MaxOccursString = "unbounded";

			foreach (var prop in type.GetAttributes<ParameterAttribute>())
			{
				var attr = MakeAttribute(prop.name, prop);
				complexType.Attributes.Add(attr);
			}

			var deAttr = type.GetAttributes<Peach.Core.Dom.DataElementAttribute>().First();

			if (deAttr.elementTypes.HasFlag(Peach.Core.Dom.DataElementTypes.DataElements))
			{
				foreach (var kv in ClassLoader.GetAllByAttribute<Peach.Core.Dom.DataElementAttribute>(null))
				{
					var parents = kv.Value.GetAttributes<Peach.Core.Dom.DataElementParentSupportedAttribute>();
					if (parents.Any() && !parents.Any(a => a.elementName == name))
						continue;

					var elem = MakeDataElement(kv.Key.elementName, kv.Value);
					schemaParticle.Items.Add(elem);
				}
			}

			foreach (var child in type.GetAttributes<Peach.Core.Dom.DataElementChildSupportedAttribute>())
			{
				var childType = ClassLoader.FindTypeByAttribute<Peach.Core.Dom.DataElementAttribute>((t, a) => a.elementName == child.elementName);
				var elem = MakeDataElement(child.elementName, childType);
				schemaParticle.Items.Add(elem);
			}

			if (deAttr.elementTypes.HasFlag(Peach.Core.Dom.DataElementTypes.Fixup))
			{
				PopulateDataElement(schemaParticle, "Fixup");
			}

			if (deAttr.elementTypes.HasFlag(Peach.Core.Dom.DataElementTypes.Hint))
			{
				var elems = MakeElement("Hint", null, typeof(DataModel).GetProperty("Hint"), null);

				foreach (var elem in elems)
				{
					elem.MinOccursString = null;
					elem.MaxOccursString = null;
					schemaParticle.Items.Add(elem);
				}
			}

			if (deAttr.elementTypes.HasFlag(Peach.Core.Dom.DataElementTypes.Transformer))
			{
				PopulateDataElement(schemaParticle, "Transformer");
			}

			if (deAttr.elementTypes.HasFlag(Peach.Core.Dom.DataElementTypes.Relation))
			{
				PopulateDataElement(schemaParticle, "Relation");
			}

			if (deAttr.elementTypes.HasFlag(Peach.Core.Dom.DataElementTypes.Analyzer))
			{
				PopulateDataElement(schemaParticle, "Analyzer");
			}

			if (schemaParticle.Items.Count > 0)
				complexType.Particle = schemaParticle;

			schemaElem.SchemaType = complexType;
		}

		private void PopulateDataElement(XmlSchemaChoice schemaParticle, string child)
		{
			var pi = typeof(DataModel).GetProperty(child);
			var pluginAttr = pi.GetAttributes<PluginElementAttribute>().FirstOrDefault();
			var elems = MakeElement(null, null, pi, pluginAttr);
			foreach (var elem in elems)
			{
				elem.MinOccursString = null;
				elem.MaxOccursString = null;
				schemaParticle.Items.Add(elem);
			}
		}
	}
}
