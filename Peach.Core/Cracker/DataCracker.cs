
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

	#endregion

	/// <summary>
	/// Crack data into a DataModel.
	/// </summary>
	public class DataCracker
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Are we looking ahead to see what will happen?
		/// </summary>
		public bool IsLookAhead { get; set; }

		/// <summary>
		/// A stack of sized DataElement containers.
		/// </summary>
		public List<DataElement> _sizedBlockStack = new List<DataElement>();
		/// <summary>
		/// Mapping of elements from _sizedBlockStack to there lengths.  All lengths are in
		/// BITS!
		/// </summary>
		public Dictionary<DataElement, long> _sizedBlockMap = new Dictionary<DataElement, long>();

		/// <summary>
		/// Elements that have analyzers attached.  We run them all post-crack.
		/// </summary>
		List<DataElement> _elementsWithAnalyzer = new List<DataElement>();

		#region Events

		public event EnterHandleNodeEventHandler EnterHandleNodeEvent;
		protected void OnEnterHandleNodeEvent(DataElement element, BitStream data)
		{
			if (IsLookAhead)
				return;

			if(EnterHandleNodeEvent != null)
				EnterHandleNodeEvent(element, data);
		}
		
		public event ExitHandleNodeEventHandler ExitHandleNodeEvent;
		protected void OnExitHandleNodeEvent(DataElement element, BitStream data)
		{
			if (IsLookAhead)
				return;

			if (ExitHandleNodeEvent != null)
				ExitHandleNodeEvent(element, data);
		}

		public event ExceptionHandleNodeEventHandler ExceptionHandleNodeEvent;
		protected void OnExceptionHandleNodeEvent(DataElement element, BitStream data, Exception e)
		{
			if (IsLookAhead)
				return;

			if (ExceptionHandleNodeEvent != null)
				ExceptionHandleNodeEvent(element, data, e);
		}


		#endregion

		/// <summary>
		/// Main entry method that will take a data stream and parse it into a data model.
		/// </summary>
		/// <remarks>
		/// Method will throw one of two exceptions on an error: CrackingFailure, or NotEnoughDataException.
		/// </remarks>
		/// <param name="model">DataModel to import data into</param>
		/// <param name="data">Data stream to read data from</param>
		public void CrackData(DataModel model, BitStream data)
		{
			_sizedBlockStack = new List<DataElement>();
			_sizedBlockMap = new Dictionary<DataElement, long>();

			IsLookAhead = false;

			handleNode(model, data);

			// Handle any Placement's
			handlePlacement(model, data);

			// Handle any analyzers
			foreach (DataElement elem in _elementsWithAnalyzer)
				elem.analyzer.asDataElement(elem, null);
		}

		protected void handlePlacement(DataModel model, BitStream data)
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
						throw new CrackingFailure("Error, unable to resolve Placement on element '" + element.name +
							"' with 'after' == '" + element.placement.after + "'.", element, data);
					newElem = element.MoveAfter(after);
				}
				else if (element.placement.before != null)
				{
					DataElement before = element.find(element.placement.before);
					if (before == null)
						throw new CrackingFailure("Error, unable to resolve Placement on element '" + element.name +
							"' with 'after' == '" + element.placement.after + "'.", element, data);
					newElem = element.MoveBefore(before);
				}

				// Update fixups
				foreach (var fixup in fixups)
				{
					fixup.Item1.updateRef(fixup.Item2, newElem.fullName);
				}
			}
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
				if (currentElement == null && (oldElement.parent == null || oldElement.parent.transformer != null))
					break;
				else if (currentElement == null)
					currentElement = oldElement.parent;
				else
				{
					if (currentElement.hasLength)
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
				else if (next.length == 0)
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
						while (sibling == null && parent != null && parent.transformer == null)
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
		/// Called to crack a DataElement based on an input stream.  This method
		/// will hand cracking off to a more specific method after performing
		/// some common tasks.
		/// </summary>
		/// <param name="element">DataElement to crack</param>
		/// <param name="data">Input stream to use for data</param>
		public void handleNode(DataElement element, BitStream data)
		{
			try
			{
				if(element == null || data == null)
					throw new CrackingFailure(element, data);

				logger.Debug("handleNode ------------------------------------");
				logger.Debug("handleNode: {0} data.TellBits: {1}/{2}", element.fullName, data.TellBits(), data.TellBytes());
				OnEnterHandleNodeEvent(element, data);

				long startingPosition = data.TellBits();
				bool hasOffsetRelation = false;

				// Offset relation
				if (element.relations.hasOfOffsetRelation)
				{
					hasOffsetRelation = true;
					OffsetRelation rel = element.relations.getOfOffsetRelation();
					long offset = (long)rel.GetValue();

					if (!rel.isRelativeOffset)
					{
						// Relative from start of data
						data.SeekBytes((int)offset, System.IO.SeekOrigin.Begin);
					}
					else if (rel.relativeTo == null)
					{
						data.SeekBytes((int)offset, System.IO.SeekOrigin.Current);
					}
					else
					{
						DataElement relativeTo = rel.From.find(rel.relativeTo);
						if (relativeTo == null)
							throw new CrackingFailure("Unable to locate 'relativeTo' element in relation attached to '" +
								rel.From.fullName + "'.", element, data);

						long relativePosition = data.DataElementPosition(relativeTo);
						data.SeekBits((int)relativePosition, System.IO.SeekOrigin.Begin);
						data.SeekBytes((int)offset, System.IO.SeekOrigin.Current);
					}
				}

				data.MarkStartOfElement(element);
				
				// If we have a data.stream == Publisher this is OK
				//if (data.TellBytes() == data.LengthBytes)
				//    throw new CrackingFailure("'" + element.fullName +
				//        "' could not be cracked sinze buffer has zero bytes left.", element, data);

                if (element.transformer != null)
                {
                    long? size = determineElementSize(element, data);

                    if (size == null)
                        throw new CrackingFailure("Could not determine size for transformer!", element, data);

                    var decodedData = element.transformer.decode(data.ReadBitsAsBitStream(size.Value));
                    element.Crack(this, decodedData);
                }
                else
				element.Crack(this, data);

				if (element.constraint != null)
				{
					logger.Debug("Running constraint [" + element.constraint + "]");

					Dictionary<string, object> scope = new Dictionary<string,object>();
					scope["element"] = element;
					
					try
					{
						scope["value"] = (string)element.InternalValue;
						logger.Debug("Constraint, value=[" + (string)element.InternalValue + "].");
					}
					catch
					{
						scope["value"] = (byte[])element.InternalValue;
						logger.Debug("Constraint, value=byte array.");
					}

					object oReturn = Scripting.EvalExpression(element.constraint, scope);

					if (!((bool)oReturn))
						throw new CrackingFailure("Constraint failed.", element, data);
				}

				if (element.analyzer != null)
					_elementsWithAnalyzer.Add(element);

				if (hasOffsetRelation)
					data.SeekBits(startingPosition, System.IO.SeekOrigin.Begin);

				OnExitHandleNodeEvent(element, data);
			}
			catch (CrackingFailure ex)
			{
				logger.Debug("handleNode: Cracking failed: {0}", ex.Message);
				throw;
			}
			catch (Exception e)
			{
				logger.Debug("handleNode: Exception occured: {0}", e.ToString());
				OnExceptionHandleNodeEvent(element, data, e);

				// Rethrow
				throw;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="data"></param>
		/// <returns>Returns size in bits</returns>
		public long? determineElementSize(DataElement element, BitStream data)
		{
			logger.Debug("determineElementSize: {0} data.TellBits: {1}/{2}", element.fullName, data.TellBits(), data.TellBytes());

			// Size in bits
			long? size = null;

			// Check for relation and/or size
			if (element.hasLength)
			{
				size = element.lengthAsBits;
			}
			else if(element.relations.hasOfSizeRelation)
			{
				size = element.relations.getOfSizeRelation().GetValue();
			}
			else
			{
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
				else if (isLastUnsizedElement(element, ref nextSize))
				{
					size = data.LengthBits - (data.TellBits() + nextSize);
				}
			}

			if(size == null)
				logger.Debug("determineElementSize: Returning: null (could not determine size)");
			else
				logger.Debug("determineElementSize: Returning: " + size);

			return size;
		}

		/// <summary>
		/// Parse ahead and verify if things work out OKAY.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool lookAhead(DataElement element, BitStream data)
		{
			try
			{
				IsLookAhead = true;


				var root = element.getRoot().Clone() as DataElementContainer;
				var node = root.find(element.fullName);
				var sibling = node.nextSibling();

				if (sibling == null)
					return true;

				long position = data.TellBits();

				try
				{
					handleNode(sibling, data);
				}
				catch (Exception)
				{
					return false;
				}
				finally
				{
					data.SeekBits(position, System.IO.SeekOrigin.Begin);
				}

				return true;
			}
			finally
			{
				IsLookAhead = false;
			}
		}
	}
}

// end
