using System;
using System.Collections.ObjectModel;
using Peach.Core.Dom;

namespace Peach.Core
{
	/// <summary>
	/// Simple interface that exposes TryGetValue.
	/// Useful for generic functions that want to use
	/// NamedCollection and OrderedDictionary
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public interface ITryGetValue<TKey, TValue>
	{
		bool TryGetValue(TKey key, out TValue value);
	}

	/// <summary>
	/// A collection of T where order is well defined.
	/// Provides finding a record by using T.name
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class NamedCollection<T> : KeyedCollection<string, T>, ITryGetValue<string, T> where T : INamed
	{
		private string baseName;

		/// <summary>
		/// Uses typeof(T).Name as the base name when generating unique names.
		/// </summary>
		public NamedCollection()
		{
			this.baseName = typeof(T).Name;
		}

		/// <summary>
		/// Specifies the base name to use when generating unique names.
		/// </summary>
		/// <param name="baseName"></param>
		public NamedCollection(string baseName)
		{
			this.baseName = baseName;
		}

		protected override string GetKeyForItem(T item)
		{
			return item.name;
		}

		public bool TryGetValue(string key, out T value)
		{
			if (Dictionary != null && Dictionary.TryGetValue(key, out value))
				return true;

			value = default(T);
			return false;
		}

		public bool ContainsKey(string key)
		{
			return Contains(key);
		}

		/// <summary>
		/// Return the next unique name that is not in this container.
		/// </summary>
		/// <returns></returns>
		public string UniqueName()
		{
			var name = baseName;

			for (int i = 1; Contains(name); ++i)
				name = string.Format("{0}_{1}", baseName, i);

			return name;
		}
	}
}
