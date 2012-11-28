
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
	[DataElement("Flags")]
	[PitParsable("Flags")]
	[DataElementChildSupported(DataElementTypes.NonDataElements)]
	[DataElementChildSupported("Flag")]
	[Parameter("name", typeof(string), "", "")]
	[Parameter("size", typeof(uint), "size in bits.  Typically [8, 16, 24, 32, 64]")]
	[Parameter("endian", typeof(string), "Byte order of number (default 'little')", "little")]
	[Serializable]
	public class Flags : DataElementContainer
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected int _size = 0;
		protected bool _littleEndian = true;

		public Flags()
		{
		}

		public Flags(string name)
			: base(name)
		{
		}

		public override void Crack(DataCracker context, BitStream data)
		{
			Flags element = this;

			logger.Trace("Crack: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			if (data.LengthBits <= (data.TellBits() + element.size))
				throw new CrackingFailure("Not enough data to crack '" + element.fullName + "'.", element, data);

			long startPos = data.TellBits();

			foreach (DataElement child in element)
			{
				if (!(child is Flag))
					throw new CrackingFailure("Found non-Flag child!", this, data);

				data.SeekBits(startPos, System.IO.SeekOrigin.Begin);
				data.SeekBits(((Flag)child).position, System.IO.SeekOrigin.Current);
				((Flag)child).Crack(context, data);
			}

			// Make sure we land at end of Flags
			data.SeekBits(startPos, System.IO.SeekOrigin.Begin);
			data.SeekBits((int)element.size, System.IO.SeekOrigin.Current);
		}

		public static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			if (node.Name != "Flags")
				return null;

			var flags = DataElement.Generate<Flags>(node);


			string strSize = node.getAttribute("size");
			if (strSize == null)
				strSize = context.getDefaultAttribute(typeof(Flags), "size");
			if (strSize == null)
				throw new PeachException("Error, Flags elements must have 'size' attribute!");

			int size;

			if (!int.TryParse(strSize, out size))
				throw new PeachException("Error, " + flags.name + " size attribute is not valid number.");

			if (size < 1 || size > 64)
				throw new PeachException(string.Format("Error, unsupported size {0} for element {1}.", size, flags.name));

			flags.size = size;

			string strEndian = node.getAttribute("endian");
			if (strEndian == null)
				strEndian = context.getDefaultAttribute(typeof(Flags), "endian");

			if (strEndian != null)
			{
				switch (strEndian.ToLower())
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
						throw new PeachException(
							string.Format("Error, unsupported value \"{0}\" for \"endian\" attribute on field \"{1}\".", strEndian, flags.name));
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

			// Expand to 'size' bits
			bits.WriteBits(0, size);

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

			return new Variant(bits);
		}

    public override object GetParameter(string parameterName)
    {
      switch (parameterName)
      {
        case "name":
          return this.name;
        case "size":
          return this.size;
        case "endian":
          return this.LittleEndian ? "little" : "big";
        default:
          throw new PeachException(System.String.Format("Parameter '{0}' does not exist in Peach.Core.Dom.Flags", parameterName));
      }
    }
	}
}

// end
