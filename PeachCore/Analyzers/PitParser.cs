
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
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using Peach.Core.Dom;

namespace Peach.Core.Analyzers
{
	public interface IPitParsable
	{
		// TODO: These should be static?  Need to look into it.

		/// <summary>
		/// Ask object if it can parse XmlNode.
		/// </summary>
		/// <param name="node">node to check</param>
		/// <param name="parent">parent of this object</param>
		/// <returns>Returns true if class can parse xml node.</returns>
		bool pit_canParse(XmlNode node, object parent);

		/// <summary>
		/// Called by PitParser analyzer to parse 
		/// current XML Node.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		object pit_handleNode(XmlNode node, object parent);
	}

	/// <summary>
	/// This is the default analyzer for Peach.  It will
	/// parse a Peach PIT file (XML document) into a Peach DOM.
	/// </summary>
	public class PitParser : Analyzer
	{
		static int ErrorsCount = 0;
		static string ErrorMessage = "";
		Dom.Dom _dom = null;
		bool isScriptingLanguageSet = false;

		/// <summary>
		/// Contains default attributes for DataElements
		/// </summary>
		Dictionary<Type, Dictionary<string, string>> dataElementDefaults = new Dictionary<Type, Dictionary<string, string>>();

		static PitParser()
		{
			PitParser.supportParser = true;
			Analyzer.defaultParser = new PitParser();
		}

		public PitParser()
		{
		}

		public override Dom.Dom asParser(Dictionary<string, string> args, Stream data)
		{
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

		/// <summary>
		/// Validate PIT XML using Schema file.
		/// </summary>
		/// <param name="fileName">Pit file to validate</param>
		/// <param name="schema">Peach XML Schema file</param>
		public void validatePit(Stream data)
		{
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
							string newFileName = Path.Combine(Assembly.GetExecutingAssembly().Location,
								fileName);

							if(!File.Exists(newFileName))
								throw new PeachException("Error: Unable to locate Pit file [" + fileName + "].\n");

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
					StateModel sm = handleStateModel(child);
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
		protected string getXmlAttribute(XmlNode node, string name)
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
		protected bool getXmlAttributeAsBool(XmlNode node, string name, bool defaultValue)
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
		protected bool hasXmlAttribute(XmlNode node, string name)
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
		protected DataElement getReference(Dom.Dom dom, string name, DataElementContainer container)
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
			DataModel dataModel = new DataModel();

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

		protected Block handleBlock(XmlNode node, DataElementContainer parent)
		{
			Block block = new Block();

			if (hasXmlAttribute(node, "ref"))
			{
				Block refObj = getReference(_dom, getXmlAttribute(node, "ref"), parent) as Block;
				if (refObj != null)
				{
					string name = block.name;
					block = ObjectCopier.Clone<Block>(refObj);
					block.name = name;
					block.isReference = true;
				}
				else
				{
					throw new PeachException("Unable to locate 'ref' [" + getXmlAttribute(node, "ref") + "] or found node did not match type. [" + node.OuterXml + "].");
				}
			}

			// name
			if(hasXmlAttribute(node, "name"))
				block.name = getXmlAttribute(node, "name");

			// lengthType
			if (hasXmlAttribute(node, "lengthType"))
			{
				block.lengthType = getXmlAttribute(node, "lengthType");
				block.lengthCalc = getXmlAttribute(node, "length");
				block.length = null;

				if (block.lengthType == null)
					throw new PeachException("Error, Block attribute 'lengthType' has invalid value.");
				if (block.lengthCalc == null)
					throw new PeachException("Error, When specifying lenghType=\"calc\" you must also provide a valid 'length' attribute.");
			}
			// length
			else if (hasXmlAttribute(node, "length"))
			{
				block.length = new Variant(Convert.ToInt32(getXmlAttribute(node, "length")));
			}

			// alignment

			handleCommonDataElementAttributes(node, block);
			handleCommonDataElementChildren(node, block);
			handleDataElementContainer(node, block);

			return block;
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
		protected void handleCommonDataElementAttributes(XmlNode node, DataElement element)
		{
			if (hasXmlAttribute(node, "token"))
				throw new NotSupportedException("implement token attribute");
			
			if (hasXmlAttribute(node, "mutable"))
				element.isMutable = false;

			if (hasXmlAttribute(node, "constraint"))
				element.constraint = getXmlAttribute(node, "constraint");

			if (hasXmlAttribute(node, "pointer"))
				throw new NotSupportedException("Implement pointer attribute");
			
			if (hasXmlAttribute(node, "pointerDepth"))
				throw new NotSupportedException("Implement pointerDepth attribute");
		}

		/// <summary>
		/// Handle parsing common dataelement children liek relation, fixupImpl and
		/// transformer.
		/// </summary>
		/// <param name="node">Node to read values from</param>
		/// <param name="element">Element to set values on</param>
		protected void handleCommonDataElementChildren(XmlNode node, DataElement element)
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
				}
			}
		}

