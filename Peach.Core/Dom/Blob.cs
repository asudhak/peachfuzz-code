
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
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

using Peach.Core.Analyzers;
using Peach.Core.IO;
using Peach.Core.Cracker;

using NLog;

namespace Peach.Core.Dom
{
	/// <summary>
	/// Binary large object data element
	/// </summary>
	[DataElement("Blob")]
	[PitParsable("Blob")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	[ParameterAttribute("length", typeof(uint), "Length in bytes", false)]
	[Serializable]
	public class Blob : DataElement
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		public Blob()
		{
			_defaultValue = new Variant(new byte[] { });
		}
		
		public Blob(string name)
		{
			this.name = name;
			_defaultValue = new Variant(new byte[] { });
		}
		
		public Blob(string name, int length)
		{
			this.name = name;
			this.length = length;
			_defaultValue = new Variant(new byte[] { });
		}
		
		public Blob(string name, int length, Variant defaultValue)
		{
			this.name = name;
			this.length = length;
			_defaultValue = defaultValue;
		}
		
		public Blob(int length)
		{
			_defaultValue = new Variant(new byte[] { });
			this.length = length;
		}
		
		public Blob(int length, Variant defaultValue)
		{
			this.length = length;
			_defaultValue = defaultValue;
		}

		public Blob(Variant defaultValue)
		{
			_defaultValue = defaultValue;
		}

		public override void Crack(DataCracker context, BitStream data)
		{
			Blob element = this;

			logger.Trace("Crack: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			// Length in bits
			long? blobLength = context.determineElementSize(element, data);

			if (blobLength == null && element.isToken)
				blobLength = ((BitStream)element.DefaultValue).LengthBits;

			if (blobLength == null)
				throw new CrackingFailure("Unable to crack Blob '" + element + "'.", element, data);

			if ((data.TellBits() + blobLength) > data.LengthBits)
				throw new CrackingFailure("Blob '" + element.fullName +
					"' has length of '" + blobLength + "' bits but buffer only has '" +
					(data.LengthBits - data.TellBits()) + "' bits left.", element, data);

			Variant defaultValue = new Variant(new byte[0]);

			if (blobLength > 0)
				defaultValue = new Variant(data.ReadBitsAsBitStream((long)blobLength));

			if (element.isToken)
				if (defaultValue != element.DefaultValue)
					throw new CrackingFailure("Blob '" + element.name + "' marked as token, values did not match '" +
						defaultValue.ToHex(100) + "' vs. '" + element.DefaultValue.ToHex(100) + "'.", element, data);

			element.DefaultValue = defaultValue;
		}

		public static DataElement PitParse(PitParser context, XmlNode node, DataElementContainer parent)
		{
			if (node.Name != "Blob")
				return null;

			var blob = new Blob();

			if (context.hasXmlAttribute(node, "name"))
				blob.name = context.getXmlAttribute(node, "name");

			context.handleCommonDataElementAttributes(node, blob);
			context.handleCommonDataElementChildren(node, blob);
			context.handleCommonDataElementValue(node, blob);

			if (blob.DefaultValue != null && blob.DefaultValue.GetVariantType() == Variant.VariantType.String)
			{
				BitStream sout = new BitStream();
				sout.BigEndian();

				if (((string)blob.DefaultValue) != null)
					sout.WriteBytes(ASCIIEncoding.ASCII.GetBytes((string)blob.DefaultValue));
				sout.SeekBytes(0, SeekOrigin.Begin);
				blob.DefaultValue = new Variant(sout);
			}

			return blob;
		}
	}
}

// end
