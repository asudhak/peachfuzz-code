using System;
using System.Collections.Generic;
using System.Text;
using PeachCore.Dom;

namespace PeachCore.Cracker
{
	/// <summary>
	/// Crack data into a DataModel.
	/// </summary>
	public class DataCracker
	{
		public DataModel CrackData(DataModel model, BitStream data)
		{
			return null;
		}

		protected void handleNode(DataElement element, BitStream data)
		{
			// Has when relation
			if (element.relations.hasWhenRelation)
			{
				throw new NotImplementedException("Handle WHen Relation!");
			}

			// Offset relation
			if (element.relations.hasOffsetRelation)
			{
				throw new NotImplementedException("Handle offset relation!");
			}
			
			// Do array handling
			if (element is Dom.Array)
			{
				handleArray(element as Dom.Array, data);
			}
			else if (element is Block)
			{
				handleBlock(element as Block, data);
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
			// else if(elemtn is Custom)
			//{
			//}
			else
			{
				throw new ApplicationException("Error, found unknown element in DOM tree! " + element.GetType().ToString());
			}
		}

		protected void handleArray(Dom.Array element, BitStream data)
		{
			if (element.minOccurs == 0)
			{
			}

			throw new NotImplementedException("Implement handArray");
		}

		protected void handleBlock(Block element, BitStream data)
		{
			// Do we have relations or a length?

			// Handle children
			foreach (DataElement child in element)
				handleNode(child, data);
		}

		protected void handleChoice(Choice element, BitStream data)
		{
			long pos = (long) data.TellBits();
			element.SelectedElement = null;

			foreach (DataElement child in element)
			{
				try
				{
					data.SeekBits(pos, System.IO.SeekOrigin.Begin);
					handleNode(child, data);
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
		}

		protected void handleNumber(Number element, BitStream data)
		{
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
