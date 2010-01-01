
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
using System.Collections;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Reflection;

namespace PeachCore.Dom
{
	public enum LengthType
	{
		String,
		Python,
		Calc
	}

	/// <summary>
	/// Base class for all data element relations
	/// </summary>
	public abstract class Relation
	{
		protected string _ofName = null;
		protected string _fromName = null;
		protected DataElement _of = null;
		protected DataElement _from = null;
		protected string _expressionGet = null;
		protected string _expressionSet = null;

		public DataElement Of
		{
			get { return _of; }
			set
			{
				if (_of != null)
				{
					// Remove existing event
					_of.Invalidated -= new InvalidatedEventHandler(OfInvalidated);
				}

				_of = value;
				_of.Invalidated += new InvalidatedEventHandler(OfInvalidated);
			}
		}

		public DataElement From
		{
			get { return _from; }
			set { _from = value; }
		}

		/// <summary>
		/// Handle invalidated event from "of" side of
		/// relation.  Need to invalidate "from".
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OfInvalidated(object sender, EventArgs e)
		{
			// Invalidate 'from' side
			_from.Invalidate();
		}

		/// <summary>
		/// Get value from relation as int.
		/// </summary>
		public abstract Variant GetValue();

		/// <summary>
		/// Set value on from side
		/// </summary>
		/// <param name="value"></param>
		public abstract void SetValue(Variant value);
	}

	public class SizeRelation : Relation
	{
		public override Variant GetValue()
		{
			ulong size = _of.Value.LengthBytes;

			// TODO: Call expressionGet

			return new Variant(size);
		}

		public override void SetValue(Variant value)
		{
			throw new NotImplementedException();
		}
	}

	public class CountRelation : Relation
	{
		public override Variant GetValue()
		{
			throw new NotImplementedException();
		}

		public override void SetValue(Variant value)
		{
			throw new NotImplementedException();
		}
	}

	public class OffsetRelation : Relation
	{
		public override Variant GetValue()
		{
			throw new NotImplementedException();
		}

		public override void SetValue(Variant value)
		{
			throw new NotImplementedException();
		}
	}

	public delegate void InvalidatedEventHandler(object sender, EventArgs e);
	public delegate void DefaultValueChangedEventHandler(object sender, EventArgs e);
	public delegate void MutatedValueChangedEventHandler(object sender, EventArgs e);

