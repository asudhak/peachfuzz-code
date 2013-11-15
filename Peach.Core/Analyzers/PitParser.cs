
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
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Linq;

using NLog;

using Peach.Core.Dom;
using Peach.Core.IO;
using System.Globalization;

namespace Peach.Core.Analyzers
{
	/// <summary>
	/// This is the default analyzer for Peach.  It will
	/// parse a Peach PIT file (XML document) into a Peach DOM.
	/// </summary>
	public class PitParser : Analyzer
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// args key for passing a dictionary of defined values to replace.
		/// </summary>
		public static string DEFINED_VALUES = "DefinedValues";

		static readonly string PEACH_NAMESPACE_URI = "http://peachfuzzer.com/2012/Peach";

		Dom.Dom _dom = null;
		bool isScriptingLanguageSet = false;

		/// <summary>
		/// Contains default attributes for DataElements
		/// </summary>
		Dictionary<Type, Dictionary<string, string>> dataElementDefaults = new Dictionary<Type, Dictionary<string, string>>();

		/// <summary>
		/// Mapping of XML ELement names to type as provided by PitParsableAttribute
		/// </summary>
		static Dictionary<string, Type> dataElementPitParsable = new Dictionary<string, Type>();
		static Dictionary<string, Type> dataModelPitParsable = new Dictionary<string, Type>();
		static readonly string[] dataElementCommon = { "Relation", "Fixup", "Transformer", "Hint", "Analyzer", "Placement" };

		static PitParser()
		{
			PitParser.supportParser = true;
			Analyzer.defaultParser = new PitParser();
			populatePitParsable();
		}

		public PitParser()
		{

		}

		public static Dictionary<string, string> parseDefines(string definedValuesFile)
		{
			var ret = new Dictionary<string, string>();
			var keys = new HashSet<string>();

			string normalized = Path.GetFullPath(definedValuesFile);

			if (!File.Exists(normalized))
				throw new PeachException("Error, defined values file \"" + definedValuesFile + "\" does not exist.");

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(normalized);

			var root = xmlDoc.FirstChild;
			if (root.Name != "PitDefines")
			{
				root = xmlDoc.FirstChild.NextSibling;
				if (root.Name != "PitDefines")
					throw new PeachException("Error, definition file root element must be PitDefines.");
			}

			foreach (XmlNode node in root.ChildNodes)
			{
				if (node is XmlComment)
					continue;

				if (node.hasAttr("platform"))
				{
					switch (node.getAttrString("platform").ToLower())
					{
						case "osx":
							if (Platform.GetOS() != Platform.OS.OSX)
								continue;
							break;
						case "linux":
							if (Platform.GetOS() != Platform.OS.Linux)
								continue;
							break;
						case "windows":
							if (Platform.GetOS() != Platform.OS.Windows)
								continue;
							break;
						default:
							throw new PeachException("Error, unknown platform name \"" + node.getAttrString("platform") + "\" in definition file.");
					}
				}
				else if (!node.hasAttr("include"))
				{
					switch (node.Name.ToLower())
					{
						case "osx":
							if (Platform.GetOS() != Platform.OS.OSX)
								continue;
							break;
						case "linux":
							if (Platform.GetOS() != Platform.OS.Linux)
								continue;
							break;
						case "windows":
							if (Platform.GetOS() != Platform.OS.Windows)
								continue;
							break;
						case "all":
							break;
						default:
							throw new PeachException("Error, unknown node name \"" + node.Name + "\" in definition file. Expecting All, Linux, OSX, or Windows.");
					}
				}

				string include = node.getAttr("include", null);
				if (include != null)
				{
					var other = parseDefines(include);
					foreach (var kv in other)
						ret[kv.Key] = kv.Value;
				}

				foreach (XmlNode defNode in node.ChildNodes)
				{
					if (defNode is XmlComment)
						continue;

					string key = defNode.getAttr("key", null);
					string value = defNode.getAttr("value", null);

					if (key == null || value == null)
						throw new PeachException("Error, Define elements in definition file must have both key and value attributes.");

					if (!keys.Add(key))
						throw new PeachException("Error, defines file '" + definedValuesFile + "' contains multiple entries for key '" + key + "'.");

					ret[key] = value;
				}
			}

			return ret;
		}

		public override Dom.Dom asParser(Dictionary<string, object> args, Stream data)
		{
			return asParser(args, data, true);
		}

		class Resetter : DataElement
		{
			public static void Reset()
			{
				DataElement._uniqueName = 0;
			}
		}

		public virtual Dom.Dom asParser(Dictionary<string, object> args, Stream data, bool doValidatePit)
		{
			// Reset the data element auto-name suffix back to zero
			Resetter.Reset();

			string xml = readWithDefines(args, data);

			if (doValidatePit)
				validatePit(xml);

			XmlDocument xmldoc = new XmlDocument();
			xmldoc.LoadXml(xml);

			_dom = new Dom.Dom();

			foreach (XmlNode child in xmldoc.ChildNodes)
			{
				if (child.Name == "Peach")
				{
					handlePeach(_dom, child, args);
					break;
				}
			}

			_dom.evaulateDataModelAnalyzers();

			return _dom;
		}

		private static string readWithDefines(Dictionary<string, object> args, Stream data)
		{
			data.Position = 0;
			string xml = new StreamReader(data).ReadToEnd();

			object obj;
			if (args != null && args.TryGetValue(DEFINED_VALUES, out obj))
			{
				var definedValues = obj as Dictionary<string, string>;
				var sb = new StringBuilder(xml);

				foreach (string key in definedValues.Keys)
				{
					sb.Replace("##" + key + "##", definedValues[key]);
				}

				xml = sb.ToString();
			}

			return xml;
		}

		public override void asParserValidation(Dictionary<string, object> args, Stream data)
		{
			string xml = readWithDefines(args, data);
			validatePit(xml);
		}

		static protected void populatePitParsable()
		{
			foreach (var kv in ClassLoader.GetAllByAttribute<PitParsableAttribute>(null))
			{
				if (kv.Key.topLevel)
					dataModelPitParsable[kv.Key.xmlElementName] = kv.Value;
				else
					dataElementPitParsable[kv.Key.xmlElementName] = kv.Value;
			}
		}