		/// <summary>
		/// Handle parsing child data types into containers.
		/// </summary>
		/// <param name="node">XmlNode tor read children elements from</param>
		/// <param name="element">Element to add items to</param>
		protected void handleDataElementContainer(XmlNode node, DataElementContainer element)
		{
			foreach (XmlNode child in node.ChildNodes)
			{
				DataElement elem = null;
				switch (child.Name)
				{
					case "Block":
						elem = handleBlock(child, element);
						break;

					case "Choice":
						elem = handleChoice(child, element);
						break;

					case "String":
						elem = handleString(child, element);
						break;

					case "Number":
						elem = handleNumber(child, element);
						break;

					case "Blob":
						elem = handleBlob(child, element);
						break;

					case "Flags":
						elem = handleFlags(child, element);
						break;

					case "Custom":
						throw new NotSupportedException("Implement custom types");
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

		protected DataElement handleChoice(XmlNode node, DataElementContainer parent)
		{
			Choice choice = new Choice();

			// First name
			if (hasXmlAttribute(node, "name"))
				choice.name = getXmlAttribute(node, "name");
			
			handleCommonDataElementAttributes(node, choice);
			handleCommonDataElementChildren(node, choice);
			handleDataElementContainer(node, choice);

			// Move children to choiceElements collection
			foreach (DataElement elem in choice)
				choice.choiceElements.Add(elem.name, elem);

			choice.Clear();

			// Array
			if (hasXmlAttribute(node, "minOccurs") || hasXmlAttribute(node, "maxOccurs"))
			{
				Dom.Array array = new Dom.Array();
				array.Add(choice);
				array.name = choice.name;

				return array;
			}

			return choice;
		}

		protected Dom.String handleString(XmlNode node, DataElementContainer parent)
		{
			Dom.String str = new Dom.String();

			if (hasXmlAttribute(node, "name"))
				str.name = getXmlAttribute(node, "name");

			if (hasXmlAttribute(node, "nullTerminated"))
				str.nullTerminated = getXmlAttributeAsBool(node, "nullTerminated", false);
			else if (hasDefaultAttribute(typeof(Dom.String), "nullTerminated"))
				str.nullTerminated = getDefaultAttributeAsBool(typeof(Dom.String), "nullTerminated", false);

			if (hasXmlAttribute(node, "type"))
				throw new NotSupportedException("Implement type attribute on String");

			if (hasXmlAttribute(node, "padCharacter"))
				throw new NotSupportedException("Implement padCharacter attribute on String");

			if (hasXmlAttribute(node, "lengthType"))
			{
				switch (getXmlAttribute(node, "lengthType"))
				{
					case "calc":
						str.lengthType = LengthType.Calc;
						break;
					case "python":
						str.lengthType = LengthType.Python;
						break;
					case "ruby":
						str.lengthType = LengthType.Ruby;
						break;
					default:
						throw new PeachException("Error, parsing lengthType on String '" + str.name + "', unknown value: '" + getXmlAttribute(node, "lengthType") + "'.");
				}
			}
			else if (hasDefaultAttribute(typeof(Dom.String), "lengthType"))
			{
				switch ((string)getDefaultAttribute(typeof(Dom.String), "lengthType"))
				{
					case "calc":
						str.lengthType = LengthType.Calc;
						break;
					case "python":
						str.lengthType = LengthType.Python;
						break;
					case "ruby":
						str.lengthType = LengthType.Ruby;
						break;
					default:
						throw new PeachException("Error, parsing lengthType on String '" + str.name + "', unknown value: '" + getXmlAttribute(node, "lengthType") + "'.");
				}
			}

			if (hasXmlAttribute(node, "length"))
			{
				if (str.lengthType == LengthType.String)
				{
					try
					{
						str.length = Int32.Parse(getXmlAttribute(node, "length"));
					}
					catch (Exception e)
					{
						throw new PeachException("Error, parsing length on String '" + str.name + "': " + e.Message);
					}
				}
				else
					str.lengthOther = getXmlAttribute(node, "length");
			}

			if (hasXmlAttribute(node, "tokens")) // This item has a default!
				throw new NotSupportedException("Implement tokens attribute on String");

			if (hasXmlAttribute(node, "analyzer")) // this should be passed via a child element me things!
				throw new NotSupportedException("Implement analyzer attribute on String");

			handleCommonDataElementAttributes(node, str);
			handleCommonDataElementValue(node, str);
			handleCommonDataElementChildren(node, str);

			return str;
		}

		Regex _hexWhiteSpace = new Regex(@"[h{},\s\r\n]+", RegexOptions.Singleline);

		protected void handleCommonDataElementValue(XmlNode node, DataElement elem)
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
							sout.WriteByte(byte.Parse(value.Substring(cnt, 2)));

						sout.SeekBits(0, SeekOrigin.Begin);

						if(elem is Number)
						{
							Number num = elem as Number;
							switch(num.Size)
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

		protected Number handleNumber(XmlNode node, DataElementContainer parent)
		{
			Number num = new Number();

			if (hasXmlAttribute(node, "name"))
				num.name = getXmlAttribute(node, "name");

			if (hasXmlAttribute(node, "signed"))
				num.Signed = getXmlAttributeAsBool(node, "signed", false);
			else if(hasDefaultAttribute(typeof(Number), "signed"))
				num.Signed = getDefaultAttributeAsBool(typeof(Number), "signed", false);

			if (hasXmlAttribute(node, "size"))
			{
				int size;
				try
				{
					size = int.Parse(getXmlAttribute(node, "size"));
				}
				catch
				{
					throw new PeachException("Error, " + num.name + " size attribute is not valid number.");
				}

				if (size < 1 || size > 64)
					throw new PeachException(string.Format("Error, unsupported size {0} for element {1}.", size, num.name));

				num.Size = size;
			}

			if (hasXmlAttribute(node, "endian"))
			{
				string endian = getXmlAttribute(node, "endian").ToLower();
				switch (endian)
				{
					case "little":
						num.LittleEndian = true;
						break;
					case "big":
						num.LittleEndian = false;
						break;
					case "network":
						num.LittleEndian = false;
						break;
					default:
						throw new PeachException(
							string.Format("Error, unsupported value \"{0}\" for \"endian\" attribute on field \"{1}\".", endian, num.name));
				}
			}
			else if (hasDefaultAttribute(typeof(Number), "endian"))
			{
				string endian = ((string)getDefaultAttribute(typeof(Number), "endian")).ToLower();
				switch (endian)
				{
					case "little":
						num.LittleEndian = true;
						break;
					case "big":
						num.LittleEndian = false;
						break;
					case "network":
						num.LittleEndian = false;
						break;
					default:
						throw new PeachException(
							string.Format("Error, unsupported value \"{0}\" for \"endian\" attribute on field \"{1}\".", endian, num.name));
				}
			}

			handleCommonDataElementAttributes(node, num);
			handleCommonDataElementChildren(node, num);
			handleCommonDataElementValue(node, num);

			return num;
		}

		protected bool hasDefaultAttribute(Type type, string key)
		{
			if(dataElementDefaults.ContainsKey(type))
				return dataElementDefaults[type].ContainsKey(key);
			return false;
		}

		protected string getDefaultAttribute(Type type, string key)
		{
			return dataElementDefaults[type][key];
		}

		protected bool getDefaultAttributeAsBool(Type type, string key, bool defaultValue)
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

		protected Blob handleBlob(XmlNode node, DataElementContainer parent)
		{
			Blob blob = new Blob();

			if (hasXmlAttribute(node, "name"))
				blob.name = getXmlAttribute(node, "name");

			string type = null;

			if (hasXmlAttribute(node, "lengthType"))
				type = getXmlAttribute(node, "lengthType");

			else if(hasDefaultAttribute(typeof(Blob), "lengthType"))
				type = (string) getDefaultAttribute(typeof(Blob), "lengthType");

			if (type != null)
			{
				switch (type)
				{
					case "calc":
						blob.lengthType = LengthType.Calc;
						blob.lengthOther = getXmlAttribute(node, "length");
						break;
					case "python":
						blob.lengthType = LengthType.Python;
						blob.lengthOther = getXmlAttribute(node, "length");
						break;
					case "ruby":
						blob.lengthType = LengthType.Ruby;
						blob.lengthOther = getXmlAttribute(node, "length");
						break;
					default:
						throw new PeachException("Error parsing Blob lengthType attribute, unknown value '" + getXmlAttribute(node, "lengthType") + "'.");
				}
			}
			else if (hasXmlAttribute(node, "length"))
			{
				try
				{
					blob.lengthType = LengthType.String;
					blob.length = int.Parse(getXmlAttribute(node, "length"));

					if (blob.length < 0)
						throw new PeachException("Lengths cannot be negative");
				}
				catch (Exception e)
				{
					throw new PeachException("Error parsing Blob length attribute: " + e.Message);
				}
			}

			handleCommonDataElementAttributes(node, blob);
			handleCommonDataElementChildren(node, blob);
			handleCommonDataElementValue(node, blob);

			if (blob.DefaultValue != null && blob.DefaultValue.GetVariantType() == Variant.VariantType.String)
			{
				BitStream sout = new BitStream();
				if( ((string)blob.DefaultValue) != null)
					sout.WriteBytes(ASCIIEncoding.ASCII.GetBytes((string)blob.DefaultValue));
				sout.SeekBytes(0, SeekOrigin.Begin);
				blob.DefaultValue = new Variant(sout);
			}

			return blob;
		}

		protected Flags handleFlags(XmlNode node, DataElementContainer parent)
		{
			Flags flags = new Flags();

			if (hasXmlAttribute(node, "name"))
				flags.name = getXmlAttribute(node, "name");

			if (hasXmlAttribute(node, "size"))
			{
				uint size;
				try
				{
					size = uint.Parse(getXmlAttribute(node, "size"));
				}
				catch
				{
					throw new PeachException("Error, " + flags.name + " size attribute is not valid number.");
				}

				if (size < 1 || size > 64)
					throw new PeachException(string.Format(
						"Error, unsupported size {0} for element {1}.", size, flags.name));

				flags.Size = size;
			}
			else if (hasDefaultAttribute(typeof(Flags), "size"))
			{
				uint size;
				try
				{
					size = uint.Parse((string)getDefaultAttribute(typeof(Flags), "size"));
				}
				catch
				{
					throw new PeachException("Error, " + flags.name + " size attribute is not valid number.");
				}

				if (size < 1 || size > 64)
					throw new PeachException(string.Format(
						"Error, unsupported size {0} for element {1}.", size, flags.name));

				flags.Size = size;
			}

			if (hasXmlAttribute(node, "endian"))
			{
				string endian = getXmlAttribute(node, "endian").ToLower();
				switch (endian)
				{
					case "little":
						flags.LittleEndian = true;
						break;
					case "big":
						flags.LittleEndian = false;
						break;
					case "network":
						flags.LittleEndian = false;
						break;
					default:
						throw new PeachException(string.Format(
							"Error, unsupported value \"{0}\" for \"endian\" attribute on field \"{1}\".", endian, flags.name));
				}
			}
			else if (hasDefaultAttribute(typeof(Flags), "endian"))
			{
				string endian = ((string)getDefaultAttribute(typeof(Flags), "endian")).ToLower();
				switch (endian)
				{
					case "little":
						flags.LittleEndian = true;
						break;
					case "big":
						flags.LittleEndian = false;
						break;
					case "network":
						flags.LittleEndian = false;
						break;
					default:
						throw new PeachException(string.Format(
							"Error, unsupported value \"{0}\" for \"endian\" attribute on field \"{1}\".", endian, flags.name));
				}
			}

			handleCommonDataElementAttributes(node, flags);
			handleCommonDataElementChildren(node, flags);

			foreach (XmlNode child in node.ChildNodes)
			{
				// Looking for "Flag" element
				if (child.Name == "Flag")
				{
					flags.Add(handleFlag(child, flags));
				}
			}

			return flags;
		}

		protected DataElement handleFlag(XmlNode node, DataElement parent)
		{
			Flag flag = new Flag();

			if (hasXmlAttribute(node, "name"))
				flag.name = getXmlAttribute(node, "name");

			if (hasXmlAttribute(node, "position"))
				flag.Position = int.Parse(getXmlAttribute(node, "position"));
			else
				throw new PeachException("Error, Flag elements must have 'position' attribute!");

			if (hasXmlAttribute(node, "size"))
			{
				try
				{
					flag.Size = int.Parse(getXmlAttribute(node, "size"));
				}
				catch (Exception e)
				{
					throw new PeachException("Error parsing Flag size attribute: " + e.Message);
				}
			}
			else
				throw new PeachException("Error, Flag elements must have 'position' attribute!");
			
			handleCommonDataElementAttributes(node, flag);
			handleCommonDataElementChildren(node, flag);
			handleCommonDataElementValue(node, flag);

			return flag;
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
						parent.relations.Add(rel);
					}
					else if (hasXmlAttribute(node, "from"))
					{
						SizeRelation rel = new SizeRelation();
						rel.FromName = getXmlAttribute(node, "from");
						parent.relations.Add(rel);
					}
					break;
				
				case "count":
					if (hasXmlAttribute(node, "of"))
					{
						CountRelation rel = new CountRelation();
						rel.OfName = getXmlAttribute(node, "of");
						parent.relations.Add(rel);
					}
					else if (hasXmlAttribute(node, "from"))
					{
						CountRelation rel = new CountRelation();
						rel.FromName = getXmlAttribute(node, "from");
						parent.relations.Add(rel);
					}
					break;
				
				case "offset":
					if (hasXmlAttribute(node, "of"))
					{
						OffsetRelation rel = new OffsetRelation();
						rel.OfName = getXmlAttribute(node, "of");
						parent.relations.Add(rel);
					}
					else if (hasXmlAttribute(node, "from"))
					{
						OffsetRelation rel = new OffsetRelation();
						rel.FromName = getXmlAttribute(node, "from");
						parent.relations.Add(rel);
					}
					break;
				
				default:
					throw new ApplicationException("Unknown relation type found '"+
						getXmlAttribute(node, "type")+"'.");
			}
		}

		protected Fixup handleFixup(XmlNode node, DataElement parent)
		{
			if (!hasXmlAttribute(node, "class"))
				throw new PeachException("Fixup element has no 'class' attribute [" + node.OuterXml + "].");

			string cls = getXmlAttribute(node, "class");
			Type tFixup = null;
			var arg = handleParams(node);

			// Locate PublisherAttribute classes and check name
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type t in a.GetExportedTypes())
				{
					if (!t.IsClass)
						continue;

					foreach (object attrib in t.GetCustomAttributes(true))
					{
						if (attrib is FixupAttribute)
						{
							if ((attrib as FixupAttribute).className == cls)
							{
								tFixup = t;

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
						}
					}
				}
			}

			throw new PeachException("Error, unable to locate Fixup named '" + cls + "'.");
		}

		protected Transformer handleTransformer(XmlNode node, DataElement parent)
		{
			string cls = getXmlAttribute(node, "class");
			Type tTransformer = null;
			var arg = handleParams(node);

			// Locate PublisherAttribute classes and check name
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type t in a.GetExportedTypes())
				{
					if (!t.IsClass)
						continue;

					foreach (object attrib in t.GetCustomAttributes(true))
					{
						if (attrib is TransformerAttribute)
						{
							if ((attrib as TransformerAttribute).elementName == cls)
							{
								tTransformer = t;

								Type[] targs = new Type[1];
								targs[0] = typeof(Dictionary<string, Variant>);

								ConstructorInfo co = tTransformer.GetConstructor(targs);

								object[] args = new object[1];
								args[0] = arg;

								parent.transformer = co.Invoke(args) as Transformer;

								return parent.transformer;
							}
						}
					}
				}
			}

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Transformer")
					throw new NotImplementedException("todo, sub-transformer");
			}

