
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

namespace PeachCore.Analyzers
{
	public class PitParser : Analyzer
	{
		static int ErrorsCount = 0;
		static string ErrorMessage = "";

		public static PitParser()
		{
			PitParser.supportParser = true;
		}

		public override Dom asParser(Dictionary<string, string> args, string fileName)
		{
			Dom dom = null;

			if (!File.Exists(fileName))
				throw PeachException("Error: Unable to locate Pit file [" + fileName + "].\n");

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
			XmlSchemaCollection xsc = new XmlSchemaCollection();
			XmlValidatingReader vr;

			xsc.Add(null, tr);

			XmlTextReader xmlFile = new XmlTextReader(fileName);
			vr = new XmlValidatingReader(xmlFile, XmlNodeType.Document, null);
			vr.Schemas.Add(xsc);
			vr.ValidationType = ValidationType.Schema;
			vr.ValidationEventHandler += new ValidationEventHandler(vr_ValidationEventHandler);

			while (vr.Read()) ;
			vr.Close();

			if (ErrorsCount > 0)
			{
				throw new PeachException("Error: Pit file failed to validate: "+ErrorMessage);
			}
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

		protected void handlePeach(XmlNode node)
		{
			Dom dom = new Dom();

			foreach (XmlNode child in node)
			{
				if (child.Name == "DataModel")
					dom.dataModels.Add(handleDataModel(child, node));
			}
		}

		protected DataModel handleDataModel(XmlNode node, XmlNode parent)
		{
			DataModel dataModel = new DataModel();

			try
			{
				dataModel.name = node.Attributes["name"];
			}
			catch
			{
				throw new PeachException("Error, DataModel missing required 'name' attribute.");
			}

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
						dataModel.Add(handleRelation(child, node));
						break;

					case "Fixup":
						dataModel.Add(handleFixup(child, node));
						break;

					case "Transformer":
						dataModel.Add(handleTransformer(child, node));
						break;

					default:
						throw new PeachException("Error, DataModel [" + dataModel.name +
							"] has unknown child node [" + child.Name + "].");
				}
			}
		}

		protected Block handleBlock(XmlNode node, XmlNode parent)
		{
			return null;
		}

		protected Choice handleChoice(XmlNode node, XmlNode parent)
		{
			return null;
		}

		protected String handleString(XmlNode node, XmlNode parent)
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
