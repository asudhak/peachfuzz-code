
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

		public Dom.Dom _dom = null;
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

		public override Dom.Dom asParser(Dictionary<string, object> args, Stream data)
		{
			return asParser(args, data, true);
		}

		public virtual Dom.Dom asParser(Dictionary<string, object> args, Stream data, bool doValidatePit)
		{
			if (doValidatePit)
				validatePit(data);

			XmlDocument xmldoc = new XmlDocument();
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

			xmldoc.LoadXml(xml);

			_dom = new Dom.Dom();

			foreach (XmlNode child in xmldoc.ChildNodes)
			{
				if (child.Name == "Peach")
				{
					handlePeach(child, _dom);
					break;
				}
			}

            _dom.evaulateDataModelAnalyzers();
			return _dom;
		}

		public override void asParserValidation(Dictionary<string, string> args, Stream data)
		{
			validatePit(data);
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
		/// <param name="fileName">Pit file to validate</param>
		/// <param name="schema">Peach XML Schema file</param>
		public void validatePit(Stream data)
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
			string xmlData = new StreamReader(data).ReadToEnd();
			doc.LoadXml(xmlData);

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
		/// <param name="node">XmlNode to parse</param>
		/// <param name="dom">DOM to fill</param>
		/// <returns>Returns the parsed Dom object.</returns>
		protected virtual Dom.Dom handlePeach(XmlNode node, Dom.Dom dom)
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
						string ns = child.getAttribute("ns");
						string fileName = child.getAttribute("src");
						fileName = fileName.Replace("file:", "");

						if (!File.Exists(fileName))
						{
							string newFileName = Path.Combine(
								Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
								fileName);

							if (!File.Exists(newFileName))
							{
								Console.WriteLine(newFileName);
								throw new PeachException("Error, Unable to locate Pit file [" + fileName + "].\n");
							}

							fileName = newFileName;
						}

						validatePit(File.OpenRead(fileName));

						XmlDocument xmldoc = new XmlDocument();
						xmldoc.Load(fileName);

						Dom.DomNamespace nsObj = new Dom.DomNamespace();
						nsObj.parent = dom;
						nsObj.name = ns;

						if (xmldoc.FirstChild.Name == "Peach")
							handlePeach(xmldoc.FirstChild, nsObj);

						dom.ns[ns] = nsObj;
						break;

					case "Require":
						Scripting.Imports.Add(child.getAttribute("require"));
						break;

					case "Import":
						if (child.hasAttribute("from"))
							throw new PeachException("Error, This version of Peach does not support the 'from' attribute for 'Import' elements.");

						Scripting.Imports.Add(child.getAttribute("import"));
						break;

					case "PythonPath":
						if (isScriptingLanguageSet &&
							Scripting.DefaultScriptingEngine != ScriptingEngines.Python)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Python;
						Scripting.Paths.Add(child.getAttribute("import"));
						isScriptingLanguageSet = true;
						break;

					case "RubyPath":
						if (isScriptingLanguageSet &&
							Scripting.DefaultScriptingEngine != ScriptingEngines.Ruby)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Ruby;
						Scripting.Paths.Add(child.getAttribute("require"));
						isScriptingLanguageSet = true;
						break;

					case "Python":
						if (isScriptingLanguageSet &&
							Scripting.DefaultScriptingEngine != ScriptingEngines.Python)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Python;
						Scripting.Exec(child.getAttribute("code"), new Dictionary<string, object>());
						isScriptingLanguageSet = true;
						break;

					case "Ruby":
						if (isScriptingLanguageSet &&
							Scripting.DefaultScriptingEngine != ScriptingEngines.Ruby)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Ruby;
						Scripting.Exec(child.getAttribute("code"), new Dictionary<string, object>());
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
				}
			}

			// Pass 3.5 - Resolve all relations

			finalUpdateRelations(dom.dataModels.Values);

			// Pass 4 - Handle Data

			foreach (XmlNode child in node)
			{
				if (child.Name == "Data")
				{
					throw new NotImplementedException("Data");
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

			return dom;
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
		/// <param name="dom">DOM to use for resolving ref.</param>
		/// <param name="name">Ref name to resolve.</param>
		/// <returns>DataElement for ref or null if not found.</returns>
		public static DataElement getReference(Dom.Dom dom, string name, DataElementContainer container)
		{
			if (name.IndexOf(':') > -1)
			{
				string ns = name.Substring(0, name.IndexOf(':') - 1);

				if (!dom.ns.Keys.Contains(ns))
					throw new PeachException("Unable to locate namespace '" + ns + "' in ref '" + name + "'.");

				name = name.Substring(name.IndexOf(':'));
				dom = dom.ns["name"];
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
						if (child.hasAttribute("endian"))
							args["endian"] = child.getAttribute("endian");
						if (child.hasAttribute("signed"))
							args["signed"] = child.getAttribute("signed");
						if (child.hasAttribute("valueType"))
							args["valueType"] = child.getAttribute("valueType");

						dataElementDefaults[typeof(Number)] = args;
						break;
					case "String":
						if (child.hasAttribute("lengthType"))
							args["lengthType"] = child.getAttribute("lengthType");
						if (child.hasAttribute("padCharacter"))
							args["padCharacter"] = child.getAttribute("padCharacter");
						if (child.hasAttribute("type"))
							args["type"] = child.getAttribute("type");
						if (child.hasAttribute("nullTerminated"))
							args["nullTerminated"] = child.getAttribute("nullTerminated");
						if (child.hasAttribute("valueType"))
							args["valueType"] = child.getAttribute("valueType");

						dataElementDefaults[typeof(Dom.String)] = args;
						break;
					case "Flags":
						if (child.hasAttribute("endian"))
							args["endian"] = child.getAttribute("endian");
						if (child.hasAttribute("size"))
							args["size"] = child.getAttribute("size");

						dataElementDefaults[typeof(Flags)] = args;
						break;
					case "Blob":
						if (child.hasAttribute("lengthType"))
							args["lengthType"] = child.getAttribute("lengthType");
						if (child.hasAttribute("valueType"))
							args["valueType"] = child.getAttribute("valueType");

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

			agent.name = node.getAttribute("name");
			agent.url = node.getAttribute("location");
			agent.password = node.getAttribute("password");

			if (agent.url == null)
				agent.url = "local://";

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Monitor")
				{
					Dom.Monitor monitor = new Monitor();

					monitor.cls = child.getAttribute("class");
					monitor.name = child.getAttribute("name");
					monitor.parameters = handleParams(child);

					agent.monitors.Add(monitor);
				}
			}

			return agent;
		}

		#region Data Model

		protected DataModel handleDataModel(XmlNode node)
		{
			DataModel dataModel = null;
			string name = node.getAttribute("name");
			string refName = node.getAttribute("ref");

			if (refName != null)
			{
				DataModel refObj = getReference(_dom, refName, null) as DataModel;
				if (refObj == null)
					throw new PeachException("Unable to locate 'ref' [" + refName + "] or found node did not match type. [" + node.OuterXml + "].");

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
			if (node.hasAttribute("minOccurs") || node.hasAttribute("maxOccurs") || node.hasAttribute("occurs"))
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
			if (node.hasAttribute("token"))
				element.isToken = true;

			if (node.hasAttribute("mutable"))
				element.isMutable = false;

			if (node.hasAttribute("constraint"))
				element.constraint = node.getAttribute("constraint");

			if (node.hasAttribute("pointer"))
				throw new NotSupportedException("Implement pointer attribute");

			if (node.hasAttribute("pointerDepth"))
				throw new NotSupportedException("Implement pointerDepth attribute");

			if (node.hasAttribute("lengthType"))
			{
				switch (node.getAttribute("lengthType"))
				{
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
							"', unknown value: '" + node.getAttribute("lengthType") + "'.");
				}
			}
			else if (hasDefaultAttribute(element.GetType(), "lengthType"))
			{
				switch ((string)getDefaultAttribute(element.GetType(), "lengthType"))
				{
					case "bytes":
						element.lengthType = LengthType.Bytes;
						break;
					case "bits":
						element.lengthType = LengthType.Bits;
						break;
					case "chars":
						element.lengthType = LengthType.Chars;
						break;
				}
			}

			if (node.hasAttribute("length"))
			{
				try
				{
					element.length = Int32.Parse(node.getAttribute("length"));
				}
				catch (Exception e)
				{
					throw new PeachException("Error, parsing length on '" + element.name + "': " + e.Message);
				}
			}

			element.lengthCalc = node.getAttribute("lengthCalc");
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
			var hint = new Hint(node.getAttribute("name"), node.getAttribute("value"));
			element.Hints.Add(hint.Name, hint);
		}

		protected void handlePlacement(XmlNode node, DataElement element)
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();

			if (node.hasAttribute("after"))
				args["after"] = new Variant(node.getAttribute("after"));
			else if (node.hasAttribute("before"))
				args["before"] = new Variant(node.getAttribute("before"));
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
				var childName = child.getAttribute("name");
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
					if (childName.IndexOf(".") > -1)
					{
						DataElement parent = element.find(childName);
						if (parent == null)
							throw new PeachException("Error, child name has dot notation but replacement element not found: '" + elem.name + ".");

						System.Diagnostics.Debug.Assert(elem.name == parent.name);
						parent.parent[parent.name] = elem;
					}
					else
					{
						try
						{
							element[elem.name] = elem;
						}
						catch
						{
							element.Add(elem);
						}
					}
				}
				// Otherwise enforce unique element names.
				else
				{
					element.Add(elem);
				}
			}
		}

		Regex _hexWhiteSpace = new Regex(@"[h{},\s\r\n]+", RegexOptions.Singleline);

		private static int GetNibble(char c)
		{
			if (c >= 'a')
				return 0xA + (int)(c - 'a');
			else if (c >= 'A')
				return 0xA + (int)(c - 'A');
			else
				return (int)(c - '0');
		}

		public void handleCommonDataElementValue(XmlNode node, DataElement elem)
		{
			string value = null;

			if (node.hasAttribute("value"))
			{
				value = node.getAttribute("value");

				value = value.Replace("\\\\", "\\");
				value = value.Replace("\\n", "\n");
				value = value.Replace("\\r", "\r");
				value = value.Replace("\\t", "\t");
			}

			string valueType = null;

			if (node.hasAttribute("valueType"))
				valueType = node.getAttribute("valueType");
			else if (hasDefaultAttribute(elem.GetType(), "valueType"))
				valueType = getDefaultAttribute(elem.GetType(), "valueType");

			if (valueType != null && value != null)
			{
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

						BitStream sout = new BitStream();

						for (int cnt = 0; cnt < value.Length; cnt += 2)
						{
							int nibble1 = GetNibble(value[cnt]);
							int nibble2 = GetNibble(value[cnt + 1]);

							if (nibble1 < 0 || nibble1 > 0xF || nibble2 < 0 | nibble2 > 0xF)
								throw new PeachException("Error, the value of element '{0}' is not a valid hex string.", elem.name);

							sout.WriteByte((byte)((nibble1 << 4) | nibble2));
						}

						sout.SeekBits(0, SeekOrigin.Begin);

						elem.DefaultValue = new Variant(sout.Value);
						break;
					case "literal":
						throw new NotImplementedException("todo valueType");
					case "string":
						// No action requried, default behaviour
						elem.DefaultValue = new Variant(value);
						break;
					default:
						throw new PeachException("Error, invalid value for 'valueType' attribute: " + node.getAttribute("valueType"));
				}
			}
			else if (value != null)
				elem.DefaultValue = new Variant(value);

		}

		public bool hasDefaultAttribute(Type type, string key)
		{
			if (dataElementDefaults.ContainsKey(type))
				return dataElementDefaults[type].ContainsKey(key);
			return false;
		}

		public string getDefaultAttribute(Type type, string key)
		{
			Dictionary<string, string> defaults = null;
			if (!dataElementDefaults.TryGetValue(type, out defaults))
				return null;

			string value = null;
			if (!defaults.TryGetValue(key, out value))
				return null;

			return value;
		}

		public bool getDefaultAttributeAsBool(Type type, string key, bool defaultValue)
		{
			string value = getDefaultAttribute(type, key);
			switch (value)
			{
				case "1":
				case "true":
					return true;
				case "0":
				case "false":
					return false;
				default:
					return defaultValue;
			}
		}

		protected void handleRelation(XmlNode node, DataElement parent)
		{
			switch (node.getAttribute("type"))
			{
				case "size":
					if (node.hasAttribute("of"))
					{
						SizeRelation rel = new SizeRelation();
						rel.OfName = node.getAttribute("of");

						if (node.hasAttribute("expressionGet"))
							rel.ExpressionGet = node.getAttribute("expressionGet");

						if (node.hasAttribute("expressionSet"))
							rel.ExpressionSet = node.getAttribute("expressionSet");

						parent.relations.Add(rel);
					}

					break;

				case "count":
					if (node.hasAttribute("of"))
					{
						CountRelation rel = new CountRelation();
						rel.OfName = node.getAttribute("of");

						if (node.hasAttribute("expressionGet"))
							rel.ExpressionGet = node.getAttribute("expressionGet");

						if (node.hasAttribute("expressionSet"))
							rel.ExpressionSet = node.getAttribute("expressionSet");

						parent.relations.Add(rel);
					}
					break;

				case "offset":
					if (node.hasAttribute("of"))
					{
						OffsetRelation rel = new OffsetRelation();
						rel.OfName = node.getAttribute("of");

						if (node.hasAttribute("expressionGet"))
							rel.ExpressionGet = node.getAttribute("expressionGet");

						if (node.hasAttribute("expressionSet"))
							rel.ExpressionSet = node.getAttribute("expressionSet");

						if (node.hasAttribute("relative"))
							rel.isRelativeOffset = true;

						if (node.hasAttribute("relativeTo"))
						{
							rel.isRelativeOffset = true;
							rel.relativeTo = node.getAttribute("relativeTo");
						}

						parent.relations.Add(rel);
					}
					break;

				default:
					throw new ApplicationException("Unknown relation type found '" +
						node.getAttribute("type") + "'.");
			}
		}

		protected T handlePlugin<T, A>(XmlNode node, DataElement parent, bool useParent)
			where T : class
			where A : PluginAttribute
		{
			var pluginType = typeof(T).Name;

			if (!node.hasAttribute("class"))
				throw new PeachException(string.Format("{0} element has no 'class' attribute [{1}]", pluginType, node.OuterXml));

			var cls = node.getAttribute("class");
			var arg = handleParams(node);

			var type = ClassLoader.FindTypeByAttribute<A>((x, y) => y.Name == cls);
			if (type == null)
				throw new PeachException(string.Format("Error, unable to locate {0} named '{1}', FindTypeByAttribute returned null.", pluginType, cls));

			var parameters = type.GetAttributes<ParameterAttribute>(null);
			validateParameterAttributes(pluginType, cls, parameters, arg);

			try
			{
				if (useParent)
				{
					return Activator.CreateInstance(type, parent, arg) as T;
				}
				else
				{
					return Activator.CreateInstance(type, arg) as T;
				}
			}
			catch (Exception e)
			{
				throw new PeachException(string.Format(
					"Error, unable to create instance of '{0}' named '{1}'.\nExtended error: Exception during object creation: {2}",
					pluginType, cls, e.InnerException.Message
				));
			}
		}

		#endregion

		#region State Model

		protected virtual StateModel handleStateModel(XmlNode node, Dom.Dom parent)
		{
			string name = node.getAttribute("name");
			string initialState = node.getAttribute("initialState");
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
			state.name = node.getAttribute("name");

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

			if (node.hasAttribute("name"))
				action.name = node.getAttribute("name");

			if (node.hasAttribute("when"))
				action.when = node.getAttribute("when");

			if (node.hasAttribute("publisher"))
				action.publisher = node.getAttribute("publisher");

			if (node.hasAttribute("type"))
			{
				switch (node.getAttribute("type").ToLower())
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
						throw new PeachException("Error, action of type '" + node.getAttribute("type") + "' is not valid.");
				}
			}

			if (node.hasAttribute("onStart"))
				action.onStart = node.getAttribute("onStart");

			if (node.hasAttribute("onComplete"))
				action.onComplete = node.getAttribute("onComplete");

			if (node.hasAttribute("ref"))
			{
				if (action.type == ActionType.ChangeState)
					action.reference = node.getAttribute("ref");
				else
					throw new PeachException("Error, only Actions of type ChangeState are allowed to use the 'ref' attribute");
			}

			if (node.hasAttribute("method"))
			{
				if (action.type != ActionType.Call)
					throw new PeachException("Error, only Actions of type Call are allowed to use the 'method' attribute");

				action.method = node.getAttribute("method");
			}

			if (node.hasAttribute("property"))
			{
				if (action.type != ActionType.GetProperty && action.type != ActionType.SetProperty)
					throw new PeachException("Error, only Actions of type GetProperty and SetProperty are allowed to use the 'property' attribute");

				action.property = node.getAttribute("property");
			}

			if (node.hasAttribute("setXpath"))
			{
				if (action.type != ActionType.Slurp)
					throw new PeachException("Error, only Actions of type Slurp are allowed to use the 'setXpath' attribute");

				action.setXpath = node.getAttribute("setXpath");
			}

			if (node.hasAttribute("valueXpath"))
			{
				if (action.type != ActionType.Slurp)
					throw new PeachException("Error, only Actions of type Slurp are allowed to use the 'valueXpath' attribute");

				action.valueXpath = node.getAttribute("valueXpath");
			}

			//if (node.hasAttribute("value"))
			//{
			//    if (action.type != ActionType.Slurp)
			//        throw new PeachException("Error, only Actions of type Slurp are allowed to use the 'value' attribute");

			//    action.value = node.getAttribute("value");
			//}

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Param")
					action.parameters.Add(handleActionParameter(child, action));

				if (child.Name == "Result")
					throw new NotImplementedException("Action.Result TODO");

				if (child.Name == "DataModel")
					action.dataModel = handleDataModel(child);

				if (child.Name == "Data")
				{
					if (action.dataSet == null)
						action.dataSet = new DataSet();
					action.dataSet.Datas.Add(handleData(child));
				}
			}

			return action;
		}

		protected virtual ActionParameter handleActionParameter(XmlNode node, Dom.Action parent)
		{
			ActionParameter param = new ActionParameter();
			Dom.Dom dom = parent.parent.parent.parent as Dom.Dom;

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "DataModel")
					param.dataModel = dom.dataModels[child.getAttribute("ref")];
				if (child.Name == "Data")
					param.data = handleData(child);
			}

			return param;
		}

		protected virtual Data handleData(XmlNode node)
		{
			Data data = new Data();
			data.name = node.getAttribute("name");
			string dataFileName = node.getAttribute("fileName");

			if (dataFileName != null)
			{
				if (Directory.Exists(dataFileName))
				{
					List<string> files = new List<string>();
					foreach (string fileName in Directory.GetFiles(dataFileName))
						files.Add(fileName);

					if (files.Count == 0)
						throw new PeachException("Error parsing Data element, folder contains no files: " + dataFileName);

					data.DataType = DataType.Files;
					data.Files = files;
				}
				else if (File.Exists(dataFileName))
				{
					data.DataType = DataType.File;
					data.FileName = dataFileName;
				}
				else
				{
					throw new PeachException("Error parsing Data element, file or folder does not exist: " + dataFileName);
				}
			}

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Field")
				{
					data.DataType = DataType.Fields;
					// Hack to call common value parsing code.
					Blob tmp = new Blob();
					handleCommonDataElementValue(child, tmp);

					data.fields.Add(child.getAttribute("name"), tmp.DefaultValue);
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
					string name = child.getAttribute("class");
					if (name == null)
						throw new PeachException("Error, Mutator element is missing 'class' attribute");

					ret.Add(name);
				}
			}

			return ret;
		}

		protected virtual Test handleTest(XmlNode node, Dom.Dom parent)
		{
			Test test = new Test();
			test.parent = parent;

			test.name = node.getAttribute("name");

			if (node.hasAttribute("waitTime"))
				test.waitTime = decimal.Parse(node.getAttribute("waitTime"));

			if (node.hasAttribute("faultWaitTime"))
				test.faultWaitTime = decimal.Parse(node.getAttribute("faultWaitTime"));

			if (node.hasAttribute("controlIteration"))
				test.controlIterationEvery = int.Parse(node.getAttribute("controlIteration"));

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Logger")
					test.logger = handlePlugin<Logger, LoggerAttribute>(child, null, false);

				// Include
				if (child.Name == "Include")
				{
					var attr = child.getAttribute("ref");

					if (attr != null)
						attr = string.Format("//{0} | //{0}/*", attr);
					else
						attr = child.getAttribute("xpath");

					if (attr == null)
						attr = "//*";

					test.mutables.Add(new Tuple<bool, string>(true, attr));
				}

				// Exclude
				if (child.Name == "Exclude")
				{
					var attr = child.getAttribute("ref");

					if (attr != null)
						attr = string.Format("//{0} | //{0}/*", attr);
					else
						attr = child.getAttribute("xpath");

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
					string refName = child.getAttribute("ref");
					try
					{
						test.agents.Add(refName, parent.agents[refName]);
					}
					catch
					{
						throw new PeachException("Error, Test::" + test.name + " Agent name in ref attribute not found");
					}

					var platform = child.getAttribute("platform");
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
					if (!child.hasAttribute("ref"))
						throw new PeachException("Error, StateModel element must have a 'ref' attribute when used as a child of Test");

					try
					{
						test.stateModel = parent.stateModels[child.getAttribute("ref")];
					}
					catch
					{
						throw new PeachException("Error, could not locate StateModel named '" +
							child.getAttribute("ref") + "' for Test '" + test.name + "'.");
					}
				}

				// Publisher
				if (child.Name == "Publisher")
				{
					string name;
					if (!child.hasAttribute("name"))
					{
						name = "Pub_" + _uniquePublisherName;
						_uniquePublisherName++;
					}
					else
						name = child.getAttribute("name");

					test.publishers.Add(name, handlePlugin<Publisher, PublisherAttribute>(child, null, false));
				}

				// Mutator
				if (child.Name == "Mutators")
				{
					string mode = child.getAttribute("mode");
					if (mode == null)
						throw new PeachException("Error, Mutators element must have a 'mode' attribute");

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
							throw new PeachException("Error, Mutators element has invalid 'mode' attribute '{0}'", mode);
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

		protected void validateParameterAttributes(string type, string name, IEnumerable<ParameterAttribute> publisherParameters,
			Dictionary<string, Variant> xmlParameters)
		{
			foreach (ParameterAttribute p in publisherParameters)
			{
				if (p.required)
				{
					if (!xmlParameters.ContainsKey(p.name))
						throw new PeachException(
							string.Format("Error, {0} '{1}' is missing required parameter '{2}'.\n{3}",
								type, name, p.name, formatParameterAttributes(publisherParameters)));
				}
			}

			bool found = false;
			foreach (string p in xmlParameters.Keys)
			{
				found = false;

				foreach (ParameterAttribute pa in publisherParameters)
				{
					if (pa.name == p)
					{
						found = true;
						break;
					}
				}

				if (!found)
					throw new PeachException(string.Format("Error, {0} '{1}' has unknown parameter '{2}'.\n{3}",
						type, name, p, formatParameterAttributes(publisherParameters)));
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

				string name = child.getAttribute("name");
				string value = child.getAttribute("value");

				if (child.hasAttribute("valueType"))
				{
					ret.Add(name, new Variant(value, child.getAttribute("valueType")));
				}
				else
				{
					ret.Add(name, new Variant(value));
				}
				//throw new NotImplementedException("TODO Handle ValueType");

			}

			return ret;
		}

		#endregion
	}
}

// end
