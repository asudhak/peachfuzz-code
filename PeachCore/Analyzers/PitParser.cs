
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
using PeachCore.Dom;
using System.Reflection;
using PeachCore;

namespace PeachCore.Analyzers
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

		static PitParser()
		{
			PitParser.supportParser = true;
			Analyzer.defaultParser = new PitParser();
		}

		public PitParser()
		{
		}

		public override Dom.Dom asParser(Dictionary<string, string> args, string fileName)
		{
			if (!File.Exists(fileName))
				throw new PeachException("Error: Unable to locate Pit file [" + fileName + "].\n");

			validatePit(fileName);

			XmlDocument xmldoc = new XmlDocument();
			xmldoc.Load(fileName);

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

		public override void asParserValidation(Dictionary<string, string> args, string fileName)
		{
			validatePit(fileName);
		}

		/// <summary>
		/// Validate PIT XML using Schema file.
		/// </summary>
		/// <param name="fileName">Pit file to validate</param>
		/// <param name="schema">Peach XML Schema file</param>
		public void validatePit(string fileName)
		{
			XmlTextReader tr = new XmlTextReader(
				Assembly.GetExecutingAssembly().GetManifestResourceStream("PeachCore.peach.xsd"));
			XmlSchemaSet set = new XmlSchemaSet();
			set.Add(null, tr);

			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			settings.Schemas = set;
			settings.ValidationType = ValidationType.Schema;
			settings.ValidationEventHandler += new ValidationEventHandler(vr_ValidationEventHandler);

			XmlReader xmlFile = XmlTextReader.Create(fileName, settings);

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

						validatePit(fileName);

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
						throw new NotSupportedException("Implement Defaults element parsing support.");
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
		protected DataElement getReference(Dom.Dom dom, string name)
		{
			if (name.IndexOf(':') > -1)
			{
				string ns = name.Substring(0, name.IndexOf(':') - 1);

				if (!dom.ns.Keys.Contains(ns))
					throw new PeachException("Unable to locate namespace '"+ns+"' in ref '"+name+"'.");

				name = name.Substring(name.IndexOf(':'));
				dom = dom.ns["name"];
			}

			foreach (DataModel model in dom.dataModels.Values)
			{
				if (model.name == name)
					return model;
			}

			return null;
		}

#endregion

		#region Data Model

		protected DataModel handleDataModel(XmlNode node)
		{
			DataModel dataModel = new DataModel();

			if (hasXmlAttribute(node, "ref"))
			{
				DataModel refObj = getReference(_dom, getXmlAttribute(node, "ref")) as DataModel;
				if (refObj != null)
				{
					dataModel = ObjectCopier.Clone<DataModel>(refObj);
				}
				else
				{
					throw new PeachException("Unable to locate 'ref' [" + getXmlAttribute(node, "ref") + "] or found node did not match type. [" + node.OuterXml + "].");
				}

				if (!hasXmlAttribute(node, "name"))
					dataModel.name = getXmlAttribute(node, "ref");
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
				Block refObj = getReference(_dom, getXmlAttribute(node, "ref")) as Block;
				if (refObj != null)
				{
					block = ObjectCopier.Clone<Block>(refObj);
				}
				else
				{
					throw new PeachException("Unable to locate 'ref' [" + getXmlAttribute(node, "ref") + "] or found node did not match type. [" + node.OuterXml + "].");
				}
			}

			// name
			string name = getXmlAttribute(node, "name");
			block.name = name;

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
				throw new NotSupportedException("Implement me!");

			if (hasXmlAttribute(node, "pointer"))
				throw new NotSupportedException("Implement pointer attribute");
			
			if (hasXmlAttribute(node, "pointerDepth"))
				throw new NotSupportedException("Implement pointerDepth attribute");
		}

		/// <summary>
		/// Handle parsing common dataelement children liek relation, fixup and
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
				switch (child.Name)
				{
					case "Block":
						element.Add(handleBlock(child, element));
						break;

					case "Choice":
						element.Add(handleChoice(child, element));
						break;

					case "String":
						element.Add(handleString(child, element));
						break;

					case "Number":
						element.Add(handleNumber(child, element));
						break;

					case "Blob":
						element.Add(handleBlob(child, element));
						break;

					case "Flags":
						element.Add(handleFlags(child, element));
						break;

					case "Custom":
						throw new NotSupportedException("Implement custom types");
				}
			}
		}

		protected Choice handleChoice(XmlNode node, DataElementContainer parent)
		{
			Choice choice = new Choice();

			// First name
			if (hasXmlAttribute(node, "name"))
				choice.name = getXmlAttribute(node, "name");
			
			handleCommonDataElementAttributes(node, choice);
			handleCommonDataElementChildren(node, choice);
			handleDataElementContainer(node, choice);

			// Array
			if (hasXmlAttribute(node, "minOccurs") || hasXmlAttribute(node, "maxOccurs"))
			{
				Dom.Array array = new Dom.Array();
				array.Add(choice);
				array.name = choice.name;
			}

			return choice;
		}

		protected Dom.String handleString(XmlNode node, DataElementContainer parent)
		{
			Dom.String str = new Dom.String();

			if (hasXmlAttribute(node, "name"))
				str.name = getXmlAttribute(node, "name");

			if (hasXmlAttribute(node, "length"))
				throw new NotSupportedException("Implement length attribute on String");

			if (hasXmlAttribute(node, "nullTerminated"))
				throw new NotSupportedException("Implement nullTerminated attribute on String");

			if (hasXmlAttribute(node, "type"))
				throw new NotSupportedException("Implement type attribute on String");

			if (hasXmlAttribute(node, "padCharacter"))
				throw new NotSupportedException("Implement padCharacter attribute on String");

			if (hasXmlAttribute(node, "lengthType"))
				throw new NotSupportedException("Implement lengthType attribute on String");

			if (hasXmlAttribute(node, "tokens"))
				throw new NotSupportedException("Implement tokens attribute on String");

			if (hasXmlAttribute(node, "analyzer"))
				throw new NotSupportedException("Implement analyzer attribute on String");

			handleCommonDataElementAttributes(node, str);
			handleCommonDataElementChildren(node, str);
			handleCommonDataElementValue(node, str);

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
			else
				elem.DefaultValue = new Variant(value);

		}

		protected Number handleNumber(XmlNode node, DataElementContainer parent)
		{
			Number num = new Number();

			if (hasXmlAttribute(node, "name"))
				num.name = getXmlAttribute(node, "name");

			if (hasXmlAttribute(node, "signed"))
				num.Signed = getXmlAttributeAsBool(node, "signed", false);

			if (hasXmlAttribute(node, "size"))
			{
				uint size;
				try
				{
					size = uint.Parse(getXmlAttribute(node, "size"));
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

			handleCommonDataElementAttributes(node, num);
			handleCommonDataElementChildren(node, num);
			handleCommonDataElementValue(node, num);

			return num;
		}

		protected Blob handleBlob(XmlNode node, DataElementContainer parent)
		{
			Blob blob = new Blob();

			if (hasXmlAttribute(node, "length"))
				throw new NotSupportedException("Implement length attribute on Blob");

			if (hasXmlAttribute(node, "lengthType"))
				throw new NotSupportedException("Implement lengthType attribute on Blob");

			handleCommonDataElementAttributes(node, blob);
			handleCommonDataElementChildren(node, blob);
			handleCommonDataElementValue(node, blob);

			if (blob.DefaultValue != null && blob.DefaultValue.GetVariantType() == Variant.VariantType.String)
			{
				BitStream sout = new BitStream();
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
				flag.Position = uint.Parse(getXmlAttribute(node, "position"));
			else
				throw new PeachException("Error, Flag elements must have 'position' attribute!");

			if (hasXmlAttribute(node, "size"))
				flag.name = getXmlAttribute(node, "size");
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
				
				case "when":
					{
						WhenRelation rel = new WhenRelation();
						rel.WhenExpression = getXmlAttribute(node, "when");
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

								object[] args = new object[1];
								args[0] = arg;

								parent.fixup = co.Invoke(args) as Fixup;

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
					Action action = handleAction(child, state);
					state.actions.Add(action);
				}
			}

			return state;
		}

		protected Action handleAction(XmlNode node, State parent)
		{
			Action action = new Action();

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

				if(child.Name == "Data")
					throw new NotImplementedException("Action.Data TODO");
			}

			return action;
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
					throw new NotImplementedException("Test.Agent TODO");
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
