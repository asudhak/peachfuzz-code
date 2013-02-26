
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
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Peach.Core.IO;

namespace Peach.Core
{
	/// <summary>
	/// Variant class emulates untyped scripting languages
	/// variables were typing can change as needed.  This class
	/// solves the problem of boxing internal types.  Instead
	/// explicit casts are used to access the value as needed.
	/// 
	/// TODO: Investigate implicit casting as well.
	/// TODO: Investigate deligates for type -> byte[] conversion.
	/// </summary>
	[Serializable]
	public class Variant : IXmlSerializable
	{
		public enum VariantType
		{
			Unknown,
			Int,
			Long,
			ULong,
			String,
			ByteString,
			BitStream,
			Boolean
		}

		VariantType _type = VariantType.Unknown;
		bool? _valueBool;
		int? _valueInt;
		long? _valueLong;
		ulong? _valueULong;
		string _valueString;
		byte[] _valueByteArray;
		BitStream _valueBitStream = null;

		public Variant()
		{
		}

		public Variant(int v)
		{
			SetValue(v);
		}

		public Variant(long v)
		{
			SetValue(v);
		}

		public Variant(ulong v)
		{
			SetValue(v);
		}

		public Variant(string v)
		{
			SetValue(v);
		}

		public Variant(string v, string type)
		{
			switch (type.ToLower())
			{
				case "system.int32":
					SetValue(Int32.Parse(v));
					break;
				case "system.string":
					SetValue(v);
					break;
				case "system.boolean":
					SetValue(bool.Parse(v));
					break;
				default:
					throw new NotImplementedException("Value Type not implemented: " + type);
			}
		}

		public Variant(byte[] v)
		{
			SetValue(v);
		}

		public Variant(BitStream v)
		{
			SetValue(v);
		}

		public VariantType GetVariantType()
		{
			return _type;
		}

		public void SetValue(int v)
		{
			_type = VariantType.Int;
			_valueInt = v;
			_valueString = null;
			_valueBitStream = null;
			_valueByteArray = null;
		}

		public void SetValue(long v)
		{
			_type = VariantType.Long;
			_valueLong = v;
			_valueString = null;
			_valueBitStream = null;
			_valueByteArray = null;
		}

		public void SetValue(ulong v)
		{
			_type = VariantType.ULong;
			_valueULong = v;
			_valueString = null;
			_valueBitStream = null;
			_valueByteArray = null;
		}

		public void SetValue(string v)
		{
			_type = VariantType.String;
			_valueString = v;
			_valueBitStream = null;
			_valueByteArray = null;
		}

		public void SetValue(byte[] v)
		{
			_type = VariantType.ByteString;
			_valueByteArray = v;
			_valueString = null;
			_valueBitStream = null;
		}

		public void SetValue(BitStream v)
		{
			_type = VariantType.BitStream;
			_valueBitStream = v;
			_valueString = null;
			_valueByteArray = null;
		}

		public void SetValue(bool v)
		{
			_type = VariantType.Boolean;
			_valueBool = v;
			_valueString = null;
			_valueByteArray = null;
		}

		/// <summary>
		/// Access variant as an int value.
		/// </summary>
		/// <param name="v">Variant to cast</param>
		/// <returns>int representation of value</returns>
		public static explicit operator int(Variant v)
		{
			if (v == null)
				throw new ApplicationException("Parameter v is null");
			unchecked
			{
				switch (v._type)
				{
					case VariantType.Int:
						return (int)v._valueInt;
					case VariantType.Long:
						if (v._valueLong > int.MaxValue || v._valueLong < int.MinValue)
							throw new ApplicationException("Converting this long to an int would cause loss of data [" + v._valueLong + "]");

						return (int)v._valueLong;
					case VariantType.ULong:
						if (v._valueULong > int.MaxValue)
							throw new ApplicationException("Converting this ulong to an int would cause loss of data [" + v._valueULong + "]");

						return (int)v._valueULong;
					case VariantType.String:
						if (v._valueString == string.Empty)
							return 0;

						return Convert.ToInt32(v._valueString);
					case VariantType.ByteString:
						BitStream bs = new BitStream(v._valueByteArray);
						switch (bs.LengthBytes)
						{
							case 8:
								return (int)bs.ReadInt8();
							case 16:
								return (int)bs.ReadInt16();
							case 32:
								return bs.ReadInt32();
						}

						throw new NotSupportedException("Unable to convert byte[] to int type.");

					case VariantType.BitStream:
						if (v._valueInt != null)
							return (int)v._valueInt;
						if (v._valueLong != null)
							return (int)v._valueLong;
						if (v._valueULong != null)
							return (int)v._valueULong;

						throw new NotSupportedException("Unable to convert BitStream to int type.");
					default:
						throw new NotSupportedException("Unable to convert to unknown type.");
				}
			}
		}

