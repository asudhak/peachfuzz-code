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
			return list.ElementAt(_random.Next(0, list.Count()));
		}
	}
}