	/// <summary>
	/// Base class for all data elements.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class DataElement
	{
		/// <summary>
		/// Mutated vale override's fixup
		///
		///  - Default Value
		///  - Relation
		///  - Fixup
		///  - Type contraints
		///  - Transformer
		/// </summary>
		public const uint MUTATE_OVERRIDE_FIXUP = 0x1;
		/// <summary>
		/// Mutated value overrides transformers
		/// </summary>
		public const uint MUTATE_OVERRIDE_TRANSFORMER = 0x2;
		/// <summary>
		/// Mutated value overrides type constraints (e.g. string length,
		/// null terminated, etc.)
		/// </summary>
		public const uint MUTATE_OVERRIDE_TYPE_CONSTRAINTS = 0x4;
		/// <summary>
		/// Mutated value overrides relations.
		/// </summary>
		public const uint MUTATE_OVERRIDE_RELATIONS = 0x8;
		/// <summary>
		/// Default mutate value
		/// </summary>
		public const uint MUTATE_DEFAULT = MUTATE_OVERRIDE_FIXUP;

		public string name;
		public bool isMutable = true;
		public uint mutationFlags = MUTATE_DEFAULT;

		protected Variant _defaultValue;
		protected Variant _mutatedValue;

		protected List<Relation> _relations = new List<Relation>();
		protected Fixup _fixup = null;
		protected Transformer _transformer = null;

		protected DataElementContainer _parent;

		protected Variant _internalValue;
		protected BitStream _value;

		protected bool _invalidated = false;

		#region Events

		public event InvalidatedEventHandler Invalidated;
		public event DefaultValueChangedEventHandler DefaultValueChanged;
		public event MutatedValueChangedEventHandler MutatedValueChanged;

		protected virtual void OnInvalidated(EventArgs e)
		{
			// Cause values to be regenerated next time they are
			// requested.  We don't want todo this now as there could
			// be a series of invalidations that occur.
			_internalValue = null;
			_value = null;

            // Bubble this up the chain
            if(_parent != null)
                _parent.Invalidate();

			if (Invalidated != null)
				Invalidated(this, e);
		}

		protected virtual void OnDefaultValueChanged(EventArgs e)
		{
			if (DefaultValueChanged != null)
				DefaultValueChanged(this, e);
		}

		protected virtual void OnMutatedValueChanged(EventArgs e)
		{
			OnInvalidated(null);

			if (MutatedValueChanged != null)
				MutatedValueChanged(this, e);
		}

		#endregion

		public static OrderedDictionary<string, Type> dataElements = new OrderedDictionary<string, Type>();
		public static void loadDataElements(Assembly assembly)
		{
			foreach (Type type in assembly.GetTypes())
			{
				if (type.IsClass && !type.IsAbstract)
				{
					object [] attr = type.GetCustomAttributes(typeof(DataElementAttribute), false);
					DataElementAttribute dea = attr[0] as DataElementAttribute;
					if (!dataElements.ContainsKey(dea.elementName))
					{
						dataElements.Add(dea.elementName, type);
					}
				}
			}
		}

		static DataElement()
		{
		}

		protected static uint _uniqueName = 0;
		public DataElement()
		{
			name = "Unknown" + _uniqueName;
			_uniqueName++;
		}

		public DataElement(string name)
		{
			this.name = name;
		}

		public string fullName
		{
			get
			{
				string fullname = name;
				DataElement obj = _parent;
				while (obj != null)
				{
					fullname = obj.name + ".";
					obj = obj.parent;
				}
			

				return fullname;
			}
		}

		public DataElementContainer parent
		{
			get
			{
				return _parent;
			}
			set
			{
				_parent = value;
			}
		}

		public DataElement getRoot()
		{
			DataElement obj = this;

			while (obj != null && obj._parent != null)
				obj = obj.parent;

			return obj;
		}

		/// <summary>
		/// Find our next sibling.
		/// </summary>
		/// <returns>Returns sibling or null.</returns>
		public DataElement nextSibling()
		{
			if (_parent == null)
				return null;

			int nextIndex = _parent.IndexOf(this) + 1;
			if (nextIndex > _parent.Count)
				return null;

			return _parent[nextIndex];
		}

		/// <summary>
		/// Find our previous sibling.
		/// </summary>
		/// <returns>Returns sibling or null.</returns>
		public DataElement previousSibling()
		{
			if (_parent == null)
				return null;

			int priorIndex = _parent.IndexOf(this) - 1;
			if (priorIndex < 0)
				return null;

			return _parent[priorIndex];
		}

		/// <summary>
		/// Call to invalidate current element and cause rebuilding
		/// of data elements dependent on this element.
		/// </summary>
		public void Invalidate()
		{
			_invalidated = true;

			OnInvalidated(null);

			if(parent != null)
				parent.Invalidate();
		}

		public virtual bool isLeafNode
		{
			get { return true; }
		}

		/// <summary>
		/// Default value for this data element.
		/// </summary>
		public virtual Variant DefaultValue
		{
			get { return _defaultValue; }
			set
			{
				_defaultValue = value;
				OnDefaultValueChanged(null);
				Invalidate();
			}
		}

		/// <summary>
		/// Current mutated value (if any) for this data element.
		/// </summary>
		public virtual Variant MutatedValue
		{
			get { return _mutatedValue; }
			set
			{
				_mutatedValue = value;
				OnMutatedValueChanged(null);
				Invalidate();
			}
		}

        /// <summary>
        /// Get the Internal Value of this data element
        /// </summary>
		public virtual Variant InternalValue
		{
			get
			{
				if (_internalValue == null || _invalidated)
					GenerateInternalValue();

				return _internalValue;
			}
		}

        /// <summary>
        /// Get the final Value of this data element
        /// </summary>
		public virtual BitStream Value
		{
			get
			{
				if (_value == null || _invalidated)
				{
					GenerateValue();
					_invalidated = false;
				}

				return _value;
			}
		}

		/// <summary>
		/// Generate the internal value of this data element
		/// </summary>
		/// <returns>Internal value in .NET form</returns>
		public virtual Variant GenerateInternalValue()
		{
			Variant value;

			// 1. Default value

			value = DefaultValue;

			// 2. Relations

			if (MutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_RELATIONS) != 0)
				return MutatedValue;

			foreach(Relation r in _relations)
			{
				if (r.Of == this)
				{
					value = r.GetValue();
				}
			}

			// 3. Fixup

			if (MutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_FIXUP) != 0)
				return MutatedValue;

