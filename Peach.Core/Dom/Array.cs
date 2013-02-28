
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
			logger.Debug("Crack: {0} Type: {1}", debugName, origionalElement.elementType);

			Array element = this;
			var origData = data;
			var origPos = origData.TellBits();
			data = ReadSizedData(data, size);

			if (this.Count > 0)
			{
				element.origionalElement = element[0];
				element.Clear(false);
			}

			if (element.relations.hasOfCountRelation || (minOccurs == 1 && maxOccurs == 1))
			{
				long count = element.relations.hasOfCountRelation ? element.relations.getOfCountRelation().GetValue() : occurs;

				logger.Debug("Crack: {0} found count relation/occurs. Count = {1}", element.fullName, count);

				if (count < 0)
					throw new CrackingFailure("Unable to crack Array '" + element.fullName + "'. Count relation negative: " + count, element, data);
				if (((count > maxOccurs && maxOccurs != -1) || count < minOccurs) && count != occurs)
					throw new CrackingFailure("Unable to crack Array '" + element.fullName + "'. Count outside of bounds of minOccurs='" +
						minOccurs + "' and maxOccurs='" + maxOccurs + "'. (Count = " + count + ")", element, data);

				for (int i = 0; i < count; i++)
				{
					logger.Debug("Crack: ======================");
					logger.Debug("Crack: {0} Trying #{1}", element, i.ToString());

					var clone = element.origionalElement;

					if (i == 0)
						element.origionalElement = clone.Clone();
					else
						clone = clone.Clone(clone.name + "_" + i);

					System.Diagnostics.Debug.Assert(!element.ContainsKey(clone.name));
					element.Add(clone);

					try
					{
						context.CrackData(clone, data);
					}
					catch
					{
						logger.Debug("Crack: {0} Failed on #{1}", element.fullName, i.ToString());
						throw;
					}
				}

				logger.Debug("Crack: {0} Done!", element.fullName);
			}

			else if (maxOccurs != 0)
			{
				int cnt = 0;
				for (cnt = 0; maxOccurs == -1 || cnt < maxOccurs; cnt++)
				{
					logger.Debug("Crack: ======================");
					logger.Debug("Crack: {0} Trying #{1}", element.fullName, cnt.ToString());

					long pos = data.TellBits();

					var clone = element.origionalElement;

					if (cnt == 0)
						element.origionalElement = clone.Clone();
					else
						clone = clone.Clone(clone.name + "_" + cnt);

					System.Diagnostics.Debug.Assert(!element.ContainsKey(clone.name));
					element.Add(clone);

					try
					{
						context.CrackData(clone, data);
					}
					catch
					{
						logger.Debug("Crack: {0} Failed on #{1}", element.fullName, cnt.ToString());
						element.RemoveAt(clone.parent.IndexOf(clone));
						data.SeekBits(pos, System.IO.SeekOrigin.Begin);
						break;
					}

					if (cnt == 0 && minOccurs == 0 && false)//!context.lookAhead(this, data))
					{
						// Broke our look ahead, must be only zero elements in this array.
						logger.Debug("Crack: {0}, minOccurs = 0, our look ahead failed, must be zero elements in this array.",
							element.fullName, cnt.ToString());

						element.RemoveAt(clone.parent.IndexOf(clone));
						data.SeekBits(pos, System.IO.SeekOrigin.Begin);

						break;
					}

					if (data.TellBits() == data.LengthBits)
					{
						logger.Debug("Crack: {0} Found EOF, all done!", element.fullName);
						// Include this successful crack in the count
						cnt++;
						break;
					}
				}

				if (cnt < minOccurs)
				{
					throw new CrackingFailure(
						string.Format("Crack: {0} Failed on #{1}. Not enough data to meet minOccurs value of {2}", element.fullName, cnt.ToString(), minOccurs),
						element, data);
				}

			}

			if (data != origData)
				origData.SeekBits(origPos + data.TellBits(), System.IO.SeekOrigin.Begin);
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
