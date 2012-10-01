
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
		Ascii,
		Utf7,
		Utf8,
		Utf16,
		Utf16be,
		Utf32
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
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
  [ParameterAttribute("name", typeof(string), "", true)]
	[ParameterAttribute("length", typeof(uint), "Length in characters", false)]
	[ParameterAttribute("nullTerminated", typeof(bool), "Is string null terminated?", false)]
	[ParameterAttribute("type", typeof(StringType), "Type of string (encoding)", true)]
	[Serializable]
	public class String : DataElement
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected StringType _type = StringType.Ascii;
		protected bool _nullTerminated = false;
		protected char _padCharacter = '\0';

		public String()
			: base()
		{
			DefaultValue = new Variant("Peach");
		}

		public String(string name)
			: base(name)
		{
			DefaultValue = new Variant("Peach");
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

		public override void Crack(DataCracker context, BitStream data)
		{
			String element = this;

			logger.Trace("Crack: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			if (element.nullTerminated)
			{
				// Locate NULL character in stream
				bool foundNull = false;
				bool twoNulls = element.stringType == StringType.Utf16 || element.stringType == StringType.Utf16be;
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
				string strValue = ASCIIEncoding.GetEncoding(element.stringType.ToString()).GetString(value);
				element.DefaultValue = new Variant(strValue);

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

			var defaultValue = new Variant(
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

			string type = null;
			if (node.hasAttribute("type"))
				type = node.getAttribute("type");
			else if (context.hasDefaultAttribute(str.GetType(), "type"))
				type = context.getDefaultAttribute(str.GetType(), "type");

			if (type != null)
			{
				switch (type.ToLower())
				{
					case "ascii":
						str.stringType = StringType.Ascii;
						break;
					case "utf16":
						str.stringType = StringType.Utf16;
						break;
					case "utf16be":
						str.stringType = StringType.Utf16be;
						break;
					case "utf32":
						str.stringType = StringType.Utf32;
						break;
					case "utf7":
						str.stringType = StringType.Utf7;
						break;
					case "utf8":
						str.stringType = StringType.Utf8;
						break;
					default:
						throw new PeachException("Error, unknown String type '" + type + "' on element '" + str.name + "'.");
				}
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

			// handle NumericalString hint properly
			if (str.DefaultValue.GetVariantType() == Variant.VariantType.BitStream || str.DefaultValue.GetVariantType() == Variant.VariantType.ByteString)
			{
				Encoding enc = null;
				switch (type)
				{
					case "ascii":
						enc = Encoding.ASCII;
						break;
					case "utf16":
						enc = Encoding.Unicode;
						break;
					case "utf16be":
						enc = Encoding.BigEndianUnicode;
						break;
					case "utf32":
						enc = (Encoding)Encoding.UTF32;
						break;
					case "utf7":
						enc = (Encoding)Encoding.UTF7;
						break;
					case "utf8":
						enc = (Encoding)Encoding.UTF8;
						break;
					default:
						type = "ascii";
						enc = (Encoding)Encoding.ASCII;
						break;
				}
				byte[] val = (byte[])str.DefaultValue;
				string asStr = enc.GetString(val);
				if (!val.SequenceEqual(enc.GetBytes(asStr)))
					throw new PeachException("String value contains invalid " + type + " bytes.");
				str.DefaultValue = new Variant(asStr);
			}

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

			if (_type == StringType.Ascii)
				value = Encoding.ASCII.GetBytes((string)v);

			else if (_type == StringType.Utf7)
				value = Encoding.UTF7.GetBytes((string)v);

			else if (_type == StringType.Utf8)
				value = Encoding.UTF8.GetBytes((string)v);

			else if (_type == StringType.Utf16)
				value = Encoding.Unicode.GetBytes((string)v);

			else if (_type == StringType.Utf16be)
				value = Encoding.BigEndianUnicode.GetBytes((string)v);

			else if (_type == StringType.Utf32)
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
						_length = value * 8; // TODO - This is a bug!
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

    //[ParameterAttribute("length", typeof(uint), "Length in characters", false)]
    //[ParameterAttribute("nullTerminated", typeof(bool), "Is string null terminated?", false)]
    //[ParameterAttribute("type", typeof(StringType), "Type of string (encoding)", true)]
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
