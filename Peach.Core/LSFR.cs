using System;

namespace Peach.Core
{
	/// <summary>
	/// Linear Shift Feedback Register
	/// Produces a pseudo random sequence of numbers from 1 to (2^N)-1
	/// </summary>
	public class LSFR
	{
		private uint value;
		private int[] taps;
		private int degree;

		public LSFR(int degree, uint init, int[] taps)
		{
			if (degree <= 1 || degree > 32)
				throw new ArgumentOutOfRangeException("degree");

			foreach (int i in taps)
			{
				if (i > degree)
					throw new ArgumentOutOfRangeException("taps");
			}

			this.degree = degree;
			this.value = init;
			this.taps = polynomials[degree];
		}

		public LSFR(int degree, uint init)
		{
			if (degree <= 1 || degree > 32)
				throw new ArgumentOutOfRangeException("degree");

			this.degree = degree;
			this.value = init;
			this.taps = polynomials[degree];
		}

		public uint Next()
		{
			// If the degree is 5, and there are taps at 5, 3
			// bit = ((value >> 0) ^ (value >> 2)) & 1
			// value = (value >> 1) | (bit << 4)

			uint bit = value >> (degree - taps[0]);
			for (int i = 1; i < taps.Length; ++i)
				bit ^= (value >> (degree - taps[i]));
			bit &= 1;
			value = (value >> 1) | (bit << (degree - 1));
			return value;
		}

		// http://www.xilinx.com/support/documentation/application_notes/xapp052.pdf
		private static int[][] polynomials = {
			null,
			null,
			new int[] { 2, 1 },
			new int[] { 3, 2 },
			new int[] { 4, 3 },
			new int[] { 5, 3 },
			new int[] { 6, 5 },
			new int[] { 7, 6 },
			new int[] { 8, 6, 5, 4 },
			new int[] { 9, 5 },
			new int[] { 10, 7 },
			new int[] { 11, 9 },
			new int[] { 12, 6, 4, 1 },
			new int[] { 13, 4, 3, 1 },
			new int[] { 14, 5, 3, 1 },
			new int[] { 15, 14 },
			new int[] { 16, 15, 13, 4 },
			new int[] { 17, 14 },
			new int[] { 18, 11 },
			new int[] { 19, 6, 2, 1 },
			new int[] { 20, 17 },
			new int[] { 21, 19 },
			new int[] { 22, 21 },
			new int[] { 23, 18 },
			new int[] { 24, 23, 22, 17 },
			new int[] { 25, 22 },
			new int[] { 26, 6, 2, 1 },
			new int[] { 27, 5, 2, 1 },
			new int[] { 28, 25 },
			new int[] { 29, 27 },
			new int[] { 30, 6, 4, 1 },
			new int[] { 31, 28 },
			new int[] { 32, 22, 2, 1 },
											};
	}

	/// <summary>
	/// Produces a pseudo random sequence of numbers from [1,max]
	/// </summary>
	public class SequenceGenerator
	{
		uint index;
		uint value;
		uint max;
		LSFR generator;

		public SequenceGenerator(uint max)
		{
			if (max == 0)
				throw new ArgumentOutOfRangeException("max");

			this.max = max;
			Init();
		}

		void Init()
		{
			this.index = 0;
			this.value = 0;

			if (max > 1)
				this.generator = new LSFR(GetDegree(max), max);
		}

		int GetDegree(uint value)
		{
			for (int i = 0; i < 32; ++i)
			{
				if ((value & 0x80000000) != 0)
					return (32 - i);
				value <<= 1;
			}
			return 0;
		}

		public uint Get(uint index)
		{
			if (index > max)
				throw new ArgumentOutOfRangeException("index");

			if (index == max)
			{
				this.index = index;
				value = max;
				return value;
			}

			if (index < this.index)
				Init();

			for (uint i = this.index; i < index; ++i)
			{
				do
				{
					value = generator.Next();
				}
				while (value > max);

				if (i == uint.MaxValue)
					break;
			}

			this.index = index;
			return value;
		}

		public uint Value
		{
			get
			{
				return value;
			}
		}
	}
}