			if (_fixup != null)
				value = _fixup.fixup(this);

			// 4. Set _internalValue

			_internalValue = value;
			return value;
		}

		protected virtual BitStream InternalValueToBitStream(Variant b)
		{
			return (BitStream)b;
		}

		/// <summary>
		/// Generate the final value of this data element
		/// </summary>
		/// <returns></returns>
		public BitStream GenerateValue()
		{
			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_TRANSFORMER) != 0)
				return new BitStream((byte[]) MutatedValue);

			BitStream value = InternalValueToBitStream(InternalValue);

			if(_transformer != null)
				value = _transformer.encode(value);

			_value = value;
			return value;
		}

		/// <summary>
		/// Find data element with specific name.
		/// </summary>
		/// <param name="name">Name to search for</param>
		/// <returns>Returns found data element or null.</returns>
		public DataElement find(string name)
		{
			string [] names = name.Split(new char[] {'.'});

			foreach (string n in names)
			{

			}

			throw new ApplicationException("TODO");
		}

		/// <summary>
		/// Fixup for this data element.  Can be null.
		/// </summary>
		public Fixup fixup
		{
			get { return _fixup; }
			set { _fixup = value; }
		}

		/// <summary>
		/// Transformer for this data element.  Can be null.
		/// </summary>
		public Transformer transformer
		{
			get { return _transformer; }
			set { _transformer = value; }
		}

		/// <summary>
		/// Relations for this data element.
		/// </summary>
		public List<Relation> relations
		{
			get { return _relations; }
		}
	}

	/// <summary>
	/// Abstract base class for DataElements that contain other
	/// data elements.  Such as Block, Choice, or Flags.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class DataElementContainer : DataElement, IEnumerable<DataElement>, IList<DataElement>
	{
		protected List<DataElement> _childrenList = new List<DataElement>();
		protected Dictionary<string, DataElement> _childrenDict = new Dictionary<string,DataElement>();

		public override bool isLeafNode
		{
			get
			{
				return _childrenList.Count == 0;
			}
		}

		public DataElement this[int index]
		{
			get { return _childrenList[index]; }
			set
			{
				if (value == null)
					throw new ApplicationException("Cannot set null value");

				_childrenDict.Remove(_childrenList[index].name);
				_childrenDict.Add(value.name, value);

				_childrenList[index].parent = null;

				_childrenList.RemoveAt(index);
				_childrenList.Insert(index, value);

				value.parent = this;

				Invalidate();
			}
		}

		public DataElement this[string key]
		{
			get { return _childrenDict[key]; }
			set
			{
				if (value == null)
					throw new ApplicationException("Cannot set null value");

				int index = _childrenList.IndexOf(_childrenDict[key]);
				_childrenList.RemoveAt(index);
				_childrenDict[key].parent = null;
				_childrenDict[key] = value;
				_childrenList.Insert(index, value);

				value.parent = this;

				Invalidate();
			}
		}

		#region IEnumerable<Element> Members

		public IEnumerator<DataElement> GetEnumerator()
		{
			return _childrenList.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _childrenList.GetEnumerator();
		}

		#endregion

		#region IList<DataElement> Members

		public int IndexOf(DataElement item)
		{
			return _childrenList.IndexOf(item);
		}

		public void Insert(int index, DataElement item)
		{
			if (_childrenDict.Keys.Contains(item.name))
				throw new ApplicationException(
					string.Format("Child DataElement named {0} already exists.", item.name));

			_childrenList.Insert(index, item);
			_childrenDict[item.name] = item;

			item.parent = this;

			Invalidate();
		}

		public void RemoveAt(int index)
		{
			_childrenDict.Remove(_childrenList[index].name);
			_childrenList[index].parent = null;
			_childrenList.RemoveAt(index);

			Invalidate();
		}

		#endregion

		#region ICollection<DataElement> Members

		public void Add(DataElement item)
		{
			if (_childrenDict.Keys.Contains(item.name))
				throw new ApplicationException(
					string.Format("Child DataElement named {0} already exists.", item.name));

			_childrenList.Add(item);
			_childrenDict[item.name] = item;
			item.parent = this;

			Invalidate();
		}

		public void Clear()
		{
			foreach (DataElement e in _childrenList)
				e.parent = null;

			_childrenList.Clear();
			_childrenDict.Clear();

			Invalidate();
		}

		public bool Contains(DataElement item)
		{
			return _childrenList.Contains(item);
		}

		public void CopyTo(DataElement[] array, int arrayIndex)
		{
			_childrenList.CopyTo(array, arrayIndex);
			foreach (DataElement e in array)
			{
				_childrenDict[e.name] = e;
				e.parent = this;
			}

			Invalidate();
		}

		public int Count
		{
			get { return _childrenList.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(DataElement item)
		{
			_childrenDict.Remove(item.name);
			bool ret = _childrenList.Remove(item);
			item.parent = null;

			Invalidate();

			return ret;
		}

		#endregion
	}

	/// <summary>
	/// Block element
	/// </summary>
	[DataElement("Block")]
	[DataElementChildSupportedAttribute(DataElementTypes.Any)]
	//[ParameterAttribute("length", typeof(uint), "Length of string in characters", false)]
	public class Block : DataElementContainer
	{
		public string lengthType;
		public string lengthCalc;
		public Variant length;

		public override Variant GenerateInternalValue()
		{
			Variant value;

			// 1. Default value

			if (_mutatedValue == null)
			{
				BitStream stream = new BitStream();
				foreach (DataElement child in this)
					stream.Write(child.Value, child);

				value = new Variant(stream);
			}
			else
			{
				value = MutatedValue;
			}

			// 2. Relations

			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_RELATIONS) != 0)
				return MutatedValue;

			foreach (Relation r in _relations)
			{
				if (r.Of == this)
				{
					value = r.GetValue();
				}
			}

			// 3. Fixup

			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_FIXUP) != 0)
				return MutatedValue;

			if (_fixup != null)
				value = _fixup.fixup(this);

			_internalValue = value;
			return value;
		}
	}

	/// <summary>
	/// DataModel is just a top level Block.
	/// </summary>
	public class DataModel : Block
	{
		public object parent;
	}

	/// <summary>
	/// Choice allows the selection of a single
	/// data element based on the current data set.
	/// 
	/// The other options in the choice are available
	/// for mutation by the mutators.
	/// </summary>
	[DataElement("Choice")]
	[DataElementChildSupportedAttribute(DataElementTypes.Any)]
	//[ParameterAttribute("length", typeof(uint), "Length of string in characters", false)]
	public class Choice : DataElementContainer
	{
	}

	/// <summary>
	/// Array of data elements.  Can be
	/// zero or more elements.
	/// </summary>
	public class Array : DataElementContainer
	{
		public uint minOccurs = 1;
		public uint maxOccurs = 1;

		/// <summary>
		/// Origional data element to use as 
		/// template for other data elements in 
		/// array.
		/// </summary>
		public DataElement origionalElemement = null;
	}

	/// <summary>
	/// A numerical data element.
	/// </summary>
	[DataElement("Number")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	[ParameterAttribute("size", typeof(uint), "Size in bits [8, 16, 24, 32, 64]", true)]
	[ParameterAttribute("signed", typeof(bool), "Is number signed (default false)", false)]
	[ParameterAttribute("endian", typeof(string), "Byte order of number (default 'little')", false)]
	public class Number : DataElement
	{
		protected uint _size = 8;
		protected ulong _max = (ulong)sbyte.MaxValue;
		protected long _min = sbyte.MinValue;
		protected bool _signed = true;
		protected bool _isLittleEndian = true;

		public override Variant DefaultValue
		{
			get { return base.DefaultValue; }
			set
			{
				if ((long)value >= _min && (ulong)value <= _max)
					base.DefaultValue = value;
				else
					throw new ApplicationException("DefaultValue not with in min/max values.");
			}
		}

		public uint Size
		{
			get { return _size; }
			set
			{
				if (value == 0)
					throw new ApplicationException("Size must be > 0");

				_size = value;

				if (_signed)
				{
					_max = (ulong)Math.Pow(2, _size) / 2;
					_min = 0 - ((long)Math.Pow(2, _size) / 2);
				}
				else
				{
					_max = (ulong)Math.Pow(2, _size) - 1;
					_min = 0;
				}
				
				Invalidate();
			}
		}

		public bool Signed
		{
			get { return _signed; }
			set
			{
				_signed = value;
				Size = Size;

				Invalidate();
			}
		}

		public bool LittleEndian
		{
			get { return _isLittleEndian; }
			set
			{
				_isLittleEndian = value;
				Invalidate();
			}
		}

		public ulong MaxValue
		{
			get { return _max; }
		}

		public long MinValue
		{
			get { return _min; }
		}

		protected override BitStream InternalValueToBitStream(Variant b)
		{
			BitStream bits = new BitStream();

			if (_isLittleEndian)
				bits.LittleEndian();
			else
				bits.BigEndian();

			bits.WriteBits((ulong)InternalValue, Size);

			return bits;
		}
	}

	public enum StringType
	{
		Ascii,
		Utf7,
		Utf8,
		Utf16,
		Utf16be,
		Utf32
	}

	/// <summary>
	/// String data element
	/// </summary>
	[DataElement("String")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	//[ParameterAttribute("size", typeof(uint), "Size in bits [8, 16, 24, 32, 64]", true)]
	public class String : DataElement
	{
		protected StringType _type = StringType.Ascii;
		protected bool _nullTerminated;
		protected uint _length;
		protected string _lengthOther;
		protected LengthType _lengthType;

		protected override BitStream InternalValueToBitStream(Variant v)
		{
			byte[] value = null;

			if (_type == StringType.Ascii)
				value = Encoding.ASCII.GetBytes((string)v);

			else if (_type == StringType.Utf7)
				value = Encoding.UTF7.GetBytes((string)v);

			else if (_type == StringType.Utf8)
				value = Encoding.UTF8.GetBytes((string)v);

			else if (_type == StringType.Utf16)
				value = Encoding.Unicode.GetBytes((string)v);

			else if (_type == StringType.Utf16be)
				value = Encoding.BigEndianUnicode.GetBytes((string)v);

			else if (_type == StringType.Utf32)
				value = Encoding.UTF32.GetBytes((string)v);

			else
				throw new ApplicationException("String._type not set properly!");
			
			return new BitStream(value);
		}
	}

	/// <summary>
	/// Binary large object data element
	/// </summary>
	[DataElement("Blob")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	//[ParameterAttribute("size", typeof(uint), "Size in bits [8, 16, 24, 32, 64]", true)]
	public class Blob : DataElement
	{
		protected uint _length;

	}

	[DataElement("Flags")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	[DataElementChildSupportedAttribute("Flag")]
	//[ParameterAttribute("size", typeof(uint), "Size in bits [8, 16, 24, 32, 64]", true)]
	public class Flags : DataElement
	{
	}

	[DataElement("Flag")]
	[DataElementChildSupportedAttribute(DataElementTypes.NonDataElements)]
	//[ParameterAttribute("size", typeof(uint), "Size in bits [8, 16, 24, 32, 64]", true)]
	public class Flag : DataElement
	{
	}

	/// <summary>
	/// Variant class emulates untyped scripting languages
	/// variables were typing can change as needed.  This class
	/// solves the problem of boxing internal types.  Instead
	/// explicit casts are used to access the value as needed.
	/// 
	/// TODO: Investigate implicit casting as well.
	/// TODO: Investigate deligates for type -> byte[] conversion.
	/// </summary>
	public class Variant
	{
		protected enum VariantType
		{
			Unknown,
			Int,
			Long,
			ULong,
			String,
			ByteString,
			BitStream
		}

		VariantType _type = VariantType.Unknown;
		int _valueInt;
		long _valueLong;
		ulong _valueULong;
		string _valueString;
		byte[] _valueByteArray;
		BitStream _valueBitStream = null;

		public Variant(int v)
		{
			SetValue(v);
		}

		public Variant(long v)
		{
			SetValue(v);
		}

		public Variant(ulong v)
		{
			SetValue(v);
		}

		public Variant(string v)
		{
			SetValue(v);
		}
		
		public Variant(byte[] v)
		{
			SetValue(v);
		}

		public Variant(BitStream v)
		{
			SetValue(v);
		}

		public void SetValue(int v)
		{
			_type = VariantType.Int;
			_valueInt = v;
			_valueString = null;
			_valueBitStream = null;
			_valueByteArray = null;
		}

		public void SetValue(long v)
		{
			_type = VariantType.Long;
			_valueLong = v;
			_valueString = null;
			_valueBitStream = null;
			_valueByteArray = null;
		}

		public void SetValue(ulong v)
		{
			_type = VariantType.ULong;
			_valueULong = v;
			_valueString = null;
			_valueBitStream = null;
			_valueByteArray = null;
		}

		public void SetValue(string v)
		{
			_type = VariantType.String;
			_valueString = v;
			_valueBitStream = null;
			_valueByteArray = null;
		}

		public void SetValue(byte[] v)
		{
			_type = VariantType.ByteString;
			_valueByteArray = v;
			_valueString = null;
			_valueBitStream = null;
		}

		public void SetValue(BitStream v)
		{
			_type = VariantType.BitStream;
			_valueBitStream = v;
			_valueString = null;
			_valueByteArray = null;
		}

		/// <summary>
		/// Access variant as an int value.
		/// </summary>
		/// <param name="v">Variant to cast</param>
		/// <returns>int representation of value</returns>
		public static explicit operator int(Variant v)
		{
			if (v == null)
				throw new ApplicationException("Parameter v is null");

			switch (v._type)
			{
				case VariantType.Int:
					return v._valueInt;
				case VariantType.Long:
					if (v._valueLong > int.MaxValue || v._valueLong < int.MinValue)
						throw new ApplicationException("Converting this long to an int would cause loss of data");

					return (int)v._valueLong;
				case VariantType.ULong:
					if (v._valueULong > int.MaxValue)
						throw new ApplicationException("Converting this ulong to an int would cause loss of data");

					return (int)v._valueULong;
				case VariantType.String:
					return Convert.ToInt32(v._valueString);
				case VariantType.ByteString:
					throw new NotSupportedException("Unable to convert byte[] to int type.");
				case VariantType.BitStream:
					throw new NotSupportedException("Unable to convert BitStream to int type.");
				default:
					throw new NotSupportedException("Unable to convert to unknown type.");
			}
		}

		public static explicit operator long(Variant v)
		{
			if (v == null)
				throw new ApplicationException("Parameter v is null");

			switch (v._type)
			{
				case VariantType.Int:
					return (long)v._valueInt;
				case VariantType.Long:
					return v._valueLong;
				case VariantType.ULong:
					if (v._valueULong > long.MaxValue)
						throw new ApplicationException("Converting this ulong to a long would cause loss of data");

					return (long)v._valueULong;
				case VariantType.String:
					return Convert.ToInt64(v._valueString);
				case VariantType.ByteString:
					throw new NotSupportedException("Unable to convert byte[] to int type.");
				case VariantType.BitStream:
					throw new NotSupportedException("Unable to convert BitStream to int type.");
				default:
					throw new NotSupportedException("Unable to convert to unknown type.");
			}
		}

		public static explicit operator ulong(Variant v)
		{
			if (v == null)
				throw new ApplicationException("Parameter v is null");

			switch (v._type)
			{
				case VariantType.Int:
					return (ulong)v._valueInt;
				case VariantType.Long:
					if ((ulong)v._valueLong > ulong.MaxValue || v._valueLong < 0)
						throw new ApplicationException("Converting this long to a ulong would cause loss of data");

					return (ulong)v._valueLong;
				case VariantType.ULong:
					return v._valueULong;
				case VariantType.String:
					return Convert.ToUInt64(v._valueString);
				case VariantType.ByteString:
					throw new NotSupportedException("Unable to convert byte[] to int type.");
				case VariantType.BitStream:
					throw new NotSupportedException("Unable to convert BitStream to int type.");
				default:
					throw new NotSupportedException("Unable to convert to unknown type.");
			}
		}

		/// <summary>
		/// Access variant as string value.
		/// </summary>
		/// <param name="v">Variant to cast</param>
		/// <returns>string representation of value</returns>
		public static explicit operator string(Variant v)
		{
			if (v == null)
				throw new ApplicationException("Parameter v is null");

			switch (v._type)
			{
				case VariantType.Int:
					return Convert.ToString(v._valueInt);
				case VariantType.Long:
					return Convert.ToString(v._valueLong);
				case VariantType.ULong:
					return Convert.ToString(v._valueULong);
				case VariantType.String:
					return v._valueString;
				case VariantType.ByteString:
					throw new NotSupportedException("Unable to convert byte[] to string type.");
				case VariantType.BitStream:
					throw new NotSupportedException("Unable to convert BitStream to int type.");
				default:
					throw new NotSupportedException("Unable to convert to unknown type.");
			}
		}

		/// <summary>
		/// Access variant as byte[] value.  This type is currently limited
		/// as neather int or string's are properly cast to byte[] since 
		/// additional information is needed.
		/// 
		/// TODO: Investigate using deligates to handle conversion.
		/// </summary>
		/// <param name="v">Variant to cast</param>
		/// <returns>byte[] representation of value</returns>
		public static explicit operator byte[](Variant v)
		{
			if (v == null)
				throw new ApplicationException("Parameter v is null");

			switch (v._type)
			{
				case VariantType.Int:
					throw new NotSupportedException("Unable to convert int to byte[] type.");
				case VariantType.Long:
					throw new NotSupportedException("Unable to convert long to byte[] type.");
				case VariantType.ULong:
					throw new NotSupportedException("Unable to convert ulong to byte[] type.");
				case VariantType.String:
					throw new NotSupportedException("Unable to convert string to byte[] type.");
				case VariantType.ByteString:
					return v._valueByteArray;
				case VariantType.BitStream:
					return v._valueBitStream.Value;
				default:
					throw new NotSupportedException("Unable to convert to unknown type.");
			}
		}

		public static explicit operator BitStream(Variant v)
		{
			if (v == null)
				throw new ApplicationException("Parameter v is null");

			switch (v._type)
			{
				case VariantType.Int:
					throw new NotSupportedException("Unable to convert int to BitStream type.");
				case VariantType.Long:
					throw new NotSupportedException("Unable to convert long to BitStream type.");
				case VariantType.ULong:
					throw new NotSupportedException("Unable to convert ulong to BitStream type.");
				case VariantType.String:
					throw new NotSupportedException("Unable to convert string to BitStream type.");
				case VariantType.ByteString:
					return new BitStream(v._valueByteArray);
				case VariantType.BitStream:
					return v._valueBitStream;
				default:
					throw new NotSupportedException("Unable to convert to unknown type.");
			}
		}
	}
}

// end