		/// <summary>
		/// Access variant as an int value.
		/// </summary>
		/// <param name="v">Variant to cast</param>
		/// <returns>int representation of value</returns>
		public static explicit operator uint(Variant v)
		{
			if (v == null)
				throw new ApplicationException("Parameter v is null");
			unchecked
			{
				switch (v._type)
				{
					case VariantType.Int:
						if (v._valueLong < 0)
							throw new ApplicationException("Converting this long to an int would cause loss of data");

						return (uint)v._valueInt;
					case VariantType.Long:
						if (v._valueLong > uint.MaxValue || v._valueLong < uint.MinValue)
							throw new ApplicationException("Converting this long to an int would cause loss of data");

						return (uint)v._valueLong;
					case VariantType.ULong:
						if (v._valueULong > uint.MaxValue)
							throw new ApplicationException("Converting this ulong to an int would cause loss of data");

						return (uint)v._valueULong;
					case VariantType.String:
						if (v._valueString == string.Empty)
							return 0;

						return Convert.ToUInt32(v._valueString);
					case VariantType.ByteString:
						BitStream bs = new BitStream(v._valueByteArray);
						switch (bs.LengthBytes)
						{
							case 8:
								return (uint)bs.ReadUInt8();
							case 16:
								return (uint)bs.ReadUInt16();
							case 32:
								return bs.ReadUInt32();
						}

						throw new NotSupportedException("Unable to convert byte[] to int type.");

					case VariantType.BitStream:
						if (v._valueInt != null)
							return (uint)v._valueInt;
						if (v._valueLong != null)
							return (uint)v._valueLong;
						if (v._valueULong != null)
							return (uint)v._valueULong;

						throw new NotSupportedException("Unable to convert BitStream to int type.");
					default:
						throw new NotSupportedException("Unable to convert to unknown type.");
				}
			}
		}

		public static explicit operator long(Variant v)
		{
			if (v == null)
				throw new ApplicationException("Parameter v is null");

			unchecked
			{
				switch (v._type)
				{
					case VariantType.Int:
						unchecked
						{
							return (long)v._valueInt;
						}
					case VariantType.Long:
						unchecked
						{
							return (long)v._valueLong;
						}
					case VariantType.ULong:
						if (v._valueULong > long.MaxValue)
							throw new ApplicationException("Converting this ulong to a long would cause loss of data");

						unchecked
						{
							return (long)v._valueULong;
						}
					case VariantType.String:
						if (v._valueString == string.Empty)
							return 0;

						return Convert.ToInt64(v._valueString);
					case VariantType.ByteString:
						throw new NotSupportedException("Unable to convert byte[] to int type.");
					case VariantType.BitStream:
						throw new NotSupportedException("Unable to convert BitStream to int type.");
					default:
						throw new NotSupportedException("Unable to convert to unknown type.");
				}
			}
		}

		public static explicit operator ulong(Variant v)
		{
			if (v == null)
				throw new ApplicationException("Parameter v is null");

			unchecked
			{
				switch (v._type)
				{
					case VariantType.Int:
						return (ulong)v._valueInt;
					case VariantType.Long:
						if ((ulong)v._valueLong > ulong.MaxValue || v._valueLong < 0)
							throw new ApplicationException("Converting this long to a ulong would cause loss of data");

						return (ulong)v._valueLong;
					case VariantType.ULong:
						return (ulong)v._valueULong;
					case VariantType.String:
						if (v._valueString == string.Empty)
							return 0;

						return Convert.ToUInt64(v._valueString);
					case VariantType.ByteString:
						if (v._valueInt != null)
							return (ulong)v._valueInt;
						if (v._valueLong != null)
							return (ulong)v._valueLong;
						if (v._valueULong != null)
							return (ulong)v._valueULong;

						throw new NotSupportedException("Unable to convert byte[] to int type.");
					case VariantType.BitStream:
						if (v._valueInt != null)
							return (ulong)v._valueInt;
						if (v._valueLong != null)
							return (ulong)v._valueLong;
						if (v._valueULong != null)
							return (ulong)v._valueULong;

						throw new NotSupportedException("Unable to convert BitStream to int type.");
					default:
						throw new NotSupportedException("Unable to convert to unknown type.");
				}
			}
		}

