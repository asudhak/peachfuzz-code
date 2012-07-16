
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

namespace Peach.Core.Dom
{
	/// <summary>
	/// A numerical data element.
	/// </summary>
	[DataElement("Number")]
	[PitParsable("Number")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
  [ParameterAttribute("name", typeof(string), "", true)]
  [ParameterAttribute("size", typeof(uint), "size in bits", true)]
	[ParameterAttribute("signed", typeof(bool), "Is number signed (default false)", false)]
	[ParameterAttribute("endian", typeof(string), "Byte order of number (default 'little')", false)]
	[Serializable]
	public class Number : DataElement
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected ulong _max = (ulong)sbyte.MaxValue;
		protected long _min = sbyte.MinValue;
		protected bool _signed = true;
		protected bool _isLittleEndian = true;

		public Number()
			: base()
		{
			length = 8;
			lengthType = LengthType.Bits;
			DefaultValue = new Variant(0);
		}

		public Number(string name)
			: base(name)
		{
			length = 8;
			lengthType = LengthType.Bits;
			DefaultValue = new Variant(0);
		}

		public Number(string name, long value, int size)
			: base(name)
		{
			lengthType = LengthType.Bits;
			length = size;
			DefaultValue = new Variant(value);
		}

		public Number(string name, long value, int size, bool signed, bool isLittleEndian)
			: base(name)
		{
			lengthType = LengthType.Bits;
			length = size;
			_signed = signed;
			_isLittleEndian = isLittleEndian;
			DefaultValue = new Variant(value);
		}

		public override void Crack(DataCracker context, BitStream data)
		{
			Number element = this;

			logger.Debug("Crack: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			if (data.LengthBits < data.TellBits() + element.lengthAsBits)
				throw new CrackingFailure("Failed cracking Number '" + element.fullName + "'.", element, data);

			if (element.LittleEndian)
				data.LittleEndian();
			else
				data.BigEndian();

			Variant defaultValue;

			if (element.Signed)
			{
				switch (element.lengthAsBits)
				{
					case 8:
						defaultValue = new Variant(data.ReadInt8());
						break;
					case 16:
						defaultValue = new Variant(data.ReadInt16());
						break;
					case 32:
						defaultValue = new Variant(data.ReadInt32());
						break;
					case 64:
						defaultValue = new Variant(data.ReadInt64());
						break;
					default:
						throw new CrackingFailure("Number '" + element.name + "' had unsupported size '" + element.lengthAsBits + "'.", element, data);
				}

				logger.Debug("Number's value is: " + (long)defaultValue);
			}
			else
			{
				switch (element.lengthAsBits)
				{
					case 8:
						defaultValue = new Variant(data.ReadUInt8());
						break;
					case 16:
						defaultValue = new Variant(data.ReadUInt16());
						break;
					case 32:
						defaultValue = new Variant(data.ReadUInt32());
						break;
					case 64:
						defaultValue = new Variant(data.ReadUInt64());
						break;
					default:
						throw new CrackingFailure("Number '" + element.name + "' had unsupported size '" + element.lengthAsBits + "'.", element, data);
				}

				logger.Debug("Number's value is: " + (ulong)defaultValue);
			}

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

			var num = new Number();

			if (context.hasXmlAttribute(node, "name"))
				num.name = context.getXmlAttribute(node, "name");

			if (context.hasXmlAttribute(node, "signed"))
				num.Signed = context.getXmlAttributeAsBool(node, "signed", false);
			else if (context.hasDefaultAttribute(typeof(Number), "signed"))
				num.Signed = context.getDefaultAttributeAsBool(typeof(Number), "signed", false);

			if (context.hasXmlAttribute(node, "size"))
			{
				int size;
				try
				{
					size = int.Parse(context.getXmlAttribute(node, "size"));
				}
				catch
				{
					throw new PeachException("Error, " + num.name + " size attribute is not valid number.");
				}

				if (size < 1 || size > 64)
					throw new PeachException(string.Format("Error, unsupported size {0} for element {1}.", size, num.name));

				num.length = size;
			}

			if (context.hasXmlAttribute(node, "endian"))
			{
				string endian = context.getXmlAttribute(node, "endian").ToLower();
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
			else if (context.hasDefaultAttribute(typeof(Number), "endian"))
			{
				string endian = ((string)context.getDefaultAttribute(typeof(Number), "endian")).ToLower();
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

				throw new NotSupportedException("Error, invalid LenngType for Number.");
			}

			set
			{
				if (value == 0)
					throw new ApplicationException("size must be > 0");

				_length = value;

				if (_signed)
				{
					_max = (ulong)(Math.Pow(2, lengthAsBits) / 2) - 1;
					_min = 0 - (long)(Math.Pow(2, lengthAsBits) / 2);
				}
				else
				{
					_max = (ulong)Math.Pow(2, lengthAsBits) - 1;
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
			set
			{
				throw new NotSupportedException("A number always has a size.");
			}
		}

		public override Variant DefaultValue
		{
			get { return base.DefaultValue; }
			set
			{
				if (Signed)
				{
					if ((long)value >= _min)
						base.DefaultValue = value;
					else
						throw new ApplicationException("DefaultValue not with in min/max values.");
				}
				else
				{
					if ((ulong)value <= _max)
						base.DefaultValue = value;
					else
						throw new ApplicationException("DefaultValue not with in min/max values.");
				}
			}
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

		protected override BitStream InternalValueToBitStream(Variant b)
		{
			BitStream bits = new BitStream();

			if (_isLittleEndian)
				bits.LittleEndian();
			else
				bits.BigEndian();

            if (Signed)
            {
                switch (lengthAsBits)
                {
                    case 8:
                        bits.WriteInt8((sbyte)InternalValue);
                        break;
                    case 16:
                        bits.WriteInt16((short)InternalValue);
                        break;
                    case 32:
                        bits.WriteInt32((int)InternalValue);
                        break;
                    case 64:
                        bits.WriteInt64((long)InternalValue);
                        break;
                    default:
                        {
                            byte[] buff = bits.bitConverter.GetBytes((long)(uint)InternalValue, (int)(lengthAsBits / 8) + 1);
                            byte lastByte = buff[buff.Length - 1];
                            bits.WriteBytes(buff, 0, buff.Length - 1);
                            bits.WriteBits(lastByte, (int)lengthAsBits % 8);
                            break;
                        }
                }
            }
            else
            {
                switch (lengthAsBits)
                {
                    case 8:
                        bits.WriteUInt8((byte)(uint)InternalValue);
                        break;
                    case 16:
                        bits.WriteUInt16((ushort)(uint)InternalValue);
                        break;
                    case 32:
                        bits.WriteUInt32((uint)InternalValue);
                        break;
                    case 64:
                        bits.WriteUInt64((ulong)InternalValue);
                        break;
                    default:
                        {
                            byte[] buff = bits.bitConverter.GetBytes((long)(uint)InternalValue, (int)(lengthAsBits / 8) + 1);
                            byte lastByte = buff[buff.Length - 1];
                            bits.WriteBytes(buff, 0, buff.Length - 1);
                            bits.WriteBits(lastByte, (int)lengthAsBits % 8);
                            break;
                        }
                }
            }

            //if (lengthAsBits % 8 == 0)
            //{
            //    bits.WriteBytes(bits.bitConverter.GetBytes((long)(uint)InternalValue, (int)lengthAsBits / 8));
            //}
            //else
            //{
            //    byte[] buff = bits.bitConverter.GetBytes((long)(uint)InternalValue, (int)(lengthAsBits / 8) + 1);
            //    byte lastByte = buff[buff.Length - 1];
            //    bits.WriteBytes(buff, 0, buff.Length - 1);
            //    bits.WriteBits(lastByte, (int)lengthAsBits % 8);
            //}

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

    public override void SetParameter(string parameterName, object value)
    {
      switch (parameterName)
      {
        case "name":
          this.name = (string)value;
          break;
        case "size":
          this.length = (long)value;
          break;
        case "signed":
          this.Signed = (bool)value;
          break;
        case "endian":
          this.LittleEndian = ((string)value == "little");
          break;
        default:
          throw new PeachException(System.String.Format("Parameter '{0}' does not exist in Peach.Core.Dom.Number", parameterName));
      }
    }
	}
}

// end