		/// <summary>
		/// Validate PIT XML using Schema file.
		/// </summary>
		/// <param name="xmlData">Pit file to validate</param>
		private void validatePit(string xmlData)
		{
			XmlSchemaSet set = new XmlSchemaSet();
			var xsd = Assembly.GetExecutingAssembly().GetManifestResourceStream("Peach.Core.peach.xsd");
			using (var tr = XmlReader.Create(xsd))
			{
				set.Add(null, tr);
			}

			var doc = new XmlDocument();
			doc.Schemas = set;
			// Mono has issues reading utf-32 BOM when just calling doc.Load(data)

			try
			{
				doc.LoadXml(xmlData);
			}
			catch (XmlException ex)
			{
				throw new PeachException("Error: XML Failed to load: " + ex.Message, ex);
			}

			// Right now XSD validation is disabled on Mono :(
			// Still load the doc to verify well formed xml
			Type t = Type.GetType("Mono.Runtime");
			if (t != null)
				return;

			foreach (XmlNode root in doc.ChildNodes)
			{
				if (root.Name == "Peach")
				{
					if (string.IsNullOrEmpty(root.NamespaceURI))
					{
						var element = root as System.Xml.XmlElement;
						element.SetAttribute("xmlns", PEACH_NAMESPACE_URI);
					}

					var ms = new MemoryStream();
					doc.Save(ms);
					ms.Position = 0;
					doc.Load(ms);
					break;
				}
			}

			string errors = "";
			doc.Validate(delegate(object sender, ValidationEventArgs e)
			{
				errors += e.Message + "\r\n";
			});

			if (!string.IsNullOrEmpty(errors))
				throw new PeachException("Error, Pit file failed to validate: " + errors);
		}

		/// <summary>
		/// Handle parsing the top level Peach node.
		/// </summary>
		/// <remarks>
		/// NOTE: This method is intended to be overriden (hence the virtual) and is 
		///			currently in use by Godel to extend the Pit Parser.
		/// </remarks>
		/// <param name="dom">Dom object</param>
		/// <param name="node">XmlNode to parse</param>
		/// <param name="args">Parser arguments</param>
		/// <returns>Returns the parsed Dom object.</returns>
		protected virtual void handlePeach(Dom.Dom dom, XmlNode node, Dictionary<string, object> args)
		{
			// Pass 0 - Basic check if Peach 2.3 ns  
			if (node.NamespaceURI.Contains("2008"))
				throw new PeachException("Error, Peach 2.3 namespace detected please upgrade the pit");

			// Pass 1 - Handle imports, includes, python path
			foreach (XmlNode child in node)
			{
				switch (child.Name)
				{
					case "Include":
						string ns = child.getAttrString("ns");
						string fileName = child.getAttrString("src");
						fileName = fileName.Replace("file:", "");
						string normalized = Path.GetFullPath(fileName);

						if (!File.Exists(normalized))
						{
							string newFileName = Path.Combine(
								Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
								fileName);

							normalized = Path.GetFullPath(newFileName);

							if (!File.Exists(normalized))
								throw new PeachException("Error, Unable to locate Pit file [" + fileName + "].\n");

							fileName = newFileName;
						}

						var newParser = new PitParser();
						Dom.Dom newDom = newParser.asParser(args, fileName);
						newDom.name = ns;
						dom.ns[ns] = newDom;
						break;

					case "Require":
						Scripting.Imports.Add(child.getAttrString("require"));
						break;

					case "Import":
						if (child.hasAttr("from"))
							throw new PeachException("Error, This version of Peach does not support the 'from' attribute for 'Import' elements.");

						Scripting.Imports.Add(child.getAttrString("import"));
						break;

					case "PythonPath":
						if (isScriptingLanguageSet &&
							Scripting.DefaultScriptingEngine != ScriptingEngines.Python)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Python;
						Scripting.Paths.Add(child.getAttrString("path"));
						isScriptingLanguageSet = true;
						break;

					case "RubyPath":
						if (isScriptingLanguageSet &&
							Scripting.DefaultScriptingEngine != ScriptingEngines.Ruby)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Ruby;
						Scripting.Paths.Add(child.getAttrString("require"));
						isScriptingLanguageSet = true;
						break;

					case "Python":
						if (isScriptingLanguageSet &&
							Scripting.DefaultScriptingEngine != ScriptingEngines.Python)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Python;
						Scripting.Exec(child.getAttrString("code"), new Dictionary<string, object>());
						isScriptingLanguageSet = true;
						break;

					case "Ruby":
						if (isScriptingLanguageSet &&
							Scripting.DefaultScriptingEngine != ScriptingEngines.Ruby)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Ruby;
						Scripting.Exec(child.getAttrString("code"), new Dictionary<string, object>());
						isScriptingLanguageSet = true;
						break;

					case "Defaults":
						handleDefaults(child);
						break;
				}
			}

			// Pass 3 - Handle data model

			foreach (XmlNode child in node)
			{
				var dm = handleDataModel(child, null);

				if (dm != null)
				{
					try
					{
						dom.dataModels.Add(dm.name, dm);
					}
					catch (ArgumentException)
					{
						var entry = dataModelPitParsable.Where(kv => kv.Value == dm.GetType()).Select(kv => kv.Key).FirstOrDefault();
						var name = entry != null ? "<" + entry + ">" : "Data Model";
						throw new PeachException("Error, a " + name + " element named '" + dm.name + "' already exists.");
					}

					finalUpdateRelations(new DataModel[] {dm});
				}
			}

			// Pass 4 - Handle Data

			foreach (XmlNode child in node)
			{
				if (child.Name == "Data")
				{
					var data = handleData(child, dom.datas.UniqueName());

					try
					{
						dom.datas.Add(data);
					}
					catch (ArgumentException)
					{
						throw new PeachException("Error, a <Data> element named '" + data.name + "' already exists.");
					}
				}
			}

			// Pass 5 - Handle State model

			foreach (XmlNode child in node)
			{
				if (child.Name == "StateModel")
				{
					StateModel sm = handleStateModel(child, dom);

					try
					{
						dom.stateModels.Add(sm.name, sm);
					}
					catch (ArgumentException)
					{
						throw new PeachException("Error, a <StateModel> element named '" + sm.name + "' already exists.");
					}
				}

				if (child.Name == "Agent")
				{
					Dom.Agent agent = handleAgent(child);

					try
					{
						dom.agents.Add(agent);
					}
					catch (ArgumentException)
					{
						throw new PeachException("Error, a <Agent> element named '" + agent.name + "' already exists.");
					}
				}
			}

			// Pass 6 - Handle Test

			foreach (XmlNode child in node)
			{
				if (child.Name == "Test")
				{
					Test test = handleTest(child, dom);

					try
					{
						dom.tests.Add(test.name, test);
					}
					catch (ArgumentException)
					{
						throw new PeachException("Error, a <Test> element named '" + test.name + "' already exists.");
					}
				}
			}
		}

		public static void displayDataModel(DataElement elem, int indent = 0)
		{
			string sIndent = "";
			for (int i = 0; i < indent; i++)
				sIndent += "  ";

			Console.WriteLine(sIndent + string.Format("{0}: {1}", elem.GetHashCode(), elem.name));

			var cont = elem as DataElementContainer;

			if (cont == null)
				return;

			foreach (var child in cont)
			{
				displayDataModel(child, indent+1);
			}
		}

		#region Utility Methods

		/// <summary>
		/// Resolve a 'ref' attribute.  Will throw a PeachException if
		/// namespace is given, but not found.
		/// </summary>
		/// <param name="name">Ref name to resolve.</param>
		/// <param name="container">Container to start searching from.</param>
		/// <returns>DataElement for ref or null if not found.</returns>
		public DataElement getReference(string name, DataElementContainer container)
		{
			return getReference(_dom, name, container);
		}