			throw new PeachException("Error, unable to locate Transformer named '" + cls + "'.");
		}

#endregion

		#region State Model

		protected StateModel handleStateModel(XmlNode node)
		{
			string name = getXmlAttribute(node, "name");
			string initialState = getXmlAttribute(node, "initialState");
			StateModel stateModel = new StateModel();
			stateModel.name = name;

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "State")
				{
					State state = handleState(child, stateModel);
					stateModel.states.Add(state);
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

			if (hasXmlAttribute(node, "value"))
			{
				if (action.type != ActionType.Slurp)
					throw new PeachException("Error, only Actions of type Slurp are allowed to use the 'value' attribute");

				action.value = getXmlAttribute(node, "value");
			}

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Param")
					throw new NotImplementedException("Action.Param TODO");
				
				if (child.Name == "Result")
					throw new NotImplementedException("Action.Result TODO");

				if (child.Name == "DataModel")
					action.dataModel = handleDataModel(child);

				if (child.Name == "Data")
					action.data = handleData(child);
			}

			return action;
		}

		protected Data handleData(XmlNode node)
		{
			Data data = new Data();
			data.name = getXmlAttribute(node, "name");

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Field")
				{
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
					throw new NotImplementedException("Test.Strategy TODO");
				}

				// Agent
				if (child.Name == "Agent")
				{
					string refName = getXmlAttribute(child, "ref");
					test.agents.Add(refName, parent.agents[refName]);
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
								argTypes[0] = typeof(Dictionary<string, string>);
								ConstructorInfo strategyCo = t.GetConstructor(argTypes);

								object[] args = new object[1];
								args[0] = new Dictionary<string, string>();

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

			// Locate PublisherAttribute classes and check name
			foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach(Type t in a.GetExportedTypes())
				{
					if (!t.IsClass)
						continue;

					foreach (object attrib in t.GetCustomAttributes(true))
					{
						if (attrib is PublisherAttribute)
						{
							if ((attrib as PublisherAttribute).invokeName == reference)
								pubType = t;
						}
					}
				}
			}

			if (pubType == null)
				throw new PeachException("Error, unable to locate publisher '" + reference + "'.");

			Type[] argTypes = new Type[1];
			argTypes[0] = typeof(Dictionary<string, Variant>);
			ConstructorInfo pubCo = pubType.GetConstructor(argTypes);

			object [] args = new object[1];
			args[0] = handleParams(node);

			Publisher pub = pubCo.Invoke(args) as Publisher;

			return pub;
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
					throw new NotImplementedException("todo Logger");

				if (child.Name == "Test")
				{
					Test test = parent.tests[getXmlAttribute(child, "ref")];
					run.tests.Add(test.name, test);
				}
			}

			return run;
		}

		#endregion
	}
}

// end
