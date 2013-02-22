
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
	[DataElementChildSupported(DataElementTypes.NonDataElements)]
	[Parameter("name", typeof(string), "Element name", "")]
	[Parameter("length", typeof(uint?), "Length in data element", "")]
	[Parameter("lengthType", typeof(LengthType), "Units of the length attribute", "bytes")]
	[Parameter("lengthCalc", typeof(string), "Scripting expression that evaluates to an integer", "")]
	[Parameter("value", typeof(string), "Default value", "")]
	[Parameter("valueType", typeof(ValueType), "Format of value attribute", "string")]
	[Parameter("token", typeof(bool), "Is element a token", "false")]
	[Parameter("mutable", typeof(bool), "Is element mutable", "false")]
	[Parameter("constraint", typeof(string), "Scripting expression that evaluates to true or false", "")]
	[Parameter("minOccurs", typeof(int), "Minimum occurances", "1")]
	[Parameter("maxOccurs", typeof(int), "Maximum occurances", "1")]
	[Parameter("occurs", typeof(int), "Actual occurances", "1")]
	[Serializable]
	public class Blob : DataElement
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		public Blob()
		{
			_defaultValue = new Variant(new byte[] { });
		}
		
		public Blob(string name)
			: base(name)
		{
			_defaultValue = new Variant(new byte[] { });
		}
		
		public Blob(string name, int length)
			: base(name)
		{
			this.length = length;
			_defaultValue = new Variant(new byte[] { });
		}
		
		public Blob(string name, int length, Variant defaultValue)
			: base(name)
		{
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
				throw new CrackingFailure("Unable to crack Blob '" + element.fullName + "'.", element, data);

			Variant defaultValue = new Variant(new byte[0]);

			if (blobLength > 0)
				defaultValue = new Variant(ReadSizedData(data, blobLength.Value));

			if (element.isToken)
				if (defaultValue != element.DefaultValue)
					throw new CrackingFailure("Blob '" + element.fullName + "' marked as token, values did not match '" +
						defaultValue.ToHex(100) + "' vs. '" + element.DefaultValue.ToHex(100) + "'.", element, data);

			element.DefaultValue = defaultValue;
		}

		public static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			if (node.Name != "Blob")
				return null;

			var blob = DataElement.Generate<Blob>(node);

			context.handleCommonDataElementAttributes(node, blob);
			context.handleCommonDataElementChildren(node, blob);
			context.handleCommonDataElementValue(node, blob);

			if (blob.DefaultValue.GetVariantType() == Variant.VariantType.String)
				blob.DefaultValue = new Variant(ASCIIEncoding.ASCII.GetBytes((string)blob.DefaultValue));

			if (blob.hasLength)
			{
				BitStream bs = (BitStream)blob.DefaultValue;
				if (bs.LengthBits > blob.lengthAsBits)
					throw new PeachException("Error, value of element \"" + blob.name + "\" is longer than specified length.");
				else if (bs.LengthBits < blob.lengthAsBits)
					ExpandDefaultValue(blob, bs);
			}

			return blob;
		}

		private static void ExpandDefaultValue(Blob blob, BitStream bs)
		{
			bs.SeekBits(blob.lengthAsBits - 1, SeekOrigin.Begin);
			bs.WriteBit(0);
			bs.SeekBits(0, SeekOrigin.Begin);
			blob.DefaultValue = new Variant(bs);
		}

    public override object GetParameter(string parameterName)
    {
      switch (parameterName)
      {
        case "name":
          return this.name;
        case "length":
          return this.length;
        default:
          throw new PeachException(System.String.Format("Parameter '{0}' does not exist in Peach.Core.Dom.Blob", parameterName));
      }
    }
	}
}

// end
