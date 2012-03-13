
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

namespace Peach.Core
{
	public delegate void AddEventHandler<TKey, TValue>(OrderedDictionary<TKey, TValue> sender, TKey key, TValue value);

	/*
	/// <summary>
	/// Represents a generic collection of key/value pairs that are ordered independently of the key and value.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
	public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>
	{
		private const int DefaultInitialCapacity = 0;

		private static readonly string _keyTypeName = typeof(TKey).FullName;
		private static readonly string _valueTypeName = typeof(TValue).FullName;
		private static readonly bool _valueTypeIsReferenceType = !typeof(ValueType).IsAssignableFrom(typeof(TValue));

		private Dictionary<TKey, TValue> _dictionary;
		private List<KeyValuePair<TKey, TValue>> _list;
		private IEqualityComparer<TKey> _comparer;
		private object _syncRoot;
		private int _initialCapacity;

		public event AddEventHandler<TKey, TValue> AddEvent;
		protected void OnAdd(TKey key, TValue value)
		{
			if (AddEvent != null)
				AddEvent(this, key, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> class.
		/// </summary>
		public OrderedDictionary()
			: this(DefaultInitialCapacity, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> class using the specified initial capacity.
		/// </summary>
		/// <param name="capacity">The initial number of elements that the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> can contain.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0</exception>
		public OrderedDictionary(int capacity)
			: this(capacity, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> class using the specified comparer.
		/// </summary>
		/// <param name="comparer">The <see cref="IEqualityComparer{TKey}">IEqualityComparer&lt;TKey&gt;</see> to use when comparing keys, or <null/> to use the default <see cref="EqualityComparer{TKey}">EqualityComparer&lt;TKey&gt;</see> for the type of the key.</param>
		public OrderedDictionary(IEqualityComparer<TKey> comparer)
			: this(DefaultInitialCapacity, comparer)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> class using the specified initial capacity and comparer.
		/// </summary>
		/// <param name="capacity">The initial number of elements that the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection can contain.</param>
		/// <param name="comparer">The <see cref="IEqualityComparer{TKey}">IEqualityComparer&lt;TKey&gt;</see> to use when comparing keys, or <null/> to use the default <see cref="EqualityComparer{TKey}">EqualityComparer&lt;TKey&gt;</see> for the type of the key.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0</exception>
		public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer)
		{
			if (0 > capacity)
				throw new ArgumentOutOfRangeException("capacity", "'capacity' must be non-negative");

			_initialCapacity = capacity;
			_comparer = comparer;
		}

		/// <summary>
		/// Converts the object passed as a key to the key type of the dictionary
		/// </summary>
		/// <param name="keyObject">The key object to check</param>
		/// <returns>The key object, cast as the key type of the dictionary</returns>
		/// <exception cref="ArgumentNullException"><paramref name="keyObject"/> is <null/>.</exception>
		/// <exception cref="ArgumentException">The key type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="keyObject"/>.</exception>
		private static TKey ConvertToKeyType(object keyObject)
		{
			if (null == keyObject)
			{
				throw new ArgumentNullException("key");
			}
			else
			{
				if (keyObject is TKey)
					return (TKey)keyObject;
			}
			throw new ArgumentException("'key' must be of type " + _keyTypeName, "key");
		}

		/// <summary>
		/// Converts the object passed as a value to the value type of the dictionary
		/// </summary>
		/// <param name="value">The object to convert to the value type of the dictionary</param>
		/// <returns>The value object, converted to the value type of the dictionary</returns>
		/// <exception cref="ArgumentNullException"><paramref name="valueObject"/> is <null/>, and the value type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is a value type.</exception>
		/// <exception cref="ArgumentException">The value type of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is not in the inheritance hierarchy of <paramref name="valueObject"/>.</exception>
		private static TValue ConvertToValueType(object value)
		{
			if (null == value)
			{
				if (_valueTypeIsReferenceType)
					return default(TValue);
				else
					throw new ArgumentNullException("value");
			}
			else
			{
				if (value is TValue)
					return (TValue)value;
			}
			throw new ArgumentException("'value' must be of type " + _valueTypeName, "value");
		}

		/// <summary>
		/// Gets the dictionary object that stores the keys and values
		/// </summary>
		/// <value>The dictionary object that stores the keys and values for the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></value>
		/// <remarks>Accessing this property will create the dictionary object if necessary</remarks>
		private Dictionary<TKey, TValue> Dictionary
		{
			get
			{
				if (null == _dictionary)
				{
					_dictionary = new Dictionary<TKey, TValue>(_initialCapacity, _comparer);
				}
				return _dictionary;
			}
		}

		/// <summary>
		/// Gets the list object that stores the key/value pairs.
		/// </summary>
		/// <value>The list object that stores the key/value pairs for the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></value>
		/// <remarks>Accessing this property will create the list object if necessary.</remarks>
		private List<KeyValuePair<TKey, TValue>> List
		{
			get
			{
				if (null == List)
				{
					_list = new List<KeyValuePair<TKey, TValue>>(_initialCapacity);
				}
				return _list;
			}
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		/// <summary>
		/// Inserts a new entry into the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection with the specified key and value at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the element should be inserted.</param>
		/// <param name="key">The key of the entry to add.</param>
		/// <param name="value">The value of the entry to add. The value can be <null/> if the type of the values in the dictionary is a reference type.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
		/// -or-<br/>
		/// <paramref name="index"/> is greater than <see cref="Count"/>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/>.</exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</exception>
		public void Insert(int index, TKey key, TValue value)
		{
			if (index > Count || index < 0)
				throw new ArgumentOutOfRangeException("index");

			Dictionary.Add(key, value);
			List.Insert(index, new KeyValuePair<TKey, TValue>(key, value));

			OnAdd(key, value);
		}

		/// <summary>
		/// Removes the entry at the specified index from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.
		/// </summary>
		/// <param name="index">The zero-based index of the entry to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
		/// -or-<br/>
		/// index is equal to or greater than <see cref="Count"/>.</exception>
		public void RemoveAt(int index)
		{
			if (index >= Count || index < 0)
				throw new ArgumentOutOfRangeException("index", "'index' must be non-negative and less than the size of the collection");

			TKey key = List[index].Key;

			List.RemoveAt(index);
			Dictionary.Remove(key);
		}

		/// <summary>
		/// Gets or sets the value at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the value to get or set.</param>
		/// <value>The value of the item at the specified index.</value>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
		/// -or-<br/>
		/// index is equal to or greater than <see cref="Count"/>.</exception>
		public TValue this[int index]
		{
			get
			{
				return List[index].Value;
			}

			set
			{
				if (index >= Count || index < 0)
					throw new ArgumentOutOfRangeException("index", "'index' must be non-negative and less than the size of the collection");

				TKey key = List[index].Key;

				List[index] = new KeyValuePair<TKey, TValue>(key, value);
				Dictionary[key] = value;

				OnAdd(key, value);
			}
		}

		/// <summary>
		/// Adds an entry with the specified key and value into the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection with the lowest available index.
		/// </summary>
		/// <param name="key">The key of the entry to add.</param>
		/// <param name="value">The value of the entry to add. This value can be <null/>.</param>
		/// <returns>The index of the newly added entry</returns>
		/// <remarks>A key cannot be <null/>, but a value can be.
		/// <para>You can also use the <see cref="P:OrderedDictionary{TKey,TValue}.Item(TKey)"/> property to add new elements by setting the value of a key that does not exist in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection; however, if the specified key already exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>, setting the <see cref="P:OrderedDictionary{TKey,TValue}.Item(TKey)"/> property overwrites the old value. In contrast, the <see cref="M:Add"/> method does not modify existing elements.</para></remarks>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/></exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></exception>
		public int Add(TKey key, TValue value)
		{
			Dictionary.Add(key, value);
			List.Add(new KeyValuePair<TKey, TValue>(key, value));
			OnAdd(key, value);
			return Count - 1;
		}

		/// <summary>
		/// Removes all elements from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.
		/// </summary>
		/// <remarks>The capacity is not changed as a result of calling this method.</remarks>
		public void Clear()
		{
			Dictionary.Clear();
			List.Clear();
		}

		/// <summary>
		/// Determines whether the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection contains a specific key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.</param>
		/// <returns><see langword="true"/> if the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="key"/> is <null/></exception>
		public bool ContainsKey(TKey key)
		{
			return Dictionary.ContainsKey(key);
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> has a fixed size.
		/// </summary>
		/// <value><see langword="true"/> if the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> has a fixed size; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
		bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection is read-only.
		/// </summary>
		/// <value><see langword="true"/> if the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> is read-only; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
		/// <remarks>
		/// A collection that is read-only does not allow the addition, removal, or modification of elements after the collection is created.
		/// <para>A collection that is read-only is simply a collection with a wrapper that prevents modification of the collection; therefore, if changes are made to the underlying collection, the read-only collection reflects those changes.</para>
		/// </remarks>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Returns the zero-based index of the specified key in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></param>
		/// <returns>The zero-based index of <paramref name="key"/>, if <paramref name="ley"/> is found in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>; otherwise, -1</returns>
		/// <remarks>This method performs a linear search; therefore it has a cost of O(n) at worst.</remarks>
		public int IndexOfKey(TKey key)
		{
			if (null == key)
				throw new ArgumentNullException("key");

			for (int index = 0; index < List.Count; index++)
			{
				KeyValuePair<TKey, TValue> entry = List[index];
				TKey next = entry.Key;
				if (null != _comparer)
				{
					if (_comparer.Equals(next, key))
					{
						return index;
					}
				}
				else if (next.Equals(key))
				{
					return index;
				}
			}

			return -1;
		}

		/// <summary>
		/// Removes the entry with the specified key from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.
		/// </summary>
		/// <param name="key">The key of the entry to remove</param>
		/// <returns><see langword="true"/> if the key was found and the corresponding element was removed; otherwise, <see langword="false"/></returns>
		public bool Remove(TKey key)
		{
			if (null == key)
				throw new ArgumentNullException("key");

			int index = IndexOfKey(key);
			if (index >= 0)
			{
				if (Dictionary.Remove(key))
				{
					List.RemoveAt(index);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets or sets the value with the specified key.
		/// </summary>
		/// <param name="key">The key of the value to get or set.</param>
		/// <value>The value associated with the specified key. If the specified key is not found, attempting to get it returns <null/>, and attempting to set it creates a new element using the specified key.</value>
		public TValue this[TKey key]
		{
			get
			{
				return Dictionary[key];
			}
			set
			{
				if (Dictionary.ContainsKey(key))
				{
					Dictionary[key] = value;
					List[IndexOfKey(key)] = new KeyValuePair<TKey, TValue>(key, value);
					OnAdd(key, value);
				}
				else
				{
					Add(key, value);
				}
			}
		}

		/// <summary>
		/// Gets the number of key/values pairs contained in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.
		/// </summary>
		/// <value>The number of key/value pairs contained in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> collection.</value>
		public int Count
		{
			get
			{
				return List.Count;
			}
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection{TKey}">ICollection&lt;TKey&gt;</see> object containing the keys in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.
		/// </summary>
		/// <value>An <see cref="T:System.Collections.Generic.ICollection{TKey}">ICollection&lt;TKey&gt;</see> object containing the keys in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</value>
		/// <remarks>The returned <see cref="T:System.Collections.Generic.ICollection{TKey}">ICollection&lt;TKey&gt;</see> object is not a static copy; instead, the collection refers back to the keys in the original <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>. Therefore, changes to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> continue to be reflected in the key collection.</remarks>
		public ICollection<TKey> Keys
		{
			get
			{
				return Dictionary.Keys;
			}
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key of the value to get.</param>
		/// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of <paramref name="value"/>. This parameter can be passed uninitialized.</param>
		/// <returns><see langword="true"/> if the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
		public bool TryGetValue(TKey key, out TValue value)
		{
			return Dictionary.TryGetValue(key, out value);
		}

		/// <summary>
		/// Gets an <see cref="T:ICollection{TValue}">ICollection&lt;TValue&gt;</see> object containing the values in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.
		/// </summary>
		/// <value>An <see cref="T:ICollection{TValue}">ICollection&lt;TValue&gt;</see> object containing the values in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</value>
		/// <remarks>The returned <see cref="T:ICollection{TValue}">ICollection&lt;TKey&gt;</see> object is not a static copy; instead, the collection refers back to the values in the original <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>. Therefore, changes to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> continue to be reflected in the value collection.</remarks>
		public ICollection<TValue> Values
		{
			get
			{
				return Dictionary.Values;
			}
		}

		/// <summary>
		/// Adds the specified value to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> with the specified key.
		/// </summary>
		/// <param name="item">The <see cref="T:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> structure representing the key and value to add to the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</param>
		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		/// <summary>
		/// Determines whether the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> contains a specific key and value.
		/// </summary>
		/// <param name="item">The <see cref="T:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> structure to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</param>
		/// <returns><see langword="true"/> if <paramref name="keyValuePair"/> is found in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>; otherwise, <see langword="false"/>.</returns>
		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
		{
			return ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Contains(item);
		}

		/// <summary>
		/// Copies the elements of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> to an array of type <see cref="T:KeyValuePair`2>"/>, starting at the specified index.
		/// </summary>
		/// <param name="array">The one-dimensional array of type <see cref="T:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> that is the destination of the <see cref="T:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> elements copied from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>. The array must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes a key and value from the dictionary.
		/// </summary>
		/// <param name="item">The <see cref="T:KeyValuePair{TKey,TValue}">KeyValuePair&lt;TKey,TValue&gt;</see> structure representing the key and value to remove from the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</param>
		/// <returns><see langword="true"/> if the key and value represented by <paramref name="keyValuePair"/> is successfully found and removed; otherwise, <see langword="false"/>. This method returns <see langword="false"/> if <paramref name="keyValuePair"/> is not found in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>.</returns>
		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			return Remove(item.Key);
		}


		#region IDictionary<TKey,TValue> Members

		void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			Add(key, value);
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		#endregion
	}
	*/

