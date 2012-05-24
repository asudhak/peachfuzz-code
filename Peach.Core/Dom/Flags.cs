
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

namespace Peach.Core.Dom
{
	[DataElement("Flags")]
	[PitParsable("Flags")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	[DataElementChildSupportedAttribute("Flag")]
	[ParameterAttribute("size", typeof(uint), "size in bits.  Typically [8, 16, 24, 32, 64]", true)]
	[ParameterAttribute("endian", typeof(string), "Byte order of number (default 'little')", false)]
	[Serializable]
	public class Flags : DataElementContainer
	{
		protected int _size = 0;
		protected bool _littleEndian = true;

		public Flags()
		{
		}

		public Flags(string name)
		{
			this.name = name;
		}

		public Flags(string name, int size)
		{
			this.name = name;
			this.size = size;
		}

		public Flags(int size)
		{
			this.size = size;
		}

		public static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			if (node.Name != "Flags")
				return null;

			var flags = new Flags();

			if (context.hasXmlAttribute(node, "name"))
				flags.name = context.getXmlAttribute(node, "name");

			if (context.hasXmlAttribute(node, "size"))
			{
				int size;
				try
				{
					size = int.Parse(context.getXmlAttribute(node, "size"));
				}
				catch
				{
					throw new PeachException("Error, " + flags.name + " size attribute is not valid number.");
				}

				if (size < 1 || size > 64)
					throw new PeachException(string.Format(
						"Error, unsupported size {0} for element {1}.", size, flags.name));

				flags.size = size;
			}
			else if (context.hasDefaultAttribute(typeof(Flags), "size"))
			{
				int size;
				try
				{
					size = int.Parse((string)context.getDefaultAttribute(typeof(Flags), "size"));
				}
				catch
				{
					throw new PeachException("Error, " + flags.name + " size attribute is not valid number.");
				}

				if (size < 1 || size > 64)
					throw new PeachException(string.Format(
						"Error, unsupported size {0} for element {1}.", size, flags.name));

				flags.size = size;
			}

			if (context.hasXmlAttribute(node, "endian"))
			{
				string endian = context.getXmlAttribute(node, "endian").ToLower();
				switch (endian)
				{
					case "little":
						flags.LittleEndian = true;
						break;
					case "big":
						flags.LittleEndian = false;
						break;
					case "network":
						flags.LittleEndian = false;
						break;
					default:
						throw new PeachException(string.Format(
							"Error, unsupported value \"{0}\" for \"endian\" attribute on field \"{1}\".", endian, flags.name));
				}
			}
			else if (context.hasDefaultAttribute(typeof(Flags), "endian"))
			{
				string endian = ((string)context.getDefaultAttribute(typeof(Flags), "endian")).ToLower();
				switch (endian)
				{
					case "little":
						flags.LittleEndian = true;
						break;
					case "big":
						flags.LittleEndian = false;
						break;
					case "network":
						flags.LittleEndian = false;
						break;
					default:
						throw new PeachException(string.Format(
							"Error, unsupported value \"{0}\" for \"endian\" attribute on field \"{1}\".", endian, flags.name));
				}
			}

			context.handleCommonDataElementAttributes(node, flags);
			context.handleCommonDataElementChildren(node, flags);

			foreach (XmlNode child in node.ChildNodes)
			{
				// Looking for "Flag" element
				if (child.Name == "Flag")
				{
					flags.Add(Flag.PitParser(context, child, flags));
				}
			}

			return flags;
		}

		public bool LittleEndian
		{
			get { return _littleEndian; }
			set
			{
				_littleEndian = value;
				Invalidate();
			}
		}

		public int size
		{
			get { return _size; }
			set
			{
				_size = value;
				Invalidate();
			}
		}

		public override Variant GenerateInternalValue()
		{
			BitStream bits = new BitStream();

			foreach (DataElement child in this)
			{
				if (child is Flag)
				{
					bits.SeekBits(((Flag)child).position, System.IO.SeekOrigin.Begin);
					bits.Write(child.Value, child);
				}
				else
					throw new ApplicationException("Flag has child thats not a flag!");
			}

			_internalValue = new Variant(bits);
			return _internalValue;
		}

	}

	[DataElement("Flag")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	[ParameterAttribute("position", typeof(int), "Bit position of flag", true)]
	[ParameterAttribute("size", typeof(int), "size in bits", true)]
	[Serializable]
	public class Flag : DataElement
	{
		protected int _size = 0;
		protected int _position = 0;

		public Flag()
		{
		}

		public Flag(string name)
		{
			this.name = name;
		}

		public Flag(string name, int size, int position)
		{
			this.name = name;
			this.size = size;
			this.position = position;
		}

		public Flag(int size, int position)
		{
			this.size = size;
			this.position = position;
		}

		public static DataElement PitParser(PitParser context, XmlNode node, Flags parent)
		{
			if(node.Name == "Flag")
				return null;

			var flag = new Flag();

			if (context.hasXmlAttribute(node, "name"))
				flag.name = context.getXmlAttribute(node, "name");

			if (context.hasXmlAttribute(node, "position"))
				flag.position = int.Parse(context.getXmlAttribute(node, "position"));
			else
				throw new PeachException("Error, Flag elements must have 'position' attribute!");

			if (context.hasXmlAttribute(node, "size"))
			{
				try
				{
					flag.size = int.Parse(context.getXmlAttribute(node, "size"));
				}
				catch (Exception e)
				{
					throw new PeachException("Error parsing Flag size attribute: " + e.Message);
				}
			}
			else
				throw new PeachException("Error, Flag elements must have 'position' attribute!");

			context.handleCommonDataElementAttributes(node, flag);
			context.handleCommonDataElementChildren(node, flag);
			context.handleCommonDataElementValue(node, flag);

			return flag;
		}

		public int size
		{
			get { return _size; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Should not be null");
				_size = value;
				Invalidate();
			}
		}

		public int position
		{
			get { return _position; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Should not be null");
				_position = value;
				Invalidate();
			}
		}

		protected override BitStream InternalValueToBitStream(Variant v)
		{
			BitStream bits = new BitStream();

			if (v == null)
				bits.WriteBits((ulong)0, size);
			else
				bits.WriteBits((ulong)v, size);

			return bits;
		}
	}
}

// end
