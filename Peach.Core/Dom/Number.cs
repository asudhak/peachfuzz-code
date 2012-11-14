
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
using System.Globalization;

namespace Peach.Core.Dom
{
	/// <summary>
	/// A numerical data element.
	/// </summary>
	[DataElement("Number")]
	[PitParsable("Number")]
	[DataElementChildSupported(DataElementTypes.NonDataElements)]
	[Parameter("name", typeof(string), "", true)]
	[Parameter("size", typeof(uint), "size in bits", true)]
	[Parameter("signed", typeof(bool), "Is number signed (default false)", false)]
	[Parameter("endian", typeof(string), "Byte order of number (default 'little')", false)]
	[Serializable]
	public class Number : DataElement
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		protected ulong _max = (ulong)sbyte.MaxValue;
		protected long _min = sbyte.MinValue;
		protected bool _signed = false;
		protected bool _isLittleEndian = true;

		public Number()
			: base()
		{
			lengthType = LengthType.Bits;
			length = 8;
			DefaultValue = new Variant(0);
		}

		public Number(string name)
			: base(name)
		{
			lengthType = LengthType.Bits;
			length = 8;
			DefaultValue = new Variant(0);
		}

		public override void Crack(DataCracker context, BitStream data)
		{
			Number element = this;

			logger.Debug("Crack: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			if (data.LengthBits < data.TellBits() + element.lengthAsBits)
				throw new CrackingFailure("Failed cracking Number '" + element.fullName + "'.", element, data);

			byte[] buf = new byte[(lengthAsBits + 7) / 8];

			int i = 0;
			int todo = (int)lengthAsBits;
			while (todo >= 8)
			{
				buf[i++] = data.ReadByte();
				todo -= 8;
			}
			if (todo > 0)
			{
				buf[i] = (byte)(data.ReadBits(todo) << (8 - todo));
			}

			Variant defaultValue = new Variant(ParseArray(buf));

			logger.Debug("Number's value is: {0}", defaultValue);

			if (element.isToken)
			{
				if (defaultValue != element.DefaultValue)
				{
					logger.Debug("Number marked as token, values did not match '" + ((string)defaultValue) + "' vs. '" + ((string)element.DefaultValue) + "'.");
					throw new CrackingFailure("Number marked as token, values did not match '" + ((string)defaultValue) + "' vs. '" + ((string)element.DefaultValue) + "'.", element, data);
				}
			}

			element.DefaultValue = defaultValue;
		}

		public static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			if (node.Name != "Number")
				return null;

			var num = DataElement.Generate<Number>(node);

			if (node.hasAttribute("signed"))
				num.Signed = node.getAttributeBool("signed", false);
			else
				num.Signed = context.getDefaultAttributeAsBool(typeof(Number), "signed", false);

			string strSize = node.getAttribute("size");
			if (strSize != null)
			{
				int size;
			
				if (!int.TryParse(strSize, out size))
					throw new PeachException("Error, " + num.name + " size attribute is not valid number.");

				if (size < 1 || size > 64)
					throw new PeachException(string.Format("Error, unsupported size {0} for element {1}.", size, num.name));

				num.lengthType = LengthType.Bits;
				num.length = size;
			}

			string strEndian = node.getAttribute("endian");
			if (strEndian == null)
				strEndian = context.getDefaultAttribute(typeof(Number), "endian");

			if (strEndian != null)
			{
				switch (strEndian.ToLower())
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
							string.Format("Error, unsupported value \"{0}\" for \"endian\" attribute on field \"{1}\".", strEndian, num.name));
				}
			}

			context.handleCommonDataElementAttributes(node, num);
			context.handleCommonDataElementChildren(node, num);
			context.handleCommonDataElementValue(node, num);

			return num;
		}

		public override long length
		{
			get
			{
				if (lengthType == LengthType.Bits)
					return _length;
				if (lengthType == LengthType.Bytes)
				{
					if (_length % 8 != 0)
						throw new InvalidOperationException("Error, Number is not power of 8, cannot return length in bytes.");

					return _length / 8;
				}

				throw new NotSupportedException("Error, invalid LengthType for Number.");
			}

			set
			{
				if (value <= 0 || value > 64)
					throw new ArgumentOutOfRangeException("Error, value must be greater than 0 and less than 65.");

				_length = value;

				if (_signed)
				{
					_max = (ulong)((ulong)1 << ((int)lengthAsBits - 1)) - 1;
					_min = 0 - (long)((ulong)1 << ((int)lengthAsBits - 1));
				}
				else
				{
					_max = (ulong)((ulong)1 << ((int)lengthAsBits - 1));
					_max += (_max - 1);
					_min = 0;
				}

				Invalidate();
			}
		}

		public override bool hasLength
		{
			get
			{
				return true;
			}
		}

		public override Variant DefaultValue
		{
			get
			{
				return base.DefaultValue;
			}
			set
			{
				base.DefaultValue = Sanitize(value);
			}
		}

		private dynamic ParseString(string str)
		{
			string strValue;
			NumberStyles style;

			if (str.StartsWith("0x"))
			{
				strValue = str.Substring(2);
				style = NumberStyles.HexNumber;
			}
			else
			{
				strValue = str;
				style = NumberStyles.Integer;
			}

			if (Signed)
			{
				long value;
				if (long.TryParse(strValue, style, null, out value))
					return value;
			}
			else
			{
				ulong value;
				if (ulong.TryParse(strValue, style, null, out value))
					return value;
			}

			throw new PeachException("Error,  {0} value \"{1}\" could not be converted to a {2}-bit {3} number.", name, str, lengthAsBits, Signed ? "signed" : "unsigned");
		}

		private dynamic ParseArray(byte[] buf)
		{
			int mask = (1 << ((int)lengthAsBits % 8)) - 1;
			if ((buf[buf.Length - 1] & mask) != 0)
				throw new PeachException("Error,  {0} value \"{1}\" has an invalid bytes for a {2}-bit {3} number.", name, buf, lengthAsBits, Signed ? "signed" : "unsigned");

			if (buf.Length != ((lengthAsBits + 7) / 8))
				throw new PeachException("Error,  {0} value \"{1}\" has an incorrect length for a {2}-bit {3} number.", name, buf, lengthAsBits, Signed ? "signed" : "unsigned");

			if (Signed)
			{
				if (LittleEndian)
					return LittleBitWriter.GetInt64(buf, (int)lengthAsBits);
				else
					return BigBitWriter.GetInt64(buf, (int)lengthAsBits);
			}
			else
			{
				if (LittleEndian)
					return LittleBitWriter.GetUInt64(buf, (int)lengthAsBits);
				else
					return BigBitWriter.GetUInt64(buf, (int)lengthAsBits);
			}
		}

		private Variant Sanitize(Variant variant)
		{
			dynamic value = 0;

			switch (variant.GetVariantType())
			{
				case Variant.VariantType.String:
					value = ParseString((string)variant);
					break;
				case Variant.VariantType.ByteString:
					value = ParseArray((byte[])variant);
					break;
				case Variant.VariantType.Int:
				case Variant.VariantType.Long:
					value = (long)variant;
					break;
				case Variant.VariantType.ULong:
					value = (ulong)variant;
					break;
				default:
					throw new ArgumentException("Variant type is unsupported.", "variant");
			}

			if (value < 0 && (long)value < _min)
				throw new PeachException("Error,  {0} value \"{1}\" is less than the minimum {2}-bit {3} number.", name, value, lengthAsBits, Signed ? "signed" : "unsigned");
			if (value > 0 && (ulong)value > _max)
				throw new PeachException("Error,  {0} value \"{1}\" is greater than the maximum {2}-bit {3} number.", name, value, lengthAsBits, Signed ? "signed" : "unsigned");

			if (Signed)
				return new Variant((long)value);
			else
				return new Variant((ulong)value);
		}

		public bool Signed
		{
			get { return _signed; }
			set
			{
				_signed = value;
				length = length;

				Invalidate();
			}
		}

		public bool LittleEndian
		{
			get { return _isLittleEndian; }
			set
			{
				_isLittleEndian = value;
				Invalidate();
			}
		}

		public ulong MaxValue
		{
			get { return _max; }
		}

		public long MinValue
		{
			get { return _min; }
		}

		protected override BitStream InternalValueToBitStream()
		{
			byte[] buf;

			if (Signed)
			{
				if (LittleEndian)
					buf = LittleBitWriter.GetBits((long)InternalValue, (int)lengthAsBits);
				else
					buf = BigBitWriter.GetBits((long)InternalValue, (int)lengthAsBits);
			}
			else
			{
				if (LittleEndian)
					buf = LittleBitWriter.GetBits((ulong)InternalValue, (int)lengthAsBits);
				else
					buf = BigBitWriter.GetBits((ulong)InternalValue, (int)lengthAsBits);
			}

			BitStream bits = new BitStream(buf);
			bits.SeekBits(lengthAsBits, System.IO.SeekOrigin.Begin);
			bits.Truncate();
			return bits;
		}

    public override object GetParameter(string parameterName)
    {
      switch (parameterName)
      {
        case "name":
          return this.name;
        case "size":
          return this.length;
        case "signed":
          return Signed;
        case "endian":
          switch (this.LittleEndian)
          {
            case true:
              return "little";
            default:
              return "big";
          }
        default:
          throw new PeachException(System.String.Format("Parameter '{0}' does not exist in Peach.Core.Dom.Number", parameterName));
      }
    }
	}
}

// end
