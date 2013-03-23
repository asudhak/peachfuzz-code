using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core
{
	[Serializable]
	public class Random
	{
		[NonSerialized]
		TinyMT32 _prng;
		protected uint _seed;

		public Random(uint seed)
		{
			_seed = seed;
			_prng = new TinyMT32(seed);
		}

		public uint Seed
		{
			get { return _seed; }
		}

		// 0 <= X < max
		public int Next(int max)
		{
			return Next(0, max);
		}

		// min <= X < max
		public int Next(int min, int max)
		{
			uint diff = (uint)(max - min);
			if (diff <= 1)
				return min;

			return (int)((uint)(_prng.Sample() * diff) + min);
		}

		// 0 <= X < max
		public uint Next(uint max)
		{
			return Next(0, max);
		}

		// min <= X < max
		public uint Next(uint min, uint max)
		{
			uint diff = (max - min);
			if (diff <= 1)
				return min;

			return (uint)(_prng.Sample() * diff) + min;
		}

		// 0 <= X < max
		public long Next(long max)
		{
			return Next(0, max);
		}

		// min <= X < max
		public long Next(long min, long max)
		{
			ulong diff = (ulong)(max - min);
			if (diff <= 1)
				return min;

			return (long)((long)(_prng.Sample() * diff) + min);
		}

		// 0 <= X < max
		public ulong Next(ulong max)
		{
			return Next(0, max);
		}

		// min <= X < max
		public ulong Next(ulong min, ulong max)
		{
			ulong diff = (max - min);
			if (diff <= 1)
				return min;

			return (ulong)(_prng.Sample() * diff) + min;
		}
		// int.MinValue <= X <= int.MaxValue
		public int NextInt32()
		{
			return (int)NextUInt32();
		}

		// 0 <= X <= int.MaxValue
		public uint NextUInt32()
		{
			return _prng.GenerateUInt();
		}

		// long.MinValue <= X <= long.MaxValue
		public long NextInt64()
		{
			return (long)NextUInt64();
		}

		// 0 <= X <= ulong.MaxValue
		public ulong NextUInt64()
		{
			return ((ulong)_prng.GenerateUInt() << 32 | _prng.GenerateUInt());
		}

		public T Choice<T>(IEnumerable<T> list)
		{
			return ElementAt<T>(list, Next(0, list.Count()));
		}

		public T[] Sample<T>(IEnumerable<T> items, int k)
		{
			List<T> ret = new List<T>();
			List<int> usedIndexes = new List<int>();
			int index;

			if (items.Count() < k)
			{
				k = items.Count();
				ret.AddRange(items);
				return Shuffle<T>(ret.ToArray());
			}

			for (int i = 0; i < k; ++i)
			{
				do
				{
					index = Next(0, items.Count());
				}
				while (usedIndexes.Contains(index));
				usedIndexes.Add(index);

				ret.Add(items.ElementAt(index));
			}

			return ret.ToArray();
		}

		/// <summary>
		/// Fisher-Yates array shuffling algorithm.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items"></param>
		/// <returns></returns>
		public T[] Shuffle<T>(T[] items)
		{
			T temp;
			int n = items.Length;
			int k = 0;

			while (n > 1)
			{
				k = Next(0, n);
				n--;
				temp = items[n];
				items[n] = items[k];
				items[k] = temp;
			}

			return items;
		}

		/// <summary>
		/// Work around for missing method in Mono
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		protected T ElementAt<T>(IEnumerable<T> list, int index)
		{
			var enumerator = list.GetEnumerator();

			// <= because Current is set before the first element and must be called once to get first element.
			for (int cnt = 0; cnt <= index; cnt++)
				enumerator.MoveNext();

			return enumerator.Current;
		}
	}
}

// end
