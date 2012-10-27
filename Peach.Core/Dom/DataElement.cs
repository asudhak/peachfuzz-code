
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
using System.Collections;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

using Peach.Core.IO;
using Peach.Core.Cracker;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using NLog;

namespace Peach.Core.Dom
{
	/// <summary>
	/// Length types
	/// </summary>
	/// <remarks>
	/// The "length" property defaults to Bytes.  Not all
	/// implementations of DataElement will support all LengthTypes.
	/// </remarks>
	public enum LengthType
	{
		Bytes,
		Bits,
		Chars
	}

	public enum ValueType
	{
		String,
		Hex,
		Python,
		Ruby
	}

	public delegate void InvalidatedEventHandler(object sender, EventArgs e);

	/// <summary>
	/// Base class for all data elements.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	[Parameter("name", typeof(string), "Optional name for element", false)]
	[Parameter("value", typeof(string), "Optional default value", false)]
	[Parameter("valueType", typeof(ValueType), "Optional name for element", false)]
	[DebuggerDisplay("{fullName}")]
	public abstract class DataElement : INamed, ICrackable
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		public static bool DebugClone = false;

		public class CloneContext
		{
			public CloneContext(DataElement root, string newName)
			{
				this.root = root;
				this.oldName = root.name;
				this.newName = newName;
				rename.Add(root);
			}

			public DataElement root = null;
			public string oldName = null;
			public string newName = null;

			public List<DataElement> rename = new List<DataElement>();
			public Dictionary<string, DataElement> elements = new Dictionary<string, DataElement>();
			public Dictionary<object, object> metadata = new Dictionary<object, object>();
		}

