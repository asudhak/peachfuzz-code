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

        public void Shuffle<T>(IEnumerable<T> items)
        {
            if (items == null)
                return;

            List<T> ret = new List<T>();

            for (int i = 0; i < items.Count(); ++i)
                ret.Add(Choice(items));

            items = ret;
        }

        //public int[] Range(int start, int stop, int step)
        //{
        //    if (step == 0)
        //        return null;

        //    List<int> ret = new List<int>();
        //    int value = start + step * ret.Count;

        //    if (step > 0)
        //    {
        //        while (value < stop)
        //        {
        //            ret.Add(value);
        //            value = start + step * ret.Count;
        //        }
        //    }
        //    else
        //    {
        //        while (value > stop)
        //        {
        //            ret.Add(value);
        //            value = start + step * ret.Count;
        //        }
        //    }

        //    return ret.ToArray();
        //}

        //public T[] Slice<T>(T[] source, int start, int end)
        //{
        //    // catch invalid ends
        //    if (end < 0)
        //    {
        //        end += source.Length;
        //    }
        //    else if (end > source.Length)
        //    {
        //        end = source.Length;
        //    }
        //    else if (start < 0)
        //    {
        //        start = 0;
        //    }
        //    else if (start > source.Length)
        //    {
        //        return new T[0];
        //    }
        //    int len = end - start;

        //    // create new array
        //    T[] ret = new T[len];
        //    for (int i = 0; i < len; i++)
        //    {
        //        ret[i] = source[i + start];
        //    }
        //    return ret;
        //}

        //public T[] Combine<T>(params T[][] arrays)
        //{
        //    T[] ret = new T[arrays.Sum(a => a.Length)];
        //    int offset = 0;

        //    foreach (T[] array in arrays)
        //    {
        //        Buffer.BlockCopy(array, 0, ret, offset, array.Length);
        //        offset += array.Length;
        //    }

        //    return ret;
        //}

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
