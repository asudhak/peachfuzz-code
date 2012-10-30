using System;

namespace Peach.Core
{
	// Implementation of TinyMT - A smaller version of the mersenne twister algorithm.
	// Based on tinymt32.c
	// See: http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/TINYMT/index.html
	public class TinyMT32
	{
		#region Constants
		public static uint MAT1 = 0x8f7011ee;
		public static uint MAT2 = 0xfc78ff1f;
		public static uint TMAT = 0x3793fdff;

		private static uint MIN_LOOP = 8;
		private static uint PRE_LOOP = 8;

		private static int SH0 = 1;
		private static int SH1 = 10;
		private static int SH8 = 8;

		private static uint MASK = 0x7fffffff;

		private static double DENOMINATOR = (double)uint.MaxValue + 1;

		#endregion

		#region Members
		private uint[] status;
		private uint mat1;
		private uint mat2;
		private uint tmat;
		#endregion

		#region Constructor
		public TinyMT32(uint seed)
			: this()
		{
			init(seed);
		}

		public TinyMT32(uint[] init_key)
			: this()
		{
			init(init_key);
		}
		#endregion

		#region Methods
		// Generates a uint X where 0 <= X < 2^32
		public uint GenerateUInt()
		{
			next_state();
			return temper();
		}

		// Generates a double X where 0 <= X < 1.0
		public double Sample()
		{
			return GenerateUInt() * 1.0 / DENOMINATOR;
		}
		#endregion

		#region Implementation
		private TinyMT32()
		{
			this.mat1 = MAT1;
			this.mat2 = MAT2;
			this.tmat = TMAT;
		}

		private void init(uint seed)
		{
			status = new uint[] { seed, mat1, mat2, tmat };

			for (uint i = 1; i < MIN_LOOP; i++)
			{
				status[i & 3] ^= i + 1812433253 * (status[(i - 1) & 3] ^ (status[(i - 1) & 3] >> 30));
			}

			period_certification();

			for (uint i = 0; i < PRE_LOOP; i++)
			{
				next_state();
			}
		}

		private void init(uint[] init_key)
		{
			const int lag = 1;
			const int mid = 1;
			const int size = 4;

			uint key_length = (uint)init_key.Length;
			uint i, j;
			uint count;
			uint r;

			status = new uint[] { 0, mat1, mat2, tmat };

			if (init_key.Length + 1 > MIN_LOOP)
				count = key_length + 1;
			else
				count = MIN_LOOP;

			r = ini_func1(status[0] ^ status[mid % size] ^ status[(size - 1) % size]);
			status[mid % size] += r;
			r += key_length;
			status[(mid + lag) % size] += r;
			status[0] = r;
			count--;

			for (i = 1, j = 0; (j < count) && (j < key_length); j++)
			{
				r = ini_func1(status[i] ^ status[(i + mid) % size] ^ status[(i + size - 1) % size]);
				status[(i + mid) % size] += r;
				r += init_key[j] + i;
				status[(i + mid + lag) % size] += r;
				status[i] = r;
				i = (i + 1) % size;
			}

			for (; j < count; j++)
			{
				r = ini_func1(status[i] ^ status[(i + mid) % size] ^ status[(i + size - 1) % size]);
				status[(i + mid) % size] += r;
				r += i;
				status[(i + mid + lag) % size] += r;
				status[i] = r;
				i = (i + 1) % size;
			}

			for (j = 0; j < size; j++)
			{
				r = ini_func2(status[i] + status[(i + mid) % size] + status[(i + size - 1) % size]);
				status[(i + mid) % size] ^= r;
				r -= i;
				status[(i + mid + lag) % size] ^= r;
				status[i] = r;
				i = (i + 1) % size;
			}

			period_certification();

			for (i = 0; i < PRE_LOOP; i++)
			{
				next_state();
			}
		}

		private void period_certification()
		{
			if ((status[0] & MASK) == 0 && status[1] == 0 && status[2] == 0 && status[3] == 0)
			{
				status[0] = 'T';
				status[1] = 'I';
				status[2] = 'N';
				status[3] = 'Y';
			}
		}

		private void next_state()
		{
			uint y = status[3];
			uint x = (status[0] & MASK) ^ status[1] ^ status[2];
			x ^= (x << SH0);
			y ^= (y >> SH0) ^ x;
			status[0] = status[1];
			status[1] = status[2];
			status[2] = (x ^ (y << SH1));
			status[3] = (y);
			status[1] ^= (uint)(-(y & 1) & mat1);
			status[2] ^= (uint)(-(y & 1) & mat2);
		}

		private uint temper()
		{
			uint t0 = status[3];
			uint t1 = status[0] + (status[2] >> SH8);
			t0 ^= t1;
			t0 ^= (uint)(-(t1 & 1) & tmat);
			return t0;
		}

		private static uint ini_func1(uint x)
		{
			return (x ^ (x >> 27)) * 1664525;
		}

		private static uint ini_func2(uint x)
		{
			return (x ^ (x >> 27)) * 1566083941;
		}
		#endregion
	}
}
