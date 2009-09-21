
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

		public override void asParser(Dictionary<string, string> args, string fileName)
		{
			if (!File.Exists(fileName))
				throw PeachException("Error: Unable to locate Pit file [" + fileName + "].\n");

			validatePit(fileName, @"c:\peach\peach.xsd");

			throw ApplicationException("TODO: Implement parser");
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
	}
}

// end
