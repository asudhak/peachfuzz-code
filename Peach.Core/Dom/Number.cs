
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
	[Parameter("name", typeof(string), "Element name", "")]
	[Parameter("size", typeof(uint), "Size in bits")]
	[Parameter("signed", typeof(bool), "Is number signed", "false")]
	[Parameter("endian", typeof(EndianType), "Byte order of number", "little")]
	[Parameter("value", typeof(string), "Default value", "")]
	[Parameter("valueType", typeof(ValueType), "Format of value attribute", "string")]
	[Parameter("token", typeof(bool), "Is element a token", "false")]
	[Parameter("mutable", typeof(bool), "Is element mutable", "false")]
	[Parameter("constraint", typeof(string), "Scripting expression that evaluates to true or false", "")]
	[Parameter("minOccurs", typeof(int), "Minimum occurances", "1")]
	[Parameter("maxOccurs", typeof(int), "Maximum occurances", "1")]
	[Parameter("occurs", typeof(int), "Actual occurances", "1")]
	[Serializable]
	public class Number : DataElement
	{
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

		public static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			if (node.Name != "Number")
				return null;

			var num = DataElement.Generate<Number>(node);

			if (node.hasAttr("signed"))
				num.Signed = node.getAttrBool("signed");
			else
				num.Signed = context.getDefaultAttr(typeof(Number), "signed", num.Signed);

			if (node.hasAttr("size"))
			{
				int size = node.getAttrInt("size");

				if (size < 1 || size > 64)
					throw new PeachException(string.Format("Error, unsupported size '{0}' for {1}.", size, num.debugName));

				num.lengthType = LengthType.Bits;
				num.length = size;
			}

			string strEndian = null;
			if (node.hasAttr("endian"))
				strEndian = node.getAttrString("endian");
			if (strEndian == null)
				strEndian = context.getDefaultAttr(typeof(Number), "endian", null);

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
							string.Format("Error, unsupported value '{0}' for 'endian' attribute on {1}.", strEndian, num.debugName));
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
				switch (_lengthType)
				{
					case LengthType.Bytes:
						return _length;
					case LengthType.Bits:
						return _length;
					case LengthType.Chars:
						throw new NotSupportedException("Length type of Chars not supported by Number.");
					default:
						throw new NotSupportedException("Error calculating length.");
				}
			}
			set
			{
				if (value <= 0 || value > 64)
					throw new ArgumentOutOfRangeException("value", value, "Value must be greater than 0 and less than 65.");

				base.length = value;

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

		#region Sanitize

		private dynamic SanitizeString(string str)
		{
			string conv = str;
			NumberStyles style = NumberStyles.AllowLeadingSign;

			if (str.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
			{
				conv = str.Substring(2);
				style = NumberStyles.AllowHexSpecifier;
			}

			if (Signed)
			{
				long value;
				if (long.TryParse(conv, style, CultureInfo.InvariantCulture, out value))
					return value;
			}
			else
			{
				ulong value;
				if (ulong.TryParse(conv, style, CultureInfo.InvariantCulture, out value))
					return value;
			}

			throw new PeachException(string.Format("Error, {0} value '{1}' could not be converted to a {2}-bit {3} number.", debugName, str, lengthAsBits, Signed ? "signed" : "unsigned"));
		}

		private dynamic SanitizeStream(BitStream bs)
		{
			if (bs.LengthBytes != ((lengthAsBits + 7) / 8))
				throw new PeachException(string.Format("Error, {0} value has an incorrect length for a {1}-bit {2} number, expected {3} bytes.", debugName, lengthAsBits, Signed ? "signed" : "unsigned", (lengthAsBits + 7) / 8));

			if (bs.LengthBits > lengthAsBits)
			{
				ulong extra = bs.ReadBits((int)(bs.LengthBits - lengthAsBits));
				if (extra != 0)
					throw new PeachException(string.Format("Error, {0} value has an invalid bytes for a {1}-bit {2} number.", debugName, lengthAsBits, Signed ? "signed" : "unsigned"));
			}

			return FromBitstream(bs);
		}

		private dynamic FromBitstream(BitStream bs)
		{
			ulong bits = bs.ReadBits((int)lengthAsBits);

			if (Signed)
			{
				if (LittleEndian)
					return LittleBitWriter.GetInt64(bits, (int)lengthAsBits);
				else
					return BigBitWriter.GetInt64(bits, (int)lengthAsBits);
			}
			else
			{
				if (LittleEndian)
					return LittleBitWriter.GetUInt64(bits, (int)lengthAsBits);
				else
					return BigBitWriter.GetUInt64(bits, (int)lengthAsBits);
			}
		}

		private Variant Sanitize(Variant variant)
		{
			dynamic value = GetNumber(variant);

			if (value < 0 && (long)value < MinValue)
				throw new PeachException(string.Format("Error, {0} value '{1}' is less than the minimum {2}-bit {3} number.", debugName, value, lengthAsBits, Signed ? "signed" : "unsigned"));
			if (value > 0 && (ulong)value > MaxValue)
				throw new PeachException(string.Format("Error, {0} value '{1}' is greater than the maximum {2}-bit {3} number.", debugName, value, lengthAsBits, Signed ? "signed" : "unsigned"));

			if (Signed)
				return new Variant((long)value);
			else
				return new Variant((ulong)value);
		}

		private dynamic GetNumber(Variant variant)
		{
			dynamic value = 0;

			switch (variant.GetVariantType())
			{
				case Variant.VariantType.String:
					value = SanitizeString((string)variant);
					break;
				case Variant.VariantType.BitStream:
				case Variant.VariantType.ByteString:
					value = SanitizeStream((BitStream)variant);
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

			return value;
		}

		#endregion

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
			ulong bits;

			dynamic value = GetNumber(InternalValue);

			if (value > 0 && (ulong)value > MaxValue)
			{
				string msg = string.Format("Error, {0} value '{1}' is greater than the maximum {2}-bit {3} number.", debugName, value, lengthAsBits, Signed ? "signed" : "unsigned");
				var inner = new OverflowException(msg);
				throw new SoftException(inner);
			}

			if (value < 0 && (long)value < MinValue)
			{
				string msg = string.Format("Error, {0} value '{1}' is less than the minimum {2}-bit {3} number.", debugName, value, lengthAsBits, Signed ? "signed" : "unsigned");
				var inner = new OverflowException(msg);
				throw new SoftException(inner);
			}

			if (LittleEndian)
				bits = LittleBitWriter.GetBits(value, (int)lengthAsBits);
			else
				bits = BigBitWriter.GetBits(value, (int)lengthAsBits);

			var bs = new BitStream();
			bs.WriteBits(bits, (int)lengthAsBits);
			return bs;
		}
	}
}

// end
