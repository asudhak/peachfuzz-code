
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
	[ParameterAttribute("size", typeof(uint), "size in bits [8, 16, 24, 32, 64]", true)]
	[ParameterAttribute("signed", typeof(bool), "Is number signed (default false)", false)]
	[ParameterAttribute("endian", typeof(string), "Byte order of number (default 'little')", false)]
	[Serializable]
	public class Number : DataElement
	{
		protected int _size = 8;
		protected ulong _max = (ulong)sbyte.MaxValue;
		protected long _min = sbyte.MinValue;
		protected bool _signed = true;
		protected bool _isLittleEndian = true;

		public Number()
			: base()
		{
			DefaultValue = new Variant(0);
		}

		public Number(string name)
			: base(name)
		{
			DefaultValue = new Variant(0);
		}

		public Number(string name, long value, int size)
			: base(name)
		{
			_size = size;
			DefaultValue = new Variant(value);
		}

		public Number(string name, long value, int size, bool signed, bool isLittleEndian)
			: base(name)
		{
			_size = size;
			_signed = signed;
			_isLittleEndian = isLittleEndian;
			DefaultValue = new Variant(value);
		}

		public override int length
		{
			get
			{
				return _size / 8;
			}
			set
			{
				throw new NotSupportedException("A numbers size must be set by size.");
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

		public override LengthType lengthType
		{
			get { return LengthType.String; }
			set { throw new NotSupportedException("Cannot set LengthType on a Number."); }
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

		public int Size
		{
			get { return _size; }
			set
			{
				if (value == 0)
					throw new ApplicationException("size must be > 0");

				_size = value;

				if (_signed)
				{
					_max = (ulong)Math.Pow(2, _size) / 2;
					_min = 0 - ((long)Math.Pow(2, _size) / 2);
				}
				else
				{
					_max = (ulong)Math.Pow(2, _size) - 1;
					_min = 0;
				}

				Invalidate();
			}
		}

		public bool Signed
		{
			get { return _signed; }
			set
			{
				_signed = value;
				Size = Size;

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
				switch (Size)
				{
					case 8:
						bits.WriteInt8((sbyte)InternalValue);
						break;
					case 16:
						bits.WriteInt16((short)InternalValue);
						break;
					case 24:
						throw new NotImplementedException("Doh!");
					//bits.WriteInt24((sbyte)InternalValue);
					//break;
					case 32:
						bits.WriteInt32((int)InternalValue);
						break;
					case 64:
						bits.WriteInt64((long)InternalValue);
						break;
					default:
						throw new NotImplementedException("Urm, yah");
				}
			}
			else
			{
				switch (Size)
				{
					case 8:
						bits.WriteUInt8((byte)(uint)InternalValue);
						break;
					case 16:
						bits.WriteUInt16((ushort)(uint)InternalValue);
						break;
					case 24:
						throw new NotImplementedException("Doh!");
					//bits.WriteInt24((sbyte)InternalValue);
					//break;
					case 32:
						bits.WriteUInt32((uint)InternalValue);
						break;
					case 64:
						bits.WriteUInt64((ulong)InternalValue);
						break;
					default:
						throw new NotImplementedException("Urm, yah");
				}
			}

			
			

			return bits;
		}
	}
}

// end
