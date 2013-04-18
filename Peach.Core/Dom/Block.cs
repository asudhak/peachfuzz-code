
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
	/// <summary>
	/// Block element
	/// </summary>
	[DataElement("Block")]
	[PitParsable("Block")]
	[DataElementChildSupported(DataElementTypes.Any)]
	[Parameter("name", typeof(string), "Element name", "")]
	[Parameter("ref", typeof(string), "Element to reference", "")]
	[Parameter("length", typeof(uint?), "Length in data element", "")]
	[Parameter("lengthType", typeof(LengthType), "Units of the length attribute", "bytes")]
	[Parameter("mutable", typeof(bool), "Is element mutable", "false")]
	[Parameter("constraint", typeof(string), "Scripting expression that evaluates to true or false", "")]
	[Parameter("minOccurs", typeof(int), "Minimum occurances", "1")]
	[Parameter("maxOccurs", typeof(int), "Maximum occurances", "1")]
	[Parameter("occurs", typeof(int), "Actual occurances", "1")]
	[Serializable]
	public class Block : DataElementContainer
	{
		public Block()
		{
		}

		public Block(string name)
			: base(name)
		{
		}

		public static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			if (node.Name != "Block")
				return null;

			Block block = null;

			if (node.hasAttr("ref"))
			{
				string name = node.getAttr("name", null);
				string refName = node.getAttrString("ref");

				DataElement refObj = context.getReference(refName, parent);
				if (refObj == null)
					throw new PeachException("Error, Block {0}could not resolve ref '{1}'. XML:\n{2}".Fmt(
						name == null ? "" : "'" + name + "' ", refName, node.OuterXml));

				if (!(refObj is Block))
					throw new PeachException("Error, Block {0}resolved ref '{1}' to unsupported element {2}. XML:\n{3}".Fmt(
						name == null ? "" : "'" + name + "' ", refName, refObj.debugName, node.OuterXml));
				
				if (string.IsNullOrEmpty(name))
					name = new Block().name;

				block = refObj.Clone(name) as Block;
				block.parent = parent;
				block.isReference = true;
				block.referenceName = refName;
			}
			else
			{
				block = DataElement.Generate<Block>(node);
			}

			context.handleCommonDataElementAttributes(node, block);
			context.handleCommonDataElementChildren(node, block);
			context.handleDataElementContainer(node, block);

			return block;
		}

		protected override Variant GenerateInternalValue()
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
				return MutatedValue;
			}

			foreach (Relation r in _relations)
			{
				if (IsFromRelation(r))
				{
					// CalculateFromValue can return null sometimes
					// when mutations mess up the relation.
					// In that case use the exsiting value for this element.

					var relationValue = r.CalculateFromValue();
					if (relationValue != null)
						value = relationValue;
				}
			}

			// 3. Fixup

			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_FIXUP) != 0)
			{
				return MutatedValue;
			}

			if (_fixup != null)
				value = _fixup.fixup(this);

			return value;
		}
	}
}

// end