		private sealed class DataElementBinder : SerializationBinder
		{
			public override Type BindToType(string assemblyName, string typeName)
			{
				foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (asm.FullName == assemblyName)
						return asm.GetType(typeName);
				}
				return null;
			}
		}

		/// <summary>
		/// Creates a deep copy of the DataElement, and updates the appropriate Relations.
		/// </summary>
		/// <returns>Returns a copy of the DataElement.</returns>
		public DataElement Clone()
		{
			return Clone(name);
		}

		/// <summary>
		/// Creates a deep copy of the DataElement, and updates the appropriate Relations.
		/// </summary>
		/// <param name="newName">What name to set on the cloned DataElement</param>
		/// <returns>Returns a copy of the DataElement.</returns>
		public DataElement Clone(string newName)
		{
			long size = 0;
			return Clone(newName, ref size);
		}

		/// <summary>
		/// Creates a deep copy of the DataElement, and updates the appropriate Relations.
		/// </summary>
		/// <param name="newName">What name to set on the cloned DataElement</param>
		/// <param name="size">The size in bytes used when performing the copy. Useful for debugging statistics.</param>
		/// <returns>Returns a copy of the DataElement.</returns>
		public DataElement Clone(string newName, ref long size)
		{
			if (DataElement.DebugClone)
				logger.Debug("Clone {0} as {1}", fullName, newName);

			var parent = this._parent;
			this._parent = null;

			CloneContext additional = new CloneContext(this, newName);
			StreamingContext context = new StreamingContext(StreamingContextStates.All, additional);
			BinaryFormatter formatter = new BinaryFormatter(null, context);
			MemoryStream stream = new MemoryStream();
			formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
			formatter.Binder = new DataElementBinder();
			formatter.Serialize(stream, this);
			stream.Seek(0, SeekOrigin.Begin);

			DataElement copy = (DataElement)formatter.Deserialize(stream);
			copy._parent = parent;
			this._parent = parent;

			size = stream.Length;

			if (DataElement.DebugClone)
				logger.Debug("Clone {0} took {1} bytes", copy.fullName, size);

			return copy;
		}

		/// <summary>
		/// Mutated vale override's fixupImpl
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
        /// Mutated value overrides type transforms.
        /// </summary>
        public const uint MUTATE_OVERRIDE_TYPE_TRANSFORM = 0x20;
		/// <summary>
		/// Default mutate value
		/// </summary>
		public const uint MUTATE_DEFAULT = MUTATE_OVERRIDE_FIXUP;

		private string _name;

		public string name
		{
			get { return _name; }
		}

		public bool isMutable = true;
		public uint mutationFlags = MUTATE_DEFAULT;
		public bool isToken = false;

		public Analyzer analyzer = null;

		protected Dictionary<string, Hint> hints = new Dictionary<string, Hint>();

		protected bool _isReference = false;

		protected Variant _defaultValue;
		protected Variant _mutatedValue;

		protected RelationContainer _relations = null;
		protected Fixup _fixup = null;
		protected Transformer _transformer = null;
		protected Placement _placement = null;

		protected DataElementContainer _parent;

		private uint _recursionDepth = 0;
		private bool _readValueCache = true;
		private bool _writeValueCache = true;
		private Variant _internalValue;
		private BitStream _value;

		private bool _invalidated = false;

		/// <summary>
		/// Does this element have a defined length?
		/// </summary>
		protected bool _hasLength = false;

		/// <summary>
		/// Length in bits
		/// </summary>
		protected long _length = 0;

		/// <summary>
		/// Determines how the length property works.
		/// </summary>
		protected LengthType _lengthType = LengthType.Bytes;

		/// <summary>
		/// Contains the calculation if any
		/// </summary>
		protected string _lengthCalc = null;

		protected string _constraint = null;

		#region Events

		[NonSerialized]
		private InvalidatedEventHandler _invalidatedEvent;

		public event InvalidatedEventHandler Invalidated
		{
			add { _invalidatedEvent += value; }
			remove { _invalidatedEvent -= value; }
		}

		public abstract void Crack(DataCracker context, BitStream data);

		protected void OnInvalidated(EventArgs e)
		{
			// Prevent infinite loops
			if (_invalidated)
				return;

			try
			{
				_invalidated = true;

				// Cause values to be regenerated next time they are
				// requested.  We don't want todo this now as there could
				// be a series of invalidations that occur.
				_internalValue = null;
				_value = null;

				// Bubble this up the chain
				if (_parent != null)
					_parent.Invalidate();

				if (_invalidatedEvent != null)
					_invalidatedEvent(this, e);
			}
			finally
			{
				_invalidated = false;
			}
		}

		#endregion

		public static OrderedDictionary<string, Type> dataElements = new OrderedDictionary<string, Type>();
		public static void loadDataElements(Assembly assembly)
		{
			foreach (Type type in assembly.GetTypes())
			{
				if (type.IsClass && !type.IsAbstract)
				{
					var attr = type.GetAttributes<DataElementAttribute>(null).First();
					if (!dataElements.ContainsKey(attr.elementName))
					{
						dataElements.Add(attr.elementName, type);
					}
				}
			}
		}

		/// <summary>
		/// Recursively returns elements of a specific type.  Will not
		/// return elements of our partent.
		/// </summary>
		/// <param name="type">Type of elements to locate and return</param>
		/// <returns>Returns elements of a specific type</returns>
		public IEnumerable<DataElement> getElementsByType(Type type)
		{
			foreach(DataElement element in EnumerateAllElements())
			{
				if(element.GetType() == type)
					yield return element;
			}
		}

		/// <summary>
		/// Dynamic properties
		/// </summary>
		/// <remarks>
		/// Any objects added to properties must be serializable!
		/// </remarks>
		public Dictionary<string, object> Properties
		{
			get;
			set;
		}

		protected static uint _uniqueName = 0;
		public DataElement()
		{
			_relations = new RelationContainer(this);
			_name = "DataElement_" + _uniqueName;
			_uniqueName++;
		}

		public DataElement(string name)
		{
			if (name.IndexOf('.') > -1)
				throw new PeachException("Error, DataElements cannot contain a period in their name. \"" + name + "\"");

			_relations = new RelationContainer(this);
			_name = name;
		}

		public static T Generate<T>(XmlNode node) where T : DataElement, new()
		{
			string name = node.getAttribute("name");

			if (string.IsNullOrEmpty(name))
			{
				return new T();
			}
			else
			{
				try
				{
					return (T)Activator.CreateInstance(typeof(T), name);
				}
				catch (TargetInvocationException ex)
				{
					throw ex.InnerException;
				}
			}
		}

		/// <summary>
		/// Full qualified name of DataElement to
		/// root DataElement.
		/// </summary>
		public string fullName
		{
			// TODO: Cache fullName if possible

			get
			{
				string fullname = name;
				DataElement obj = _parent;
				while (obj != null)
				{
					fullname = obj.name + "." + fullname;
					obj = obj.parent;
				}

				return fullname;
			}
		}

		/// <summary>
		/// Recursively execute analyzers
		/// </summary>
		public virtual void evaulateAnalyzers()
		{
			if (analyzer == null)
				return;

			analyzer.asDataElement(this, null);
		}

		public Dictionary<string, Hint> Hints
		{
			get { return hints; }
			set { hints = value; }
		}

		/// <summary>
		/// Constraint on value of data element.
		/// </summery>
		/// <remarks>
		/// This
		/// constraint is only enforced when loading data into
		/// the object.  It will not affect values that are
		/// produced during fuzzing.
		/// </remarks>
		public string constraint
		{
			get { return _constraint; }
			set { _constraint = value; }
		}

		/// <summary>
		/// Is this DataElement created by a 
		/// reference to another DataElement?
		/// </summary>
		public bool isReference
		{
			get { return _isReference; }
			set { _isReference = value; }
		}

		string _referenceName;

		/// <summary>
		/// If created by reference, has the reference name
		/// </summary>
		public string referenceName
		{
			get { return _referenceName; }
			set { _referenceName = value; }
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
			if (nextIndex >= _parent.Count)
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
			//_invalidated = true;

			OnInvalidated(null);
		}

		/// <summary>
		/// Is this a leaf of the DataModel tree?
		/// 
		/// True if DataElement has no children.
		/// </summary>
		public virtual bool isLeafNode
		{
			get { return true; }
		}

		/// <summary>
		/// Does element have a length?  This is
		/// separate from Relations.
		/// </summary>
		public virtual bool hasLength
		{
			get
			{
				if (isToken && DefaultValue != null)
					return true;

				return _hasLength;
			}
			set { _hasLength = value; }
		}

		/// <summary>
		/// Length of element in bits.
		/// </summary>
		/// <remarks>
		/// In the case that LengthType == "Calc" we will evaluate the
		/// expression.
		/// </remarks>
		public virtual long length
		{
			get
			{
				if (_lengthCalc != null)
				{
					Dictionary<string, object> scope = new Dictionary<string, object>();
					scope["self"] = this;
					return (int)Scripting.EvalExpression(_lengthCalc, scope);
				}

				if (_hasLength)
				{
					switch (_lengthType)
					{
						case LengthType.Bytes:
							return _length / 8;
						case LengthType.Bits:
							return _length;
						case LengthType.Chars:
							throw new NotSupportedException("Length type of Chars not supported by DataElement.");
						default:
							throw new NotSupportedException("Error calculating length.");
					}
				}
				else if (isToken && DefaultValue != null)
				{
					return Value.Value.Length;
				}
				else
				{
					switch (_lengthType)
					{
						case LengthType.Bytes:
							return Value.LengthBytes;
						case LengthType.Bits:
							return Value.LengthBits;
						case LengthType.Chars:
							throw new NotSupportedException("Length type of Chars not supported by DataElement.");
						default:
							throw new NotSupportedException("Error calculating length.");
					}

				}
			}

			set
			{
				switch (_lengthType)
				{
					case LengthType.Bytes:
						_length = value * 8;
						break;
					case LengthType.Bits:
						_length = value;
						break;
					case LengthType.Chars:
						throw new NotSupportedException("Length type of Chars not supported by DataElement.");
				}

				_hasLength = true;
			}
		}

		/// <summary>
		/// Returns length as bits.
		/// </summary>
		public virtual long lengthAsBits
		{
			get
			{
				switch (_lengthType)
				{
					case LengthType.Bytes:
						return length * 8;
					case LengthType.Bits:
						return length;
					case LengthType.Chars:
						throw new NotSupportedException("Length type of Chars not supported by DataElement.");
					default:
						throw new NotSupportedException("Error calculating length.");
				}
			}
		}

		/// <summary>
		/// Length expression.  This expression is used
		/// to calculate the length of this element.
		/// </summary>
		public virtual string lengthCalc
		{
			get { return _lengthCalc; }
			set { _lengthCalc= value; }
		}

		/// <summary>
		/// Type of length.
		/// </summary>
		/// <remarks>
		/// Not all DataElement implementations support "Chars".
		/// 
		/// Note: A breaking change between Peach 2.3 and Peach 3 is 
		/// the removal of the "calc" length type.  Instead use the
		/// "lengthCalc" property.
		/// </remarks>
		public virtual LengthType lengthType
		{
			get { return _lengthType; }
			set { _lengthType = value; }
		}

		/// <summary>
		/// Default value for this data element.
		/// 
		/// Changing the default value will invalidate
		/// the model.
		/// </summary>
		public virtual Variant DefaultValue
		{
			get { return _defaultValue; }
			set
			{
				_defaultValue = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Current mutated value (if any) for this data element.
		/// 
		/// Changing the MutatedValue will invalidate the model.
		/// </summary>
		public virtual Variant MutatedValue
		{
			get { return _mutatedValue; }
			set
			{
				_mutatedValue = value;
				Invalidate();
			}
		}

        /// <summary>
        /// Get the Internal Value of this data element
        /// </summary>
		public Variant InternalValue
		{
			get
			{
				if (_internalValue == null || _invalidated || !_readValueCache)
				{
					var internalValue = GenerateInternalValue();

					if (CacheValue)
						_internalValue = internalValue;

					return internalValue;
				}

				return _internalValue;
			}
		}

        /// <summary>
        /// Get the final Value of this data element
        /// </summary>
		public BitStream Value
		{
			get
			{
				// If cache reads have not been disabled, inherit value from parent
				var oldReadCache = _readValueCache;
				if (_readValueCache && parent != null)
					_readValueCache = parent._readValueCache;

				// If cache writes have not been disabled, inherit value from parent
				var oldWriteCache = _writeValueCache;
				if (_writeValueCache && parent != null)
					_writeValueCache = parent._writeValueCache;

				try
				{
					if (_value == null || _invalidated || !_readValueCache)
					{
						_recursionDepth++;

						var value = GenerateValue();
						_invalidated = false;

						if (CacheValue)
							_value = value;

						_recursionDepth--;

						return value;
					}

					return _value;
				}
				finally
				{
					// Restore values
					_writeValueCache = oldWriteCache;
					_readValueCache = oldReadCache;
				}
			}
		}

		private bool CacheValue
		{
			get
			{
				if (!_writeValueCache || _recursionDepth > 1)
					return false;

				if (_fixup != null)
				{
					// The root can't have a fixup!
					System.Diagnostics.Debug.Assert(_parent != null);

					// We can only have a valid fixup value when the parent
					// has not recursed onto itself
					if (_parent._recursionDepth > 1)
						return false;
				}

				return true;
			}
		}

		public long CalcLengthBits()
		{
			// Turn off read and write caching of 'Value'
			var oldReadCache = _readValueCache;
			_readValueCache = false;
			var oldWriteCache = _writeValueCache;
			_writeValueCache = false;

			var ret = Value.LengthBits;

			_writeValueCache = oldWriteCache;
			_readValueCache = oldReadCache;

			return ret;
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

			// 2. Check for type transformations

			if (MutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_TYPE_TRANSFORM) != 0)
			{
				return MutatedValue;
			}

			// 3. Relations

			if (MutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_RELATIONS) != 0)
			{
				return MutatedValue;
			}

			foreach(Relation r in _relations)
			{
				if (IsFromRelation(r))
				{
					// CalculateFromValue can return null sometimes
					// when mutations mess up the relation.
					// In that case use the exsiting value for this element.

					var relationValue = r.CalculateFromValue();
					if (relationValue != null)
						value = relationValue;
				}
			}

			// 4. Fixup

			if (MutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_FIXUP) != 0)
			{
				return MutatedValue;
			}

			if (_fixup != null)
				value = _fixup.fixup(this);

			return value;
		}

		protected virtual BitStream InternalValueToBitStream()
		{
			var ret = InternalValue;
			if (ret == null)
				return new BitStream();
			return (BitStream)ret;
		}

		/// <summary>
		/// How many times GenerateValue has been called on this element
		/// </summary>
		/// <returns></returns>
		public uint GenerateCount { get; private set; }

		/// <summary>
		/// Generate the final value of this data element
		/// </summary>
		/// <returns></returns>
		public BitStream GenerateValue()
		{
			++GenerateCount;

			BitStream value = null;

			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_TYPE_TRANSFORM) != 0)
			{
				value = (BitStream)_mutatedValue;
			}
			else
			{
				value = InternalValueToBitStream();
			}

            if (_mutatedValue == null || (mutationFlags & MUTATE_OVERRIDE_TRANSFORMER) != 0)
                if (_transformer != null)
                    value = _transformer.encode(value);

			return value;
		}

		/// <summary>
		/// Enumerates all DataElements starting from 'start.'
		/// 
		/// This method will first return children, then siblings, then children
		/// of siblings as it walks up the parent chain.  It will not return
		/// any duplicate elements.
		/// 
		/// Note: This is not the fastest way to enumerate all elements in the
		/// tree, it's specifically intended for findings Elements in a search
		/// pattern that matches a persons assumptions about name resolution.
		/// </summary>
		/// <param name="start">Starting DataElement</param>
		/// <returns>All DataElements in model.</returns>
		public static IEnumerable EnumerateAllElementsFromHere(DataElement start)
		{
			foreach(DataElement elem in EnumerateAllElementsFromHere(start, new List<DataElement>()))
				yield return elem;
		}

		/// <summary>
		/// Enumerates all DataElements starting from 'start.'
		/// 
		/// This method will first return children, then siblings, then children
		/// of siblings as it walks up the parent chain.  It will not return
		/// any duplicate elements.
		/// 
		/// Note: This is not the fastest way to enumerate all elements in the
		/// tree, it's specifically intended for findings Elements in a search
		/// pattern that matches a persons assumptions about name resolution.
		/// </summary>
		/// <param name="start">Starting DataElement</param>
		/// <param name="cache">Cache of DataElements already returned</param>
		/// <returns>All DataElements in model.</returns>
		public static IEnumerable EnumerateAllElementsFromHere(DataElement start, 
			List<DataElement> cache)
		{
			// Add ourselvs to the cache is not already done
			if (!cache.Contains(start))
				cache.Add(start);

			// 1. Enumerate all siblings

			if (start.parent != null)
			{
				foreach (DataElement elem in start.parent)
					if (!cache.Contains(elem))
						yield return elem;
			}

			// 2. Children

			foreach (DataElement elem in EnumerateChildrenElements(start, cache))
				yield return elem;

			// 3. Children of siblings

			if (start.parent != null)
			{
				foreach (DataElement elem in start.parent)
				{
					if (!cache.Contains(elem))
					{
						cache.Add(elem);
						foreach(DataElement ret in EnumerateChildrenElements(elem, cache))
							yield return ret;
					}
				}
			}

			// 4. Parent, walk up tree

			if (start.parent != null)
				foreach (DataElement elem in EnumerateAllElementsFromHere(start.parent))
					yield return elem;
		}

		/// <summary>
		/// Enumerates all children starting from, but not including
		/// 'start.'  Will also enumerate the children of children until
		/// leaf nodes are hit.
		/// </summary>
		/// <param name="start">Starting DataElement</param>
		/// <param name="cache">Cache of already seen elements</param>
		/// <returns>Returns DataElement children of start.</returns>
		protected static IEnumerable EnumerateChildrenElements(DataElement start, List<DataElement> cache)
		{
			if (!(start is DataElementContainer))
				yield break;

			foreach (DataElement elem in start as DataElementContainer)
				if (!cache.Contains(elem))
					yield return elem;

			foreach (DataElement elem in start as DataElementContainer)
			{
				if (!cache.Contains(elem))
				{
					cache.Add(elem);
					foreach (DataElement ret in EnumerateAllElementsFromHere(elem, cache))
						yield return ret;
				}
			}
		}

		/// <summary>
		/// Find data element with specific name.
		/// </summary>
		/// <remarks>
		/// We will search starting at our level in the tree, then moving
		/// to children from our level, then walk up node by node to the
		/// root of the tree.
		/// </remarks>
		/// <param name="name">Name to search for</param>
		/// <returns>Returns found data element or null.</returns>
		public DataElement find(string name)
		{
			string [] names = name.Split(new char[] {'.'});

			if (names.Length == 1)
			{
				// Make sure it's not us :)
				if (this.name == names[0])
					return this;

				// Check our children
				foreach (DataElement elem in EnumerateElementsUpTree())
				{
					if(elem.name == names[0])
						return elem;
				}

				// Can't locate!
				return null;
			}

			foreach (DataElement elem in EnumerateElementsUpTree())
			{
				if (elem.fullName == name)
					return elem;

				if (!(elem is DataElementContainer))
					continue;

				DataElement ret = ((DataElementContainer)elem).QuickNameMatch(names);
				if (ret != null)
					return ret;
			}

			DataElement root = getRoot();
			if (root == this)
				return null;

			return root.find(name);
		}

		/// <summary>
		/// Enumerate all items in tree starting with our current position
		/// then moving up towards the root.
		/// </summary>
		/// <remarks>
		/// This method uses yields to allow for efficient use even if the
		/// quired node is found quickely.
		/// 
		/// The method in which we return elements should match a human
		/// search pattern of a tree.  We start with our current position and
		/// return all children then start walking up the tree towards the root.
		/// At each parent node we return all children (excluding already returned
		/// nodes).
		/// 
		/// This method is ideal for locating objects in the tree in a way indented
		/// a human user.
		/// </remarks>
		/// <returns></returns>
		public IEnumerable<DataElement> EnumerateElementsUpTree()
		{
			foreach (DataElement e in EnumerateElementsUpTree(new List<DataElement>()))
				yield return e;
		}

		/// <summary>
		/// Enumerate all items in tree starting with our current position
		/// then moving up towards the root.
		/// </summary>
		/// <remarks>
		/// This method uses yields to allow for efficient use even if the
		/// quired node is found quickely.
		/// 
		/// The method in which we return elements should match a human
		/// search pattern of a tree.  We start with our current position and
		/// return all children then start walking up the tree towards the root.
		/// At each parent node we return all children (excluding already returned
		/// nodes).
		/// 
		/// This method is ideal for locating objects in the tree in a way indented
		/// a human user.
		/// </remarks>
		/// <param name="knownParents">List of known parents to stop duplicates</param>
		/// <returns></returns>
		public IEnumerable<DataElement> EnumerateElementsUpTree(List<DataElement> knownParents)
		{
			List<DataElement> toRoot = new List<DataElement>();
			DataElement cur = this;
			while (cur != null)
			{
				toRoot.Add(cur);
				cur = cur.parent;
			}

			foreach (DataElement item in toRoot)
			{
				if (!knownParents.Contains(item))
				{
					foreach (DataElement e in item.EnumerateAllElements())
						yield return e;

					knownParents.Add(item);
				}
			}

			// Root will not be returned otherwise
			yield return getRoot();
		}

		/// <summary>
		/// Enumerate all child elements recursevely.
		/// </summary>
		/// <remarks>
		/// This method will return this objects direct children
		/// and finally recursevely return children's children.
		/// </remarks>
		/// <returns></returns>
		public IEnumerable<DataElement> EnumerateAllElements()
		{
			foreach (DataElement e in EnumerateAllElements(new List<DataElement>()))
				yield return e;
		}

		/// <summary>
		/// Enumerate all child elements recursevely.
		/// </summary>
		/// <remarks>
		/// This method will return this objects direct children
		/// and finally recursevely return children's children.
		/// </remarks>
		/// <param name="knownParents">List of known parents to skip</param>
		/// <returns></returns>
		public virtual IEnumerable<DataElement> EnumerateAllElements(List<DataElement> knownParents)
		{
			yield break;
		}

    public abstract object GetParameter(string parameterName);

		/// <summary>
		/// Fixup for this data element.  Can be null.
		/// </summary>
		public Fixup fixup
		{
			get { return _fixup; }
			set { _fixup = value; }
		}

		/// <summary>
		/// Placement for this data element. Can be null.
		/// </summary>
		public Placement placement
		{
			get { return _placement; }
			set { _placement = value; }
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
		public RelationContainer relations
		{
			get { return _relations; }
		}

		private static string FmtMessage(Relation r, DataElement obj, string who)
		{
			return string.Format("Relation Of=\"{0}\" From=\"{1}\" not {2}element \"{3}\"",
					r.Of.fullName, r.From.fullName, who, obj.fullName);
		}

		private bool ContainsNamedRelation(Relation r)
		{
			string fullFrom = r.From.fullName;
			string fullOf = r.Of.fullName;

			foreach (var item in _relations)
			{
				if (fullOf == item.Of.fullName && fullFrom == item.From.fullName)
					return true;
			}

			return false;
		}

		protected bool IsFromRelation(Relation r)
		{
#if DEBUG
			if (!_relations.Contains(r))
				throw new ArgumentException(FmtMessage(r, this, "referenced by "));

			if (r.From.parent == null)
				throw new PeachException(FmtMessage(r, r.From, "valid parent in from="));

			// r.Of.parent can be null if r.Of is the data model

			if (!r.From.ContainsNamedRelation(r))
				throw new PeachException(FmtMessage(r, r.From, "referenced in from="));

			if (!r.Of.ContainsNamedRelation(r))
				throw new PeachException(FmtMessage(r, r.Of, "contained in of="));

			if (!r.From.relations.Contains(r))
				throw new PeachException(FmtMessage(r, r.From, "contained in from="));

			if (!r.Of.relations.Contains(r))
				throw new PeachException(FmtMessage(r, r.Of, "referenced in of="));

			bool notFromStr = r.From.fullName != this.fullName;
			bool notOfStr = r.Of.fullName != this.fullName;

			if (notOfStr == notFromStr)
				throw new PeachException(FmtMessage(r, this, "named from or of="));

			bool notFrom = r.From != this;
			bool notOf = r.Of != this;

			if (notOf == notFrom)
				throw new PeachException(FmtMessage(r, this, "from or of="));
#endif
			return r.From == this;
		}

		public void VerifyRelations()
		{
#if DEBUG
			foreach (var r in _relations)
				IsFromRelation(r);

			DataElementContainer cont = this as DataElementContainer;
			if (cont == null)
				return;

			foreach (var c in cont)
				c.VerifyRelations();
#endif
		}

		public void ClearRelations()
		{
			foreach (var r in _relations)
			{
				// Remove toasts r.parent, so resolve 'From' and 'Of' 1st
				var from = r.From;
				var of = r.Of;

				if (from != this)
					from.relations.Remove(r);
				if (of != this)
					of.relations.Remove(r);
			}

			_relations.Clear();

			DataElementContainer cont = this as DataElementContainer;
			if (cont == null)
				return;

			foreach (var child in cont)
				child.ClearRelations();
		}

		/// <summary>
		/// Determines whether or not a DataElement is a child of this DataElement.
		/// Computes the relative name from 'this' to 'dataElement'.  If 'dataElement'
		/// is not a child of 'this', the absolute path of 'dataElement' is computed.
		/// </summary>
		/// <param name="dataElement">The DataElement to test for a child relationship.</param>
		/// <param name="relName">String to receive the realitive name of 'dataElement'.</param>
		/// <returns>Returns true if 'dataElement' is a child, false otherwise.</returns>
		public bool isChildOf(DataElement dataElement, out string relName)
		{
			relName = name;

			DataElement obj = _parent;
			while (obj != null)
			{
				if (obj == dataElement)
					return true;

				relName = obj.name + "." + relName;
				obj = obj.parent;
			}

			return false;
		}

		[OnSerializing]
		private void OnSerializing(StreamingContext context)
		{
			DataElement.CloneContext ctx = context.Context as DataElement.CloneContext;
			if (ctx == null)
				return;

			if (DataElement.DebugClone)
				logger.Debug("Serializing {0}", fullName);

			System.Diagnostics.Debug.Assert(!ctx.metadata.ContainsKey(this));

			if (ctx.rename.Contains(this))
			{
				ctx.metadata.Add(this, _name);
				_name = ctx.newName;
			}
		}

		[OnSerialized]
		private void OnSerialized(StreamingContext context)
		{
			DataElement.CloneContext ctx = context.Context as DataElement.CloneContext;
			if (ctx == null)
				return;

			object obj;
			if (ctx.metadata.TryGetValue(this, out obj))
				_name = obj as string;
		}

	}
}

// end
