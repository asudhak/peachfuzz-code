
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

				if (currentElement == null)
					break;

				// Make sure we scape Choice's!
				do
				{
					currentElement = currentElement.parent;
				}
				while (currentElement != null && currentElement is Choice);

				if (currentElement == null)
					break;

				if (currentElement.isToken)
				{
					token = currentElement;
					logger.Debug("isTokenNext(true): {0} {1}", element.fullName, size);
					return true;
				}

				if (currentElement.hasLength)
				{
					size += currentElement.lengthAsBits;
				}
				else
				{
					size = 0;
					logger.Debug("isTokenNext(false): {0} {1}", element.fullName, size);
					return false;
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
		public void handleNode(DataElement element, BitStream data)
		{
			try
			{
				logger.Trace("handleNode: {0} data.TellBits: {1}/{2}", element.fullName, data.TellBits(), data.TellBytes());
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

				element.Crack(this, data);

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

			logger.Trace("determineElementSize: Returning: "+size);
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
			var root = ObjectCopier.Clone<DataElementContainer>(element.getRoot() as DataElementContainer);
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
	}
}

// end
