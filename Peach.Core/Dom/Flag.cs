
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
	[DataElement("Flag")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	[ParameterAttribute("position", typeof(int), "Bit position of flag", true)]
	[ParameterAttribute("size", typeof(int), "size in bits", true)]
	[Serializable]
	public class Flag : DataElement
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
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

		public override void Crack(DataCracker context, BitStream data)
		{
			Flag element = this;

			logger.Trace("Crack: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			var defaultValue = new Variant(data.ReadBits(element.size));

			if (element.isToken)
				if (defaultValue != element.DefaultValue)
					throw new CrackingFailure("Flag '" + element.name + "' marked as token, values did not match '" + (string)defaultValue + "' vs. '" + (string)element.DefaultValue + "'.", element, data);

			element.DefaultValue = defaultValue;
		}

		public static DataElement PitParser(PitParser context, XmlNode node, Flags parent)
		{
			if (node.Name == "Flag")
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
