
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

	public delegate void EnterHandleNodeEventHandler(DataElement element, long position, BitStream data);
	public delegate void ExitHandleNodeEventHandler(DataElement element, long position, BitStream data);
	public delegate void ExceptionHandleNodeEventHandler(DataElement element, long position, BitStream data, Exception e);
	public delegate void PlacementEventHandler(DataElement oldElement, DataElement newElement, DataElementContainer oldParent);
	public delegate void AnalyzerEventHandler(DataElement element, BitStream data);

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
		List<DataElement> _elementsWithAnalyzer;

		#endregion

		#region Events

		public event EnterHandleNodeEventHandler EnterHandleNodeEvent;
		protected void OnEnterHandleNodeEvent(DataElement element, long position, BitStream data)
		{
			if(EnterHandleNodeEvent != null)
				EnterHandleNodeEvent(element, position, data);
		}
		
		public event ExitHandleNodeEventHandler ExitHandleNodeEvent;
		protected void OnExitHandleNodeEvent(DataElement element, long position, BitStream data)
		{
			if (ExitHandleNodeEvent != null)
				ExitHandleNodeEvent(element, position, data);
		}

		public event ExceptionHandleNodeEventHandler ExceptionHandleNodeEvent;
		protected void OnExceptionHandleNodeEvent(DataElement element, long position, BitStream data, Exception e)
		{
			if (ExceptionHandleNodeEvent != null)
				ExceptionHandleNodeEvent(element, position, data, e);
		}

		public event PlacementEventHandler PlacementEvent;
		protected void OnPlacementEvent(DataElement oldElement, DataElement newElement, DataElementContainer oldParent)
		{
			if (PlacementEvent != null)
				PlacementEvent(oldElement, newElement, oldParent);
		}

		public event AnalyzerEventHandler AnalyzerEvent;
		protected void OnAnalyzerEvent(DataElement element, BitStream data)
		{
			if (AnalyzerEvent != null)
				AnalyzerEvent(element, data);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Main entry method that will take a data stream and parse it into a data model.
		/// </summary>
		/// <remarks>
		/// Method will throw one of two exceptions on an error: CrackingFailure, or NotEnoughDataException.
		/// </remarks>
		/// <param name="element">DataElement to import data into</param>
		/// <param name="data">Data stream to read data from</param>
		public void CrackData(DataElement element, BitStream data)
		{
			try
			{
				_dataStack.Insert(0, data);

				if (_dataStack.Count == 1)
					handleRoot(element, data);
				else if (element.placement != null)
					handlePlacelemt(element, data);
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
		/// Determines if the From half of a relation has been cracked.
		/// </summary>
		/// <param name="rel">The Relation to test.</param>
		/// <returns>True if the From half has been cracked, false otherwise.</returns>
		public bool HasCracked(Relation rel)
		{
			return _sizedElements.ContainsKey(rel.From);
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
			long offset = 0;

			for (int i = _dataStack.Count - 2, prev = i + 1; i >= 0; --i)
			{
				if (_dataStack[i] != _dataStack[prev])
				{
					offset += _dataStack[prev].TellBits() - _dataStack[i].LengthBits;
					prev = i;
				}
			}

			System.Diagnostics.Debug.Assert(offset >= 0);
			return offset;
		}

		void addElements(DataElement de, BitStream data, long start, long end)
		{
			OnEnterHandleNodeEvent(de, start, data);

			var cont = de as DataElementContainer;
			if (cont != null)
			{
				foreach (var child in cont)
					addElements(child, data, 0, 0);
			}

			OnExitHandleNodeEvent(de, end, data);
		}

		#endregion

		#region Handlers

		#region Top Level Handlers

		void handleRoot(DataElement element, BitStream data)
		{
			_sizedElements = new OrderedDictionary<DataElement, Position>();
			_sizeRelations = new List<SizeRelation>();
			_elementsWithAnalyzer = new List<DataElement>();

			// Crack the model
			handleNode(element, data);

			// Handle any analyzers
			foreach (DataElement elem in _elementsWithAnalyzer)
			{
				OnAnalyzerEvent(elem, data);

				DataElementContainer parent = elem.parent;
				elem.analyzer.asDataElement(elem, null);
				var de = parent[elem.name];
				var pos = _sizedElements[elem];
				addElements(de, data, pos.begin, pos.end);
			}
		}

		/// <summary>
		/// Called to crack a DataElement based on an input stream.  This method
		/// will hand cracking off to a more specific method after performing
		/// some common tasks.
		/// </summary>
		/// <param name="elem">DataElement to crack</param>
		/// <param name="data">Input stream to use for data</param>
		void handleNode(DataElement elem, BitStream data)
		{
			List<BitStream> oldStack = null;

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
					long startPos = data.TellBits();
					var sizedData = elem.ReadSizedData(data, pos.size);
					var decodedData = elem.transformer.decode(sizedData);

					// Make a new stack of data for the decoded data
					oldStack = _dataStack;
					_dataStack = new List<BitStream>();
					_dataStack.Add(decodedData);

					// Use the size of the transformed data as the new size of the element
					handleCrack(elem, decodedData, decodedData.LengthBits);

					// Make sure the non-decoded data is at the right place
					if (data == decodedData)
						data.SeekBits(startPos + decodedData.LengthBits, System.IO.SeekOrigin.Begin);
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
			finally
			{
				if (oldStack != null)
					_dataStack = oldStack;
			}
		}

		void handlePlacelemt(DataElement element, BitStream data)
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
			DataElementContainer root = element.getRoot() as DataElementContainer;
			foreach (DataElement child in root.EnumerateAllElements())
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

			string debugName = element.debugName;
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

			// Clear placement now that it has occured
			newElem.placement = null;

			logger.Debug("handlePlacement: {0} -> {1}", debugName, newElem.fullName);

			OnPlacementEvent(element, newElem, oldParent);
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

			OnExceptionHandleNodeEvent(elem, data.TellBits(), data, e);
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
				var rel = elem.relations.Of<SizeRelation>();
				_sizeRelations.AddRange(rel);
			}

			OnEnterHandleNodeEvent(elem, pos.begin, data);

			return pos;
		}

		void handleNodeEnd(DataElement elem, BitStream data, Position pos)
		{
			// Completing this element might allow us to evaluate
			// outstanding size reation computations.

			for (int i = _sizeRelations.Count - 1; i >= 0; --i)
			{
				var rel = _sizeRelations[i];

				if (elem == rel.From)
				{
					var other = _sizedElements[rel.Of];
					long size = rel.GetValue();

					if (other.size.HasValue)
						logger.Debug("Size relation of {0} cracked again. Updating size from: {1} to: {2}",
							rel.Of.debugName, other.size, size);
					else
						logger.Debug("Size relation of {0} cracked. Updating size to: {1}",
							rel.Of.debugName, size);

					other.size = size;
					_sizeRelations.RemoveAt(i);
				}

				var cont = elem as DataElementContainer;
				if (cont != null && cont.isParentOf(rel.From))
				{
					// If we have finished cracking the parent of the From half of
					// an outstanding size relation, this means we never cracked
					// the From element. This can happen when the From half is in
					// a choice. Just stop tracking the incomplete relation and
					// keep going.

					_sizeRelations.RemoveAt(i);
				}
			}

			// Mark the end position of this element
			pos.end = data.TellBits() + getDataOffset();

			OnExitHandleNodeEvent(elem, pos.end, data);
		}

		void handleCrack(DataElement elem, BitStream data, long? size)
		{
			logger.Debug("Crack: {0} Size: {1}, {2}", elem.debugName,
				size.HasValue ? size.ToString() : "<null>", data.Progress);

			data.MarkStartOfElement(elem);

			elem.Crack(this, data, size);
		}

		#endregion

		#endregion

		#region Calculate Element Size

		long? getRelativeOffset(DataElement elem, BitStream data, long minOffset = 0)
		{
			var relations = elem.relations.Of<OffsetRelation>();
			if (!relations.Any())
				return null;

			// Ensure we have cracked the from half of the relation
			var rel = relations.Where(HasCracked).FirstOrDefault();
			if (rel == null)
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

		/// <summary>
		/// Searches data for the first occurance of token starting at offset.
		/// </summary>
		/// <param name="data">BitStream to search in.</param>
		/// <param name="token">BitStream to search for.</param>
		/// <param name="offset">How many bits after the current position of data to start searching.</param>
		/// <returns>The location of the token in data from the current position or null.</returns>
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

		bool? scanArray(Dom.Array array, ref long pos, List<Mark> tokens, Until until)
		{
			logger.Debug("scanArray: {0}", array.debugName);

			int tokenCount = tokens.Count;
			long arrayPos = 0;
			var ret = scan(array.origionalElement, ref arrayPos, tokens, null, until);

			for (int i = tokenCount; i < tokens.Count; ++i)
			{
				tokens[i].Optional = array.Count >= array.minOccurs;
				tokens[i].Position += pos;
			}

			if (ret.HasValue && ret.Value)
			{
				if (until == Until.FirstSized)
					ret = false;

				var relations = array.relations.Of<CountRelation>();
				if (relations.Any())
				{
					var rel = relations.Where(HasCracked).FirstOrDefault();
					if (rel != null)
					{
						arrayPos *= rel.GetValue();
						pos += arrayPos;
						logger.Debug("scanArray: {0} -> Count Relation: {1}, Size: {2}",
							array.debugName, rel.GetValue(), arrayPos);
						return ret;
					}
					else
					{
						logger.Debug("scanArray: {0} -> Count Relation: ???", array.debugName);
						return null;
					}
				}
				else if (array.minOccurs == 1 && array.maxOccurs == 1)
				{
					arrayPos *= array.occurs;
					pos += arrayPos;
					logger.Debug("scanArray: {0} -> Occurs: {1}, Size: {2}",
						array.debugName, array.occurs, arrayPos);
					return ret;
				}
				else
				{
					// If the count is unknown, treat the array unsized
					ret = null;

					// If no tokens were found in the array, we are done
					if (tokenCount == tokens.Count)
					{
						logger.Debug("scanArray: {0} -> Count Unknown", array.debugName);
						return ret;
					}
				}
			}

			// If we are looking for the first sized element, try cracking our first element
			if (until == Until.FirstSized)
			{
				logger.Debug("scanArray: {0} -> FirstSized", array.debugName);
				return false;
			}

			if (tokenCount == tokens.Count)
			{
				logger.Debug("scanArray: {0} -> No Tokens", array.debugName);
					//ret.HasValue ? "Deterministic" : "Unsized");
				return false;
			}

			// If we have tokens, keep scanning thru the dom.
			logger.Debug("scanArray: {0} -> Tokens", array.debugName);
			return true;
		}

		class Mark
		{
			public DataElement Element { get; set; }
			public long Position { get; set; }
			public bool Optional { get; set; }
		}

		enum Until { FirstSized, FirstUnsized };

		/// <summary>
		/// Scan elem and all children looking for a target element.
		/// The target can either be the first sized element or the first unsized element.
		/// If an unsized element is found, keep track of the determinism of the element.
		/// An element is determinstic if its size is unknown, but can be determined by calling
		/// crack(). Examples are a container with sized children or a null terminated string.
		/// </summary>
		/// <param name="elem">Element to start scanning at.</param>
		/// <param name="pos">The position of the scanner when 'until' occurs.</param>
		/// <param name="tokens">List of tokens found when scanning.</param>
		/// <param name="end">If non-null and an element with an offset relation is detected,
		/// record the element's absolute position and stop scanning.</param>
		/// <param name="until">When to stop scanning.
		/// Either first sized element or first unsized element.</param>
		/// <returns>Null if an unsized element was found.
		/// False if a deterministic element was found.
		/// True if all elements are sized.</returns>
		bool? scan(DataElement elem, ref long pos, List<Mark> tokens, Mark end, Until until)
		{
			if (elem.isToken)
			{
				tokens.Add(new Mark() { Element = elem, Position = pos, Optional = false });
				logger.Debug("scan: {0} -> Pos: {1}, Saving Token", elem.debugName, pos);
			}

			if (end != null)
			{
				long? offRel = getRelativeOffset(elem, _dataStack.First(), pos);
				if (offRel.HasValue)
				{
					end.Element = elem;
					end.Position = offRel.Value;
					logger.Debug("scan: {0} -> Pos: {1}, Offset relation: {2}", elem.debugName, pos, end.Position);
					return true;
				}
			}

			// See if we have a size relation
			var relations = elem.relations.Of<SizeRelation>();
			if (relations.Any())
			{
				var sizeRel = relations.Where(HasCracked).FirstOrDefault();

				if (sizeRel != null)
				{
					pos += sizeRel.GetValue();
					logger.Debug("scan: {0} -> Pos: {1}, Size relation: {2}", elem.debugName, pos, sizeRel.GetValue());
					return true;
				}
				else
				{
					logger.Debug("scan: {0} -> Pos: {1}, Size relation: ???", elem.debugName, pos);
					return null;
				}
			}

			// See if our length is defined
			if (elem.hasLength)
			{
				pos += elem.lengthAsBits;
				logger.Debug("scan: {0} -> Pos: {1}, Length: {2}", elem.debugName, pos, elem.lengthAsBits);
				return true;
			}

			// See if our length is determinstic, size is determined by cracking
			if (elem.isDeterministic)
			{
				logger.Debug("scan: {0} -> Pos: {1}, Determinstic", elem.debugName, pos);
				return false;
			}

			// If we are unsized, see if we are a container
			var cont = elem as DataElementContainer;
			if (cont == null)
			{
				logger.Debug("scan: {0} -> Offset: {1}, Unsized element", elem.debugName, pos);
				return null;
			}

			// Elements with transformers require a size
			if (cont.transformer != null)
			{
				logger.Debug("scan: {0} -> Offset: {1}, Unsized transformer", elem.debugName, pos);
				return null;
			}

			// Treat choices as unsized
			if (cont is Dom.Choice)
			{
				var choice = (Dom.Choice)cont;
				if (choice.choiceElements.Count == 1)
					return scan(choice.choiceElements[0], ref pos, tokens, end, until);

				logger.Debug("scan: {0} -> Offset: {1}, Unsized choice", elem.debugName, pos);

				if (until == Until.FirstSized)
					return false;

				return null;
			}

			if (cont is Dom.Array)
			{
				return scanArray((Dom.Array)cont, ref pos, tokens, until);
			}

			logger.Debug("scan: {0}", elem.debugName);

			foreach (var child in cont)
			{
				bool? ret = scan(child, ref pos, tokens, end, until);

				// An unsized element was found
				if (!ret.HasValue)
					return ret;

				// Aa unsized but deterministic element was found
				if (ret.Value == false)
					return ret;

				// If we are looking for the first sized element than this
				// element size is determined by cracking all the children
				if (until == Until.FirstSized)
					return false;
			}

			// All children are sized, so we are sized
			return true;
		}

		/// <summary>
		/// Get the size of the data element.
		/// </summary>
		/// <param name="elem">Element to size</param>
		/// <param name="data">Bits to crack</param>
		/// <returns>Null if size is unknown or the size in bits.</returns>
		long? getSize(DataElement elem, BitStream data)
		{
			logger.Debug("getSize: -----> {0}", elem.debugName);

			long pos = 0;

			var ret = scan(elem, ref pos, new List<Mark>(), null, Until.FirstSized);

			if (ret.HasValue)
			{
				if (ret.Value)
				{
					logger.Debug("getSize: <----- Size: {0}", pos);
					return pos;
				}

				logger.Debug("getSize: <----- Deterministic: ???");
				return null;
			}

			var tokens = new List<Mark>();
			var end = new Mark();

			ret = lookahead(elem, ref pos, tokens, end);

			// 1st priority, end placement
			if (end.Element != null)
			{
				pos = end.Position - pos;
				logger.Debug("getSize: <----- Placement: {0}", pos);
				return pos;
			}

			// 2rd priority, token scan
			long? closest = null;
			Mark winner = null;

			foreach (var token in tokens)
			{
				long? where = findToken(data, token.Element.Value, token.Position);
				if (!where.HasValue && !token.Optional)
				{
					logger.Debug("getSize: <----- Missing Required Token: ???");
					return where;
				}

				if (!where.HasValue)
					continue;

				if (!closest.HasValue || closest.Value > where.Value)
				{
					closest = where.Value;
					winner = token;
				}
			}

			if (closest.HasValue)
			{
				logger.Debug("getSize: <----- {0} Token: {1}",
					winner.Optional ? "Optional" : "Required",
					closest.ToString());
				return closest;
			}

			if (tokens.Count > 0 && ret.HasValue && ret.Value)
			{
				pos = data.LengthBits - (data.TellBits() + pos);
				logger.Debug("getSize: <----- Missing Optional Token: {0}", pos);
				return pos;
			}

			// 3nd priority, last unsized element
			if (ret.HasValue)
			{
				if (ret.Value && (pos != 0 || !(elem is DataElementContainer)))
				{
					pos = data.LengthBits - (data.TellBits() + pos);
					logger.Debug("getSize: <----- Last Unsized: {0}", pos);
					return pos;
				}

				logger.Debug("getSize: <----- Last Unsized: ???");
				return null;
			}

			if (elem is Dom.Array)
			{
				logger.Debug("getSize: <----- Array Not Last Unsized: ???");
				return null;
			}

			if (elem is Dom.Choice)
			{
				logger.Debug("getSize: <----- Choice Not Last Unsized: ???");
				return null;
			}

			logger.Debug("getSize: <----- Not Last Unsized: ???");
			return null;
		}

		/// <summary>
		/// Scan all elements after elem looking for the first unsized element.
		/// If an unsized element is found, keep track of the determinism of the element.
		/// An element is determinstic if its size is unknown, but can be determined by calling
		/// crack(). Examples are a container with sized children or a null terminated string.
		/// </summary>
		/// <param name="elem">Start scanning at this element's next sibling.</param>
		/// <param name="pos">The position of the scanner when 'until' occurs.</param>
		/// <param name="tokens">List of tokens found when scanning.</param>
		/// <param name="end">If non-null and an element with an offset relation is detected,
		/// record the element's absolute position and stop scanning.
		/// Either first sized element or first unsized element.</param>
		/// <returns>Null if an unsized element was found.
		/// False if a deterministic element was found.
		/// True if all elements are sized.</returns>
		bool? lookahead(DataElement elem, ref long pos, List<Mark> tokens, Mark end)
		{
			logger.Debug("lookahead: {0}", elem.debugName);

			// Ensure all elements are sized until we reach either
			// 1) A token
			// 2) An offset relation we have cracked that can be satisfied
			// 3) The end of the data model

			DataElement prev = elem;
			bool? final = true;

			while (true)
			{
				// Get the next sibling
				var curr = prev.nextSibling();

				if (curr != null)
				{
					var ret = scan(curr, ref pos, tokens, end, Until.FirstUnsized);
					if (!ret.HasValue || ret.Value == false)
						return ret;

					if (end.Element != null)
						return final;
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
				else
				{
					if (!(elem is DataElementContainer) && (prev.parent is Dom.Array))
					{
						var array = (Dom.Array)prev.parent;
						if (array.maxOccurs == -1 || array.Count < array.maxOccurs)
						{
							long arrayPos = pos;
							var ret = scanArray(array, ref arrayPos, tokens, Until.FirstUnsized);

							// If the array isn't sized and we haven't met the minimum, propigate
							// the lack of size, otherwise keep scanning
							if ((!ret.HasValue || ret.Value == false) && array.Count < array.minOccurs)
									return ret;
						}
					}

					// no more siblings, ascend
					curr = prev.parent;
				}

				prev = curr;
			}

			return final;
		}

		#endregion
	}
}

// end
