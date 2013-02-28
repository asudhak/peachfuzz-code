
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

		#region Determine Element Size

		/// <summary>
		/// Is element last unsized element in currently sized area.  If not
		/// then 'size' is set to the number of bytes from element to ened of
		/// the sized data.
		/// </summary>
		/// <param name="element">Element to check</param>
		/// <param name="size">Set to the number of BITS from element to end of the data.</param>
		/// <returns>Returns true if last unsigned element, else false.</returns>
		protected bool isLastUnsizedElement(DataElement element, ref long outSize)
		{
			logger.Trace("isLastUnsizedElement: {0}", element.fullName);

			long size = 0;
			DataElement oldElement = element;
			DataElement currentElement = element;

			while (true)
			{
				currentElement = oldElement.nextSibling();
				if (currentElement == null && (oldElement.parent == null || oldElement.parent.transformer != null || GetElementSize(oldElement.parent).HasValue))
					break;
				else if (currentElement == null)
					currentElement = oldElement.parent;
				else
				{
					var sizeRel = currentElement.relations.getOfSizeRelation();
					if (sizeRel != null)
					{
						if (!_sizedElements.ContainsKey(sizeRel.From))
							return false;

						size += sizeRel.GetValue();
					}

					else if (currentElement.hasLength)
						size += currentElement.lengthAsBits;

					else if (currentElement is DataElementContainer)
					{
						foreach(DataElement child in ((DataElementContainer)currentElement))
						{
							if (!_isLastUnsizedElementRecursive(child, ref size))
							{
								logger.Debug("isLastUnsizedElement(false): {0} {1}", element.fullName, size);
								return false;
							}
						}
					}
					else
					{
						logger.Debug("isLastUnsizedElement(false): {0} {1}", element.fullName, size);
						return false;
					}
				}

				oldElement = currentElement;
			}

			logger.Debug("isLastUnsizedElement(true): {0} {1}", element.fullName, size);
			outSize = size;
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="elem"></param>
		/// <param name="size">In bits</param>
		/// <returns></returns>
		protected bool _isLastUnsizedElementRecursive(DataElement elem, ref long size)
		{
			if (elem == null)
				return false;

			var sizeRel = elem.relations.getOfSizeRelation();
			if (sizeRel != null)
			{
				if (!_sizedElements.ContainsKey(sizeRel.From))
					return false;

				size += sizeRel.GetValue();
				return true;
			}

			if (elem.hasLength)
			{
				size += elem.lengthAsBits;
				return true;
			}

			if(!(elem is DataElementContainer))
				return false;

			foreach(DataElement child in ((DataElementContainer)elem))
			{
				if (!_isLastUnsizedElementRecursive(child, ref size))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Keep walking the data model looking for a token.
		/// Walking stops when either a token is found or an unsized element is found.
		/// </summary>
		/// <param name="element">Element to check</param>
		/// <param name="size">Set to the number of bits from element to token.</param>
		/// <param name="token">Set to token element if found</param>
		/// <returns>Returns true if should keep going, else false.</returns>
		protected bool _isTokenNextRecursive(DataElement element, ref long size, ref DataElement token)
		{
			System.Diagnostics.Debug.Assert(element != null);
			System.Diagnostics.Debug.Assert(token == null);

			DataElement next = element;

			while (next != null)
			{
				if (next is DataElementContainer)
				{
					DataElementContainer cont = (DataElementContainer)next;

					if (!cont.isLeafNode)
					{
						logger.Trace("_isTokenNextRecursive: Descending into {0}", cont.fullName);

						if (!_isTokenNextRecursive(cont[0], ref size, ref token))
							return false;
					}
					else
					{
						logger.Trace("_isTokenNextRecursive: Skipping leaf {0}", element.fullName);
					}
				}
				else if (!next.hasLength)
				{
					// Found an unsised element before finding token, so bail
					logger.Trace("_isTokenNextRecursive: Unsized {0}", next.fullName);
					return false;
				}
				else
				{
					if (next.isToken)
					{
						// Found a token, so bail
						logger.Trace("_isTokenNextRecursive: Found Token {0} {1}", element.fullName, size);
						token = next;
						return false;
					}

					size += next.lengthAsBits;
					logger.Trace("_isTokenNextRecursive: Adding {0} {1}", element.fullName, size);
				}

				next = next.nextSibling();
			}

			// Keep going
			return true;
		}

		/// <summary>
		/// Is there a token next in the list of elements to parse, or
		/// can we calculate our distance to the next token?
		/// </summary>
		/// <param name="element">Element to check</param>
		/// <param name="size">Set to the number of bits from element to token.</param>
		/// <param name="token">Set to token element if found</param>
		/// <returns>Returns true if found token, else false.</returns>
		protected bool isTokenNext(DataElement element, ref long outSize, ref DataElement token)
		{
				System.Diagnostics.Debug.Assert(element != null);
				System.Diagnostics.Debug.Assert(token == null);

				long size = 0;
				DataElement next = element;
				DataElement sibling = null;

				while (next != null)
				{
					sibling = next.nextSibling();

					if (sibling == null)
					{
						var parent = next.parent;
						while (sibling == null && parent != null && parent.transformer == null && !GetElementSize(parent).HasValue)
						{
							sibling = parent.nextSibling();
							parent = parent.parent;
						}

						if (sibling == null)
							return false;

						while (sibling is DataElementContainer && ((DataElementContainer)sibling).Count > 0)
							sibling = ((DataElementContainer)sibling)[0];
					}

					next = sibling;

					if (next.isToken)
					{
						token = next;
						outSize = size;
						return true;
					}

					if (!_isTokenNextRecursive(next, ref size, ref token))
					{
						if (token != null)
						{
							outSize = size;
							return true;
						}
						return false;
					}

					next = next.parent;
				}

				return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="data"></param>
		/// <returns>Returns size in bits</returns>
		private long? determineElementSize(DataElement element, BitStream data)
		{
			logger.Debug("determineElementSize: {0}", element.debugName);

			// Size in bits
			long? size = null;

			SizeRelation sizeRelation = element.relations.getOfSizeRelation();
			if (sizeRelation != null)
			{
				if (_sizedElements.ContainsKey(sizeRelation.From))
					size = sizeRelation.GetValue();

				logger.Debug("determineElementSize: Size relation from {0} {1} cracked. Returning: {2}",
					sizeRelation.From.debugName,
					size.HasValue ? "already" : "not",
					size.HasValue ? size.ToString() : "<null>");

				//				return size;
			}

			// Check for relation and/or size
			else if (element is Dom.String && ((Dom.String)element).readCharacters)
			{
				return size;
			}
			else if (element.hasLength)
			{
				size = element.lengthAsBits;
			}
			if (!size.HasValue)//(element is DataElementContainer))
			{
				/*
				 * string:
				 * 
				 * if (!_hasLength && nullTerminated)
				 * if (lengthType == LengthType.Chars && _hasLength)
				 */

				long nextSize = 0;
				DataElement token = null;

				// Note: nextSize is in bits
				if (isTokenNext(element, ref nextSize, ref token))
				{
					while (true)
					{
						long start = data.TellBits();
						long end = data.IndexOf(token.Value, start + nextSize);
						if (end >= 0)
						{
							logger.Debug("determineElementSize: Token was found in data stream, able to determine element size.");
							size = end - start - nextSize;
							break;
						}

						long dataLen = data.LengthBytes;
						data.WantBytes(token.Value.Value.Length);
						if (dataLen == data.LengthBytes)
						{
							logger.Debug("determineElementSize: Token was not found in data stream.");
							break;
						}

					}
				}

				if (!size.HasValue && isLastUnsizedElement(element, ref nextSize))
				{
					if (nextSize != 0 || !(element is DataElementContainer))
						size = data.LengthBits - (data.TellBits() + nextSize);
				}
			}

			logger.Debug("determineElementSize: Returning: {0}", size.HasValue ? size.ToString() : "<null>");
			return size;
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
			OffsetRelation rel = element.relations.getOfOffsetRelation();

			if (rel == null)
				return;

			// Offset is in bytes
			long offset = (long)rel.GetValue() * 8;

			if (rel.isRelativeOffset)
			{
				DataElement from = rel.From;

				if (rel.relativeTo != null)
					from = from.find(rel.relativeTo);

				if (from == null)
					throw new CrackingFailure("Unable to locate 'relativeTo' element in relation attached to " +
						rel.From.debugName + "'.", element, data);

				// If relativeTo, offset is from beginning of relativeTo element
				// Otherwise, offset is after the From element
				var pos = _sizedElements[from];
				offset += rel.relativeTo != null ? pos.begin : pos.end;
			}

			// Handle case where data is a slice from the root BitStream
			offset -= getDataOffset();

			if (offset < data.TellBits())
			{
				string msg = "{0} has offset of {1} bits but already read {2} bits.".Fmt(
					element.debugName, offset, data.TellBits());
				throw new CrackingFailure(msg, element, data);
			}

			if (offset > data.LengthBits)
				data.WantBytes((offset + 7 - data.LengthBits) / 8);

			if (offset > data.LengthBits)
			{
				string msg = "{0} has offset of {1} bits but buffer only has {2} bits.".Fmt(
					element.debugName, offset, data.LengthBits);
				throw new CrackingFailure(msg, element, data);
			}

			data.SeekBits(offset, System.IO.SeekOrigin.Begin);
		}

		void handleException(DataElement elem, BitStream data, Exception e)
		{
			_sizedElements.Remove(elem);
			_sizeRelations.RemoveAll(r => r.Of == elem);

			CrackingFailure ex = e as CrackingFailure;
			if (ex != null)
			{
				if (!ex.logged)
					logger.Debug("{0} failed to crack. {1}", elem.debugName, ex.Message);
				else
					logger.Debug("{0} failed to crack.", elem.debugName);

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

			long? size = determineElementSize(elem, data);

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
	}
}

// end
