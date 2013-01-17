
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
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.IO;

using NLog;

namespace Peach.Core.Dom
{
	public enum StringType
	{
		ascii,
		utf7,
		utf8,
		utf16,
		utf16be,
		utf32
	}
	
	/// <summary>
	/// String data element.  String elements support numerouse encodings
	/// such as straight ASCII through UTF-32.  Both little and big endian
	/// strings are supported.
	/// 
	/// Strings also support standard attributes such as length, null termination,
	/// etc.
	/// </summary>
	[DataElement("String")]
	[PitParsable("String")]
	[DataElementChildSupported(DataElementTypes.NonDataElements)]
	[Parameter("name", typeof(string), "Element name", "")]
	[Parameter("length", typeof(uint?), "Length in data element", "")]
	[Parameter("lengthType", typeof(LengthType), "Units of the length attribute", "bytes")]
	[Parameter("lengthCalc", typeof(string), "Scripting expression that evaluates to an integer", "")]
	[Parameter("nullTerminated", typeof(bool), "Is string null terminated?", "false")]
	[Parameter("type", typeof(StringType), "Type of string (encoding)", "ascii")]
	[Parameter("value", typeof(string), "Default value", "")]
	[Parameter("valueType", typeof(ValueType), "Format of value attribute", "string")]
	[Parameter("token", typeof(bool), "Is element a token", "false")]
	[Parameter("mutable", typeof(bool), "Is element mutable", "false")]
	[Parameter("constraint", typeof(string), "Scripting expression that evaluates to true or false", "")]
	[Parameter("minOccurs", typeof(int), "Minimum occurances", "1")]
	[Parameter("maxOccurs", typeof(int), "Maximum occurances", "1")]
	[Parameter("occurs", typeof(int), "Actual occurances", "1")]
	[Serializable]
	public class String : DataElement
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected StringType _type = StringType.ascii;
		protected bool _nullTerminated = false;
		protected char _padCharacter = '\0';
		protected Encoding encoding = null;

		public String()
			: base()
		{
			DefaultValue = new Variant("");
		}

		public String(string name)
			: base(name)
		{
			DefaultValue = new Variant("");
		}

		public String(string name, string defaultValue)
			: base(name)
		{
			DefaultValue = new Variant(defaultValue);
		}

		public String(string name, Variant defaultValue)
		{
			DefaultValue = defaultValue;
		}

		public String(string name, string defaultValue, StringType type, bool nullTerminated)
			: base(name)
		{
			DefaultValue = new Variant(defaultValue);
			_type = type;
			_nullTerminated = nullTerminated;
		}

		public String(string name, string defaultValue, StringType type, bool nullTerminated, int length)
			: base(name)
		{
			DefaultValue = new Variant(defaultValue);
			_type = type;
			_nullTerminated = nullTerminated;
			_length = length;
			_lengthType = LengthType.Bytes;
		}

		protected char ReadCharacter(BitStream data)
		{
			int maxBytes = UTF8Encoding.UTF8.GetMaxByteCount(1);
			byte[] buff = new byte[maxBytes];
			char[] chars = null;

			for (int count = 0; count < maxBytes; count++)
			{
				buff[count] = data.ReadByte();
				chars = UTF8Encoding.UTF8.GetChars(buff, 0, count+1);
				
				if (chars.Count() == 1)
					return chars[0];
			}

			throw new CrackingFailure("Unable to read character from stream", this, data);
		}

