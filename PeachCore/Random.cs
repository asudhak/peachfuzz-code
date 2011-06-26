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
			for (int cnt = 0; cnt < index; cnt++)
				enumerator.MoveNext();

			return enumerator.Current;
		}
	}
}
