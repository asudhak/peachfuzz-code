using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;

namespace Peach.Core.Cracker
{
	/// <summary>
	/// Crack data into a DataModel.
	/// </summary>
	public class DataCracker
	{
		/// <summary>
		/// A stack of sized DataElement containers.
		/// </summary>
		List<DataElement> _sizedBlockStack = new List<DataElement>();
		/// <summary>
		/// Mapping of elements from _sizedBlockStack to there lengths.
		/// </summary>
		Dictionary<DataElement, int> _sizedBlockMap = new Dictionary<DataElement, int>();

		/// <summary>
		/// The full data stream.
		/// </summary>
		BitStream _data = null;

		public DataModel CrackData(DataModel model, BitStream data)
		{
			_sizedBlockStack = new List<DataElement>();
			_sizedBlockMap = new Dictionary<DataElement, int>();
			_data = data;

			handleNode(model, data);

			return model;
		}

		/// <summary>
		/// Is element last unsized element in currently sized area.  If not
		/// then 'size' is set to the number of bytes from element to ened of
		/// the sized data.
		/// </summary>
		/// <param name="element">Element to check</param>
		/// <param name="size">Set to the number of bytes from element to end of the data.</param>
		/// <returns>Returns true if last unsigned element, else false.</returns>
		protected bool isLastUnsizedElement(DataElement element, ref int size)
		{
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
						size += currentElement.length;
					else if (currentElement is DataElementContainer)
					{
						foreach(DataElement child in ((DataElementContainer)currentElement))
						{
							if (!_isLastUnsizedElementRecursive(child, ref size))
								return false;
						}
					}
					else
					{
						size = 0;
						return false;
					}
				}

				oldElement = currentElement;
			}