	/// <summary>
	/// Represents a generic collection of key/value pairs that are ordered independently of the key and value.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
	public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		/// <summary>
		/// Inserts a new entry into the <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see> collection with the specified key and value at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the element should be inserted.</param>
		/// <param name="key">The key of the entry to add.</param>
		/// <param name="value">The value of the entry to add. The value can be <null/> if the type of the values in the dictionary is a reference type.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
		/// -or-<br/>
		/// <paramref name="index"/> is greater than <see cref="System.Collections.ICollection.Count"/>.</exception>
		/// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see>.</exception>
		/// <exception cref="NotSupportedException">The <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see> is read-only.<br/>
		/// -or-<br/>
		/// The <see cref="IOrderedDictionary{TKey,TValue}">IOrderedDictionary&lt;TKey,TValue&gt;</see> has a fized size.</exception>
		void Insert(int index, TKey key, TValue value);

		/// <summary>
		/// Gets or sets the value at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the value to get or set.</param>
		/// <value>The value of the item at the specified index.</value>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0.<br/>
		/// -or-<br/>
		/// <paramref name="index"/> is equal to or greater than <see cref="System.Collections.ICollection.Count"/>.</exception>
		TValue this[int index]
		{
			get;
			set;
		}
		
		/// <summary>
		/// Returns the zero-based index of the specified key in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></param>
		/// <returns>The zero-based index of <paramref name="key"/>, if <paramref name="ley"/> is found in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>; otherwise, -1</returns>
		/// <remarks>This method performs a linear search; therefore it has a cost of O(n) at worst.</remarks>
		int IndexOfKey(TKey key);
	}

	/// <summary>
	/// Represents a generic collection of key/value pairs that are ordered independently of the key and value.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
	[Serializable]
	public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>
	{
		private const int DefaultInitialCapacity = 0;

		private static readonly string _keyTypeName = typeof(TKey).FullName;
		private static readonly string _valueTypeName = typeof(TValue).FullName;
		private static readonly bool _valueTypeIsReferenceType = !typeof(ValueType).IsAssignableFrom(typeof(TValue));

		private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey,TValue>();
		private List<KeyValuePair<TKey, TValue>> _list = new List<KeyValuePair<TKey,TValue>>();
		private IEqualityComparer<TKey> _comparer = null;
		private object _syncRoot = null;

		public event AddEventHandler<TKey, TValue> AddEvent;
		protected void OnAdd(TKey key, TValue value)
		{
			if (AddEvent != null)
				AddEvent(this, key, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see> class.
		/// </summary>
		public OrderedDictionary()
		{
		}

		#region IOrderedDictionary<TKey,TValue> Members
		/// <summary>
		/// Returns the zero-based index of the specified key in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see></param>
		/// <returns>The zero-based index of <paramref name="key"/>, if <paramref name="ley"/> is found in the <see cref="OrderedDictionary{TKey,TValue}">OrderedDictionary&lt;TKey,TValue&gt;</see>; otherwise, -1</returns>
		/// <remarks>This method performs a linear search; therefore it has a cost of O(n) at worst.</remarks>
		public int IndexOfKey(TKey key)
		{
			if (null == key)
				throw new ArgumentNullException("key");

			for (int index = 0; index < _list.Count; index++)
			{
				KeyValuePair<TKey, TValue> entry = _list[index];
				TKey next = entry.Key;
				if (null != _comparer)
				{
					if (_comparer.Equals(next, key))
					{
						return index;
					}
				}
				else if (next.Equals(key))
				{
					return index;
				}
			}

			return -1;
		}

		public void Insert(int index, TKey key, TValue value)
		{
			if (index > Count || index < 0)
				throw new ArgumentOutOfRangeException("index");

			_dictionary.Add(key, value);
			_list.Insert(index, new KeyValuePair<TKey, TValue>(key, value));

			OnAdd(key, value);
		}

		public TValue this[int index]
		{
			get
			{
				return _list[index].Value;
			}

			set
			{
				if (index >= Count || index < 0)
					throw new ArgumentOutOfRangeException("index", "'index' must be non-negative and less than the size of the collection");

				TKey key = _list[index].Key;

				_list[index] = new KeyValuePair<TKey, TValue>(key, value);
				_dictionary[key] = value;

				OnAdd(key, value);
			}
		}

		#endregion

		#region IDictionary<TKey,TValue> Members

		public void Add(TKey key, TValue value)
		{
			_dictionary.Add(key, value);
			_list.Add(new KeyValuePair<TKey, TValue>(key, value));
			OnAdd(key, value);
			//return Count - 1;
		}

		public bool ContainsKey(TKey key)
		{
			return _dictionary.ContainsKey(key);
		}

		public ICollection<TKey> Keys
		{
			get
			{
				List<TKey> keys = new List<TKey>();

				foreach (var item in _list)
					keys.Add(item.Key);

				return keys;
			}
		}

		public bool Remove(TKey key)
		{
			if (null == key)
				throw new ArgumentNullException("key");

			int index = IndexOfKey(key);
			if (index >= 0)
			{
				if (_dictionary.Remove(key))
				{
					_list.RemoveAt(index);
					return true;
				}
			}
			return false;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			throw new NotImplementedException();
		}

		public ICollection<TValue> Values
		{
			get
			{
				return this._dictionary.Values;
			}
		}

		public TValue this[TKey key]
		{
			get
			{
				return _dictionary[key];
			}
			set
			{
				if (_dictionary.ContainsKey(key))
				{
					_dictionary[key] = value;
					_list[IndexOfKey(key)] = new KeyValuePair<TKey, TValue>(key, value);
					OnAdd(key, value);
				}
				else
				{
					Add(key, value);
				}
			}
		}

		#endregion

		#region ICollection<KeyValuePair<TKey,TValue>> Members

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public int Count
		{
			get { return this._list.Count; }
		}

		public bool IsReadOnly
		{
			get { throw new NotImplementedException(); }
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IEnumerable<KeyValuePair<TKey,TValue>> Members

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		#endregion
	}

}

// end
