
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
	[DataElementChildSupported(DataElementTypes.NonDataElements)]
	[Parameter("name", typeof(string), "Element name", "")]
	[Parameter("position", typeof(int), "Bit position of flag")]
	[Parameter("size", typeof(int), "size in bits")]
	[Parameter("value", typeof(string), "Default value", "")]
	[Parameter("valueType", typeof(ValueType), "Format of value attribute", "string")]
	[Parameter("token", typeof(bool), "Is element a token", "false")]
	[Parameter("mutable", typeof(bool), "Is element mutable", "false")]
	[Serializable]
	public class Flag : Number
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected int _size = 0;
		protected int _position = 0;

		public Flag()
		{
		}

		public Flag(string name)
			: base(name)
		{
		}

		public Flag(string name, int size, int position)
			: base(name)
		{
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

			logger.Debug("Crack: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			if (data.LengthBits < data.TellBits() + element.lengthAsBits)
				throw new CrackingFailure("Failed cracking Flag '" + element.fullName + "'.", element, data);

			Variant defaultValue = new Variant(FromBitstream(data));

			logger.Debug("Flag's value is: {0}", defaultValue);

			if (element.isToken)
			{
				if (defaultValue != element.DefaultValue)
				{
					logger.Debug("Flag marked as token, values did not match '" + ((string)defaultValue) + "' vs. '" + ((string)element.DefaultValue) + "'.");
					throw new CrackingFailure("Flag marked as token, values did not match '" + ((string)defaultValue) + "' vs. '" + ((string)element.DefaultValue) + "'.", element, data);
				}
			}

			element.DefaultValue = defaultValue;
		}

		/// <summary>
		/// Determines if a flag at position 'position' with size 'size' overlapps this element
		/// </summary>
		/// <param name="position">Position to test</param>
		/// <param name="size">Size to test</param>
		/// <returns>True if overlapps, false otherwise</returns>
		protected bool Overlapps(int position, int size)
		{
			if (position >= this.position)
			{
				if (position < (this.position + this.size))
					return true;
			}
			else
			{
				int end = position + size;
				if (end > this.position && end <= (this.position + size))
					return true;
			}

			return false;
		}

		public static DataElement PitParser(PitParser context, XmlNode node, Flags parent)
		{
			if (node.Name == "Flags")
				return null;

			var flag = DataElement.Generate<Flag>(node);

			int position = node.getAttrInt("position");
			int size = node.getAttrInt("size");

			if (position < 0 || size < 0 || (position + size) > parent.size)
				throw new PeachException("Error, flag " + flag.name + " is placed outside its parent.");

			if (parent.LittleEndian)
				position = parent.size - size - position;

			foreach (Flag other in parent)
			{
				if (other.Overlapps(position, size))
					throw new PeachException("Error, flag " + flag.name + " overlapps with flag " + other.name + ".");
			}

			flag.size = (int)size;
			flag.lengthType = LengthType.Bits;
			flag.length = size;
			flag.position = (int)position;
			flag.LittleEndian = parent.LittleEndian;

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
	}
}

// end