		/// <summary>
		/// TODO - Use ReadCharacter method when length is char length.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="data"></param>
		public override void Crack(DataCracker context, BitStream data)
		{
			String element = this;
			Variant defaultValue;

			logger.Trace("Crack: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			if (element.nullTerminated)
			{
				// Locate NULL character in stream
				bool foundNull = false;
				bool twoNulls = element.stringType == StringType.utf16 || element.stringType == StringType.utf16be;
				long currentPos = data.TellBits();

				for (long i = data.TellBytes(); i < data.LengthBytes; i++)
				{
					if (data.ReadByte() == 0)
					{
						if (twoNulls)
						{
							if (data.ReadByte() == 0)
							{
								foundNull = true;
								break;
							}
							else
							{
								data.SeekBits(-8, System.IO.SeekOrigin.Current);
								continue;
							}
						}
						else
						{
							foundNull = true;
							break;
						}
					}
				}

				if (!foundNull)
					throw new CrackingFailure("Did not locate NULL in data stream for String '" + element.fullName + "'.", element, data);

				long endPos = data.TellBits();

				// Do not include NULLs in our read.
				long byteCount = ((endPos - currentPos) / 8) - 1;
				if (twoNulls)
					byteCount--;

				data.SeekBits(currentPos, System.IO.SeekOrigin.Begin);
				byte[] value = data.ReadBytes(byteCount);
				string strValue = Encoding.GetEncoding(element.stringType.ToString()).GetString(value);
				defaultValue = new Variant(strValue);

				if (element.isToken)
					if (defaultValue != element.DefaultValue)
						throw new CrackingFailure("String marked as token, values did not match '" + ((string)defaultValue) + "' vs. '" + ((string)element.DefaultValue) + "'.", element, data);

				element.DefaultValue = defaultValue;

				// Now skip past nulls
				if (twoNulls)
					data.SeekBits(16, System.IO.SeekOrigin.Current);
				else
					data.SeekBits(8, System.IO.SeekOrigin.Current);

				return;
			}

			// String length in bytes
			long? stringLength = null;

			// TODO - Make both length and size for strings.  Length is always in chars.
			if (stringLength == null && element.isToken)
			{
				if (element.DefaultValue == null)
					throw new PeachException("Error, element \"" + element.fullName + "\" is a token but has no default value.");

				stringLength = ((string)element.DefaultValue).Length;
			}

			if(stringLength == null)
				stringLength = (context.determineElementSize(element, data) / 8);

			if (stringLength == null)
				throw new CrackingFailure("Unable to crack '" + element.fullName + "'.", element, data);

			data.WantBytes((long)stringLength);

			if ((data.TellBytes() + stringLength) > data.LengthBytes)
				throw new CrackingFailure("String '" + element.fullName +
					"' has length of '" + stringLength + "' but buffer only has '" +
					(data.LengthBytes - data.TellBytes()) + "' bytes left.", element, data);

			defaultValue = new Variant(
				ASCIIEncoding.GetEncoding(element.stringType.ToString()).GetString(
				data.ReadBytes((int)stringLength)));

			if (element.isToken)
				if (defaultValue != element.DefaultValue)
					throw new CrackingFailure("String marked as token, values did not match '" + ((string)defaultValue) + "' vs. '" + ((string)element.DefaultValue) + "'.", element, data);

			element.DefaultValue = defaultValue;

			string str = (string)defaultValue;
			if(str.Length > 50)
				str = str.Substring(0, 50);

			logger.Debug("String's value is: " + str);

		}

		public static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			if (node.Name != "String")
				return null;

			var str = DataElement.Generate<String>(node);

			if (node.hasAttribute("nullTerminated"))
				str.nullTerminated = node.getAttributeBool("nullTerminated", false);
			else if (context.hasDefaultAttribute(typeof(String), "nullTerminated"))
				str.nullTerminated = context.getDefaultAttributeAsBool(typeof(String), "nullTerminated", false);

			if (str.nullTerminated && node.hasAttribute("length"))
				throw new PeachException("Error, String element '" + str.name + "' can have a length or be null terminated, but not both.");

			string type = "ascii";
			if (node.hasAttribute("type"))
				type = node.getAttribute("type");
			else if (context.hasDefaultAttribute(str.GetType(), "type"))
				type = context.getDefaultAttribute(str.GetType(), "type");

			switch (type.ToLower())
			{
				case "ascii":
					str.stringType = StringType.ascii;
					str.encoding = Encoding.ASCII;
					break;
				case "utf16":
					str.stringType = StringType.utf16;
					str.encoding = Encoding.Unicode;
					break;
				case "utf16be":
					str.stringType = StringType.utf16be;
					str.encoding = Encoding.BigEndianUnicode;
					break;
				case "utf32":
					str.stringType = StringType.utf32;
					str.encoding = Encoding.UTF32;
					break;
				case "utf7":
					str.stringType = StringType.utf7;
					str.encoding = Encoding.UTF7;
					break;
				case "utf8":
					str.stringType = StringType.utf8;
					str.encoding = Encoding.UTF8;
					break;
				default:
					throw new PeachException("Error, unknown String type '" + type + "' on element '" + str.name + "'.");
			}

			if (node.hasAttribute("padCharacter"))
			{
				str.padCharacter = node.getAttribute("padCharacter")[0];
			}
			else if (context.hasDefaultAttribute(str.GetType(), "padCharacter"))
			{
				str.padCharacter = context.getDefaultAttribute(str.GetType(), "padCharacter")[0];
			}

			if (node.hasAttribute("tokens")) // This item has a default!
				throw new NotSupportedException("Tokens attribute is depricated in Peach 3.  Use parameter to StringToken analyzer isntead.");

			if (node.hasAttribute("analyzer")) // this should be passed via a child element me things!
				throw new NotSupportedException("Analyzer attribute is depricated in Peach 3.  Use a child element instead.");

			context.handleCommonDataElementAttributes(node, str);
			context.handleCommonDataElementValue(node, str);
			context.handleCommonDataElementChildren(node, str);

			if (!node.hasAttribute("value"))
				str.DefaultValue = new Variant("");

			int test;
			if (int.TryParse((string)str.DefaultValue, out test))
			{
				if (!str.Hints.ContainsKey("NumericalString"))
					str.Hints.Add("NumericalString", new Hint("NumericalString", "true"));
			}
			else
			{
				if (str.Hints.ContainsKey("NumericalString"))
					str.Hints.Remove("NumericalString");
			}

			return str;
		}

