
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

namespace Peach.Core.Analyzers
{
	/// <summary>
	/// This is the default analyzer for Peach.  It will
	/// parse a Peach PIT file (XML document) into a Peach DOM.
	/// </summary>
	public class PitParser : Analyzer
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		static int ErrorsCount = 0;
		static string ErrorMessage = "";
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

        public override Dom.Dom asParser(Dictionary<string, string> args, Stream data)
        {
            return asParser(args,data,true);
        }
        public Dom.Dom asParser(Dictionary<string, string> args, Stream data, bool doValidatePit)
		{
			
            if(doValidatePit)
			    validatePit(data);

			data.Seek(0, SeekOrigin.Begin);
			XmlDocument xmldoc = new XmlDocument();
			xmldoc.Load(data);

			_dom = new Dom.Dom();

			foreach(XmlNode child in xmldoc.ChildNodes)
			{
				if (child.Name == "Peach")
				{
					handlePeach(child, _dom);
					break;
				}
			}

			return _dom;
		}

		public override void asParserValidation(Dictionary<string, string> args, Stream data)
		{
			validatePit(data);
		}

		static protected void populateDataElementPitParsable()
		{
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type t in a.GetExportedTypes())
				{
					if (!t.IsClass)
						continue;

					foreach (object attrib in t.GetCustomAttributes(true))
					{
						if (attrib is PitParsableAttribute)
							dataElementPitParsable[((PitParsableAttribute)attrib).xmlElementName] = t;
					}
				}
			}
		}

		/// <summary>
		/// Validate PIT XML using Schema file.
		/// </summary>
		/// <param name="fileName">Pit file to validate</param>
		/// <param name="schema">Peach XML Schema file</param>
		public void validatePit(Stream data)
		{
			// Right now XSD validation is disabled on Mono :(
			Type t = Type.GetType("Mono.Runtime");
			if (t != null)
				return;

			XmlTextReader tr = new XmlTextReader(
				Assembly.GetExecutingAssembly().GetManifestResourceStream("Peach.Core.peach.xsd"));
			XmlSchemaSet set = new XmlSchemaSet();
			set.Add(null, tr);

			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			settings.Schemas = set;
			settings.ValidationType = ValidationType.Schema;
			settings.ValidationEventHandler += new ValidationEventHandler(vr_ValidationEventHandler);

			XmlReader xmlFile = XmlTextReader.Create(data, settings);

			while (xmlFile.Read()) ;
			xmlFile.Close();

			if (ErrorsCount > 0)
				throw new PeachException("Error: Pit file failed to validate: " + ErrorMessage);
		}

		/// <summary>
		/// Called when the Schema validator hits an error.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void vr_ValidationEventHandler(object sender, ValidationEventArgs e)
		{
			ErrorMessage = ErrorMessage + e.Message + "\r\n";
			ErrorsCount++;
		}

		/// <summary>
		/// Handle parsing the top level Peach node.
		/// </summary>
		/// <param name="node">XmlNode to parse</param>
		/// <param name="dom">DOM to fill</param>
		/// <returns>Returns the parsed Dom object.</returns>
		protected Dom.Dom handlePeach(XmlNode node, Dom.Dom dom)
		{
			// Pass 1 - Handle imports, includes, python path

			foreach (XmlNode child in node)
			{
				switch (child.Name)
				{
					case "Include":
						string ns = getXmlAttribute(child, "ns");
						string fileName = getXmlAttribute(child, "src");
						fileName = fileName.Replace("file:", "");

						PitParser parser = new PitParser();
						if (!File.Exists(fileName))
						{
							string newFileName = Path.Combine(
								Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
								fileName);

							if (!File.Exists(newFileName))
							{
								Console.WriteLine(newFileName);
								throw new PeachException("Error: Unable to locate Pit file [" + fileName + "].\n");
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
						Scripting.Imports.Add(getXmlAttribute(child, "require"));
						break;

					case "Import":
						if (hasXmlAttribute(child, "from"))
							throw new PeachException("Error, This version of Peach does not support the 'from' attribute for 'Import' elements.");

						Scripting.Imports.Add(getXmlAttribute(child, "import"));
						break;

					case "PythonPath":
						if (isScriptingLanguageSet && 
							Scripting.DefaultScriptingEngine != ScriptingEngines.Python)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Python;
						Scripting.Paths.Add(getXmlAttribute(child, "import"));
						isScriptingLanguageSet = true;
						break;

					case "RubyPath":
						if (isScriptingLanguageSet && 
							Scripting.DefaultScriptingEngine != ScriptingEngines.Ruby)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Ruby;
						Scripting.Paths.Add(getXmlAttribute(child, "require"));
						isScriptingLanguageSet = true;
						break;

					case "Python":
						if (isScriptingLanguageSet && 
							Scripting.DefaultScriptingEngine != ScriptingEngines.Python)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Python;
						Scripting.Exec(getXmlAttribute(child, "code"), new Dictionary<string,object>());
						isScriptingLanguageSet = true;
						break;

					case "Ruby":
						if (isScriptingLanguageSet && 
							Scripting.DefaultScriptingEngine != ScriptingEngines.Ruby)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Ruby;
						Scripting.Exec(getXmlAttribute(child, "code"), new Dictionary<string, object>());
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

			// Pass 7 - Handle Run

			foreach (XmlNode child in node)
			{
				if (child.Name == "Run")
				{
					Run run = handleRun(child, dom);
					dom.runs.Add(run.name, run);
				}
			}

			return dom;
		}

		#region Utility Methods

		/// <summary>
		/// Get attribute from XmlNode object.
		/// </summary>
		/// <param name="node">XmlNode to get attribute from</param>
		/// <param name="name">Name of attribute</param>
		/// <returns>Returns innerText or null.</returns>
		public string getXmlAttribute(XmlNode node, string name)
		{
			try
			{
				return node.Attributes[name].InnerText;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Get attribute from XmlNode object.
		/// </summary>
		/// <param name="node">XmlNode to get attribute from</param>
		/// <param name="name">Name of attribute</param>
		/// <param name="defaultValue">Default value if attribute is missing</param>
		/// <returns>Returns true/false or default value</returns>
		public bool getXmlAttributeAsBool(XmlNode node, string name, bool defaultValue)
		{
			try
			{
				string value = node.Attributes[name].InnerText.ToLower();
				switch (value)
				{
					case "1":
					case "true":
						return true;
					case "0":
					case "false":
						return false;
					default:
						throw new PeachException("Error, " + name + " has unknown value, should be boolean.");
				}
			}
			catch
			{
				return defaultValue;
			}
		}

		/// <summary>
		/// Check to see if XmlNode has specific attribute.
		/// </summary>
		/// <param name="node">XmlNode to check</param>
		/// <param name="name">Name of attribute</param>
		/// <returns>Returns boolean true or false.</returns>
		public bool hasXmlAttribute(XmlNode node, string name)
		{
			try
			{
				object o = node.Attributes.GetNamedItem(name);
				return o != null;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Resolve a 'ref' attribute.  Will throw a PeachException if
		/// namespace is given, but not found.
		/// </summary>
		/// <param name="dom">DOM to use for resolving ref.</param>
		/// <param name="name">Ref name to resolve.</param>
		/// <returns>DataElement for ref or null if not found.</returns>
		public DataElement getReference(Dom.Dom dom, string name, DataElementContainer container)
		{
			if (name.IndexOf(':') > -1)
			{
				string ns = name.Substring(0, name.IndexOf(':') - 1);

				if (!dom.ns.Keys.Contains(ns))
					throw new PeachException("Unable to locate namespace '"+ns+"' in ref '"+name+"'.");

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
				logger.Debug("finalUpdateRelations: DataModel: " + model.name);

				foreach (DataElement elem in model.EnumerateAllElements())
				{
					logger.Debug("finalUpdateRelations: " + elem.fullName);

					foreach (Relation rel in elem.relations)
					{
						logger.Debug("finalUpdateRelations: Relation " + rel.GetType().Name);

						if (rel.From == elem)
						{
							DataElement of = rel.Of;
							if (of == null)
								throw new PeachException("Error, unable to resolve '" +
									rel.OfName + "' from relation attached to '" + elem.fullName + "'.");

							if (!of.relations.Contains(rel))
								of.relations.Add(rel);
						}
						else if (rel.Of == elem)
						{
							DataElement from = rel.From;
							if (from == null)
								throw new PeachException("Error, unable to resolve '" +
									rel.OfName + "' from relation attached to '" + elem.fullName + "'.");

							if (!from.relations.Contains(rel))
								from.relations.Add(rel);
						}
						else
						{
							logger.Debug("finalUpdateRelations: From/Of don't be a matching our element");
							throw new PeachException("Error, relation attached to element \"" + elem.fullName + "\" is not resolving correctly.");
						}
					}
				}
			}
		}

		protected void handleDefaults(XmlNode node)
		{
			foreach (XmlNode child in node.ChildNodes)
			{
				Dictionary<string, string> args = new Dictionary<string, string>();

				switch (child.Name)
				{
					case "Number":
						if (hasXmlAttribute(child, "endian"))
							args["endian"] = getXmlAttribute(child, "endian");
						if (hasXmlAttribute(child, "signed"))
							args["signed"] = getXmlAttribute(child, "signed");

						dataElementDefaults[typeof(Number)] = args;
						break;
					case "String":
						if (hasXmlAttribute(child, "lengthType"))
							args["lengthType"] = getXmlAttribute(child, "lengthType");
						if (hasXmlAttribute(child, "padCharacter"))
							args["padCharacter"] = getXmlAttribute(child, "padCharacter");
						if (hasXmlAttribute(child, "type"))
							args["type"] = getXmlAttribute(child, "type");
						if (hasXmlAttribute(child, "nullTerminated"))
							args["nullTerminated"] = getXmlAttribute(child, "nullTerminated");

						dataElementDefaults[typeof(Dom.String)] = args;
						break;
					case "Flags":
						if (hasXmlAttribute(child, "endian"))
							args["endian"] = getXmlAttribute(child, "endian");
						if(hasXmlAttribute(child, "size"))
							args["size"] = getXmlAttribute(child, "size");

						dataElementDefaults[typeof(Flags)] = args;
						break;
					case "Blob":
						if (hasXmlAttribute(child, "lengthType"))
							args["lengthType"] = getXmlAttribute(child, "lengthType");

						dataElementDefaults[typeof(Blob)] = args;
						break;
					default:
						throw new PeachException("Error, defaults not supported for '" + child.Name + "'.");
				}
			}
		}

		protected Dom.Agent handleAgent(XmlNode node)
		{
			Dom.Agent agent = new Dom.Agent();

			agent.name = getXmlAttribute(node, "name");
			agent.url = getXmlAttribute(node, "location");
			agent.password = getXmlAttribute(node, "password");
			
			if (agent.url == null)
				agent.url = "local://";

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Monitor")
				{
					Dom.Monitor monitor = new Monitor();

					monitor.cls = getXmlAttribute(child, "class");
					monitor.parameters = handleParams(child);

					agent.monitors.Add(monitor);
				}
			}

			return agent;
		}

		#region Data Model

		protected DataModel handleDataModel(XmlNode node)
		{
			var dataModel = new DataModel();

			if (hasXmlAttribute(node, "ref"))
			{
				DataModel refObj = getReference(_dom, getXmlAttribute(node, "ref"), null) as DataModel;
				if (refObj != null)
				{
					string name = dataModel.name;
					dataModel = ObjectCopier.Clone<DataModel>(refObj);
					dataModel.name = name;
					dataModel.isReference = true;
				}
				else
				{
					throw new PeachException("Unable to locate 'ref' [" + getXmlAttribute(node, "ref") + "] or found node did not match type. [" + node.OuterXml + "].");
				}

				if (!hasXmlAttribute(node, "name"))
					dataModel.name = getXmlAttribute(node, "ref");
				else
					dataModel.name = getXmlAttribute(node, "name");
			}
			else
			{
				dataModel.name = getXmlAttribute(node, "name");
				if (dataModel.name == null)
					throw new PeachException("Error, DataModel missing required 'name' attribute.");

			}

			handleCommonDataElementAttributes(node, dataModel);
			handleCommonDataElementChildren(node, dataModel);
			handleDataElementContainer(node, dataModel);

			return dataModel;
		}

		protected Dom.Array handleArray(XmlNode node, DataElementContainer parent)
		{
			return (Dom.Array) Dom.Array.PitParser(this, node, parent);
		}

		protected bool IsArray(XmlNode node)
		{
			if (hasXmlAttribute(node, "minOccurs") || hasXmlAttribute(node, "maxOccurs") || hasXmlAttribute(node, "occurs"))
				return true;

			return false;
		}

		protected Block handleBlock(XmlNode node, DataElementContainer parent)
		{
			return (Block) Block.PitParser(this, node, parent);
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
			if (hasXmlAttribute(node, "token"))
				element.isToken = true;
			
			if (hasXmlAttribute(node, "mutable"))
				element.isMutable = false;

			if (hasXmlAttribute(node, "constraint"))
				element.constraint = getXmlAttribute(node, "constraint");

			if (hasXmlAttribute(node, "pointer"))
				throw new NotSupportedException("Implement pointer attribute");
			
			if (hasXmlAttribute(node, "pointerDepth"))
				throw new NotSupportedException("Implement pointerDepth attribute");

			if (hasXmlAttribute(node, "lengthType"))
			{
				switch (getXmlAttribute(node, "lengthType"))
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
							"', unknown value: '" + getXmlAttribute(node, "lengthType") + "'.");
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

			if (hasXmlAttribute(node, "length"))
			{
				try
				{
					element.length = Int32.Parse(getXmlAttribute(node, "length"));
				}
				catch (Exception e)
				{
					throw new PeachException("Error, parsing length on '" + element.name + "': " + e.Message);
				}
			}

			element.lengthCalc = getXmlAttribute(node, "lengthCalc");
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
						element.fixup = handleFixup(child, element);
						break;

					case "Transformer":
						element.transformer = handleTransformer(child, element);
						break;

					case "Hint":
						handleHint(child, element);
						break;

					case "Analyzer":
						handleAnalyzerDataElement(child, element);
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
			var hint = new Hint(getXmlAttribute(node, "name"), getXmlAttribute(node, "value"));
			element.Hints.Add(hint.Name, hint);
		}

		protected void handlePlacement(XmlNode node, DataElement element)
		{
			Dictionary<string, Variant> args = new Dictionary<string, Variant>();

			if (hasXmlAttribute(node, "after"))
				args["after"] = new Variant(getXmlAttribute(node, "after"));
			else if (hasXmlAttribute(node, "before"))
				args["before"] = new Variant(getXmlAttribute(node, "before"));
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
                    if(((IList<string>)dataElementCommon).Contains(child.Name))
                        continue;
                    else
					    throw new PeachException("Error, found unknown data element in pit file: " + child.Name);
                }

				Type dataElementType = dataElementPitParsable[child.Name];
				MethodInfo pitParsableMethod = dataElementType.GetMethod("PitParser");
				if (pitParsableMethod == null)
					throw new PeachException("Error, type with PitParsableAttribute is missing static PitParser(...) method: " + dataElementType.FullName);

				PitParserDelegate delegateAction = Delegate.CreateDelegate(typeof(PitParserDelegate), pitParsableMethod) as PitParserDelegate;

                elem = delegateAction(this, child, element);
                
                if (elem == null)
					throw new PeachException("Error, type failed to parse provided XML: " + dataElementType.FullName);

				// Wrap elements that are arrays with an Array object
				if (IsArray(child))
				{
					var array = Dom.Array.PitParser(this, child, element) as Dom.Array;
					array.Add(elem);
					array.origionalElement = elem;

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
					if (elem.name.IndexOf(".") > -1)
					{
						DataElement parent = element.find(elem.name);
						if (parent == null)
							throw new PeachException("Error, child name has dot notation but replacement element not found: '" + elem.name + ".");

						elem.name = parent.name;
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

		public void handleCommonDataElementValue(XmlNode node, DataElement elem)
		{
			string value = null;

			if (hasXmlAttribute(node, "value"))
			{
				value = getXmlAttribute(node, "value");

				value = value.Replace("\\\\", "\\");
				value = value.Replace("\\n", "\n");
				value = value.Replace("\\r", "\r");
				value = value.Replace("\\t", "\t");
			}

			if (hasXmlAttribute(node, "valueType"))
			{
				switch (getXmlAttribute(node, "valueType").ToLower())
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
						
						if (elem is Number)
						{
							if (((Number)elem).LittleEndian)
								sout.LittleEndian();
							else
								sout.BigEndian();
						}

						for (int cnt = 0; cnt < value.Length; cnt += 2)
							sout.WriteByte(Convert.ToByte(value.Substring(cnt, 2), 16));

						sout.SeekBits(0, SeekOrigin.Begin);

						if(elem is Number)
						{
							Number num = elem as Number;
							switch(num.lengthAsBits)
							{
								case 8:
									if(num.Signed)
										elem.DefaultValue = new Variant(sout.ReadInt8());
									else
										elem.DefaultValue = new Variant(sout.ReadUInt8());
									break;
								case 16:
									if(num.Signed)
										elem.DefaultValue = new Variant(sout.ReadInt16());
									else
										elem.DefaultValue = new Variant(sout.ReadUInt16());
									break;
								case 32:
									if(num.Signed)
										elem.DefaultValue = new Variant(sout.ReadInt32());
									else
										elem.DefaultValue = new Variant(sout.ReadUInt32());
									break;
								case 64:
									if(num.Signed)
										elem.DefaultValue = new Variant(sout.ReadInt64());
									else
										elem.DefaultValue = new Variant(sout.ReadUInt64());
									break;
								default:
									throw new NotImplementedException("todo, variable bit Numbers");
							}
						}
						else
							elem.DefaultValue = new Variant(sout);

						break;
					case "literal":
						throw new NotImplementedException("todo valueType");
					case "string":
						// No action requried, default behaviour
						elem.DefaultValue = new Variant(value);
						break;
					default:
						throw new PeachException("Error, invalid value for 'valueType' attribute: " + getXmlAttribute(node, "valueType"));
				}
			}
			else if(value != null)
				elem.DefaultValue = new Variant(value);

		}

		public bool hasDefaultAttribute(Type type, string key)
		{
			if(dataElementDefaults.ContainsKey(type))
				return dataElementDefaults[type].ContainsKey(key);
			return false;
		}

		public string getDefaultAttribute(Type type, string key)
		{
			return dataElementDefaults[type][key];
		}

		public bool getDefaultAttributeAsBool(Type type, string key, bool defaultValue)
		{
			try
			{
				string value = dataElementDefaults[type][key].ToLower();
				switch (value)
				{
					case "1":
					case "true":
						return true;
					case "0":
					case "false":
						return false;
					default:
						throw new PeachException("Error, " + key + " has unknown value, should be boolean.");
				}
			}
			catch
			{
				return defaultValue;
			}
		}

		protected void handleRelation(XmlNode node, DataElement parent)
		{
			switch (getXmlAttribute(node, "type"))
			{
				case "size":
					if (hasXmlAttribute(node, "of"))
					{
						SizeRelation rel = new SizeRelation();
						rel.OfName = getXmlAttribute(node, "of");
						
						if(hasXmlAttribute(node, "expressionGet"))
							rel.ExpressionGet = getXmlAttribute(node, "expressionGet");

						if(hasXmlAttribute(node, "expressionSet"))
							rel.ExpressionSet = getXmlAttribute(node, "expressionSet");
						
						parent.relations.Add(rel);
					}

					break;
				
				case "count":
					if (hasXmlAttribute(node, "of"))
					{
						CountRelation rel = new CountRelation();
						rel.OfName = getXmlAttribute(node, "of");

						if (hasXmlAttribute(node, "expressionGet"))
							rel.ExpressionGet = getXmlAttribute(node, "expressionGet");

						if (hasXmlAttribute(node, "expressionSet"))
							rel.ExpressionSet = getXmlAttribute(node, "expressionSet");

						parent.relations.Add(rel);
					}
					break;
				
				case "offset":
					if (hasXmlAttribute(node, "of"))
					{
						OffsetRelation rel = new OffsetRelation();
						rel.OfName = getXmlAttribute(node, "of");

						if (hasXmlAttribute(node, "expressionGet"))
							rel.ExpressionGet = getXmlAttribute(node, "expressionGet");

						if (hasXmlAttribute(node, "expressionSet"))
							rel.ExpressionSet = getXmlAttribute(node, "expressionSet");

						if (hasXmlAttribute(node, "relative"))
							rel.isRelativeOffset = true;

						if (hasXmlAttribute(node, "relativeTo"))
						{
							rel.isRelativeOffset = true;
							rel.relativeTo = getXmlAttribute(node, "relativeTo");
						}

						parent.relations.Add(rel);
					}
					break;
				
				default:
					throw new ApplicationException("Unknown relation type found '"+
						getXmlAttribute(node, "type")+"'.");
			}
		}

		protected Analyzer handleAnalyzerDataElement(XmlNode node, DataElement parent)
		{
			if (!hasXmlAttribute(node, "class"))
				throw new PeachException("Analyzer element has no 'class' attribute [" + node.OuterXml + "].");

			string cls = getXmlAttribute(node, "class");
			Type tFixup = null;
			var arg = handleParams(node);
			List<ParameterAttribute> parameters = new List<ParameterAttribute>();

			// Locate PublisherAttribute classes and check name
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type t in a.GetExportedTypes())
				{
					if (!t.IsClass)
						continue;

					parameters.Clear();

					foreach (object attrib in t.GetCustomAttributes(true))
					{
						if (attrib is AnalyzerAttribute && (attrib as AnalyzerAttribute).invokeName == cls)
						{
							tFixup = t;
						}
						else if (attrib is ParameterAttribute)
							parameters.Add(attrib as ParameterAttribute);
					}

					if (tFixup != null)
						break;
				}

				if (tFixup != null)
					break;
			}

			if (tFixup == null)
				throw new PeachException("Error, unable to locate Analyzer named '" + cls + "'.");

			validateParameterAttributes("Analyzer", cls, parameters, arg);

			Type[] targs = new Type[1];
			targs[0] = typeof(Dictionary<string, Variant>);

			ConstructorInfo co = tFixup.GetConstructor(targs);

			if (co == null)
				throw new PeachException("Error, unable to locate Analyzer named '" + cls + "'.\nExtended error: Was unable to find correct constructor.");

			object[] args = new object[1];
			args[0] = arg;

			try
			{
				parent.analyzer = co.Invoke(args) as Analyzer;
			}
			catch (Exception e)
			{
				throw new PeachException("Error, unable to locate Analyzer named '" + cls + "'.\nExtended error: Exception during object creation: " + e.Message);
			}

			return parent.analyzer;
		}

		protected MutationStrategy handleMutationStrategy(XmlNode node)
		{
			if (!hasXmlAttribute(node, "class"))
				throw new PeachException("Strategy element has no 'class' attribute [" + node.OuterXml + "].");

			string cls = getXmlAttribute(node, "class");
			Type tFixup = null;
			var arg = handleParams(node);
			List<ParameterAttribute> parameters = new List<ParameterAttribute>();

			// Locate PublisherAttribute classes and check name
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type t in a.GetExportedTypes())
				{
					if (!t.IsClass)
						continue;

					parameters.Clear();

					foreach (object attrib in t.GetCustomAttributes(true))
					{
						if (attrib is MutationStrategyAttribute && (attrib as MutationStrategyAttribute).name == cls)
						{
							tFixup = t;
						}
						else if (attrib is ParameterAttribute)
							parameters.Add(attrib as ParameterAttribute);
					}

					if (tFixup != null)
						break;
				}

				if (tFixup != null)
					break;
			}

			if (tFixup == null)
				throw new PeachException("Error, unable to locate Strategy named '" + cls + "'.");

			validateParameterAttributes("MutationStrategy", cls, parameters, arg);

			Type[] targs = new Type[1];
			targs[0] = typeof(Dictionary<string, Variant>);

			ConstructorInfo co = tFixup.GetConstructor(targs);

			if (co == null)
				throw new PeachException("Error, unable to locate MutationStrategy named '" + cls + "'.\nExtended error: Was unable to find correct constructor.");

			object[] args = new object[1];
			args[0] = arg;

			try
			{
				return co.Invoke(args) as MutationStrategy;
			}
			catch (Exception e)
			{
				throw new PeachException("Error, unable to locate MutationStrategy named '" + cls + "'.\nExtended error: Exception during object creation: " + e.Message);
			}
		}

		protected Fixup handleFixup(XmlNode node, DataElement parent)
		{
			if (!hasXmlAttribute(node, "class"))
				throw new PeachException("Fixup element has no 'class' attribute [" + node.OuterXml + "].");

			string cls = getXmlAttribute(node, "class");
			Type tFixup = null;
			var arg = handleParams(node);
			List<ParameterAttribute> parameters = new List<ParameterAttribute>();

			// Locate PublisherAttribute classes and check name
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type t in a.GetExportedTypes())
				{
					if (!t.IsClass)
						continue;

					parameters.Clear();

					foreach (object attrib in t.GetCustomAttributes(true))
					{
						if (attrib is FixupAttribute && (attrib as FixupAttribute).className == cls)
						{
							tFixup = t;
						}
						else if (attrib is ParameterAttribute)
							parameters.Add(attrib as ParameterAttribute);
					}

					if (tFixup != null)
						break;
				}

				if (tFixup != null)
					break;
			}

			if (tFixup == null)
				throw new PeachException("Error, unable to locate Fixup named '" + cls + "'.");

			validateParameterAttributes("Fixup", cls, parameters, arg);

			Type[] targs = new Type[1];
			targs[0] = typeof(Dictionary<string, Variant>);

			ConstructorInfo co = tFixup.GetConstructor(targs);

			if (co == null)
				throw new PeachException("Error, unable to locate Fixup named '" + cls + "'.\nExtended error: Was unable to find correct constructor.");

			object[] args = new object[1];
			args[0] = arg;

			try
			{
				parent.fixup = co.Invoke(args) as Fixup;
			}
			catch (Exception e)
			{
				throw new PeachException("Error, unable to locate Fixup named '" + cls + "'.\nExtended error: Exception during object creation: " + e.Message);
			}

			return parent.fixup;


		}

		protected Transformer handleTransformer(XmlNode node, DataElement parent)
		{
			string cls = getXmlAttribute(node, "class");
			Type tTransformer = null;
			var arg = handleParams(node);
			List<ParameterAttribute> parameters = new List<ParameterAttribute>();

			// Locate PublisherAttribute classes and check name
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type t in a.GetExportedTypes())
				{
					if (!t.IsClass)
						continue;

					parameters.Clear();

					foreach (object attrib in t.GetCustomAttributes(true))
					{
						if (attrib is TransformerAttribute && (attrib as TransformerAttribute).elementName == cls)
							tTransformer = t;
						else if (attrib is ParameterAttribute)
							parameters.Add(attrib as ParameterAttribute);
					}

					if (tTransformer != null)
						break;
				}

				if (tTransformer != null)
					break;
			}

			if (tTransformer == null)
				throw new PeachException("Error, unable to locate Transformer named '" + cls + "'.");

			validateParameterAttributes("Transformer", cls, parameters, arg);

			Type[] targs = new Type[1];
			targs[0] = typeof(Dictionary<string, Variant>);

			ConstructorInfo co = tTransformer.GetConstructor(targs);

			object[] args = new object[1];
			args[0] = arg;

			parent.transformer = co.Invoke(args) as Transformer;

			return parent.transformer;
		}