		/// <summary>
		/// Access variant as string value.
		/// </summary>
		/// <param name="v">Variant to cast</param>
		/// <returns>string representation of value</returns>
		public static explicit operator string(Variant v)
		{
			if (v == null)
				throw new ApplicationException("Parameter v is null");

			switch (v._type)
			{
				case VariantType.Int:
					return Convert.ToString(v._valueInt);
				case VariantType.Long:
					return Convert.ToString(v._valueLong);
				case VariantType.ULong:
					return Convert.ToString(v._valueULong);
				case VariantType.String:
					return v._valueString;
				case VariantType.Boolean:
					return Convert.ToString(v._valueBool);
				case VariantType.ByteString:
					throw new NotSupportedException("Unable to convert byte[] to string type.");
				case VariantType.BitStream:
					throw new NotSupportedException("Unable to convert BitStream to string type.");
				default:
					throw new NotSupportedException("Unable to convert to unknown type.");
			}
		}

		/// <summary>
		/// Access variant as byte[] value.  This type is currently limited
		/// as neather int or string's are properly cast to byte[] since 
		/// additional information is needed.
		/// 
		/// TODO: Investigate using deligates to handle conversion.
		/// </summary>
		/// <param name="v">Variant to cast</param>
		/// <returns>byte[] representation of value</returns>
		public static explicit operator byte[](Variant v)
		{
			if (v == null)
				throw new ApplicationException("Parameter v is null");

			switch (v._type)
			{
				case VariantType.Int:
					throw new NotSupportedException("Unable to convert int to byte[] type.");
				case VariantType.Long:
					throw new NotSupportedException("Unable to convert long to byte[] type.");
				case VariantType.ULong:
					throw new NotSupportedException("Unable to convert ulong to byte[] type.");
				case VariantType.String:
					throw new NotSupportedException("Unable to convert string to byte[] type.");
				case VariantType.ByteString:
					return v._valueByteArray;
				case VariantType.BitStream:
					return v._valueBitStream.Value;
				default:
					throw new NotSupportedException("Unable to convert to unknown type.");
			}
		}

		public static explicit operator BitStream(Variant v)
		{
			if (v == null)
				throw new ApplicationException("Parameter v is null");

			switch (v._type)
			{
				case VariantType.Int:
					throw new NotSupportedException("Unable to convert int to BitStream type.");
				case VariantType.Long:
					throw new NotSupportedException("Unable to convert long to BitStream type.");
				case VariantType.ULong:
					throw new NotSupportedException("Unable to convert ulong to BitStream type.");
				case VariantType.String:
					throw new NotSupportedException("Unable to convert string to BitStream type.");
				case VariantType.ByteString:
					return new BitStream(v._valueByteArray);
				case VariantType.BitStream:
					return v._valueBitStream;
				default:
					throw new NotSupportedException("Unable to convert to unknown type.");
			}
		}

		public static explicit operator bool(Variant v)
		{
			if (v == null)
				throw new ApplicationException("Parameter v is null");

			switch (v._type)
			{
				case VariantType.Boolean:
					return v._valueBool.Value;
				case VariantType.Int:
					throw new NotSupportedException("Unable to convert int to bool type.");
				case VariantType.Long:
					throw new NotSupportedException("Unable to convert long to bool type.");
				case VariantType.ULong:
					throw new NotSupportedException("Unable to convert ulong to bool type.");
				case VariantType.String:
					throw new NotSupportedException("Unable to convert string to bool type.");
				case VariantType.ByteString:
					throw new NotSupportedException("Unable to convert byte[] to bool type.");
				case VariantType.BitStream:
					throw new NotSupportedException("Unable to convert BitStream to bool type.");
				default:
					throw new NotSupportedException("Unable to convert unknown to bool type.");
			}
		}

		public static bool operator ==(Variant a, Variant b)
		{
			if (((object)a == null) && ((object)b == null))
				return true;

			if (((object)a == null) || ((object)b == null))
				return false;

			try
			{
				string stra = (string)a;
				string strb = (string)b;

				if (stra.Equals(strb))
					return true;
				else
					return false;
			}
			catch { }

			byte[] aa = (byte[])a;
			byte[] bb = (byte[])b;

			if (aa.Length != bb.Length)
				return false;

			for (int cnt = 0; cnt < aa.Length; cnt++)
				if (aa[cnt] != bb[cnt])
					return false;

			return true;
		}

		public static bool operator !=(Variant a, Variant b)
		{
			return !(a == b);
		}