		public override Variant DefaultValue
		{
			get
			{
				return base.DefaultValue;
			}
			set
			{
				string final = null;

				if (value.GetVariantType() == Variant.VariantType.BitStream || value.GetVariantType() == Variant.VariantType.ByteString)
				{
					byte[] val = (byte[])value;
					final = encoding.GetString(val);
					if (!val.SequenceEqual(encoding.GetBytes(final)))
						throw new PeachException("String value contains invalid " + stringType + " bytes.");
				}
				else
				{
					final = (string)value;
				}

				if (_hasLength)
				{
					var lenType = lengthType;
					var len = length;

					if (lenType == LengthType.Chars)
					{
						len /= 8;

						if (NeedsExpand(final.Length, len, nullTerminated, final))
						{
							if (nullTerminated)
								len -= 1;

							final += MakePad((int)len - final.Length);
						}
					}
					else
					{
						if (lenType == LengthType.Bits)
						{
							if ((len % 8) != 0)
								throw new PeachException("Error, {2} string '{0}' has invalid length of {1} bits.", name, len, stringType);

							len = len / 8;
							lenType = LengthType.Bytes;
						}

						System.Diagnostics.Debug.Assert(lenType == LengthType.Bytes);

						int actual = encoding.GetByteCount(final);

						if (NeedsExpand(actual, len, nullTerminated, final))
						{
							int nullLen = encoding.GetByteCount("\0");
							int padLen = encoding.GetByteCount(new char[1] { padCharacter });

							int grow = (int)len - actual;

							if (nullTerminated)
								grow -= nullLen;

							if (grow < 0 || (grow % padLen) != 0)
								throw new PeachException("Error, can not satisfy length requirement of {1} {2} when padding {3} string '{0}'.",
									name, lengthType == LengthType.Bits ? len * 8 : len, lengthType.ToString().ToLower(), stringType);

							final += MakePad(grow / padLen);
						}
					}
				}
				else if (nullTerminated && !final.EndsWith("\0"))
				{
					final += "\0";
				}

				base.DefaultValue = new Variant(final);
			}
		}

		private string MakePad(int numPadChars)
		{
			string ret = new string(padCharacter, numPadChars);
			if (nullTerminated)
				ret += '\0';
			return ret;
		}

		private bool NeedsExpand(int actual, long desired, bool nullTerm, string value)
		{
			if (actual > desired)
				throw new PeachException("Error, value of {3} string '{0}' is longer than the specified length of {1} {2}.",
					name, lengthType == LengthType.Bits ? desired * 8 : desired, lengthType.ToString().ToLower(), stringType);

			if (actual == desired)
			{
				if (nullTerm && !value.EndsWith("\0"))
					throw new PeachException("Error, adding null terminator to {3} string '{0}' makes it longer than the specified length of {1} {2}.",
						name, lengthType == LengthType.Bits ? desired * 8 : desired, lengthType.ToString().ToLower(), stringType);

				return false;
			}

			return true;
		}

