
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
using System.Text;
using System.Linq;

using Peach.Core.Dom;
using Peach.Core.IO;

using NLog;

namespace Peach.Core.Cracker
{
	#region Event Delegates

	public delegate void EnterHandleNodeEventHandler(DataElement element, BitStream data);
	public delegate void ExitHandleNodeEventHandler(DataElement element, BitStream data);
	public delegate void ExceptionHandleNodeEventHandler(DataElement element, BitStream data, Exception e);
	public delegate void PlacementEventHandler(DataElement oldElement, DataElement newElement, DataElementContainer oldParent);

	#endregion

	/// <summary>
	/// Crack data into a DataModel.
	/// </summary>
	public class DataCracker
	{
		#region Private Members

		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		#region Position Class

		/// <summary>
		/// Helper class for tracking positions of cracked elements
		/// </summary>
		class Position
		{
			public long begin;
			public long end;
			public long? size;

			public override string ToString()
			{
				return "Begin: {0}, Size: {1}, End: {2}".Fmt(
					begin,
					size.HasValue ? size.Value.ToString() : "<null>",
					end);
			}
		}

		#endregion

		/// <summary>
		/// Collection of all elements that have been cracked so far.
		/// </summary>
		OrderedDictionary<DataElement, Position> _sizedElements;

		/// <summary>
		/// List of all unresolved size relations.
		/// This occurs when the 'Of' is cracked before the 'From'.
		/// </summary>
		List<SizeRelation> _sizeRelations;

		/// <summary>
		/// Stack of all BitStream objects passed to CrackData().
		/// This is used for determining absolute locations from relative offsets.
		/// </summary>
		List<BitStream> _dataStack = new List<BitStream>();

		/// <summary>
		/// Elements that have analyzers attached.  We run them all post-crack.
		/// </summary>
		List<DataElement> _elementsWithAnalyzer = new List<DataElement>();

		#endregion

		#region Events

		public event EnterHandleNodeEventHandler EnterHandleNodeEvent;
		protected void OnEnterHandleNodeEvent(DataElement element, BitStream data)
		{
			if(EnterHandleNodeEvent != null)
				EnterHandleNodeEvent(element, data);
		}
		
		public event ExitHandleNodeEventHandler ExitHandleNodeEvent;
		protected void OnExitHandleNodeEvent(DataElement element, BitStream data)
		{
			if (ExitHandleNodeEvent != null)
				ExitHandleNodeEvent(element, data);
		}

		public event ExceptionHandleNodeEventHandler ExceptionHandleNodeEvent;
		protected void OnExceptionHandleNodeEvent(DataElement element, BitStream data, Exception e)
		{
			if (ExceptionHandleNodeEvent != null)
				ExceptionHandleNodeEvent(element, data, e);
		}