#endregion

		#region State Model

		protected StateModel handleStateModel(XmlNode node, Dom.Dom parent)
		{
			string name = getXmlAttribute(node, "name");
			string initialState = getXmlAttribute(node, "initialState");
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

		protected State handleState(XmlNode node, StateModel parent)
		{
			State state = new State();
			state.parent = parent;
			state.name = getXmlAttribute(node, "name");

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

		protected Core.Dom.Action handleAction(XmlNode node, State parent)
		{
			Core.Dom.Action action = new Core.Dom.Action();
			action.parent = parent;

			if (hasXmlAttribute(node, "name"))
				action.name = getXmlAttribute(node, "name");

			if (hasXmlAttribute(node, "when"))
				action.when = getXmlAttribute(node, "when");

			if (hasXmlAttribute(node, "publisher"))
				action.publisher = getXmlAttribute(node, "publisher");

			if (hasXmlAttribute(node, "type"))
			{
				switch (getXmlAttribute(node, "type").ToLower())
				{
					case "accept":
						action.type = ActionType.Accept;
						break;
					case "call":
						action.type = ActionType.Call;
						break;
					case "changeState":
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
						throw new PeachException("Error, action of type '" + getXmlAttribute(node, "type") + "' is not valid.");
				}
			}

			if (hasXmlAttribute(node, "onStart"))
				action.onStart = getXmlAttribute(node, "onStart");

			if (hasXmlAttribute(node, "onComplete"))
				action.onComplete = getXmlAttribute(node, "onComplete");

			if (hasXmlAttribute(node, "ref"))
			{
				if (action.type == ActionType.ChangeState)
					action.name = getXmlAttribute(node, "ref");
				else
					throw new PeachException("Error, only Actions of type ChangeState are allowed to use the 'ref' attribute");
			}

			if (hasXmlAttribute(node, "method"))
			{
				if (action.type != ActionType.Call)
					throw new PeachException("Error, only Actions of type Call are allowed to use the 'method' attribute");

				action.method = getXmlAttribute(node, "method");
			}

			if (hasXmlAttribute(node, "property"))
			{
				if (action.type != ActionType.GetProperty && action.type != ActionType.SetProperty)
					throw new PeachException("Error, only Actions of type GetProperty and SetProperty are allowed to use the 'property' attribute");

				action.property = getXmlAttribute(node, "property");
			}

			if (hasXmlAttribute(node, "setXpath"))
			{
				if (action.type != ActionType.Slurp)
					throw new PeachException("Error, only Actions of type Slurp are allowed to use the 'setXpath' attribute");

				action.setXpath = getXmlAttribute(node, "setXpath");
			}

			if (hasXmlAttribute(node, "valueXpath"))
			{
				if (action.type != ActionType.Slurp)
					throw new PeachException("Error, only Actions of type Slurp are allowed to use the 'valueXpath' attribute");

				action.valueXpath = getXmlAttribute(node, "valueXpath");
			}

			//if (hasXmlAttribute(node, "value"))
			//{
			//    if (action.type != ActionType.Slurp)
			//        throw new PeachException("Error, only Actions of type Slurp are allowed to use the 'value' attribute");

			//    action.value = getXmlAttribute(node, "value");
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
					// TODO - Expand support
					action.dataSet = new DataSet();
					action.dataSet.Datas.Add(handleData(child));
				}
			}

			// Still old way todo things.
			if (action.dataModel != null && action.dataSet != null &&
				action.dataSet.Datas.Count > 0 &&
				(action.dataSet.Datas[0].FileName != null || action.dataSet.Datas[0].Files.Count>0))
			{
				string fileName = null;
				if(action.dataSet.Datas[0].FileName != null)
					fileName = action.dataSet.Datas[0].FileName;
				else
					fileName = action.dataSet.Datas[0].Files[0];

				// update origionalDataModel
				if (action.origionalDataModel != null)
					action.origionalDataModel = ObjectCopier.Clone<DataModel>(action.dataModel);
			}

			return action;
		}

		protected ActionParameter handleActionParameter(XmlNode node, Dom.Action parent)
		{
			ActionParameter param = new ActionParameter();
			Dom.Dom dom = parent.parent.parent.parent as Dom.Dom;

			foreach (XmlNode child in node.ChildNodes)
			{
				if(child.Name == "DataModel")
					param.dataModel = dom.dataModels[getXmlAttribute(child, "ref")];
				if (child.Name == "Data")
					param.data = handleData(child);
			}

			return param;
		}

		protected Data handleData(XmlNode node)
		{
			Data data = new Data();
			data.name = getXmlAttribute(node, "name");
			string dataFileName = getXmlAttribute(node, "fileName");

			if (Directory.Exists(dataFileName))
			{
				List<string> files = new List<string>();
				foreach (string fileName in Directory.GetFiles(dataFileName))
					files.Add(fileName);

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

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Field")
				{
					data.DataType = DataType.Fields;
					// Hack to call common value parsing code.
					Blob tmp = new Blob();
					handleCommonDataElementValue(child, tmp);

					data.fields.Add(getXmlAttribute(child,"name"), tmp.DefaultValue);
				}
			}

			return data;
		}

		protected Test handleTest(XmlNode node, Dom.Dom parent)
		{
			Test test = new Test();
			test.parent = parent;

			test.name = getXmlAttribute(node, "name");

			foreach (XmlNode child in node.ChildNodes)
			{
				// Include
				if (child.Name == "Include")
				{
					throw new NotImplementedException("Test.Include TODO");
				}

				// Exclude
				if (child.Name == "Exclude")
				{
					throw new NotImplementedException("Test.Exclude TODO");
				}

				// Strategy
				if (child.Name == "Strategy")
				{
					test.strategy = handleMutationStrategy(child);
				}

				// Agent
				if (child.Name == "Agent")
				{
					string refName = getXmlAttribute(child, "ref");
					test.agents.Add(refName, parent.agents[refName]);

					var platform = getXmlAttribute(child, "platform");
					if (platform != null)
					{
						switch (platform.ToLower())
						{
							case "windows":
								parent.agents[refName].platform = Platform.OS.Windows;
								break;
							case "osx":
								parent.agents[refName].platform = Platform.OS.Mac;
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
					if (!hasXmlAttribute(child, "ref"))
						throw new PeachException("Error, StateModel element must have a 'ref' attribute when used as a child of Test");

					try
					{
						test.stateModel = parent.stateModels[getXmlAttribute(child, "ref")];
					}
					catch
					{
						throw new PeachException("Error, could not locate StateModel named '" + 
							getXmlAttribute(child, "ref") + "' for Test '" + test.name + "'.");
					}
				}

				// Publisher
				if (child.Name == "Publisher")
				{
					string name;
					if (!hasXmlAttribute(child, "name"))
					{
						name = "Pub_" + _uniquePublisherName;
						_uniquePublisherName++;
					}
					else
						name = getXmlAttribute(child, "name");

					test.publishers.Add(name, handlePublisher(child, test));
				}

				// Mutator
				if (child.Name == "Mutator")
				{
					throw new NotImplementedException("Test.Mutator TODO");
				}
			}

			if (test.stateModel == null)
				throw new PeachException("Test '" + test.name + "' missing StateModel element.");
			if(test.publishers.Count == 0)
				throw new PeachException("Test '" + test.name + "' missing Publisher element.");

			if (test.strategy == null)
			{
				// Locate and load default strategy.
				foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (Type t in a.GetExportedTypes())
					{
						if (!t.IsClass)
							continue;

						foreach (object attrib in t.GetCustomAttributes(true))
						{
							if (attrib is DefaultMutationStrategyAttribute)
							{
								Type[] argTypes = new Type[1];
								argTypes[0] = typeof(Dictionary<string, Variant>);
								ConstructorInfo strategyCo = t.GetConstructor(argTypes);

								object[] args = new object[1];
								args[0] = new Dictionary<string, Variant>();

								test.strategy = strategyCo.Invoke(args) as MutationStrategy;
							}
						}
					}
				}

			}

			return test;
		}

		public static uint _uniquePublisherName = 0;

		protected Publisher handlePublisher(XmlNode node, Test parent)
		{
			string reference = getXmlAttribute(node, "class");
			Type pubType = null;
			List<ParameterAttribute> publisherParameters = new List<ParameterAttribute>();

			// Locate PublisherAttribute classes and check name
			foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach(Type t in a.GetExportedTypes())
				{
					if (!t.IsClass)
						continue;

					publisherParameters.Clear();

					foreach (object attrib in t.GetCustomAttributes(true))
					{
						if (attrib is PublisherAttribute && (attrib as PublisherAttribute).invokeName == reference)
						{
								pubType = t;
						}
						else if (attrib is ParameterAttribute)
						{
							publisherParameters.Add(attrib as ParameterAttribute);
						}
					}

					if (pubType != null)
						break;
				}

				if (pubType != null)
					break;
			}

			if (pubType == null)
				throw new PeachException("Error, unable to locate publisher '" + reference + "'.");

			Type[] argTypes = new Type[1];
			argTypes[0] = typeof(Dictionary<string, Variant>);
			ConstructorInfo pubCo = pubType.GetConstructor(argTypes);

			// Validate parameters
			var xmlParams = handleParams(node);
			validateParameterAttributes("Publisher", reference, publisherParameters, xmlParams);

			// Create instance of publisher
			object [] args = new object[1];
			args[0] = xmlParams;

			Publisher pub = pubCo.Invoke(args) as Publisher;

			return pub;
		}

		protected void validateParameterAttributes(string type, string name, List<ParameterAttribute> publisherParameters,
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

				if(!found)
					throw new PeachException(string.Format("Error, {0} '{1}' has unknown parameter '{2}'.\n{3}",
						type, name, p, formatParameterAttributes(publisherParameters)));
			}
		}

		protected string formatParameterAttributes(List<ParameterAttribute> publisherParameters)
		{
			publisherParameters.Reverse();

			string s = "\nSupported Parameters:\n\n";
			foreach (var p in publisherParameters)
			{
				if(p.required)
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

				string name = getXmlAttribute(child, "name");
				string value = getXmlAttribute(child, "value");

				if (hasXmlAttribute(child, "valueType"))
					throw new NotImplementedException("TODO Handle ValueType");

				ret.Add(name, new Variant(value));
			}

			return ret;
		}

		protected Run handleRun(XmlNode node, Dom.Dom parent)
		{
			Run run = new Run();
			run.name = getXmlAttribute(node, "name");
			run.parent = parent;

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Logger")
					run.logger = handleLogger(child);

				if (child.Name == "Test")
				{
					Test test = parent.tests[getXmlAttribute(child, "ref")];
					run.tests.Add(test.name, test);
				}
			}

			return run;
		}

		protected Logger handleLogger(XmlNode node)
		{
			string reference = getXmlAttribute(node, "class");
			Type pubType = null;
			var arg = handleParams(node);
			List<ParameterAttribute> parameters = new List<ParameterAttribute>();

			// Locate PublisherAttribute classes and check name
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type t in a.GetExportedTypes())
				{
					if (!t.IsClass)
						continue;

					parameters.Clear();

					foreach (object attrib in t.GetCustomAttributes(true))
					{
						if (attrib is LoggerAttribute && (attrib as LoggerAttribute).invokeName == reference)
							pubType = t;
						else if (attrib is ParameterAttribute)
							parameters.Add(attrib as ParameterAttribute);
					}

					if (pubType != null)
						break;
				}

				if (pubType != null)
					break;
			}

			if (pubType == null)
				throw new PeachException("Error, unable to locate logger '" + reference + "'.");

			validateParameterAttributes("Logger", reference, parameters, arg);

			Type[] argTypes = new Type[1];
			argTypes[0] = typeof(Dictionary<string, Variant>);
			ConstructorInfo pubCo = pubType.GetConstructor(argTypes);

			object[] args = new object[1];
			args[0] = arg;

			Logger logger = pubCo.Invoke(args) as Logger;

			if(logger == null)
				throw new PeachException("Error, unable to create logger '" + reference + "'.");

			return logger;
		}

		#endregion
	}
}

// end
