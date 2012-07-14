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
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Peach.Core.Xml
{
	/// <summary>
	/// Process sample files to determin data types
	/// and set of default values.
	/// </summary>
	public class Defaults
	{
		protected Dictionary<string, Element> elements = null;
		protected bool verbose = false;

		public Defaults(Dictionary<string, Element> elements, bool verbose)
		{
			this.verbose = verbose;
			this.elements = elements;
		}

		/// <summary>
		/// Process all files in folder as sample files.  We will
		/// use this information to guess data types and default
		/// values.
		/// </summary>
		/// <param name="folder">Folder to pull sample files from.</param>
		public void ProcessFolder(string folder)
		{
			foreach (string file in Directory.GetFiles(folder))
				ProcessFile(file);
		}

		protected static IEnumerable<XmlNode> GenerateXmlNodes(XmlNode node)
		{
			if (node != null)
				foreach (XmlNode child in node.ChildNodes)
				{
					yield return child;
					GenerateXmlNodes(child);
				}
		}

		/// <summary>
		/// Process a single file for sample data and data types.
		/// </summary>
		/// <param name="file">File to use as sample.</param>
		public void ProcessFile(string file)
		{
			try
			{
				//if (verbose)
				//    Console.Write(".");

				XmlDocument doc = new XmlDocument();
				doc.XmlResolver = null;
				doc.Load(File.OpenRead(file));

				foreach (XmlNode node in GenerateXmlNodes(doc.DocumentElement))
				{
					if (!elements.ContainsKey(node.Name))
						continue;

					Element element = elements[node.Name];
					if (!(node.Value == null || node.Value == String.Empty))
					{
						if (element.dataType == DataType.Unknown)
							element.dataType = GuessDataType(node.Value);

						if (!element.defaultValues.Contains(node.Value))
							element.defaultValues.Add(node.Value);
					}

					// Handle any attributes for this element.

					foreach (XmlAttribute attrib in node.Attributes)
					{
						if (!element.attributes.ContainsKey(attrib.Name))
							continue;

						Attribute attribute = element.attributes[attrib.Name];

						if (attribute.dataType == DataType.Unknown)
							attribute.dataType = GuessDataType(attrib.Value);

						attribute.defaultValues.Add(attrib.Value);
					}
				}
			}
			catch
			{
				//Debugger.Break();
			}
		}

		/// <summary>
		/// Guess actual data type based on sample values.
		/// </summary>
		/// <param name="value">Value to guess about</param>
		/// <returns>Returns </returns>
		public DataType GuessDataType(string value)
		{
			if (value == null || value == string.Empty)
				return DataType.Unknown;

			try
			{
				int.Parse(value);
				return DataType.Integer;
			}
			catch
			{
			}

			try
			{
				double.Parse(value);
				return DataType.Double;
			}
			catch
			{
			}

			return DataType.String;
		}
	}
}
