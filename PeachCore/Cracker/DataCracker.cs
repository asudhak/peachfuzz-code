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
		Dictionary<DataElement, ulong> _sizedBlockMap = new Dictionary<DataElement, ulong>();

		/// <summary>
		/// The full data stream.
		/// </summary>
		BitStream _data = null;

		public DataModel CrackData(DataModel model, BitStream data)
		{
			_sizedBlockStack = new List<DataElement>();
			_sizedBlockMap = new Dictionary<DataElement, ulong>();
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
		protected bool isLastUnsizedElement(DataElement element, ref ulong size)
		{
			DataElement currentElement = element;
			size = 0;

			while (true)
			{
				currentElement = currentElement.nextSibling();
				if (currentElement == null && currentElement.parent == null)
					break;
				else if (currentElement == null)
					currentElement = currentElement.parent;
				else
				{
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
		protected bool isTokenNext(DataElement element, ref ulong size, ref DataElement token)
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
			ulong startingPosition = data.TellBits();
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
			else if (element is DataElementContainer) // Should also catch DataModel's
			{
				handleDataElementContainer(element as DataElementContainer, data);
			}
			else if (element is Choice)
			{
				handleChoice(element as Choice, data);
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
				data.SeekBits((long)startingPosition, System.IO.SeekOrigin.Begin);
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
			ulong startPosition = data.TellBytes();

			// Do we have relations or a length?
			if (element.relations.hasSizeRelation)
			{
				sizeRelation = element.relations.getSizeRelation();

				if (!element.isParentOf(sizeRelation.From))
				{
					ulong size = (ulong)sizeRelation.GetValue();
					_sizedBlockStack.Add(element);
					_sizedBlockMap[element] = size;

					sizedData = new BitStream(data.ReadBytes(size));
					sizeRelation = null;
				}
			}
			else if (element.hasLength)
			{
				ulong size = (ulong)element.length;
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
						ulong size = (ulong)sizeRelation.GetValue();
						_sizedBlockStack.Add(element);
						_sizedBlockMap[element] = size;

						// update size based on what we have currently read
						size -= data.TellBytes() - startPosition;

						sizedData = new BitStream(data.ReadBytes(size));
						sizeRelation = null;
					}
					else if(child == sizeRelation.From)
					{
						ulong size = (ulong)sizeRelation.GetValue();
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
			element.SelectedElement = null;

			// Do we have relations or a length?
			if (element.relations.hasSizeRelation)
			{
				sizeRelation = element.relations.getSizeRelation();

				if (!element.isParentOf(sizeRelation.From))
				{
					ulong size = (ulong)sizeRelation.GetValue();
					_sizedBlockStack.Add(element);
					_sizedBlockMap[element] = size;

					sizedData = new BitStream(data.ReadBytes(size));
					sizeRelation = null;
				}
			}
			else if (element.hasLength)
			{
				ulong size = (ulong)element.length;
				_sizedBlockStack.Add(element);
				_sizedBlockMap[element] = size;

				sizedData = new BitStream(data.ReadBytes(size));
			}

			ulong startPosition = sizedData.TellBits();

			foreach (DataElement child in element)
			{
				try
				{
					sizedData.SeekBits((long)startPosition, System.IO.SeekOrigin.Begin);
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
				throw new CrackingFailure("Unable to crack '"+element.fullName+"'.");
		}

		protected void handleString(Dom.String element, BitStream data)
		{
			if (element.nullTerminated)
			{
				// Locate NULL character in stream
				bool foundNull = false;
				bool twoNulls = element.stringType == StringType.Utf16 || element.stringType == StringType.Utf16be;
				ulong currentPos = data.TellBits();

				for (ulong i = data.TellBytes(); i < data.LengthBytes; i++)
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
					throw new CrackingFailure("Did not locate NULL in data stream for String '" + element.fullName + "'.");

				ulong endPos = data.TellBits();

				// Do not include NULLs in our read.
				ulong byteCount = ((endPos - currentPos) / 8) - 1;
				if (twoNulls)
					byteCount--;

				data.SeekBits((long)currentPos, System.IO.SeekOrigin.Begin);
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

			ulong? stringLength = null;

			// Check for relation and/or size
			if (element.relations.hasSizeRelation)
			{
				SizeRelation rel = element.relations.getSizeRelation();
				stringLength = (ulong)rel.GetValue();
			}
			else if (element.hasLength)
			{
				stringLength = element.length;
			}
			else
			{
				ulong size = 0;
				DataElement token = null;

				if (isLastUnsizedElement(element, ref size))
					stringLength = data.LengthBytes - (data.TellBytes()+size);
				else if (isTokenNext(element, ref size, ref token))
				{
					throw new NotImplementedException("Need to implement this!");
				}
			}

			if (stringLength != null)
			{
				if ((data.TellBytes() + stringLength) >= data.LengthBytes)
					throw new CrackingFailure("String '" + element.fullName +
						"' has length of '" + stringLength + "' but buffer only has '" +
						(data.LengthBytes - data.TellBytes()) + "' bytes left.");

				element.DefaultValue = new Variant(
					ASCIIEncoding.GetEncoding(element.stringType.ToString()).GetString(
					data.ReadBytes((uint)stringLength)));

				return;
			}

			throw new CrackingFailure("Unable to crack '" + element.fullName + "'.");
		}

		protected void handleNumber(Number element, BitStream data)
		{
			if (data.LengthBits < data.TellBits() + element.Size)
				throw new CrackingFailure("Failed cracking Number '" + element.fullName + "'.");

			if (element.LittleEndian)
				data.LittleEndian();
			else
				data.BigEndian();

			ulong value = data.ReadBits(element.Size);

			element.DefaultValue = new Variant(value);
		}

		protected void handleFlags(Flags element, BitStream data)
		{
			if (data.LengthBits <= (data.TellBits() + element.Size))
				throw new CrackingFailure("Not enough data to crack '"+element.fullName+"'.");
			
			foreach (DataElement child in element)
				handleFlag(child as Flag, data);
		}

		protected void handleFlag(Flag element, BitStream data)
		{
		}

		protected void handleBlob(Blob element, BitStream data)
		{
		}
	}

	public class CrackingFailure : ApplicationException
	{
		public CrackingFailure() : base("Unknown error")
		{
		}

		public CrackingFailure(string msg) : base(msg)
		{
		}
	}

	public class NotEnoughData : ApplicationException
	{
	}
}
