
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
using System.Linq;

using Peach.Core.Analyzers;
using Peach.Core.IO;
using Peach.Core.Cracker;

using NLog;

namespace Peach.Core.Dom
{

	/// <summary>
	/// Array of data elements.  Can be
	/// zero or more elements.
	/// </summary>
	[Serializable]
	[DataElement("Array")]
	public class Array : Block
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		public int minOccurs = 1;
		public int maxOccurs = 1;
		public int occurs = 1;

		public bool hasExpanded = false;
		public int? overrideCount = null;

		public DataElement origionalElement = null;

		public Array()
		{
		}

		public Array(string name)
			: base(name)
		{
		}

		public override IEnumerable<DataElement> EnumerateAllElements(List<DataElement> knownParents)
		{
			if (Count == 0)
			{
				// Mutation might have erased all of our children
				if (origionalElement == null)
					yield break;

				// First our origionalElement
				yield return origionalElement;

				// Next our origionalElement element's children
				foreach (var item in origionalElement.EnumerateAllElements(knownParents))
					yield return item;
			}
			else
			{
				// Default to our base to enumerate array elements
				foreach (var item in base.EnumerateAllElements(knownParents))
					yield return item;
			}
		}

		public override void Crack(DataCracker context, BitStream data, long? size)
		{
			long startPos = data.TellBits();
			BitStream sizedData = ReadSizedData(data, size);

			if (this.Count > 0)
			{
				origionalElement = this[0];
				Clear(false);
			}

			long min = minOccurs;
			long max = maxOccurs;

			var rel = relations.Of<CountRelation>().Where(context.HasCracked).FirstOrDefault();
			if (rel != null)
				min = max = rel.GetValue();
			else if (minOccurs == 1 && maxOccurs == 1)
				min = max = occurs;

			if (((min > maxOccurs && maxOccurs != -1) || (min < minOccurs)) && min != occurs)
			{
				string msg = "{0} has invalid count of {1} (minOccurs={2}, maxOccurs={3}, occurs={4}).".Fmt(
				    debugName, min, minOccurs, maxOccurs, occurs);
				throw new CrackingFailure(msg, this, data);
			}

			for (int i = 0; max == -1 || i < max; ++i)
			{
				logger.Debug("Crack: ======================");
				logger.Debug("Crack: {0} Trying #{1}", origionalElement.debugName, i+1);

				long pos = sizedData.TellBits();
				if (pos == sizedData.LengthBits)
				{
					logger.Debug("Crack: Consumed all bytes. {0}", sizedData.Progress);
					break;
				}

				var clone = makeElement(i);
				Add(clone);

				try
				{
					context.CrackData(clone, sizedData);

					// If we used 0 bytes and met the minimum, we are done
					if (pos == sizedData.TellBits() && i == min)
					{
						RemoveAt(clone.parent.IndexOf(clone));
						break;
					}
				}
				catch (CrackingFailure)
				{
					logger.Debug("Crack: {0} Failed on #{1}", debugName, i+1);

					// If we couldn't satisfy the minimum propigate failure
					if (i < min)
						throw;

					RemoveAt(clone.parent.IndexOf(clone));
					sizedData.SeekBits(pos, System.IO.SeekOrigin.Begin);
					break;
				}
			}

			if (this.Count < min)
			{
				string msg = "{0} only cracked {1} of {2} elements.".Fmt(debugName, Count, min);
				throw new CrackingFailure(msg, this, data);
			}

			if (size.HasValue && data != sizedData)
				data.SeekBits(startPos + sizedData.TellBits(), System.IO.SeekOrigin.Begin);
		}

		public new static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			var array = DataElement.Generate<Array>(node);

			if (node.hasAttr("minOccurs"))
			{
				array.minOccurs = node.getAttrInt("minOccurs");
				array.maxOccurs = -1;
				array.occurs = array.minOccurs;
			}

			if (node.hasAttr("maxOccurs"))
				array.maxOccurs = node.getAttrInt("maxOccurs");

			if (node.hasAttr("occurs"))
				array.occurs = node.getAttrInt("occurs");

			return array;
		}

		private DataElement makeElement(int index)
		{
			var clone = origionalElement;

			if (index == 0)
				origionalElement = clone.Clone();
			else
				clone = clone.Clone(clone.name + "_" + index);

			System.Diagnostics.Debug.Assert(!ContainsKey(clone.name));

			return clone;
		}

		[OnSerializing]
		private void OnSerializing(StreamingContext context)
		{
			DataElement.CloneContext ctx = context.Context as DataElement.CloneContext;
			if (ctx == null)
				return;

			// If we are the root of the clone operation, and we have a child
			// that shares our name, so tell the cloner to rename our 1st child
			if (this == ctx.root && this.Count > 0 && ctx.oldName == this[0].name)
				ctx.rename.Add(this[0]);
		}

		/// <summary>
		/// Expands the size of the array to be 'num' long.
		/// Does this by adding the same instance of the last
		/// item in the array until the Count is num.
		/// </summary>
		/// <param name="num">The total size the array should be.</param>
		public void ExpandTo(int num)
		{
			System.Diagnostics.Debug.Assert(Count > 0 || origionalElement != null);

			DataElement item = null;
			if (Count == 0)
				item = origionalElement;
			else
				item = this[Count - 1];


			var bs = new BitStream();
			bs.Write(item.Value);
			bs.ClearElementPositions();

			var clone = item.Clone();
			clone.MutatedValue = new Variant(bs);
			clone.mutationFlags = DataElement.MUTATE_DEFAULT | DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;

			// Force the same element to be duplicated in the DataElementContainer
			for (int i = Count; i < num; ++i)
			{
				_childrenList.Insert(i, clone);
				_childrenDict[clone.name] = clone;
			}

			Invalidate();
		}
	}
}

// end
