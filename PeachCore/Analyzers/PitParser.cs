
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
using System.Linq;
using System.Text;
using System.IO;
using PeachCore.Dom;

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

	public class PitParser : Analyzer
	{
		static int ErrorsCount = 0;
		static string ErrorMessage = "";

		static PitParser()
		{
			PitParser.supportParser = true;
			Analyzer.defaultParser = new PitParser();
		}

		public override Dom.Dom asParser(Dictionary<string, string> args, string fileName)
		{
			Dom.Dom dom = null;

			if (!File.Exists(fileName))
				throw new PeachException("Error: Unable to locate Pit file [" + fileName + "].\n");

			validatePit(fileName, @"c:\peach\peach.xsd");

			XmlDocument xmldoc = new XmlDocument();
			xmldoc.Load(fileName);

			if (xmldoc.FirstChild.Name == "Peach")
				dom = handlePeach(xmldoc.FirstChild);

			return dom;
		}

		/// <summary>
		/// Validate PIT XML using Schema file.
		/// </summary>
		/// <param name="fileName">Pit file to validate</param>
		/// <param name="schema">Peach XML Schema file</param>
		public void validatePit(string fileName, string schema)
		{
			XmlTextReader tr = new XmlTextReader(schema);
			XmlSchemaSet set = new XmlSchemaSet();
			set.Add(null, tr);

			XmlTextReader xmlFile = new XmlTextReader(fileName);
			xmlFile.Settings.IgnoreComments = true;
			xmlFile.Settings.Schemas = set;
			xmlFile.Settings.ValidationType = ValidationType.Schema;
			xmlFile.Settings.ValidationEventHandler += new ValidationEventHandler(vr_ValidationEventHandler);

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

		protected Dom.Dom handlePeach(XmlNode node)
		{
			Dom.Dom dom = new Dom.Dom();

			// Pass 1 - Handle imports, includes, python path

			foreach (XmlNode child in node)
			{
				switch (child.Name)
				{
					case "Import":
						break;
					case "Include":
						break;
					case "PythonPath":
						break;
				}
			}

			// Pass 2 - Handle namespace 

			foreach (XmlNode child in node)
			{
				if (child.Name == "Namespace")
				{
				}
			}

			// Pass 3 - Handle data model

			foreach (XmlNode child in node)
			{
				if (child.Name == "DataModel")
				{
					DataModel dm = handleDataModel(child, node);
					dom.dataModels.Add(dm.name, dm);
				}
			}

			// Pass 4 - Handle Data

			foreach (XmlNode child in node)
			{
				if (child.Name == "Data")
				{
				}
			}

			// Pass 5 - Handle State model

			foreach (XmlNode child in node)
			{
				if (child.Name == "StateModel")
				{
				}
			}

			// Pass 6 - Handle Test

			foreach (XmlNode child in node)
			{
				if (child.Name == "Test")
				{
				}
			}

			// Pass 7 - Handle Run

			foreach (XmlNode child in node)
			{
				if (child.Name == "Run")
				{
				}
			}

			return dom;
		}

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

		protected bool hasXmlAttribute(XmlNode node, string name)
		{
			try
			{
				object o = node.Attributes[name];
				return false;
			}
			catch
			{
				return true;
			}
		}

		protected DataModel handleDataModel(XmlNode node, XmlNode parent)
		{
			DataModel dataModel = new DataModel();

			dataModel.name = getXmlAttribute(node, "name");
			if(dataModel.name == null)
				throw new PeachException("Error, DataModel missing required 'name' attribute.");

			foreach (XmlNode child in node.ChildNodes)
			{
				switch (child.Name)
				{
					case "Block":
						dataModel.Add(handleBlock(child, node));
						break;

					case "Choice":
						dataModel.Add(handleChoice(child, node));
						break;

					case "String":
						dataModel.Add(handleString(child, node));
						break;

					case "Number":
						dataModel.Add(handleNumber(child, node));
						break;

					case "Blob":
						dataModel.Add(handleBlob(child, node));
						break;

					case "Flags":
						dataModel.Add(handleFlags(child, node));
						break;

					case "Relation":
						dataModel.relations.Add(handleRelation(child, node));
						break;

					case "Fixup":
						dataModel.fixup = handleFixup(child, node);
						break;

					case "Transformer":
						dataModel.transformer = handleTransformer(child, node);
						break;

					default:
						throw new PeachException("Error, DataModel [" + dataModel.name +
							"] has unknown child node [" + child.Name + "].");
				}
			}

			return dataModel;
		}

		protected Block handleBlock(XmlNode node, XmlNode parent)
		{
			// name
			string name = getXmlAttribute(node, "name");
			// ref
			string reference = getXmlAttribute(node, "ref");

			Block block = new Block();
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
			// common data element attributes
			// data container children

			return block;
		}

		protected Choice handleChoice(XmlNode node, XmlNode parent)
		{
			return null;
		}

		protected Dom.String handleString(XmlNode node, XmlNode parent)
		{
			return null;
		}

		protected Number handleNumber(XmlNode node, XmlNode parent)
		{
			return null;
		}

		protected Blob handleBlob(XmlNode node, XmlNode parent)
		{
			return null;
		}

		protected Flags handleFlags(XmlNode node, XmlNode parent)
		{
			return null;
		}

		protected Relation handleRelation(XmlNode node, XmlNode parent)
		{
			return null;
		}

		protected Fixup handleFixup(XmlNode node, XmlNode parent)
		{
			return null;
		}

		protected Transformer handleTransformer(XmlNode node, XmlNode parent)
		{
			return null;
		}
	}
}

// end