			return true;
		}

		protected bool _isLastUnsizedElementRecursive(DataElement elem, ref int size)
		{
			if (elem == null)
				return false;

			if (elem.hasLength)
			{
				size += elem.length;
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
		/// <param name="size">Set to the number of bytes from element to token.</param>
		/// <param name="token">Set to token element if found</param>
		/// <returns>Returns true if found token, else false.</returns>
		protected bool isTokenNext(DataElement element, ref int size, ref DataElement token)
		{
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
						return true;
					}
					if (currentElement.hasLength)
						size += currentElement.length;
					else
					{
						size = 0;
						return false;
					}
				}
			}

			size = 0;
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
			int startingPosition = data.TellBits();
			bool hasOffsetRelation = false;

			// Offset relation
			if (element.relations.hasOffsetRelation)
			{
				hasOffsetRelation = true;
				OffsetRelation rel = element.relations.getOffsetRelation();
				long offset = (long)rel.GetValue();

				if (!rel.isRelativeOffset)
				{
					// Relative from start of data
					
				}
				else if (rel.relativeTo == null)
				{
					throw new NotImplementedException("Yah, we need some looove....");
				}
				else
				{
					throw new NotImplementedException("Yah, we need some looove....");
				}
			}
			
			// Do array handling
			if (element is Dom.Array)
			{
				handleArray(element as Dom.Array, data);
			}
			else if (element is Choice)
			{
				handleChoice(element as Choice, data);
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
			else if (element is Flags)
			{
				handleFlags(element as Flags, data);
			}
			else if (element is Blob)
			{
				handleBlob(element as Blob, data);
			}
			else
			{
				throw new ApplicationException("Error, found unknown element in DOM tree! " + element.GetType().ToString());
			}

			if (hasOffsetRelation)
				data.SeekBits(startingPosition, System.IO.SeekOrigin.Begin);
		}

		protected void handleArray(Dom.Array element, BitStream data)
		{
			if (element.minOccurs == 0)
			{
			}

			throw new NotImplementedException("Implement handArray");
		}

		/// <summary>
		/// Handle crack a Block element.
		/// </summary>
		/// <param name="element">Block to crack</param>
		/// <param name="data">Data stream to use when cracking</param>
		protected void handleDataElementContainer(DataElementContainer element, BitStream data)
		{
			BitStream sizedData = data;
			SizeRelation sizeRelation = null;
			int startPosition = data.TellBytes();

			// Do we have relations or a length?
			if (element.relations.hasSizeRelation)
			{
				sizeRelation = element.relations.getSizeRelation();

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
				int size = element.length;
				_sizedBlockStack.Add(element);
				_sizedBlockMap[element] = size;

				sizedData = new BitStream(data.ReadBytes(size));
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
						int size = (int)sizeRelation.GetValue();
						_sizedBlockStack.Add(element);
						_sizedBlockMap[element] = size;

						// update size based on what we have currently read
						size -= data.TellBytes() - startPosition;

						sizedData = new BitStream(data.ReadBytes(size));
						sizeRelation = null;
					}
					else if(child == sizeRelation.From)
					{
						int size = (int)sizeRelation.GetValue();
						_sizedBlockStack.Add(element);
						_sizedBlockMap[element] = size;

						// update size based on what we have currently read
						size -= data.TellBytes() - startPosition;

						sizedData = new BitStream(data.ReadBytes(size));
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
			BitStream sizedData = data;
			SizeRelation sizeRelation = null;

			// Do we have relations or a length?
			if (element.relations.hasSizeRelation)
			{
				sizeRelation = element.relations.getSizeRelation();

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
				int size = element.length;
				_sizedBlockStack.Add(element);
				_sizedBlockMap[element] = size;

				sizedData = new BitStream(data.ReadBytes(size));
			}

			int startPosition = sizedData.TellBits();

			foreach (DataElement child in element.choiceElements.Values)
			{
				try
				{
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
			if (element.nullTerminated)
			{
				// Locate NULL character in stream
				bool foundNull = false;
				bool twoNulls = element.stringType == StringType.Utf16 || element.stringType == StringType.Utf16be;
				int currentPos = data.TellBits();

				for (int i = data.TellBytes(); i < data.LengthBytes; i++)
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

				int endPos = data.TellBits();

				// Do not include NULLs in our read.
				int byteCount = ((endPos - currentPos) / 8) - 1;
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

			int? stringLength = determineElementSize(element, data);

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
					throw new CrackingFailure("String marked as token, values did not match '" + defaultValue + "' vs. '" + element.DefaultValue + "'.", element, data);

			element.DefaultValue = defaultValue;
		}

		protected int? determineElementSize(DataElement element, BitStream data)
		{
			int? size = null;

			// Check for relation and/or size
			if (element.relations.hasSizeRelation)
			{
				SizeRelation rel = element.relations.getSizeRelation();
				size = (int)rel.GetValue();
			}
			else if (element.hasLength)
			{
				size = element.length;
			}
			else
			{
				int nextSize = 0;
				DataElement token = null;

				if (isLastUnsizedElement(element, ref nextSize))
					size = data.LengthBytes - (data.TellBytes() + nextSize);
				else if (isTokenNext(element, ref nextSize, ref token))
				{
					throw new NotImplementedException("Need to implement this!");
				}
			}

			return size;
		}

		protected void handleNumber(Number element, BitStream data)
		{
			if (data.LengthBits < data.TellBits() + element.Size)
				throw new CrackingFailure("Failed cracking Number '" + element.fullName + "'.", element, data);

			if (element.LittleEndian)
				data.LittleEndian();
			else
				data.BigEndian();

			ulong value = 0;

			switch (element.Size)
			{
				case 8:
					value = (ulong)data.ReadInt8();
					break;
				case 16:
					value = (ulong)data.ReadInt16();
					break;
				case 32:
					value = (ulong)data.ReadInt32();
					break;
				case 64:
					value = (ulong)data.ReadInt64();
					break;
				default:
					throw new CrackingFailure("Number '" + element.name + "' had unsupported size '" + element.Size + "'.", element, data);
			}

			var defaultValue = new Variant(value);

			if(element.isToken)
				if(defaultValue != element.DefaultValue)
					throw new CrackingFailure("Number marked as token, values did not match '"+ ((string)defaultValue) +"' vs. '"+((string)element.DefaultValue)+"'.", element, data);

			element.DefaultValue = defaultValue;
		}

		protected void handleFlags(Flags element, BitStream data)
		{
			if (data.LengthBits <= (data.TellBits() + element.Size))
				throw new CrackingFailure("Not enough data to crack '"+element.fullName+"'.", element, data);
			
			foreach (DataElement child in element)
				handleFlag(child as Flag, data);
		}

		protected void handleFlag(Flag element, BitStream data)
		{
			//if (element.isToken)
			//    if (defaultValue != element.DefaultValue)
			//        throw new CrackingFailure("Blob marked as token, values did not match '" + defaultValue + "' vs. '" + element.DefaultValue + "'.", element, data);

			//element.DefaultValue = defaultValue;
		}

		protected void handleBlob(Blob element, BitStream data)
		{
			int? blobLength = determineElementSize(element, data);

			if (blobLength == null)
				throw new CrackingFailure("Unable to crack Blob '" + element + "'.", element, data);

			if ((data.TellBytes() + blobLength) > data.LengthBytes)
				throw new CrackingFailure("Blob '" + element.fullName +
					"' has length of '" + blobLength + "' but buffer only has '" +
					(data.LengthBytes - data.TellBytes()) + "' bytes left.", element, data);

			var defaultValue = new Variant(data.ReadBytes((int)blobLength));

			if (element.isToken)
				if (defaultValue != element.DefaultValue)
					throw new CrackingFailure("Blob marked as token, values did not match '" + defaultValue + "' vs. '" + element.DefaultValue + "'.", element, data);

			element.DefaultValue = defaultValue;
		}
	}

	public class CrackingFailure : ApplicationException
	{
		public DataElement element;
		public BitStream data;

		public CrackingFailure(DataElement element, BitStream data)
			: base("Unknown error")
		{
			this.element = element;
			this.data = data;
		}

		public CrackingFailure(string msg, DataElement element, BitStream data) : base(msg)
		{
			this.element = element;
			this.data = data;
		}
	}

	public class NotEnoughData : ApplicationException
	{
	}
}

// end
