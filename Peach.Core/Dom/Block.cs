
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
	/// <summary>
	/// Block element
	/// </summary>
	[DataElement("Block")]
	[PitParsable("Block")]
	[DataElementChildSupportedAttribute(DataElementTypes.Any)]
	[Serializable]
	public class Block : DataElementContainer
	{
		public Block()
		{
		}

		public Block(string name) : base()
		{
			this.name = name;
		}

		public static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			if (node.Name != "Block")
				return null;

			var block = new Block();

			if (context.hasXmlAttribute(node, "ref"))
			{
				Block refObj = context.getReference(context._dom, context.getXmlAttribute(node, "ref"), parent) as Block;
				if (refObj != null)
				{
					string name = block.name;
					block = ObjectCopier.Clone<Block>(refObj);
					block.name = name;
					block.isReference = true;
				}
				else
				{
					throw new PeachException("Unable to locate 'ref' [" + context.getXmlAttribute(node, "ref") + 
						"] or found node did not match type. [" + node.OuterXml + "].");
				}
			}

			// name
			if (context.hasXmlAttribute(node, "name"))
				block.name = context.getXmlAttribute(node, "name");

			// alignment

			context.handleCommonDataElementAttributes(node, block);
			context.handleCommonDataElementChildren(node, block);
			context.handleDataElementContainer(node, block);

			return block;
		}

		public override Variant GenerateInternalValue()
		{
			Variant value;

			// 1. Default value

			if (_mutatedValue == null)
			{
				BitStream stream = new BitStream();
				foreach (DataElement child in this)
					stream.Write(child.Value, child);

				// TODO - Remove this debugging code!
                //if (stream.TellBytes() != stream.Value.Length)
                //    throw new ApplicationException("Whoa, something is way off here: " +
                //        stream.TellBytes() + " != " + stream.Value.Length);

				value = new Variant(stream);
			}
			else
			{
				value = MutatedValue;
			}

			// 2. Relations

			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_RELATIONS) != 0)
			{
				_internalValue = _mutatedValue;
				return MutatedValue;
			}

			foreach (Relation r in _relations)
			{
				if (r.Of != this)
				{
					value = r.CalculateFromValue();
				}
			}

			// 3. Fixup

			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_FIXUP) != 0)
			{
				_internalValue = _mutatedValue;
				return MutatedValue;
			}

			if (_fixup != null)
				value = _fixup.fixup(this);

			_internalValue = value;
			return value;
		}
	}
}

// end