		protected DataElement getReference(Dom.Dom dom, string name, DataElementContainer container)
		{
			if (name.IndexOf(':') > -1)
			{
				string ns = name.Substring(0, name.IndexOf(':'));

				Dom.Dom other;
				if (!dom.ns.TryGetValue(ns, out other))
					throw new PeachException("Unable to locate namespace '" + ns + "' in ref '" + name + "'.");

				name = name.Substring(name.IndexOf(':') + 1);

				return getReference(other, name, container);
			}

			if (container != null)
			{
				DataElement elem = container.find(name);
				if (elem != null)
					return elem;
			}

			foreach (DataModel model in dom.dataModels.Values)
			{
				if (model.name == name)
					return model;
			}

			foreach (DataModel model in dom.dataModels.Values)
			{
				DataElement elem = model.find(name);
				if (elem != null)
					return elem;
			}

			return null;
		}



		/// <summary>
		/// Find a referenced Dom element by name, taking into account namespace prefixes.
		/// </summary>
		/// <typeparam name="T">Type of Dom element.</typeparam>
		/// <param name="dom">Dom to search in</param>
		/// <param name="refName">Name of reference</param>
		/// <param name="predicate">Selector predicate that returns the element collection</param>
		/// <returns></returns>
		protected T getRef<T>(Dom.Dom dom, string refName, Func<Dom.Dom, ITryGetValue<string, T>> predicate)
		{
			return dom.getRef<T>(refName, predicate);
		}

		#endregion

		/// <summary>
		/// Locate all relations and pair from/of.
		/// </summary>
		/// <remarks>
		/// After we have completed creating our base dom tree we will perform
		/// a second pass on all data models to locate every relation and resolve
		/// both from/of and verify both sides are connected correctly.
		/// 
		/// After this, all relations will be bound to both from and of elements.
		/// </remarks>
		/// <param name="models"></param>
		protected void finalUpdateRelations(ICollection<DataModel> models)
		{
			logger.Trace("finalUpdateRelations");

			foreach (DataModel model in models)
			{
				//logger.Debug("finalUpdateRelations: DataModel: " + model.name);

				foreach (DataElement elem in model.EnumerateAllElements())
				{
					//logger.Debug("finalUpdateRelations: " + elem.fullName);

					foreach (var rel in elem.relations.From<Binding>())
					{
						//logger.Debug("finalUpdateRelations: Relation " + rel.GetType().Name);

						rel.Resolve();
					}
				}
			}
		}

		protected virtual void handleDefaults(XmlNode node)
		{
			foreach (XmlNode child in node.ChildNodes)
			{
				Dictionary<string, string> args = new Dictionary<string, string>();

				switch (child.Name)
				{
					case "Number":
						if (child.hasAttr("endian"))
							args["endian"] = child.getAttrString("endian");
						if (child.hasAttr("signed"))
							args["signed"] = child.getAttrString("signed");
						if (child.hasAttr("valueType"))
							args["valueType"] = child.getAttrString("valueType");

						dataElementDefaults[typeof(Number)] = args;
						break;
					case "String":
						if (child.hasAttr("lengthType"))
							args["lengthType"] = child.getAttrString("lengthType");
						if (child.hasAttr("padCharacter"))
							args["padCharacter"] = child.getAttrString("padCharacter");
						if (child.hasAttr("type"))
							args["type"] = child.getAttrString("type");
						if (child.hasAttr("nullTerminated"))
							args["nullTerminated"] = child.getAttrString("nullTerminated");
						if (child.hasAttr("valueType"))
							args["valueType"] = child.getAttrString("valueType");

						dataElementDefaults[typeof(Dom.String)] = args;
						break;
					case "Flags":
						if (child.hasAttr("endian"))
							args["endian"] = child.getAttrString("endian");
						if (child.hasAttr("size"))
							args["size"] = child.getAttrString("size");

						dataElementDefaults[typeof(Flags)] = args;
						break;
					case "Blob":
						if (child.hasAttr("lengthType"))
							args["lengthType"] = child.getAttrString("lengthType");
						if (child.hasAttr("valueType"))
							args["valueType"] = child.getAttrString("valueType");

						dataElementDefaults[typeof(Blob)] = args;
						break;
					default:
						throw new PeachException("Error, defaults not supported for '" + child.Name + "'.");
				}
			}
		}

		protected virtual Dom.Agent handleAgent(XmlNode node)
		{
			Dom.Agent agent = new Dom.Agent();

			agent.name = node.getAttrString("name");
			agent.url = node.getAttr("location", null);
			agent.password = node.getAttr("password", null);

			if (agent.url == null)
				agent.url = "local://";

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Monitor")
				{
					Dom.Monitor monitor = new Monitor();

					monitor.cls = child.getAttrString("class");
					monitor.name = child.getAttr("name", agent.monitors.UniqueName());
					monitor.parameters = handleParamsOrdered(child);

					try
					{
						agent.monitors.Add(monitor);
					}
					catch (ArgumentException)
					{
						throw new PeachException("Error, a <Monitor> element named '{0}' already exists in agent '{1}'.".Fmt(monitor.name, agent.name));
					}
				}
			}