		/// <summary>
		/// String type/encoding to be used.  Default is 
		/// ASCII.
		/// </summary>
		public StringType stringType
		{
			get { return _type; }
			set { _type = value; }
		}

		/// <summary>
		/// Is string null terminated?  For ASCII strings this
		/// is a single NULL characters, for WCHAR's, two NULL 
		/// characters are used.
		/// </summary>
		public bool nullTerminated
		{
			get { return _nullTerminated; }
			set
			{
				_nullTerminated = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Pad character for string.  Defaults to NULL.
		/// </summary>
		public char padCharacter
		{
			get { return _padCharacter; }
			set
			{
				_padCharacter = value;
				Invalidate();
			}
		}

		protected override BitStream InternalValueToBitStream()
		{
			byte[] value = null;

			if ((mutationFlags & DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM) != 0 && MutatedValue != null)
				return (BitStream)MutatedValue;

			Variant v = InternalValue;

			if (_type == StringType.ascii)
				value = Encoding.ASCII.GetBytes((string)v);

			else if (_type == StringType.utf7)
				value = Encoding.UTF7.GetBytes((string)v);

			else if (_type == StringType.utf8)
				value = Encoding.UTF8.GetBytes((string)v);

			else if (_type == StringType.utf16)
				value = Encoding.Unicode.GetBytes((string)v);

			else if (_type == StringType.utf16be)
				value = Encoding.BigEndianUnicode.GetBytes((string)v);

			else if (_type == StringType.utf32)
				value = Encoding.UTF32.GetBytes((string)v);

			else
				throw new ApplicationException("String._type not set properly!");

			return new BitStream(value);
		}

		/// <summary>
		/// Length of element in bits.
		/// </summary>
		/// <remarks>
		/// In the case that LengthType == "Calc" we will evaluate the
		/// expression.
		/// </remarks>
		public override long length
		{
			get
			{
				if (_lengthCalc != null)
				{
					Dictionary<string, object> scope = new Dictionary<string, object>();
					scope["self"] = this;
					return (int)Scripting.EvalExpression(_lengthCalc, scope);
				}

				if (_hasLength)
				{
					switch (_lengthType)
					{
						case LengthType.Bytes:
							return _length / 8;
						case LengthType.Bits:
							return _length;
						case LengthType.Chars:
							return _length;
						default:
							throw new NotSupportedException("Error calculating length.");
					}
				}
				else
				{
					switch (_lengthType)
					{
						case LengthType.Bytes:
							return Value.LengthBytes;
						case LengthType.Bits:
							return Value.LengthBits;
						case LengthType.Chars:
							if (InternalValue.GetVariantType() == Variant.VariantType.String)
							{
								return ((string)InternalValue).Length;
							}
							else
							{
								// Assume byte length is greater or equal to string char count
								return Value.LengthBytes;
							}
						default:
							throw new NotSupportedException("Error calculating length.");
					}

				}
			}

			set
			{
				switch (_lengthType)
				{
					case LengthType.Bytes:
						_length = value * 8;
						break;
					case LengthType.Bits:
						_length = value;
						break;
					case LengthType.Chars:
						_length = value * 8;
						break;
				}

				_hasLength = true;
			}
		}

		/// <summary>
		/// Returns length as bits.
		/// </summary>
		public override long lengthAsBits
		{
			get
			{
				switch (_lengthType)
				{
					case LengthType.Bytes:
						return length * 8;
					case LengthType.Bits:
						return length;
					case LengthType.Chars:
						return Value.LengthBits;
					default:
						throw new NotSupportedException("Error calculating length.");
				}
			}
		}

    public override object GetParameter(string parameterName)
    {
      switch (parameterName)
      {
        case "name":
          return this.name;
        case "length":
          return this.length;
        case "nullTerminated":
          return this.nullTerminated;
        case "type":
          return this.stringType;
        default:
          throw new PeachException(System.String.Format("Parameter '{0}' does not exist in Peach.Core.Dom.String", parameterName));
      }
    }
	}

}

// end
