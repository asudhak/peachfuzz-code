
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
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Reflection;
using System.Runtime.Serialization;
using Peach.Core.IO;

namespace Peach.Core.Dom
{
	/// <summary>
	/// A numerical data element.
	/// </summary>
	[DataElement("Number")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	[ParameterAttribute("size", typeof(uint), "size in bits", true)]
	[ParameterAttribute("signed", typeof(bool), "Is number signed (default false)", false)]
	[ParameterAttribute("endian", typeof(string), "Byte order of number (default 'little')", false)]
	[Serializable]
	public class Number : DataElement
	{
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

			if (lengthAsBits % 8 == 0)
			{
				bits.WriteBytes(bits.bitConverter.GetBytes((long)(uint)InternalValue, (int)lengthAsBits / 8));
			}
			else
			{
				byte[] buff = bits.bitConverter.GetBytes((long)(uint)InternalValue, (int)(lengthAsBits / 8) + 1);
				byte lastByte = buff[buff.Length - 1];
				bits.WriteBytes(buff, 0, buff.Length - 1);
				bits.WriteBits(lastByte, (int)lengthAsBits % 8);
			}

			//if (Signed)
			//{
			//    switch (lengthAsBits)
			//    {
			//        case 8:
			//            bits.WriteInt8((sbyte)InternalValue);
			//            break;
			//        case 16:
			//            bits.WriteInt16((short)InternalValue);
			//            break;
			//        case 24:
			//            throw new NotImplementedException("Doh!");
			//        //bits.WriteInt24((sbyte)InternalValue);
			//        //break;
			//        case 32:
			//            bits.WriteInt32((int)InternalValue);
			//            break;
			//        case 64:
			//            bits.WriteInt64((long)InternalValue);
			//            break;
			//        default:
			//            throw new NotImplementedException("Urm, yah");
			//    }
			//}
			//else
			//{
			//    if (lengthAsBits % 8 == 0)
			//    {
			//        bits.WriteBytes(bits.bitConverter.GetBytes((long)(uint)InternalValue, (int)lengthAsBits / 8));
			//    }
			//    else
			//    {
			//        byte [] buff = bits.bitConverter.GetBytes((long)(uint)InternalValue, (int)(lengthAsBits/8)+1);
			//        byte lastByte = buff[buff.Length - 1];
			//        bits.WriteBytes(buff, 0, buff.Length - 1);
			//        bits.WriteBits(lastByte, (int)lengthAsBits % 8);
			//    }
			//}

			return bits;
		}
	}
}

// end