			return agent;
		}

		#region Data Model

		protected DataModel handleDataModel(XmlNode node, DataModel old)
		{
			Type type;
			if (!dataModelPitParsable.TryGetValue(node.Name, out type))
				return old;

			if (old != null)
				throw new PeachException("Error, more than one {0} not allowed. XML:\n{1}".Fmt(
					string.Join(",", dataModelPitParsable.Keys), node.OuterXml));

			MethodInfo pitParsableMethod = type.GetMethod("PitParser");
			if (pitParsableMethod == null)
				throw new PeachException("Error, type with PitParsableAttribute is missing static PitParser(...) method: " + type.FullName);

			PitParserDelegate delegateAction = Delegate.CreateDelegate(typeof(PitParserDelegate), pitParsableMethod) as PitParserDelegate;

			var elem = delegateAction(this, node, null);
			if (elem == null)
				throw new PeachException("Error, type failed to parse provided XML: " + type.FullName);

			var dataModel = elem as DataModel;
			if (dataModel == null)
				throw new PeachException("Error, type failed to return top level element: " + type.FullName);

			return dataModel;
		}

		protected bool IsArray(XmlNode node)
		{
			if (node.hasAttr("minOccurs") || node.hasAttr("maxOccurs") || node.hasAttr("occurs"))
				return true;

			return false;
		}

		/// <summary>
		/// Handle common attributes such as the following:
		/// 
		///  * mutable
		///  * contraint
		///  * pointer
		///  * pointerDepth
		///  * token
		///  
		/// </summary>
		/// <param name="node">XmlNode to read attributes from</param>
		/// <param name="element">Element to set attributes on</param>
		public void handleCommonDataElementAttributes(XmlNode node, DataElement element)
		{
			if (node.hasAttr("token"))
				element.isToken = node.getAttrBool("token");

			if (node.hasAttr("mutable"))
				element.isMutable = node.getAttrBool("mutable");

			if (node.hasAttr("constraint"))
				element.constraint = node.getAttrString("constraint");

			if (node.hasAttr("pointer"))
				throw new NotSupportedException("Implement pointer attribute");

			if (node.hasAttr("pointerDepth"))
				throw new NotSupportedException("Implement pointerDepth attribute");

			string strLenType = null;
			if (node.hasAttr("lengthType"))
				strLenType = node.getAttrString("lengthType");
			else
				strLenType = getDefaultAttr(element.GetType(), "lengthType", null);

			switch (strLenType)
			{
				case null:
					break;
				case "bytes":
					element.lengthType = LengthType.Bytes;
					break;
				case "bits":
					element.lengthType = LengthType.Bits;
					break;
				case "chars":
					element.lengthType = LengthType.Chars;
					break;
				default:
					throw new PeachException("Error, parsing lengthType on '" + element.name +
						"', unknown value: '" + strLenType + "'.");
			}

			if (node.hasAttr("length"))
			{
				int length = node.getAttrInt("length");

				try
				{
					element.length = length;
				}
				catch (Exception e)
				{
					throw new PeachException("Error, setting length on element '" + element.name + "'.  " + e.Message, e);
				}
			}
		}

		/// <summary>
		/// Handle parsing common dataelement children liek relation, fixupImpl and
		/// transformer.
		/// </summary>
		/// <param name="node">Node to read values from</param>
		/// <param name="element">Element to set values on</param>
		public void handleCommonDataElementChildren(XmlNode node, DataElement element)
		{
			foreach (XmlNode child in node.ChildNodes)
			{
				switch (child.Name)
				{
					case "Relation":
						handleRelation(child, element);
						break;

					case "Fixup":
						handleFixup(child, element);
						break;

					case "Transformer":
						handleTransformer(child, element);
						break;

					case "Hint":
						handleHint(child, element);
						break;

					case "Analyzer":
						handleAnalyzer(child, element);
						break;

					case "Placement":
						handlePlacement(child, element);
						break;
				}
			}
		}

		protected void handleFixup(XmlNode node, DataElement element)
		{
			if (element.fixup != null)
				throw new PeachException("Error, multiple fixups defined on element '" + element.name + "'.");

			element.fixup = handlePlugin<Fixup, FixupAttribute>(node, element, true);
		}

		protected void handleAnalyzer(XmlNode node, DataElement element)
		{
			if (element.analyzer != null)
				throw new PeachException("Error, multiple analyzers are defined on element '" + element.name + "'.");

			element.analyzer = handlePlugin<Analyzer, AnalyzerAttribute>(node, element, false);
		}

		protected void handleTransformer(XmlNode node, DataElement element)
		{
			if (element.transformer != null)
				throw new PeachException("Error, multiple transformers are defined on element '" + element.name + "'.");

			element.transformer = handlePlugin<Transformer, TransformerAttribute>(node, element, false);

			handleNestedTransformer(node, element, element.transformer);
		}

		protected void handleNestedTransformer(XmlNode node, DataElement element, Transformer transformer)
		{
			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Transformer")
				{
					if (transformer.anotherTransformer != null)
						throw new PeachException("Error, multiple nested transformers are defined on element '" + element.name + "'.");

					transformer.anotherTransformer = handlePlugin<Transformer, TransformerAttribute>(child, element, false);

					handleNestedTransformer(child, element, transformer.anotherTransformer);
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="node">XmlNode tor read children elements from</param>
		/// <param name="element">Element to add items to</param>
		protected void handleHint(XmlNode node, DataElement element)
		{
			var hint = new Hint(node.getAttrString("name"), node.getAttrString("value"));
			element.Hints.Add(hint.Name, hint);
			logger.Debug("handleHint: " + hint.Name + ": " + hint.Value);
		}

		protected void handlePlacement(XmlNode node, DataElement element)
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();

			if (node.hasAttr("after"))
				args["after"] = new Variant(node.getAttrString("after"));
			else if (node.hasAttr("before"))
				args["before"] = new Variant(node.getAttrString("before"));
			else
				throw new PeachException("Error, Placement on element \"" + element.name + "\" is missing 'after' or 'before' attribute.");

			Placement placement = new Placement(args);
			element.placement = placement;
		}

		/// <summary>
		/// Handle parsing child data types into containers.
		/// </summary>
		/// <param name="node">XmlNode tor read children elements from</param>
		/// <param name="element">Element to add items to</param>
		public void handleDataElementContainer(XmlNode node, DataElementContainer element)
		{
			foreach (XmlNode child in node.ChildNodes)
			{
				DataElement elem = null;

				if (child.Name == "#comment")
					continue;

				Type dataElementType;
				
				if (!dataElementPitParsable.TryGetValue(child.Name, out dataElementType))
				{
					if (((IList<string>)dataElementCommon).Contains(child.Name))
						continue;
					else
						throw new PeachException("Error, found unknown data element in pit file: " + child.Name);
				}

				MethodInfo pitParsableMethod = dataElementType.GetMethod("PitParser");
				if (pitParsableMethod == null)
					throw new PeachException("Error, type with PitParsableAttribute is missing static PitParser(...) method: " + dataElementType.FullName);

				PitParserDelegate delegateAction = Delegate.CreateDelegate(typeof(PitParserDelegate), pitParsableMethod) as PitParserDelegate;

				// Prevent dots from being in the name for element construction, they get resolved afterwards
				string childName = null;
				if (child.hasAttr("name"))
					childName = child.getAttrString("name");

				if (element.isReference && !string.IsNullOrEmpty(childName))
				{
					var refname = childName.Split('.');
					child.Attributes["name"].InnerText = refname[refname.Length - 1];
				}

				elem = delegateAction(this, child, element);

				if (elem == null)
					throw new PeachException("Error, type failed to parse provided XML: " + dataElementType.FullName);

				// Wrap elements that are arrays with an Array object
				if (IsArray(child))
				{
					// Ensure the array has the same name as the 1st element
					((System.Xml.XmlElement)child).SetAttribute("name", elem.name);

					var array = Dom.Array.PitParser(this, child, element) as Dom.Array;
					array.origionalElement = elem;

					if (array.occurs > 0)
						array.Add(elem);
					else
						elem.parent = array;

					// Expand all occurances
					for (int i = 1; i < array.occurs; ++i)
						array.Add(elem.Clone(elem.name + "_" + i));

					// Copy over hints, some may be for array
					foreach (var key in elem.Hints.Keys)
						array.Hints[key] = elem.Hints[key];

					elem = array;
				}

				// If parent was created by a reference (ref attribute)
				// then allow replacing existing elements with new
				// elements.  This includes "deep" replacement using "."
				// notation.
				if (element.isReference)
				{
					var parent = element;

					if (childName != null && childName.IndexOf(".") > -1)
					{
						var parentName = childName.Substring(0, childName.LastIndexOf('.'));
						parent = element.find(parentName) as DataElementContainer;

						if (parent == null)
							throw new PeachException("Error, child name has dot notation but parent element not found: '" + parentName + ".");
					}

					parent.ApplyReference(elem);
				}
				else
				{
					// Otherwise enforce unique element names.
					element.Add(elem);
				}
			}
		}

		Regex _hexWhiteSpace = new Regex(@"[h{},\s\r\n]+", RegexOptions.Singleline);
		Regex _escapeSlash = new Regex(@"\\\\|\\n|\\r|\\t");

		private static string replaceSlash(Match m)
		{
			string s = m.ToString();

			switch (s)
			{
				case "\\\\": return "\\";
				case "\\n": return "\n";
				case "\\r": return "\r";
				case "\\t": return "\t";
			}
			
			throw new ArgumentOutOfRangeException("m");
		}

		public void handleCommonDataElementValue(XmlNode node, DataElement elem)
		{
			if (!node.hasAttr("value"))
				return;

			string value = node.getAttrString("value");

			value = _escapeSlash.Replace(value, new MatchEvaluator(replaceSlash));

			string valueType = null;

			if (node.hasAttr("valueType"))
				valueType = node.getAttrString("valueType");
			else
				valueType = getDefaultAttr(elem.GetType(), "valueType", "string");

			switch (valueType.ToLower())
			{
				case "hex":
					// Handle hex data.

					// 1. Remove white space
					value = _hexWhiteSpace.Replace(value, "");

					// 3. Remove 0x
					value = value.Replace("0x", "");

					// 4. remove \x
					value = value.Replace("\\x", "");

					if (value.Length % 2 != 0)
						throw new PeachException("Error, the hex value of " + elem.debugName + " must contain an even number of characters.");

					var array = HexString.ToArray(value);
					if (array == null)
						throw new PeachException("Error, the value of " + elem.debugName + " contains invalid hex characters.");

					elem.DefaultValue = new Variant(new BitStream(array));
					break;
				case "literal":

					var localScope = new Dictionary<string, object>();
					localScope["self"] = elem;
					localScope["node"] = node;
					localScope["Parser"] = this;
					localScope["Context"] = this._dom.context;
					
					var obj = Scripting.EvalExpression(value, localScope);

					if (obj == null)
						throw new PeachException("Error, the value of " + elem.debugName + " is not a valid eval statement.");

					elem.DefaultValue = new Variant(obj.ToString());
					break;
				case "string":
					// No action requried, default behaviour
					elem.DefaultValue = new Variant(value);
					break;
				default:
					throw new PeachException("Error, invalid value for 'valueType' attribute: " + valueType);
			}
		}

		private static string getDefaultError(Type type, string key)
		{
			return string.Format("Error, element '{0}' has an invalid default value for attribute '{1}'.", type.Name, key);
		}

		public string getDefaultAttr(Type type, string key, string defaultValue)
		{
			Dictionary<string, string> defaults = null;
			if (!dataElementDefaults.TryGetValue(type, out defaults))
				return defaultValue;

			string value = null;
			if (!defaults.TryGetValue(key, out value))
				return defaultValue;

			return value;
		}

		public bool getDefaultAttr(Type type, string key, bool defaultValue)
		{
			string value = getDefaultAttr(type, key, null);

			switch (value)
			{
				case null:
					return defaultValue;
				case "1":
				case "true":
					return true;
				case "0":
				case "false":
					return false;
				default:
					throw new PeachException(getDefaultError(type, key) + "  Could not convert value '" + value + "' to a boolean.");
			}
		}

		public int getDefaultAttr(Type type, string key, int defaultValue)
		{
			string value = getDefaultAttr(type, key, null);
			if (value == null)
				return defaultValue;

			int ret;
			if (!int.TryParse(value, out ret))
				throw new PeachException(getDefaultError(type, key) + "  Could not convert value '" + value + "' to an integer.");

			return ret;
		}

		public char getDefaultAttr(Type type, string key, char defaultValue)
		{
			string value = getDefaultAttr(type, key, null);
			if (value == null)
				return defaultValue;

			if (value.Length != 1)
				throw new PeachException(getDefaultError(type, key) + "  Could not convert value '" + value + "' to a character.");

			return value[0];
		}

		protected void handleRelation(XmlNode node, DataElement parent)
		{
			string value = node.getAttrString("type");
			switch (value)
			{
				case "size":
					if (node.hasAttr("of"))
					{
						SizeRelation rel = new SizeRelation(parent);
						rel.OfName = node.getAttrString("of");

						if (node.hasAttr("expressionGet"))
							rel.ExpressionGet = node.getAttrString("expressionGet");

						if (node.hasAttr("expressionSet"))
							rel.ExpressionSet = node.getAttrString("expressionSet");

						var strType = node.getAttr("lengthType", rel.lengthType.ToString());
						LengthType lenType;
						if (!Enum.TryParse(strType, true, out lenType))
							throw new PeachException("Error, size relation on element '" + parent.name + "' has invalid lengthType '" + strType + "'.");

						rel.lengthType = lenType;
					}

					break;

				case "count":
					if (node.hasAttr("of"))
					{
						CountRelation rel = new CountRelation(parent);
						rel.OfName = node.getAttrString("of");

						if (node.hasAttr("expressionGet"))
							rel.ExpressionGet = node.getAttrString("expressionGet");

						if (node.hasAttr("expressionSet"))
							rel.ExpressionSet = node.getAttrString("expressionSet");
					}
					break;

				case "offset":
					if (node.hasAttr("of"))
					{
						OffsetRelation rel = new OffsetRelation(parent);
						rel.OfName = node.getAttrString("of");

						if (node.hasAttr("expressionGet"))
							rel.ExpressionGet = node.getAttrString("expressionGet");

						if (node.hasAttr("expressionSet"))
							rel.ExpressionSet = node.getAttrString("expressionSet");

						if (node.hasAttr("relative"))
							rel.isRelativeOffset = true;

						if (node.hasAttr("relativeTo"))
						{
							rel.isRelativeOffset = true;
							rel.relativeTo = node.getAttrString("relativeTo");
						}
					}
					break;

				default:
					throw new PeachException("Error, element '" + parent.name + "' has unknown relation type '" + value + "'.");
			}
		}

		protected T handlePlugin<T, A>(XmlNode node, DataElement parent, bool useParent)
			where T : class
			where A : PluginAttribute
		{
			var pluginType = typeof(T).Name;

			var cls = node.getAttrString("class");
			IDictionary<string,Variant> arg;

			if (typeof(T) == typeof(Monitor))
				arg = handleParamsOrdered(node);
			else
				arg = handleParams(node);

			var type = ClassLoader.FindTypeByAttribute<A>((x, y) => y.Name == cls);
			if (type == null)
				throw new PeachException(string.Format("Error, unable to locate {0} named '{1}', FindTypeByAttribute returned null.", pluginType, cls));

			validateParameterAttributes<A>(type, pluginType, cls, arg);

			try
			{
				if (useParent)
				{
					if (arg is Dictionary<string, Variant>)
						return Activator.CreateInstance(type, parent, (Dictionary<string, Variant>)arg) as T;
					else
						return Activator.CreateInstance(type, parent, (SerializableDictionary<string, Variant>)arg) as T;
				}
				else
				{
					if (arg is Dictionary<string, Variant>)
						return Activator.CreateInstance(type, (Dictionary<string, Variant>) arg) as T;
					else
						return Activator.CreateInstance(type, (SerializableDictionary<string, Variant>)arg) as T;
				}
			}
			catch (Exception e)
			{
				if (e.InnerException != null)
				{
					throw new PeachException(string.Format(
						"Error, unable to create instance of '{0}' named '{1}'.\nExtended error: Exception during object creation: {2}",
						pluginType, cls, e.InnerException.Message
					));
				}

				throw new PeachException(string.Format(
					"Error, unable to create instance of '{0}' named '{1}'.\nExtended error: Exception during object creation: {2}",
					pluginType, cls, e.InnerException.Message
				), e);
			}
		}

		#endregion

		#region State Model

		protected virtual StateModel handleStateModel(XmlNode node, Dom.Dom parent)
		{
			string name = node.getAttrString("name");
			string initialState = node.getAttrString("initialState");
			StateModel stateModel = new StateModel();
			stateModel.name = name;
			stateModel.parent = parent;

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "State")
				{
					State state = handleState(child, stateModel);

					try
					{
						stateModel.states.Add(state.name, state);
					}
					catch (ArgumentException)
					{
						throw new PeachException("Error, a <State> element named '" + state.name + "' already exists in state model '" + stateModel.name + "'.");
					}

					if (state.name == initialState)
						stateModel.initialState = state;
				}
			}

			if (stateModel.initialState == null)
				throw new PeachException("Error, did not locate inital ('" + initialState + "') for state model '" + name + "'.");

			return stateModel;
		}

		protected virtual State handleState(XmlNode node, StateModel parent)
		{
			State state = new State();
			state.parent = parent;
			state.name = node.getAttrString("name");

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Action")
				{
					Core.Dom.Action action = handleAction(child, state);

					try
					{
						state.actions.Add(action);
					}
					catch (ArgumentException)
					{
						throw new PeachException("Error, a <Action> element named '" + action.name + "' already exists in state '" + state.parent.name + "." + state.name + "'.");
					}
				}
			}

			return state;
		}

		protected virtual void handleActionAttr(XmlNode node, Dom.Action action, params string[] badAttrs)
		{
			foreach (var attr in badAttrs)
				if (node.hasAttr(attr))
					throw new PeachException("Error, action '{0}.{1}.{2}' has invalid attribute '{3}'.".Fmt(
						action.parent.parent.name,
						action.parent.name,
						action.name,
						attr));
		}

		protected virtual void handleActionParameter(XmlNode node, Dom.Actions.Call action)
		{
			string strType = node.getAttr("type", "in");
			ActionParameter.Type type;
			if (!Enum.TryParse(strType, true, out type))
				throw new PeachException("Error, type attribute '{0}' on <Param> child of action '{1}.{2}.{3}' is invalid.".Fmt(
					strType,
					action.parent.parent.name,
					action.parent.name,
					action.name));

			var name = node.getAttr("name", action.parameters.UniqueName());
			var data = new ActionParameter(name)
			{
				action = action,
				type = type,
			};

			// 'Out' params are input and can't have <Data>
			handleActionData(node, data, "<Param> child of ", type != ActionParameter.Type.Out);

			try
			{
				action.parameters.Add(data);
			}
			catch (ArgumentException)
			{
				throw new PeachException("Error, a <Param> element named '{0}' already exists in action '{1}.{2}.{3}'.".Fmt(
					data.name,
					action.parent.parent.name,
					action.parent.name,
					action.name));
			}
		}

		protected virtual void handleActionResult(XmlNode node, Dom.Actions.Call action)
		{
			action.result = new ActionResult()
			{
				action = action
			};

			handleActionData(node, action.result, "<Result> child of ", false);
		}

		protected virtual void handleActionCall(XmlNode node, Dom.Actions.Call action)
		{
			action.method = node.getAttrString("method");

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Param")
					handleActionParameter(child, action);
				else if (child.Name == "Result")
					handleActionResult(child, action);
			}

			handleActionAttr(node, action, "ref", "property", "setXpath", "valueXpath");
		}

		protected virtual void handleActionChangeState(XmlNode node, Dom.Actions.ChangeState action)
		{
			action.reference = node.getAttrString("ref");

			handleActionAttr(node, action, "method", "property", "setXpath", "valueXpath");
		}

		protected virtual void handleActionSlurp(XmlNode node, Dom.Actions.Slurp action)
		{
			action.setXpath = node.getAttrString("setXpath");
			action.valueXpath = node.getAttrString("valueXpath");

			handleActionAttr(node, action, "ref", "method", "property");
		}

		protected virtual void handleActionSetProperty(XmlNode node, Dom.Actions.SetProperty action)
		{
			action.property = node.getAttrString("property");
			action.data = new ActionData()
			{
				action = action
			};

			handleActionData(node, action.data, "", true);

			handleActionAttr(node, action, "ref", "method", "setXpath", "valueXpath");
		}

		protected virtual void handleActionGetProperty(XmlNode node, Dom.Actions.GetProperty action)
		{
			action.property = node.getAttrString("property");
			action.data = new ActionData()
			{
				action = action
			};

			handleActionData(node, action.data, "", false);

			handleActionAttr(node, action, "ref", "method", "setXpath", "valueXpath");
		}

		protected virtual void handleActionOutput(XmlNode node, Dom.Actions.Output action)
		{
			action.data = new ActionData()
			{
				action = action
			};

			handleActionData(node, action.data, "", true);

			handleActionAttr(node, action, "ref", "method", "property", "setXpath", "valueXpath");
		}

		protected virtual void handleActionInput(XmlNode node, Dom.Actions.Input action)
		{
			action.data = new ActionData()
			{
				action = action
			};

			handleActionData(node, action.data, "", false);

			handleActionAttr(node, action, "ref", "method", "property", "setXpath", "valueXpath");
		}

		protected virtual void handleActionData(XmlNode node, ActionData data, string type, bool hasData)
		{
			foreach (XmlNode child in node.ChildNodes)
			{
				data.dataModel = handleDataModel(child, data.dataModel);

				if (data.dataModel != null)
				{
					data.dataModel.dom = null;
					data.dataModel.action = data.action;
				}

				if (child.Name == "Data")
				{
					if (!hasData)
						throw new PeachException("Error, {0}action '{1}.{2}.{3}' has unsupported child element <Data>.".Fmt(
							type,
							data.action.parent.parent.name,
							data.action.parent.name,
							data.action.name));

					var item = handleData(child, data.dataSets.UniqueName());

					try
					{
						data.dataSets.Add(item);
					}
					catch (ArgumentException)
					{
						throw new PeachException("Error, a <Data> element named '{0}' already exists in {1}action '{2}.{3}.{4}'.".Fmt(
							item.name,
							type,
							data.action.parent.parent.name,
							data.action.parent.name,
							data.action.name));
					}
				}
			}

			if (data.dataModel == null)
				throw new PeachException("Error, {0}action '{1}.{2}.{3}' is missing required child element <DataModel>.".Fmt(
					type,
					data.action.parent.parent.name,
					data.action.parent.name,
					data.action.name));
		}

		protected virtual Core.Dom.Action handleAction(XmlNode node, State parent)
		{
			var strType = node.getAttrString("type");
			var type = ClassLoader.FindTypeByAttribute<ActionAttribute>((t, a) => 0 == string.Compare(a.Name, strType, true));
			if (type == null)
				throw new PeachException("Error, state '" + parent.name + "' has an invalid action type '" + strType + "'.");

			var name = node.getAttr("name", parent.actions.UniqueName());

			var action = (Dom.Action)Activator.CreateInstance(type);

			action.name = name;
			action.parent = parent;
			action.when = node.getAttr("when",null);
			action.publisher = node.getAttr("publisher", null);
			action.onStart = node.getAttr("onStart", null);
			action.onComplete = node.getAttr("onComplete", null);

			if (action is Dom.Actions.Call)
				handleActionCall(node, (Dom.Actions.Call)action);
			else if (action is Dom.Actions.ChangeState)
				handleActionChangeState(node, (Dom.Actions.ChangeState)action);
			else if (action is Dom.Actions.Slurp)
				handleActionSlurp(node, (Dom.Actions.Slurp)action);
			else if (action is Dom.Actions.GetProperty)
				handleActionGetProperty(node, (Dom.Actions.GetProperty)action);
			else if (action is Dom.Actions.SetProperty)
				handleActionSetProperty(node, (Dom.Actions.SetProperty)action);
			else if (action is Dom.Actions.Input)
				handleActionInput(node, (Dom.Actions.Input)action);
			else if (action is Dom.Actions.Output)
				handleActionOutput(node, (Dom.Actions.Output)action);

			return action;
		}


		protected virtual DataSet handleData(XmlNode node, string uniqueName)
		{
			DataSet dataSet = null;

			if (node.hasAttr("ref"))
			{
				string refName = node.getAttrString("ref");

				var other = getRef<DataSet>(_dom, refName, a => a.datas);
				if (other == null)
					throw new PeachException("Error, could not resolve Data element ref attribute value '" + refName + "'.");

				dataSet = ObjectCopier.Clone(other);
			}
			else
			{
				dataSet = new DataSet();
			}

			dataSet.name = node.getAttr("name", uniqueName);

			if (node.hasAttr("fileName"))
			{
				if (node.ChildNodes.AsEnumerable().Where(n => n.Name == "Field").Any())
					throw new PeachException("Can't specify fields and file names.");

				dataSet.Clear();

				string dataFileName = node.getAttrString("fileName");

				if (dataFileName.Contains('*'))
				{
					string pattern = Path.GetFileName(dataFileName);
					string dir = dataFileName.Substring(0, dataFileName.Length - pattern.Length);

					if (dir == "")
						dir = ".";

					try
					{
						dir = Path.GetFullPath(dir);
						string[] files = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);
						foreach (var item in files)
							dataSet.Add(new DataFile(dataSet, item));
					}
					catch (ArgumentException ex)
					{
						// Directory is not legal
						throw new PeachException("Error parsing Data element, fileName contains invalid characters: " + dataFileName, ex);
					}

					if (dataSet.Count == 0)
						throw new PeachException("Error parsing Data element, no matching files found: " + dataFileName);
				}
				else
				{
					try
					{
						string normalized = Path.GetFullPath(dataFileName);

						if (Directory.Exists(normalized))
						{
							foreach (string fileName in Directory.GetFiles(normalized))
								dataSet.Add(new DataFile(dataSet, fileName));

							if (dataSet.Count == 0)
								throw new PeachException("Error parsing Data element, folder contains no files: " + dataFileName);
						}
						else if (File.Exists(normalized))
						{
							dataSet.Add(new DataFile(dataSet, normalized));
						}
						else
						{
							throw new PeachException("Error parsing Data element, file or folder does not exist: " + dataFileName);
						}
					}
					catch (ArgumentException ex)
					{
						throw new PeachException("Error parsing Data element, fileName contains invalid characters: " + dataFileName, ex);
					}
				}
			}
			else if (node.ChildNodes.AsEnumerable().Where(n => n.Name == "Field").Any())
			{
				// Ensure <Field> and fileName="" aren't used together
				if (node.hasAttr("fileName"))
					throw new PeachException("Can't specify fields and file names.");

				// If this ref'd an existing Data element, clear all non FieldData children
				if (dataSet.Where(o => !(o is DataField)).Any())
					dataSet.Clear();

				// Ensure there is a field data record we can populate
				if (dataSet.Count == 0)
					dataSet.Add(new DataField(dataSet));

				var fieldData = (DataField)dataSet[0];

				var dupes = new HashSet<string>();

				foreach (XmlNode child in node.ChildNodes)
				{
					if (child.Name == "Field")
					{
						string name = child.getAttrString("name");

						if (!dupes.Add(name))
							throw new PeachException("Error, Data element has multiple entries for field '" + name + "'.");

						// Hack to call common value parsing code.
						Blob tmp = new Blob();
						handleCommonDataElementValue(child, tmp);

						fieldData.Fields.Remove(name);
						fieldData.Fields.Add(new DataField.Field() { Name = name, Value = tmp.DefaultValue });
					}
				}
			}

			if (dataSet.Count == 0)
				throw new PeachException("Error, <Data> element is missing required 'fileName' attribute or <Field> child element.");

			return dataSet;
		}

		protected virtual List<string> handleMutators(XmlNode node)
		{
			var ret = new List<string>();

			foreach (XmlNode child in node)
			{
				if (child.Name == "Mutator")
				{
					string name = child.getAttrString("class");
					ret.Add(name);
				}
			}

			return ret;
		}

		protected virtual Test handleTest(XmlNode node, Dom.Dom parent)
		{
			Test test = new Test();
			test.parent = parent;

			test.name = node.getAttrString("name");

			if (node.hasAttr("waitTime"))
				test.waitTime = decimal.Parse(node.getAttrString("waitTime"));

			if (node.hasAttr("faultWaitTime"))
				test.faultWaitTime = decimal.Parse(node.getAttrString("faultWaitTime"));

			if (node.hasAttr("controlIteration"))
				test.controlIterationEvery = int.Parse(node.getAttrString("controlIteration"));

			if (node.hasAttr("replayEnabled"))
				test.replayEnabled = node.getAttrBool("replayEnabled");

			if (node.hasAttr("nonDeterministicActions"))
				test.nonDeterministicActions = node.getAttrBool("nonDeterministicActions");

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Logger")
					test.loggers.Add(handlePlugin<Logger, LoggerAttribute>(child, null, false));

				// Include
				if (child.Name == "Include")
				{
					var attr = child.getAttr("ref", null);

					if (attr != null)
						attr = string.Format("//{0}", attr);
					else
						attr = child.getAttr("xpath", null);

					if (attr == null)
						attr = "//*";

					test.mutables.Add(new Tuple<bool, string>(true, attr));
				}

				// Exclude
				if (child.Name == "Exclude")
				{
					var attr = child.getAttr("ref", null);

					if (attr != null)
						attr = string.Format("//{0}", attr);
					else
						attr = child.getAttr("xpath", null);

					if (attr == null)
						attr = "//*";

					test.mutables.Add(new Tuple<bool, string>(false, attr));
				}

				// Strategy
				if (child.Name == "Strategy")
				{
					test.strategy = handlePlugin<MutationStrategy, MutationStrategyAttribute>(child, null, false);
				}

				// Agent
				if (child.Name == "Agent")
				{
					string refName = child.getAttrString("ref");

					var agent = getRef<Dom.Agent>(parent, refName, a => a.agents);
					if (agent == null)
						throw new PeachException("Error, Test::" + test.name + " Agent name in ref attribute not found.");

					test.agents.Add(refName, agent);

					var platform = child.getAttr("platform", null);
					if (platform != null)
					{
						switch (platform.ToLower())
						{
							case "windows":
								parent.agents[refName].platform = Platform.OS.Windows;
								break;
							case "osx":
								parent.agents[refName].platform = Platform.OS.OSX;
								break;
							case "linux":
							case "unix":
								parent.agents[refName].platform = Platform.OS.Linux;
								break;
						}
					}
				}

				// StateModel
				if (child.Name == "StateModel")
				{
					string strRef = child.getAttrString("ref");

					test.stateModel = getRef<Dom.StateModel>(parent, strRef, a => a.stateModels);
					if (test.stateModel == null)
						throw new PeachException("Error, could not locate StateModel named '" +
							strRef + "' for Test '" + test.name + "'.");

					test.stateModel.name = strRef;
					test.stateModel.parent = test.parent;
				}

				// Publisher
				if (child.Name == "Publisher")
				{

					string name = child.getAttr("name", null);
					if (name == null)
					{
						int i = 0;
						name = "Pub";
						while (test.publishers.ContainsKey(name))
							name = "Pub_" + (++i).ToString();
					}

					var pub = handlePlugin<Publisher, PublisherAttribute>(child, null, false);

					try
					{
						test.publishers.Add(name, pub);
					}
					catch (ArgumentException)
					{
						throw new PeachException("Error, a <Publisher> element named '{0}' already exists in test '{1}'.".Fmt(name, test.name));
					}
				}

				// Mutator
				if (child.Name == "Mutators")
				{
					string mode = child.getAttrString("mode");

					var list = handleMutators(child);

					switch (mode.ToLower())
					{
						case "include":
							test.includedMutators.AddRange(list);
							break;
						case "exclude":
							test.excludedMutators.AddRange(list);
							break;
						default:
							throw new PeachException("Error, Mutators element has invalid 'mode' attribute '" + mode + "'");
					}
				}
			}

			if (test.stateModel == null)
				throw new PeachException("Test '" + test.name + "' missing StateModel element.");
			if (test.publishers.Count == 0)
				throw new PeachException("Test '" + test.name + "' missing Publisher element.");

			if (test.strategy == null)
			{
				var type = ClassLoader.FindTypeByAttribute<DefaultMutationStrategyAttribute>(null);
				test.strategy = Activator.CreateInstance(type, new Dictionary<string, Variant>()) as MutationStrategy;
			}

			return test;
		}

		public static uint _uniquePublisherName = 0;

		protected void validateParameterAttributes<A>(Type type, string pluginType, string name, IDictionary<string, Variant> xmlParameters) where A : PluginAttribute
		{
			var objParams = type.GetAttributes<ParameterAttribute>(null);

			var inherit = type.GetAttributes<InheritParameterAttribute>(null).FirstOrDefault();
			if (inherit != null)
			{
				string otherClass = (string)xmlParameters[inherit.parameter];

				var otherType = ClassLoader.FindTypeByAttribute<A>((x, y) => y.Name == otherClass);
				if (otherType == null)
					return;

				var otherParams = otherType.GetAttributes<ParameterAttribute>(null);
				objParams = otherParams.Concat(objParams);
			}

			var missing = objParams.Where(a => a.required && !xmlParameters.ContainsKey(a.name)).Select(a => a.name).FirstOrDefault();
			if (missing != null)
			{
				throw new PeachException(
					string.Format("Error, {0} '{1}' is missing required parameter '{2}'.\n{3}",
						pluginType, name, missing, formatParameterAttributes(objParams)));
			}

			var extra = xmlParameters.Select(kv => kv.Key).Where(k => null == objParams.FirstOrDefault(a => a.name == k)).FirstOrDefault();
			if (extra != null)
			{
				throw new PeachException(
					string.Format("Error, {0} '{1}' has unknown parameter '{2}'.\n{3}",
						pluginType, name, extra, formatParameterAttributes(objParams)));
			}
		}

		protected string formatParameterAttributes(IEnumerable<ParameterAttribute> publisherParameters)
		{
			// XXX: why is this reversed?
			var reversed = new List<ParameterAttribute>(publisherParameters);
			reversed.Reverse();

			string s = "\nSupported Parameters:\n\n";
			foreach (var p in reversed)
			{
				if (p.required)
					s += "  " + p.name + ": [REQUIRED] " + p.description + "\n";
				else
					s += "  " + p.name + ": " + p.description + "\n";
			}
			s += "\n";

			return s;
		}

		protected Dictionary<string, Variant> handleParams(XmlNode node)
		{
			Dictionary<string, Variant> ret = new Dictionary<string, Variant>();
			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name != "Param")
					continue;

				string name = child.getAttrString("name");
				string value = child.getAttrString("value");

				if (child.hasAttr("valueType"))
				{
					ret.Add(name, new Variant(value, child.getAttrString("valueType")));
				}
				else
				{
					ret.Add(name, new Variant(value));
				}
			}

			return ret;
		}

		protected SerializableDictionary<string, Variant> handleParamsOrdered(XmlNode node)
		{
			SerializableDictionary<string, Variant> ret = new SerializableDictionary<string, Variant>();
			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name != "Param")
					continue;

				string name = child.getAttrString("name");
				string value = child.getAttrString("value");

				if (child.hasAttr("valueType"))
				{
					ret.Add(name, new Variant(value, child.getAttrString("valueType")));
				}
				else
				{
					ret.Add(name, new Variant(value));
				}
			}

			return ret;
		}

		#endregion
	}
}

// end
