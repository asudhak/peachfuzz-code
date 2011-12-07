using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core
{
	public class Random
	{
		System.Random _random;
		protected int _seed;

		public Random()
		{
			_seed = (int)DateTime.Now.Ticks;
			_random = new System.Random(_seed);
		}

		public Random(int seed)
		{
			_seed = seed;
			_random = new System.Random(seed);
		}

		public int Seed
		{
			get { return _seed; }
			set { _seed = value; }
		}

		public int Next()
		{
			return _random.Next();
		}

		public int Next(int max)
		{
			return _random.Next(max);
		}

		public int Next(int min, int max)
		{
			return _random.Next(min, max);
		}

        public UInt32 NextUInt32(UInt32 min = 0, UInt32 max = UInt32.MaxValue)
        {
            return (UInt32)(min + (UInt32)(_random.NextDouble() * (max - min)));
        }

        public Int64 NextInt64(Int64 min = Int64.MinValue, Int64 max = Int64.MaxValue)
        {
            return (Int64)(min + (Int64)(_random.NextDouble() * (max - min)));
        }

        public UInt64 NextUInt64(UInt64 min = 0, UInt64 max = UInt64.MaxValue)
        {
            return (UInt64)(min + (UInt64)(_random.NextDouble() * (max - min)));
        }

		public T Choice<T>(IEnumerable<T> list)
		{
			return ElementAt<T>(list, _random.Next(0, list.Count()));
		}

        public T[] Sample<T>(IEnumerable<T> items, int k)
        {
            List<T> ret = new List<T>();

            for (int i = 0; i < k; ++i)
                ret.Add(items.ElementAt(_random.Next(0, items.Count())));

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
            T[] ret = items;
            T temp;
            int n = items.Length;
            int k = 0;

            while (n > 1)
            {
                k = Next(n);
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