		public event PlacementEventHandler PlacementEvent;
		protected void OnPlacementEvent(DataElement oldElement, DataElement newElement, DataElementContainer oldParent)
		{
			if (PlacementEvent != null)
				PlacementEvent(oldElement, newElement, oldParent);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Main entry method that will take a data stream and parse it into a data model.
		/// </summary>
		/// <remarks>
		/// Method will throw one of two exceptions on an error: CrackingFailure, or NotEnoughDataException.
		/// </remarks>
		/// <param name="model">DataModel to import data into</param>
		/// <param name="data">Data stream to read data from</param>
		public void CrackData(DataElement element, BitStream data)
		{
			try
			{
				_dataStack.Insert(0, data);

				if (_dataStack.Count == 1)
					handleRoot(element, data);
				else
					handleNode(element, data);
			}
			finally
			{
				_dataStack.RemoveAt(0);
			}

		}

		/// <summary>
		/// Get the size of an element that has already been cracked.
		/// The size only has a value if the element has a length attribute
		/// or the element has a size relation that has successfully resolved.
		/// </summary>
		/// <param name="elem">Element to query</param>
		/// <returns>size of the element</returns>
		public long? GetElementSize(DataElement elem)
		{
			return _sizedElements[elem].size;
		}

		/// <summary>
		/// Perform optimizations of data model for cracking
		/// </summary>
		/// <remarks>
		/// Optimization can be performed once on a data model and used
		/// for any clones made.  Optimizations will increase the speed
		/// of data cracking.
		/// </remarks>
		/// <param name="model">DataModel to optimize</param>
		public void OptimizeDataModel(DataModel model)
		{
			foreach (var element in model.EnumerateElementsUpTree())
			{
				if (element is Choice)
				{
					// TODO - Fast CACHE IT!
				}
			}
		}

		#endregion

		#region Private Helpers

		long getDataOffset()
		{
			var curr = _dataStack.First();
			var root = _dataStack.Last();

			if (curr == root)
				return 0;

			long offset = root.TellBits() - curr.LengthBits;
			System.Diagnostics.Debug.Assert(offset >= 0);
			return offset;
		}

		#endregion

		#region Handlers

		#region Top Level Handlers

		void handleRoot(DataElement element, BitStream data)
		{
			_sizedElements = new OrderedDictionary<DataElement, Position>();
			_sizeRelations = new List<SizeRelation>();

			// Crack the model
			handleNode(element, data);

			// Handle any Placement's
			handlePlacement(element, data);

			// Handle any analyzers
			foreach (DataElement elem in _elementsWithAnalyzer)
				elem.analyzer.asDataElement(elem, null);
		}

		/// <summary>
		/// Called to crack a DataElement based on an input stream.  This method
		/// will hand cracking off to a more specific method after performing
		/// some common tasks.
		/// </summary>
		/// <param name="element">DataElement to crack</param>
		/// <param name="data">Input stream to use for data</param>
		void handleNode(DataElement elem, BitStream data)
		{
			try
			{
				if (elem == null)
					throw new ArgumentNullException("elem");
				if (data == null)
					throw new ArgumentNullException("data");

				logger.Debug("------------------------------------");
				logger.Debug("{0} {1}", elem.debugName, data.Progress);

				var pos = handleNodeBegin(elem, data);

				if (elem.transformer != null)
				{
					var sizedData = elem.ReadSizedData(data, pos.size);
					var decodedData = elem.transformer.decode(sizedData);

					// Use the size of the transformed data as the new size of the element
					handleCrack(elem, decodedData, decodedData.LengthBits);
				}
				else
				{
					handleCrack(elem, data, pos.size);
				}

				if (elem.constraint != null)
					handleConstraint(elem, data);

				if (elem.analyzer != null)
					_elementsWithAnalyzer.Add(elem);

				handleNodeEnd(elem, data, pos);
			}
			catch (Exception e)
			{
				handleException(elem, data, e);
				throw;
			}
		}

		void handlePlacement(DataElement model, BitStream data)
		{
			List<DataElement> elementsWithPlacement = new List<DataElement>();
			foreach (DataElement element in model.EnumerateAllElements())
			{
				if (element.placement != null)
					elementsWithPlacement.Add(element);
			}

			foreach (DataElement element in elementsWithPlacement)
			{
				var fixups = new List<Tuple<Fixup, string>>();
				DataElementContainer oldParent = element.parent;

				// Ensure relations are resolved
				foreach (Relation relation in element.relations)
				{
					if (relation.Of != element && relation.From != element)
						throw new CrackingFailure("Error, unable to resolve Relations of/from to match current element.", element, data);
				}

				// Locate relevant fixups
				foreach (DataElement child in model.EnumerateAllElements())
				{
					if (child.fixup == null)
						continue;

					foreach (var item in child.fixup.references)
					{
						if (item.Item2 != element.name)
							continue;

						var refElem = child.find(item.Item2);
						if (refElem == null)
							throw new CrackingFailure("Error, unable to resolve Fixup reference to match current element.", element, data);

						if (refElem == element)
							fixups.Add(new Tuple<Fixup, string>(child.fixup, item.Item1));
					}
				}

				DataElement newElem = null;

				if (element.placement.after != null)
				{
					var after = element.find(element.placement.after);
					if (after == null)
						throw new CrackingFailure("Error, unable to resolve Placement on element '" + element.fullName +
							"' with 'after' == '" + element.placement.after + "'.", element, data);
					newElem = element.MoveAfter(after);
				}
				else if (element.placement.before != null)
				{
					DataElement before = element.find(element.placement.before);
					if (before == null)
						throw new CrackingFailure("Error, unable to resolve Placement on element '" + element.fullName +
							"' with 'after' == '" + element.placement.after + "'.", element, data);
					newElem = element.MoveBefore(before);
				}

				// Update fixups
				foreach (var fixup in fixups)
				{
					fixup.Item1.updateRef(fixup.Item2, newElem.fullName);
				}

				OnPlacementEvent(element, newElem, oldParent);
			}
		}

		#endregion

		#region Helpers

		void handleOffsetRelation(DataElement element, BitStream data)
		{
			long? offset = getRelativeOffset(element, data, 0);

			if (!offset.HasValue)
				return;

			offset += data.TellBits();

			if (offset > data.LengthBits)
				data.WantBytes((offset.Value + 7 - data.LengthBits) / 8);

			if (offset > data.LengthBits)
			{
				string msg = "{0} has offset of {1} bits but buffer only has {2} bits.".Fmt(
					element.debugName, offset, data.LengthBits);
				throw new CrackingFailure(msg, element, data);
			}

			data.SeekBits(offset.Value, System.IO.SeekOrigin.Begin);
		}

		void handleException(DataElement elem, BitStream data, Exception e)
		{
			_sizedElements.Remove(elem);
			_sizeRelations.RemoveAll(r => r.Of == elem);

			CrackingFailure ex = e as CrackingFailure;
			if (ex != null)
			{
				logger.Debug("{0} failed to crack.", elem.debugName);
				if (!ex.logged)
					logger.Debug(ex.Message);
				ex.logged = true;
			}
			else
			{
				logger.Debug("Exception occured: {0}", e.ToString());
			}

			OnExceptionHandleNodeEvent(elem, data, e);
		}

		void handleConstraint(DataElement element, BitStream data)
		{
			logger.Debug("Running constraint [" + element.constraint + "]");

			Dictionary<string, object> scope = new Dictionary<string, object>();
			scope["element"] = element;

			var iv = element.InternalValue;
			if (iv.GetVariantType() == Variant.VariantType.ByteString || iv.GetVariantType() == Variant.VariantType.BitStream)
			{
				scope["value"] = (byte[])iv;
				logger.Debug("Constraint, value=byte array.");
			}
			else
			{
				scope["value"] = (string)iv;
				logger.Debug("Constraint, value=[" + (string)iv + "].");
			}

			object oReturn = Scripting.EvalExpression(element.constraint, scope);

			if (!((bool)oReturn))
				throw new CrackingFailure("Constraint failed.", element, data);
		}

		Position handleNodeBegin(DataElement elem, BitStream data)
		{
			OnEnterHandleNodeEvent(elem, data);

			handleOffsetRelation(elem, data);

			System.Diagnostics.Debug.Assert(!_sizedElements.ContainsKey(elem));

			long? size = getSize(elem, data);

			var pos = new Position();
			pos.begin = data.TellBits() + getDataOffset();
			pos.size = size;

			_sizedElements.Add(elem, pos);

			// If this element does not have a size but has a size relation,
			// keep track of the relation for evaluation in the future
			if (!size.HasValue)
			{
				SizeRelation rel = elem.relations.getOfSizeRelation();
				if (rel != null)
					_sizeRelations.Add(rel);
			}

			return pos;
		}

		void handleNodeEnd(DataElement elem, BitStream data, Position pos)
		{
			// Completing this element might allow us to evaluate
			// outstanding size reation computations.
			for (int i = _sizeRelations.Count - 1; i >= 0; --i)
			{
				var rel = _sizeRelations[i];

				if (elem == rel.From || (elem is DataElementContainer &&
					((DataElementContainer)elem).isParentOf(rel.From)))
				{
					var other = _sizedElements[rel.Of];
					System.Diagnostics.Debug.Assert(!other.size.HasValue);
					other.size = rel.GetValue();
					_sizeRelations.RemoveAt(i);

					logger.Debug("Size relation of {0} cracked. Updating size: {1}",
						rel.Of.debugName, other.size);
				}
			}

			// Mark the end position of this element
			pos.end = data.TellBits() + getDataOffset();

			OnExitHandleNodeEvent(elem, data);
		}

		void handleCrack(DataElement elem, BitStream data, long? size)
		{
			logger.Debug("Crack: {0} Size: {1}, {2}", elem.debugName,
				size.HasValue ? size.ToString() : "<null>", data.Progress);

			elem.Crack(this, data, size);
		}

		#endregion

		#endregion

		#region Calculate Element Size

		long? getRelativeOffset(DataElement elem, BitStream data, long minOffset = 0)
		{
			OffsetRelation rel = elem.relations.getOfOffsetRelation();

			if (rel == null)
				return null;

			// Ensure we have cracked the from half of the relation
			if (!_sizedElements.ContainsKey(rel.From))
				return null;

			// Offset is in bytes
			long offset = (long)rel.GetValue() * 8;

			if (rel.isRelativeOffset)
			{
				DataElement from = rel.From;

				if (rel.relativeTo != null)
					from = from.find(rel.relativeTo);

				if (from == null)
					throw new CrackingFailure("Unable to locate 'relativeTo' element in relation attached to " +
						elem.debugName + "'.", elem, data);

				// Get the position we are related to
				Position pos;
				if (!_sizedElements.TryGetValue(from, out pos))
					return null;

				// If relativeTo, offset is from beginning of relativeTo element
				// Otherwise, offset is after the From element
				offset += rel.relativeTo != null ? pos.begin : pos.end;
			}

			// Adjust offset to be relative to the current BitStream
			offset -= getDataOffset();

			// Ensure the offset is not before our current position
			if (offset < data.TellBits())
			{
				string msg = "{0} has offset of {1} bits but already read {2} bits.".Fmt(
					elem.debugName, offset, data.TellBits());
				throw new CrackingFailure(msg, elem, data);
			}

			// Make offset relative to current position
			offset -= data.TellBits();

			// Ensure the offset satisfies the minimum
			if (offset < minOffset)
			{
				string msg = "{0} has offset of {1} bits but must be at least {2} bits.".Fmt(
					elem.debugName, offset, minOffset);
				throw new CrackingFailure(msg, elem, data);
			}

			return offset;
		}

		long? findToken(BitStream data, BitStream token, long offset)
		{
			while (true)
			{
				long start = data.TellBits();
				long end = data.IndexOf(token, start + offset);

				if (end >= 0)
					return end - start - offset;

				long dataLen = data.LengthBytes;
				data.WantBytes(token.LengthBytes);

				if (dataLen == data.LengthBytes)
					return null;
			}
		}

		bool checkArray(DataElementContainer cont, ref long offset, ref long end, bool checkSize, bool checkToken)
		{
			var array = cont as Dom.Array;
			if (array == null)
				return false;

			logger.Debug("checkArray: {0}", cont.debugName);

			BitStream token = null;
			long arrayOffset = 0;
			long arrayEnd = -1;

			if (scan(array.origionalElement, ref arrayOffset, ref arrayEnd, ref token, false, checkToken) && checkSize)
			{
				var rel = array.relations.getOfCountRelation();
				if (rel != null && _sizedElements.ContainsKey(rel.From))
				{
					arrayOffset *= rel.GetValue();
					offset += arrayOffset;
					logger.Debug("checkArray: {0} -> Count Relation: {1}, Size: {2}",
						cont.debugName, rel.GetValue(), arrayOffset);
					return true;
				}
				else if (array.minOccurs == 1 && array.maxOccurs == 1)
				{
					arrayOffset *= array.occurs;
					offset += arrayOffset;
					logger.Debug("checkArray: {0} -> Occurs: {1}, Size: {2}",
						cont.debugName, array.occurs, arrayOffset);
					return true;
				}
			}

			// If we aren't scanning for a token or we didn't find one, we are done
			if (!checkToken || token == null)
				return false;

			// Array is fully cracked
			if (array.maxOccurs != -1 && array.Count >= array.maxOccurs)
				return false;

			// try and find token
			long? where = findToken(_dataStack.First(), token, offset + arrayOffset);
			if (!where.HasValue)
			{
				logger.Debug("checkArray: {0} -> No token found", cont.debugName);
				return array.minOccurs == 0;
			}

			end = where.Value;
			logger.Debug("checkArray: {0} -> Found token, setting end to {1}", cont.debugName, end);
			return true;
		}

		bool scan(DataElement elem, ref long offset, ref long end, ref BitStream token, bool checkEnd, bool checkToken)
		{
			if (checkToken && elem.isToken)
			{
				token = elem.Value;
				logger.Debug("scan: {0} -> Offset: {1}, Token!", elem.debugName, offset);
				return true;
			}

			if (checkEnd)
			{
				long? offRel = getRelativeOffset(elem, _dataStack.First(), offset);
				if (offRel.HasValue)
				{
					end = offRel.Value;
					logger.Debug("scan: {0} -> Offset: {1}, Offset relation: {2}", elem.debugName, offset, end);
					return true;
				}
			}

			// See if we have a size relation
			SizeRelation sizeRel = elem.relations.getOfSizeRelation();
			if (sizeRel != null)
			{
				if (_sizedElements.ContainsKey(sizeRel.From))
				{
					offset += sizeRel.GetValue();
					logger.Debug("scan: {0} -> Offset: {1}, Size relation: {2}", elem.debugName, offset, sizeRel.GetValue());
					return true;
				}
				else
				{
					logger.Debug("scan: {0} -> Offset: {1}, Size relation: ???", elem.debugName, offset);
					return false;
				}
			}

			// See if our length is defined
			if (elem.hasLength)
			{
				offset += elem.lengthAsBits;
				logger.Debug("scan: {0} -> Offset: {1}, Length: {2}", elem.debugName, offset, elem.lengthAsBits);
				return true;
			}

			// If we are unsized, see if we are a container
			var cont = elem as DataElementContainer;
			if (cont == null)
			{
				logger.Debug("scan: {0} -> Offset: {1}, {2}", elem.debugName, offset,
					elem.isDeterministic ? "Determinstic" : "Unsized");
				return false;
			}

			logger.Debug("scan: {0}", elem.debugName);

			// If we are scanning for tokens, see if a token occurs in the array
			if (checkArray(cont, ref offset, ref end, true, checkToken))
				return true;

			if (cont.transformer != null || cont is Dom.Array || cont is Dom.Choice)
				return false;

			foreach (var child in cont)
			{
				// Descend into child
				if (!scan(child, ref offset, ref end, ref token, checkEnd, checkToken))
					return false;

				// If we found a token or end marker we are done
				if (token != null || end >= 0)
					break;
			}

			return true;
		}

		long? getSize(DataElement elem, BitStream data)
		{
			// Quick optimiation
			if (elem is DataModel)
				return null;

			long offset = 0;
			long size = 0;
			long end = -1;
			BitStream token = null;

			// First, check and see if this element is sized
			if (scan(elem, ref size, ref end, ref token, false, false))
			{
				logger.Debug("getSize: {0} -> Size: {1}", elem.debugName, size);
				return size;
			}

			// If is unsized but determistic, return unsized
			if (elem.isDeterministic)
			{
				logger.Debug("getSize: {0} -> Deterministic: ???", elem.debugName);
				return null;
			}

			// Ensure all elements are sized until we reach either
			// 1) A token
			// 2) An offset relation we have cracked that can be satisfied
			// 3) The end of the data model

			DataElement prev = elem;

			while (true)
			{
				logger.Debug("getSize: {0} -> Scanning", prev.debugName);

				// Get the next sibling
				var curr = prev.nextSibling();

				if (curr != null)
				{
					// Descend into next sibling
					if (!scan(curr, ref offset, ref end, ref token, true, true))
					{
						logger.Debug("getSize: {0} -> Not Last Unsized: ???", elem.debugName);
						return null;
					}

					// If we found a token or end marker we are done
					if (token != null || end >= 0)
						break;
				}
				else if (prev.parent == null)
				{
					// hit the top
					break;
				}
				else if (GetElementSize(prev.parent).HasValue)
				{
					// Parent is bound by size
					break;
				}
				else if (!(elem is DataElementContainer) && checkArray(prev.parent, ref offset, ref end, false, true))
				{
					// Parent is array and matched token in array element
					break;
				}
				else
				{
					// no more siblings, ascend
					curr = prev.parent;
				}

				prev = curr;
			}

			// Turn the offset to a marker into the element size
			long? ret = null;

			if (token != null)
			{
				ret = findToken(data, token, offset);
				logger.Debug("getSize: {0} -> Token: {1}", elem.debugName,
					ret.HasValue ? ret.ToString() : "???");
				return ret;
			}
			else if (end >= 0)
			{
				ret = end - offset;
				logger.Debug("getSize: {0} -> End: {1}", elem.debugName, ret);
			}
			else if (offset != 0 || !(elem is DataElementContainer))
			{
				ret = data.LengthBits - (data.TellBits() + offset);
				logger.Debug("getSize: {0} -> Last Unsized: {1}", elem.debugName, ret);
			}
			else
			{
				logger.Debug("getSize: {0} -> Last Unsized: ???", elem.debugName);
			}

			return ret;
		}

		#endregion
	}
}

// end
