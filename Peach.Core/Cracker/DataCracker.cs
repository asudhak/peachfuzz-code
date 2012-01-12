
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
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
		NLog.Logger logger = LogManager.GetLogger("Peach.Core.Cracker.DataCracker");

		/// <summary>
		/// A stack of sized DataElement containers.
		/// </summary>
		List<DataElement> _sizedBlockStack = new List<DataElement>();
		/// <summary>
		/// Mapping of elements from _sizedBlockStack to there lengths.  All lengths are in
		/// BITS!
		/// </summary>
		Dictionary<DataElement, long> _sizedBlockMap = new Dictionary<DataElement, long>();

		/// <summary>
		/// The full data stream.
		/// </summary>
		BitStream _data = null;

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
			if(ExitHandleNodeEvent != null)
				ExitHandleNodeEvent(element, data);
		}

		public event ExceptionHandleNodeEventHandler ExceptionHandleNodeEvent;
		protected void OnExceptionHandleNodeEvent(DataElement element, BitStream data, Exception e)
		{
			if(ExceptionHandleNodeEvent != null)
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
			_data = data;

			handleNode(model, data);

			// Handle any Placement's
			handlePlacement(model, data);
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
				// Locate any fixups and relations so we can update them

				List<Relation> ofs = new List<Relation>();
				List<Relation> froms = new List<Relation>();
				List<Fixup> fixups = new List<Fixup>();

				foreach (Relation relation in element.relations)
				{
					if(relation.Of == element)
						ofs.Add(relation);
					else if(relation.From == element)
						froms.Add(relation);
					else
						throw new CrackingFailure("Error, unable to resolve Relations of/from to match current element.",
							element, data);
				}

				foreach (DataElement child in model.EnumerateAllElements())
				{
					if (child.relations.Count > 0)
					{
						foreach (Relation relation in child.relations)
						{
							if (relation.Of == element)
								ofs.Add(relation);
						}
					}

					if (child.fixup != null && child.fixup.arguments.ContainsKey("ref"))
					{
						if (child.find((string)child.fixup.arguments["ref"]) == element)
							fixups.Add(child.fixup);
					}
				}

				// Move element

				DataElementContainer oldParent = element.parent;
				DataElementContainer newParent = null;

				string oldName = element.name;
				string newName = null;
				string newFullname = null;

				if (element.placement.after != null)
				{
					DataElement after = element.find(element.placement.after);
					if (after == null)
						throw new CrackingFailure("Error, unable to resolve Placement on element '" + element.name + 
							"' with 'after' == '" + element.placement.after + "'.", element, data);

					newParent = after.parent;

					newName = oldName;
					for (int i = 0; newParent.ContainsKey(newName); i++)
						newName = oldName + "_" + i;

					element.parent.Remove(element);
					element.name = newName;

					newParent.Insert(newParent.IndexOf(after)+1, element);
				}
				else if (element.placement.before != null)
				{
					DataElement before = element.find(element.placement.before);
					if (before == null)
						throw new CrackingFailure("Error, unable to resolve Placement on element '" + element.name + 
							"' with 'before' == '" + element.placement.before + "'.", element, data);

					newParent = before.parent;

					newName = oldName;
					for (int i = 0; newParent.ContainsKey(oldName); i++)
						newName = oldName + "_" + i;

					element.parent.Remove(element);
					element.name = newName;

					newParent.Insert(newParent.IndexOf(before), element);
				}

				newFullname = element.fullName;

				// Update relations

				foreach (Relation relation in ofs)
				{
					relation.OfName = newFullname;
				}
				foreach (Relation relation in froms)
				{
					relation.FromName = newFullname;
				}

				// Update fixups

				foreach (Fixup fixup in fixups)
				{
					// We might have to create a new fixup!

					fixup.arguments["ref"] = new Variant(newFullname);
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
					var choice = element as Choice;

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
		protected bool isLastUnsizedElement(DataElement element, ref long size)
		{
			logger.Trace("isLastUnsizedElement: {0} {1}", element.fullName, size);

			DataElement oldElement = element;
			DataElement currentElement = element;

			while (true)
			{
				currentElement = oldElement.nextSibling();
				if (currentElement == null && oldElement.parent == null)
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
						size = 0;
						logger.Debug("isLastUnsizedElement(false): {0} {1}", element.fullName, size);
						return false;
					}
				}

				oldElement = currentElement;
			}

			logger.Debug("isLastUnsizedElement(true): {0} {1}", element.fullName, size);
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
		/// Is there a token next in the list of elements to parse, or
		/// can we calculate our distance to the next token?
		/// </summary>
		/// <param name="element">Element to check</param>
		/// <param name="size">Set to the number of bits from element to token.</param>
		/// <param name="token">Set to token element if found</param>
		/// <returns>Returns true if found token, else false.</returns>
		protected bool isTokenNext(DataElement element, ref long size, ref DataElement token)
		{
			logger.Trace("isTokenNext: {0} {1}", element.fullName, size);

			DataElement currentElement = element;
			token = null;
			size = 0;

			while (currentElement != null)
			{
				currentElement = currentElement.nextSibling();
				if (currentElement == null && currentElement.parent == null)
					break;
				else if (currentElement == null)
				{
					// Make sure we scape Choice's!
					do
					{
						currentElement = currentElement.parent;
					}
					while (currentElement is Choice);
				}
				else
				{
					if (currentElement.isToken)
					{
						token = currentElement;
						logger.Debug("isTokenNext(true): {0} {1}", element.fullName, size);
						return true;
					}
					if (currentElement.hasLength)
						size += currentElement.lengthAsBits;
					else
					{
						size = 0;
						logger.Debug("isTokenNext(false): {0} {1}", element.fullName, size);
						return false;
					}
				}
			}

			size = 0;
			logger.Debug("isTokenNext(false): {0} {1}", element.fullName, size);
			return false;
		}

		/// <summary>
		/// Called to crack a DataElement based on an input stream.  This method
		/// will hand cracking off to a more specific method after performing
		/// some common tasks.
		/// </summary>
		/// <param name="element">DataElement to crack</param>
		/// <param name="data">Input stream to use for data</param>
		protected void handleNode(DataElement element, BitStream data)
		{
			try
			{
				logger.Trace("handleNode: {0} data.TellBits: {1}", element.fullName, data.TellBits());
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

				// Do array handling
				if (element is Dom.Array)
				{
					handleArray(element as Dom.Array, data);
				}
				else if (element is Choice)
				{
					handleChoice(element as Choice, data);
				}
				else if (element is Flags)
				{
					handleFlags(element as Flags, data);
				}
				else if (element is DataElementContainer) // Should also catch DataModel's
				{
					handleDataElementContainer(element as DataElementContainer, data);
				}
				else if (element is Dom.String)
				{
					handleString(element as Dom.String, data);
				}
				else if (element is Number)
				{
					handleNumber(element as Number, data);
				}
				else if (element is Blob)
				{
					handleBlob(element as Blob, data);
				}
				else if (element is Padding)
				{
					handlePadding(element as Padding, data);
				}
				else
				{
					throw new ApplicationException("Error, found unknown element in DOM tree! " + element.GetType().ToString());
				}

				data.MarkEndOfElement(element);

				if (hasOffsetRelation)
					data.SeekBits(startingPosition, System.IO.SeekOrigin.Begin);

				OnExitHandleNodeEvent(element, data);
			}
			catch (Exception e)
			{
				logger.Debug("handleNode: Exception occured: {0}", e.ToString());
				OnExceptionHandleNodeEvent(element, data, e);

				// Rethrow
				throw;
			}
		}

		protected void handleArray(Dom.Array element, BitStream data)
		{
			logger.Trace("handleArray: {0} data.TellBits: {1}", element.fullName, data.TellBits());
			logger.Debug("handleArray: {0} type: {1}", element.fullName, element[0].GetType());

			element.origionalElement = element[0];
			element.Clear();

			if (element.maxOccurs > 1)
			{
				for (int cnt = 0; true; cnt++)
				{
					logger.Debug("handleArray: Trying #{0}", cnt.ToString());

					long pos = data.TellBits();
					DataElement clone = ObjectCopier.Clone<DataElement>(element.origionalElement);
					clone.name = clone.name + "_" + cnt.ToString();
					clone.parent = element;
					element.Add(clone);

					try
					{
						handleNode(clone, data);
					}
					catch
					{
						logger.Debug("handleArray: Failed on #{0}", cnt.ToString());
						element.Remove(clone);
						data.SeekBits(pos, System.IO.SeekOrigin.Begin);
						break;
					}

					if (data.TellBits() == data.LengthBits)
					{
						logger.Debug("handleArray: Found EOF, all done!");
						break;
					}
				}
			}
		}

		/// <summary>
		/// Handle crack a Block element.
		/// </summary>
		/// <param name="element">Block to crack</param>
		/// <param name="data">Data stream to use when cracking</param>
		protected void handleDataElementContainer(DataElementContainer element, BitStream data)
		{
			logger.Trace("handleDataElementContainer: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			BitStream sizedData = data;
			SizeRelation sizeRelation = null;
			long startPosition = data.TellBits();

			// Do we have relations or a length?
			if (element.relations.hasOfSizeRelation)
			{
				sizeRelation = element.relations.getOfSizeRelation();

				if (!element.isParentOf(sizeRelation.From))
				{
					long size = sizeRelation.GetValue();
					_sizedBlockStack.Add(element);
					_sizedBlockMap[element] = size;

					sizedData = data.ReadBitsAsBitStream(size);
					sizeRelation = null;
				}
			}
			else if (element.hasLength)
			{
				long size = element.lengthAsBits;
				_sizedBlockStack.Add(element);
				_sizedBlockMap[element] = size;

				sizedData = data.ReadBitsAsBitStream(size);
			}

			// Handle children
			foreach (DataElement child in element)
			{
				handleNode(child, sizedData);

				// If we have an unused size relation, wait until we
				// can use it then re-size our data.
				if (sizeRelation != null)
				{
					if (child is DataElementContainer && 
						((DataElementContainer)child).isParentOf(sizeRelation.From))
					{
						long size = (long)sizeRelation.GetValue();
						_sizedBlockStack.Add(element);
						_sizedBlockMap[element] = size;

						// update size based on what we have currently read
						size -= data.TellBits() - startPosition;

						sizedData = data.ReadBitsAsBitStream(size);
						sizeRelation = null;
					}
					else if(child == sizeRelation.From)
					{
						long size = (long)sizeRelation.GetValue();
						_sizedBlockStack.Add(element);
						_sizedBlockMap[element] = size;

						// update size based on what we have currently read
						size -= data.TellBits() - startPosition;

						sizedData = data.ReadBitsAsBitStream(size);
						sizeRelation = null;
					}
				}
			}

			// Remove our element from the stack & map
			if (sizedData != data)
			{
				_sizedBlockStack.Remove(element);
				_sizedBlockMap.Remove(element);
			}
		}

		protected void handleChoice(Choice element, BitStream data)
		{
			logger.Trace("handleChoice: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			BitStream sizedData = data;
			SizeRelation sizeRelation = null;

			// Do we have relations or a length?
			if (element.relations.hasOfSizeRelation)
			{
				sizeRelation = element.relations.getOfSizeRelation();

				if (!element.isParentOf(sizeRelation.From))
				{
					int size = (int)sizeRelation.GetValue();
					_sizedBlockStack.Add(element);
					_sizedBlockMap[element] = size;

					sizedData = new BitStream(data.ReadBytes(size));
					sizeRelation = null;
				}
			}
			else if (element.hasLength)
			{
				long size = element.lengthAsBits;
				_sizedBlockStack.Add(element);
				_sizedBlockMap[element] = size;

				sizedData = new BitStream(data.ReadBytes(size));
			}

			long startPosition = sizedData.TellBits();

			foreach (DataElement child in element.choiceElements.Values)
			{
				try
				{
					child.parent = element;
					sizedData.SeekBits(startPosition, System.IO.SeekOrigin.Begin);
					handleNode(child, sizedData);
					element.SelectedElement = child;
					break;
				}
				catch (CrackingFailure)
				{
					// NEXT!
				}
			}

			if (element.SelectedElement == null)
				throw new CrackingFailure("Unable to crack '"+element.fullName+"'.", element, data);
		}

		protected void handleString(Dom.String element, BitStream data)
		{
			logger.Trace("handleString: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			if (element.nullTerminated)
			{
				// Locate NULL character in stream
				bool foundNull = false;
				bool twoNulls = element.stringType == StringType.Utf16 || element.stringType == StringType.Utf16be;
				long currentPos = data.TellBits();

				for (long i = data.TellBytes(); i < data.LengthBytes; i++)
				{
					if (data.ReadByte() == 0)
					{
						if (twoNulls)
						{
							if (data.ReadByte() == 0)
							{
								foundNull = true;
								break;
							}
							else
							{
								data.SeekBits(-8, System.IO.SeekOrigin.Current);
								continue;
							}
						}
						else
						{
							foundNull = true;
							break;
						}
					}
				}

				if (!foundNull)
					throw new CrackingFailure("Did not locate NULL in data stream for String '" + element.fullName + "'.", element, data);

				long endPos = data.TellBits();

				// Do not include NULLs in our read.
				long byteCount = ((endPos - currentPos) / 8) - 1;
				if (twoNulls)
					byteCount--;

				data.SeekBits(currentPos, System.IO.SeekOrigin.Begin);
				byte [] value = data.ReadBytes(byteCount);
				string strValue = ASCIIEncoding.GetEncoding(element.stringType.ToString()).GetString(value);
				element.DefaultValue = new Variant(strValue);

				// Now skip past nulls
				if (twoNulls)
					data.SeekBits(16, System.IO.SeekOrigin.Current);
				else
					data.SeekBits(8, System.IO.SeekOrigin.Current);

				return;
			}

			// String length in bytes
			long? stringLength = determineElementSize(element, data) / 8 ;

			// TODO - Make both length and size for strings.  Length is always in chars.
			if (stringLength == null && element.isToken)
				stringLength = ((string)element.DefaultValue).Length;

			if (stringLength == null)
				throw new CrackingFailure("Unable to crack '" + element.fullName + "'.", element, data);

			if ((data.TellBytes() + stringLength) > data.LengthBytes)
				throw new CrackingFailure("String '" + element.fullName +
					"' has length of '" + stringLength + "' but buffer only has '" +
					(data.LengthBytes - data.TellBytes()) + "' bytes left.", element, data);

			var defaultValue = new Variant(
				ASCIIEncoding.GetEncoding(element.stringType.ToString()).GetString(
				data.ReadBytes((int)stringLength)));

			if (element.isToken)
				if (defaultValue != element.DefaultValue)
					throw new CrackingFailure("String marked as token, values did not match '" + ((string)defaultValue) + "' vs. '" + ((string)element.DefaultValue) + "'.", element, data);

			element.DefaultValue = defaultValue;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="element"></param>
		/// <param name="data"></param>
		/// <returns>Returns size in bits</returns>
		protected long? determineElementSize(DataElement element, BitStream data)
		{
			logger.Trace("determineElementSize: {0} data.TellBits: {1}", element.fullName, data.TellBits());

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

				if (isLastUnsizedElement(element, ref nextSize))
					size = data.LengthBits - (data.TellBits() + nextSize);
				
				else if (isTokenNext(element, ref nextSize, ref token))
				{
					throw new NotImplementedException("Need to implement this!");
				}
			}

			return size;
		}

		protected void handleNumber(Number element, BitStream data)
		{
			logger.Trace("handleNumber: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			if (data.LengthBits < data.TellBits() + element.lengthAsBits)
				throw new CrackingFailure("Failed cracking Number '" + element.fullName + "'.", element, data);

			if (element.LittleEndian)
				data.LittleEndian();
			else
				data.BigEndian();

			Variant defaultValue;

			if (element.Signed)
			{
				switch (element.lengthAsBits)
				{
					case 8:
						defaultValue = new Variant(data.ReadInt8());
						break;
					case 16:
						defaultValue = new Variant(data.ReadInt16());
						break;
					case 32:
						defaultValue = new Variant(data.ReadInt32());
						break;
					case 64:
						defaultValue = new Variant(data.ReadInt64());
						break;
					default:
						throw new CrackingFailure("Number '" + element.name + "' had unsupported size '" + element.lengthAsBits + "'.", element, data);
				}
			}
			else
			{
				switch (element.lengthAsBits)
				{
					case 8:
						defaultValue = new Variant(data.ReadUInt8());
						break;
					case 16:
						defaultValue = new Variant(data.ReadUInt16());
						break;
					case 32:
						defaultValue = new Variant(data.ReadUInt32());
						break;
					case 64:
						defaultValue = new Variant(data.ReadUInt64());
						break;
					default:
						throw new CrackingFailure("Number '" + element.name + "' had unsupported size '" + element.lengthAsBits + "'.", element, data);
				}
			}

			if(element.isToken)
				if(defaultValue != element.DefaultValue)
					throw new CrackingFailure("Number marked as token, values did not match '"+ ((string)defaultValue) +"' vs. '"+((string)element.DefaultValue)+"'.", element, data);

			element.DefaultValue = defaultValue;
		}

		protected void handleFlags(Flags element, BitStream data)
		{
			logger.Trace("handleFlags: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			if (data.LengthBits <= (data.TellBits() + element.size))
				throw new CrackingFailure("Not enough data to crack '"+element.fullName+"'.", element, data);

			long startPos = data.TellBits();

			foreach (DataElement child in element)
			{
				data.SeekBits(startPos, System.IO.SeekOrigin.Begin);
				data.SeekBits(((Flag)child).position, System.IO.SeekOrigin.Current);
				handleFlag(child as Flag, data);
			}

			// Make sure we land at end of Flags
			data.SeekBits(startPos, System.IO.SeekOrigin.Begin);
			data.SeekBits((int)element.size, System.IO.SeekOrigin.Current);
		}

		protected void handleFlag(Flag element, BitStream data)
		{
			logger.Trace("handleFlag: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			var defaultValue = new Variant(data.ReadBits(element.size));

			if (element.isToken)
			    if (defaultValue != element.DefaultValue)
			        throw new CrackingFailure("Flag '" + element.name + "' marked as token, values did not match '" + (string)defaultValue + "' vs. '" + (string)element.DefaultValue + "'.", element, data);

			element.DefaultValue = defaultValue;
		}

		protected void handleBlob(Blob element, BitStream data)
		{
			logger.Trace("handleBlob: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			// Length in bits
			long? blobLength = determineElementSize(element, data);

			if (blobLength == null && element.isToken)
				blobLength = ((BitStream)element.DefaultValue).LengthBits;

			if (blobLength == null)
				throw new CrackingFailure("Unable to crack Blob '" + element + "'.", element, data);

			if ((data.TellBits() + blobLength) > data.LengthBits)
				throw new CrackingFailure("Blob '" + element.fullName +
					"' has length of '" + blobLength + "' bits but buffer only has '" +
					(data.LengthBits - data.TellBits()) + "' bits left.", element, data);

			Variant defaultValue = new Variant(new byte[0]);

			if (blobLength > 0)
				defaultValue = new Variant(data.ReadBitsAsBitStream((long)blobLength));

			if (element.isToken)
				if (defaultValue != element.DefaultValue)
					throw new CrackingFailure("Blob '" + element.name + "' marked as token, values did not match '" +
						defaultValue.ToHex(100) + "' vs. '" + element.DefaultValue.ToHex(100) + "'.", element, data);

			element.DefaultValue = defaultValue;
		}

		protected void handlePadding(Padding element, BitStream data)
		{
			logger.Trace("handlePadding: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			// Length in bits
			long paddingLength = element.Value.LengthBits;

			if ((data.TellBits() + paddingLength) > data.LengthBits)
				throw new CrackingFailure("Placement '" + element.fullName +
					"' has length of '" + paddingLength + "' bits but buffer only has '" +
					(data.LengthBits - data.TellBits()) + "' bits left.", element, data);

			data.SeekBits(paddingLength, System.IO.SeekOrigin.Current);
		}
	}
}

// end
