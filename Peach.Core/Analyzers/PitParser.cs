
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
		static readonly string[] dataElementCommon = { "Relation", "Fixup", "Transformer", "Hint", "Analyzer", "Placement" };

		static PitParser()
		{
			PitParser.supportParser = true;
			Analyzer.defaultParser = new PitParser();
			populateDataElementPitParsable();
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

		public virtual Dom.Dom asParser(Dictionary<string, object> args, Stream data, bool doValidatePit)
		{
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

			if (args != null && args.ContainsKey(DEFINED_VALUES))
			{
				var definedValues = args[DEFINED_VALUES] as Dictionary<string, string>;
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

		static protected void populateDataElementPitParsable()
		{
			foreach (var kv in ClassLoader.GetAllByAttribute<PitParsableAttribute>(null))
			{
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
				if (child.Name == "DataModel")
				{
					DataModel dm = handleDataModel(child);
					dom.dataModels.Add(dm.name, dm);
					finalUpdateRelations(new DataModel[] { dm });
				}
			}

			// Pass 4 - Handle Data

			foreach (XmlNode child in node)
			{
				if (child.Name == "Data")
				{
					var data = handleData(child);

					if (dom.datas.ContainsKey(data.name))
						throw new PeachException("Error, a Data element named '" + data.name + "' already exists.");

					dom.datas.Add(data.name, data);
				}
			}

			// Pass 5 - Handle State model

			foreach (XmlNode child in node)
			{
				if (child.Name == "StateModel")
				{
					StateModel sm = handleStateModel(child, dom);
					dom.stateModels.Add(sm.name, sm);
				}

				if (child.Name == "Agent")
				{
					Dom.Agent agent = handleAgent(child);
					dom.agents[agent.name] = agent;
				}
			}

			// Pass 6 - Handle Test

			foreach (XmlNode child in node)
			{
				if (child.Name == "Test")
				{
					Test test = handleTest(child, dom);
					dom.tests.Add(test.name, test);
				}
			}

			// Pass 8 - Mark mutated

			foreach (Test test in dom.tests.Values)
			{
				test.markMutableElements();
			}
		}

		public static void displayDataModel(DataElement elem, int indent = 0)
		{
			string sIndent = "";
			for (int i = 0; i < indent; i++)
				sIndent += "  ";

			Console.WriteLine(sIndent + string.Format("{0}: {1}", elem.GetHashCode(), elem.name));

			foreach (var rel in elem.relations)
			{
				if (rel.parent != elem)
					Console.WriteLine("Relation.parent != parent");

				if (rel.parent.getRoot() != elem.getRoot())
					Console.WriteLine("Relation.parent.getRoot != parent.getRoot");

				if (rel.Of.getRoot() != elem.getRoot())
					Console.WriteLine("Relation root != element root");
			}

			if (!(elem is DataElementContainer))
				return;

			foreach (var child in ((DataElementContainer)elem))
			{
				if (child.parent != elem)
					Console.WriteLine("Child parent != actual parent");

				if(child.getRoot() != elem.getRoot())
					Console.WriteLine("Child getRoot != elem getRoor");

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
		protected T getRef<T>(Dom.Dom dom, string refName, Func<Dom.Dom, OrderedDictionary<string, T>> predicate)
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

					foreach (Relation rel in elem.relations)
					{
						//logger.Debug("finalUpdateRelations: Relation " + rel.GetType().Name);

						try
						{
							if (rel.From == elem)
							{
								rel.parent = elem;

								DataElement of = rel.Of;
								if (of == null)
									continue;

								if (!of.relations.Contains(rel))
									of.relations.Add(rel, false);
							}
							else if (rel.Of == elem)
							{
								DataElement from = rel.From;
								if (from == null)
									continue;

								if (!from.relations.Contains(rel))
									from.relations.Add(rel, false);
							}
							else
							{
								logger.Debug("finalUpdateRelations: From/Of don't be a matching our element");
								throw new PeachException("Error, relation attached to element \"" + elem.fullName + "\" is not resolving correctly.");
							}
						}
						catch (Exception ex)
						{
							logger.Debug("finalUpdateRelations: Exception: " + ex.Message);
						}
					}
				}
			}
		}

		protected bool hasRelationShipFrom(DataElement from, Relation rel)
		{
			foreach (var relation in from.relations)
			{
				if (relation.From.fullName == from.fullName && relation.GetType() == rel.GetType())
					return true;
			}

			return false;
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
					monitor.name = child.getAttr("name", null);
					monitor.parameters = handleParamsOrdered(child);

					agent.monitors.Add(monitor);
				}
			}

			return agent;
		}

		#region Data Model

		protected DataModel handleDataModel(XmlNode node)
		{
			DataModel dataModel = null;
			string name = node.getAttr("name", null);
			string refName = node.getAttr("ref", null);

			if (refName != null)
			{
				DataModel refObj = getRef<Dom.DataModel>(_dom, refName, a => a.dataModels);
				if (refObj == null)
					throw new PeachException("Error, DataModel {0}could not resolve ref '{1}'. XML:\n{2}".Fmt(
						name == null ? "" : "'" + name + "' ", refName, node.OuterXml));

				if (string.IsNullOrEmpty(name))
					name = refName;

				dataModel = refObj.Clone(name) as DataModel;
				dataModel.isReference = true;
				dataModel.referenceName = refName;
			}
			else
			{
				if (string.IsNullOrEmpty(name))
					throw new PeachException("Error, DataModel missing required 'name' attribute.");

				dataModel = new DataModel(name);
			}

			handleCommonDataElementAttributes(node, dataModel);
			handleCommonDataElementChildren(node, dataModel);
			handleDataElementContainer(node, dataModel);

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
						element.fixup = handlePlugin<Fixup, FixupAttribute>(child, element, true);
						break;

					case "Transformer":
						element.transformer = handlePlugin<Transformer, TransformerAttribute>(child, element, false);
						break;

					case "Hint":
						handleHint(child, element);
						break;

					case "Analyzer":
						element.analyzer = handlePlugin<Analyzer, AnalyzerAttribute>(child, element, false);
						break;

					case "Placement":
						handlePlacement(child, element);
						break;
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

				if (!dataElementPitParsable.ContainsKey(child.Name))
				{
					if (((IList<string>)dataElementCommon).Contains(child.Name))
						continue;
					else
						throw new PeachException("Error, found unknown data element in pit file: " + child.Name);
				}

				Type dataElementType = dataElementPitParsable[child.Name];
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
					if (childName != null && childName.IndexOf(".") > -1)
					{
						var parentName = childName.Substring(0, childName.LastIndexOf('.'));
						var parent = element.find(parentName) as DataElementContainer;

						if (parent == null)
							throw new PeachException("Error, child name has dot notation but parent element not found: '" + parentName + ".");

						var choice = parent as Choice;
						if (choice != null)
						{
							updateChoice(choice, elem);
						}
						else
						{
							if (parent.ContainsKey(elem.name))
								replaceChild(parent, elem);
							else
								parent.Add(elem);
						}
					}
					else
					{
						if (element.ContainsKey(elem.name))
							replaceChild(element, elem);
						else
							element.Add(elem);
					}
				}
				// Otherwise enforce unique element names.
				else
				{
					element.Add(elem);
				}
			}
		}

		private static void replaceRelations(DataElement newChild, DataElement oldChild, DataElement elem)
		{
			foreach (var rel in elem.relations)
			{
				DataElement which = rel.Of == elem ? rel.From : rel.Of;
				string relName;

				if (which.isChildOf(oldChild, out relName))
					continue;

				var other = newChild.find(elem.fullName);

				if (elem == other)
					continue;

				if (other == null)
				{
					rel.Reset();
					continue;
				}

				other.relations.Add(rel);

				if (rel.From == elem)
					rel.From = other;

				if (rel.Of == elem)
					rel.Of = other;
			}
		}

		private static void replaceChild(DataElementContainer parent, DataElement newChild)
		{
			var oldChild = parent[newChild.name];
			oldChild.parent = null;

			replaceRelations(newChild, oldChild, oldChild);

			foreach (var elem in oldChild.EnumerateAllElements())
			{
				replaceRelations(newChild, oldChild, elem);
			}

			parent[newChild.name] = newChild;
		}

		private static void updateChoice(Choice parent, DataElement newChild)
		{
			if (!parent.choiceElements.ContainsKey(newChild.name))
			{
				parent.choiceElements.Add(newChild.name, newChild);
				newChild.parent = parent;
				return;
			}

			var oldChild = parent.choiceElements[newChild.name];
			oldChild.parent = null;

			replaceRelations(newChild, oldChild, oldChild);

			foreach (var elem in oldChild.EnumerateAllElements())
			{
				replaceRelations(newChild, oldChild, elem);
			}

			parent.choiceElements[newChild.name] = newChild;
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
						value = "0" + value;

					var array = HexString.ToArray(value);

					if (array == null)
						throw new PeachException("Error, the value of element '" + elem.name + "' is not a valid hex string.");

					elem.DefaultValue = new Variant(array);
					break;
				case "literal":
					throw new NotImplementedException("todo valueType");
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
						SizeRelation rel = new SizeRelation();
						rel.OfName = node.getAttrString("of");

						if (node.hasAttr("expressionGet"))
							rel.ExpressionGet = node.getAttrString("expressionGet");

						if (node.hasAttr("expressionSet"))
							rel.ExpressionSet = node.getAttrString("expressionSet");

						parent.relations.Add(rel);
					}

					break;

				case "count":
					if (node.hasAttr("of"))
					{
						CountRelation rel = new CountRelation();
						rel.OfName = node.getAttrString("of");

						if (node.hasAttr("expressionGet"))
							rel.ExpressionGet = node.getAttrString("expressionGet");

						if (node.hasAttr("expressionSet"))
							rel.ExpressionSet = node.getAttrString("expressionSet");

						parent.relations.Add(rel);
					}
					break;

				case "offset":
					if (node.hasAttr("of"))
					{
						OffsetRelation rel = new OffsetRelation();
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

						parent.relations.Add(rel);
					}
					break;

				default:
					throw new PeachException("Error, element '" + parent.name + "' has nknown relation type '" + value + "'.");
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
					stateModel.states.Add(state.name, state);
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
					state.actions.Add(action);
				}
			}

			return state;
		}

		protected virtual Core.Dom.Action handleAction(XmlNode node, State parent)
		{
			Core.Dom.Action action = new Core.Dom.Action();
			action.parent = parent;

			if (node.hasAttr("name"))
				action.name = node.getAttrString("name");

			if (node.hasAttr("when"))
				action.when = node.getAttrString("when");

			if (node.hasAttr("publisher"))
				action.publisher = node.getAttrString("publisher");

			if (node.hasAttr("type"))
			{
				string type = node.getAttrString("type");
				switch (type.ToLower())
				{
					case "accept":
						action.type = ActionType.Accept;
						break;
					case "call":
						action.type = ActionType.Call;
						break;
					case "changestate":
						action.type = ActionType.ChangeState;
						break;
					case "close":
						action.type = ActionType.Close;
						break;
					case "connect":
						action.type = ActionType.Connect;
						break;
					case "getproperty":
						action.type = ActionType.GetProperty;
						break;
					case "input":
						action.type = ActionType.Input;
						break;
					case "open":
						action.type = ActionType.Open;
						break;
					case "output":
						action.type = ActionType.Output;
						break;
					case "setproperty":
						action.type = ActionType.SetProperty;
						break;
					case "slurp":
						action.type = ActionType.Slurp;
						break;
					case "start":
						action.type = ActionType.Start;
						break;
					case "stop":
						action.type = ActionType.Stop;
						break;
					default:
						throw new PeachException("Error, state '" + parent.name + "' has an invalid action type '" + type + "'.");
				}
			}

			if (node.hasAttr("onStart"))
				action.onStart = node.getAttrString("onStart");

			if (node.hasAttr("onComplete"))
				action.onComplete = node.getAttrString("onComplete");

			if (node.hasAttr("ref"))
			{
				if (action.type != ActionType.ChangeState)
					throw new PeachException("Error, only Actions of type ChangeState are allowed to use the 'ref' attribute");

				action.reference = node.getAttrString("ref");
			}

			if (node.hasAttr("method"))
			{
				if (action.type != ActionType.Call)
					throw new PeachException("Error, only Actions of type Call are allowed to use the 'method' attribute");

				action.method = node.getAttrString("method");
			}

			if (node.hasAttr("property"))
			{
				if (action.type != ActionType.GetProperty && action.type != ActionType.SetProperty)
					throw new PeachException("Error, only Actions of type GetProperty and SetProperty are allowed to use the 'property' attribute");

				action.property = node.getAttrString("property");
			}

			if (node.hasAttr("setXpath"))
			{
				if (action.type != ActionType.Slurp)
					throw new PeachException("Error, only Actions of type Slurp are allowed to use the 'setXpath' attribute");

				action.setXpath = node.getAttrString("setXpath");
			}

			if (node.hasAttr("valueXpath"))
			{
				if (action.type != ActionType.Slurp)
					throw new PeachException("Error, only Actions of type Slurp are allowed to use the 'valueXpath' attribute");

				action.valueXpath = node.getAttrString("valueXpath");
			}

			//if (node.hasAttr("value"))
			//{
			//    if (action.type != ActionType.Slurp)
			//        throw new PeachException("Error, only Actions of type Slurp are allowed to use the 'value' attribute");

			//    action.value = node.getAttrString("value");
			//}

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Param")
					action.parameters.Add(handleActionParameter(child, action));

				if (child.Name == "Result")
					action.result = handleActionResult(child, action);

				if (child.Name == "DataModel")
					action.dataModel = handleDataModel(child);

				if (child.Name == "Data")
				{
					if (action.dataSet == null)
						action.dataSet = new DataSet();
					action.dataSet.Datas.Add(handleData(child));
				}
			}

			if (action.dataModelRequired && action.dataModel == null)
				throw new PeachException("Error, action '" + action.name + "' is missing required child element <DataModel>.");

			if (action.dataSet != null && action.dataModel == null)
				throw new PeachException("Error, action '" + action.name + "' has child element <Data> but is missing child element <DataModel>.");

			return action;
		}

		protected virtual ActionParameter handleActionParameter(XmlNode node, Dom.Action parent)
		{
			ActionParameter param = new ActionParameter();

			if (node.hasAttr("name"))
				param.name = node.getAttrString("name");

			string strType = node.getAttr("type", "in");
			ActionParameterType type;
			if (!Enum.TryParse(strType, true, out type))
				throw new PeachException("Error, type attribute '" + strType + "' on <Param> child of action '" + parent.name + "' is invalid");
			param.type = type;

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "DataModel")
					param.dataModel = handleDataModel(child);
				if (child.Name == "Data")
					param.data = handleData(child);
			}

			if (param.dataModel == null)
				throw new PeachException("Error, <Param> child of action '" + parent.name + "' is missing required child element <DataModel>.");

			param.dataModel.action = parent;

			return param;
		}

		protected virtual ActionResult handleActionResult(XmlNode node, Dom.Action parent)
		{
			ActionResult result = new ActionResult();

			if (node.hasAttr("name"))
				result.name = node.getAttrString("name");

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "DataModel")
					result.dataModel = handleDataModel(child);
			}

			if (result.dataModel == null)
				throw new PeachException("Error, <Result> child of action '" + parent.name + "' is missing required child element <DataModel>.");

			result.dataModel.action = parent;

			return result;
		}

		protected virtual Data handleData(XmlNode node)
		{
			Data data = null;

			if (node.hasAttr("ref"))
			{
				string refName = node.getAttrString("ref");

				Data other = getRef<Data>(_dom, refName, a => a.datas);
				if (other == null)
					throw new PeachException("Error, could not resolve Data element ref attribute value '" + refName + "'.");

				data = ObjectCopier.Clone(other);
				data.name = node.getAttr("name", new Data().name);
			}
			else
			{
				data = new Data();

				if (node.hasAttr("name"))
					data.name = node.getAttrString("name");
			}

			if (node.hasAttr("fileName"))
			{
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
						data.Files.AddRange(files);
					}
					catch (ArgumentException ex)
					{
						// Directory is not legal
						throw new PeachException("Error parsing Data element, fileName contains invalid characters: " + dataFileName, ex);
					}

					if (data.Files.Count == 0)
						throw new PeachException("Error parsing Data element, no matching files found: " + dataFileName);

					data.DataType = DataType.Files;
				}
				else
				{
					try
					{
						string normalized = Path.GetFullPath(dataFileName);

						if (Directory.Exists(normalized))
						{
							List<string> files = new List<string>();
							foreach (string fileName in Directory.GetFiles(normalized))
								files.Add(fileName);

							if (files.Count == 0)
								throw new PeachException("Error parsing Data element, folder contains no files: " + dataFileName);

							data.DataType = DataType.Files;
							data.Files = files;
						}
						else if (File.Exists(normalized))
						{
							data.DataType = DataType.File;
							data.FileName = normalized;
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

			var names = new HashSet<string>();

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Field")
				{
					string name = child.getAttrString("name");

					if (!names.Add(name))
						throw new PeachException("Error, Data element has multiple entries for field '" + name + "'.");

					data.DataType = DataType.Fields;

					// Hack to call common value parsing code.
					Blob tmp = new Blob();
					handleCommonDataElementValue(child, tmp);

					data.fields[name] = tmp.DefaultValue;
				}
			}

			return data;
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
					string name;
					if (!child.hasAttr("name"))
					{
						name = "Pub_" + _uniquePublisherName;
						_uniquePublisherName++;
					}
					else
						name = child.getAttrString("name");

					test.publishers.Add(name, handlePlugin<Publisher, PublisherAttribute>(child, null, false));
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