		private static string BitsToString(BitStream bs)
		{
			long end = Math.Min(32, bs.LengthBytes);
			if (end == 0)
				return "";

			long pos = bs.TellBits();
			bs.SeekBits(0, System.IO.SeekOrigin.Begin);
			byte[] buf = bs.ReadBitsAsBytes(end * 8);
			bs.SeekBits(pos, System.IO.SeekOrigin.Begin);

			StringBuilder ret = new StringBuilder();
			ret.AppendFormat("{0:x2}", buf[0]);

			for (long i = 1; i < end; ++i)
				ret.AppendFormat(" {0:x2}", buf[i]);

			if (end != bs.LengthBytes)
				ret.AppendFormat(".. (Len: {0} bits)", bs.LengthBits);

			return ret.ToString();
		}

		private static string BytesToString(byte[] buf)
		{
			if (buf.Length == 0)
				return "";

			StringBuilder ret = new StringBuilder();
			ret.AppendFormat("{0:x2}", buf[0]);

			int end = Math.Min(32, buf.Length);
			for (int i = 1; i < end; ++i)
				ret.AppendFormat(" {0:x2}", buf[i]);

			if (end != buf.Length)
				ret.AppendFormat(".. (Len: {0} bytes)", buf.Length);

			return ret.ToString();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			if (obj.GetType() != obj.GetType())
				return false;

			return ((Variant)obj) == this;
		}

		public override int GetHashCode()
		{
			switch (_type)
			{
				case VariantType.Int:
					return this._valueInt.GetHashCode();
				case VariantType.Long:
					return this._valueLong.GetHashCode();
				case VariantType.ULong:
					return this._valueULong.GetHashCode();
				case VariantType.String:
					return this._valueString.GetHashCode();
				case VariantType.ByteString:
					return _valueByteArray.GetHashCode();
				case VariantType.BitStream:
					return _valueBitStream.GetHashCode();
				default:
					return base.GetHashCode();
			}
		}

		public override string ToString()
		{
			switch (_type)
			{
				case VariantType.Int:
					return this._valueInt.ToString();
				case VariantType.Long:
					return this._valueLong.ToString();
				case VariantType.ULong:
					return this._valueULong.ToString();
				case VariantType.String:
					if (this._valueString.Length <= 80)
						return this._valueString.ToString();
					return _valueString.Substring(0, 64) + ".. (Len: " + _valueString.Length + " chars)";
				case VariantType.ByteString:
					return BytesToString(_valueByteArray);
				case VariantType.BitStream:
					return BitsToString(_valueBitStream);
				default:
					return base.ToString();
			}
		}

		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(System.Xml.XmlReader reader)
		{
			XmlSerializer serializer;

			if (!reader.Read())
				return;

			reader.ReadStartElement("type");
			_type = (VariantType) reader.ReadContentAsInt();
			reader.ReadEndElement();

			reader.ReadStartElement("value");
			
			switch (_type)
			{
				case VariantType.Int:
					_valueInt = reader.ReadContentAsInt();
					break;
				case VariantType.Long:
					_valueLong = reader.ReadContentAsLong();
					break;
				case VariantType.ULong:
					_valueULong = (ulong) reader.ReadContentAsLong();
					break;
				case VariantType.String:
					_valueString = reader.ReadContentAsString();
					break;
				case VariantType.ByteString:
					serializer = new XmlSerializer(typeof(byte[]));
					_valueByteArray = (byte[])serializer.Deserialize(reader);
					break;
				case VariantType.BitStream:
					serializer = new XmlSerializer(typeof(BitStream));
					_valueBitStream = (BitStream) serializer.Deserialize(reader);
					break;
			}

			reader.ReadEndElement();
		}

		public void WriteXml(System.Xml.XmlWriter writer)
		{
			XmlSerializer serializer;

			writer.WriteStartElement("type");
			writer.WriteValue((int)_type);
			writer.WriteEndElement();

			writer.WriteStartElement("value");

			switch (_type)
			{
				case VariantType.Int:
					writer.WriteValue(_valueInt);
					break;
				case VariantType.Long:
					writer.WriteValue(_valueLong);
					break;
				case VariantType.ULong:
					writer.WriteValue(_valueULong);
					break;
				case VariantType.String:
					writer.WriteValue(_valueString);
					break;
				case VariantType.ByteString:
					serializer = new XmlSerializer(typeof(byte[]));
					serializer.Serialize(writer, _valueByteArray);
					break;
				case VariantType.BitStream:
					serializer = new XmlSerializer(typeof(BitStream));
					serializer.Serialize(writer, _valueBitStream);
					break;
			}

			writer.WriteEndElement();
		}
	}
}
