using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;

namespace Peach.Core.Test
{
	[TestFixture]
	class RandomTest
	{
		// Precomputed values for a seed of 0
		static uint[] precomp =
		{
			2081790247,
			3105921834,
			760524185,
			303856848,
			2371835568,
			713149915,
			1499016781,
			3619796040,
			2298896773,
			1125491363,
		};

		[Test]
		public void Test1()
		{
			// Ensure random numbers are determinstic for a given seed
			Random rand = new Random(0);

			foreach (var exp in precomp)
			{
				var val = rand.NextUInt32();
				Assert.AreEqual(exp, val);
			}

			rand = new Random(0);

			foreach (var exp in precomp)
			{
				var val = rand.NextUInt32();
				Assert.AreEqual(exp, val);
			}
		}

		[Test]
		public void Test2()
		{
			// Ensure random numbers are determinstic for a given seed
			Random rand = new Random(1);

			uint[] res = new uint[precomp.Length];

			for (int i = 0; i < precomp.Length; ++i)
			{
				res[i] = rand.NextUInt32();
				Assert.AreNotEqual(precomp[i], res[i]);
			}

			rand = new Random(1);

			for (int i = 0; i < precomp.Length; ++i)
			{
				var val = rand.NextUInt32();
				Assert.AreEqual(res[i], val);
			}
		}

		[Test]
		public void Test3()
		{
			// Based on check32.c and check32.out.txt

			TinyMT32 prng = new TinyMT32(1);

			uint[] init =
			{
				2545341989, 981918433 , 3715302833, 2387538352, 3591001365,
				3820442102, 2114400566, 2196103051, 2783359912,  764534509,
				 643179475, 1822416315,  881558334, 4207026366, 3690273640,
				3240535687, 2921447122, 3984931427, 4092394160,   44209675,
				2188315343, 2908663843, 1834519336, 3774670961, 3019990707,
				4065554902, 1239765502, 4035716197, 3412127188,  552822483,
				 161364450,  353727785,  140085994,  149132008, 2547770827,
				4064042525, 4078297538, 2057335507,  622384752, 2041665899,
				2193913817, 1080849512,   33160901,  662956935,  642999063,
				3384709977, 1723175122, 3866752252,  521822317, 2292524454,
			};

			foreach (var exp in init)
			{
				var val = prng.GenerateUInt();
				Assert.AreEqual(exp, val);
			}

			prng = new TinyMT32(new uint[] { 1 });

			string[] init_by_array =
			{
				"0.0132459", "0.2083900", "0.1457998", "0.1144078", "0.6173239",
				"0.0522397", "0.9873815", "0.1503185", "0.4039059", "0.6909349",
				"0.0908061", "0.0637298", "0.5002119", "0.1056944", "0.0936889",
				"0.0609042", "0.0725737", "0.7802557", "0.8761557", "0.5714423",
				"0.1706455", "0.4046336", "0.4131218", "0.2825145", "0.8249400",
				"0.4180386", "0.2152816", "0.4346161", "0.4916836", "0.5997445",
				"0.9118823", "0.1928336", "0.7523277", "0.9890286", "0.7421532",
				"0.9053972", "0.3542483", "0.9161059", "0.1209783", "0.8205475",
				"0.8592416", "0.8379903", "0.6638085", "0.8796422", "0.8608698",
				"0.9255103", "0.6475282", "0.7260163", "0.8757524", "0.0845953",
			};

			foreach (var exp in init_by_array)
			{
				var val = prng.Sample();
				Assert.AreEqual(exp, val.ToString("0.0000000"));
			}
		}

		[Test]
		public void Test4()
		{
			var rng = new Random(1);

			int[] vals = new int[13];

			for (int i = 0; i < 1000000; ++i)
			{
				int ret = rng.Next(13);
				Assert.GreaterOrEqual(ret, 0);
				Assert.Less(ret, 13);
				vals[ret] += 1;
			}

			// find the average
			double avg = vals.Average();
			// sum the square of the delta from the mean
			double sum = vals.Sum(d => Math.Pow(d - avg, 2));
			// compute stddev
			double stddev = Math.Sqrt(sum / (vals.Count()));

			// 1000000 samples, 13 buckets = 76923 per bucket
			// Allow +/- 300 per bucket for a small percentage of play
			Assert.GreaterOrEqual(stddev, 0.0);
			Assert.Less(stddev, 300.0);
		}

		[Test]
		public void Test5()
		{
			// Test that -100 <= X < 100
			var rng = new Random(1);

			int[] vals = new int[200];

			for (int i = 0; i < 1000000; ++i)
			{
				int ret = rng.Next(-100, 100);
				Assert.GreaterOrEqual(ret, -100);
				Assert.Less(ret, 100);
				vals[ret + 100] += 1;
			}

			foreach (int val in vals)
			{
				Assert.AreNotEqual(0, val);
			}
		}

		[Test]
		public void TestSmallRange()
		{
			// Test that 0 <= X < 4
			var rng = new Random(277208);

			int[] vals = new int[4];

			for (int i = 0; i < 1000; ++i)
			{
				int ret = rng.Next(0, 4);
				Assert.GreaterOrEqual(ret, 0);
				Assert.Less(ret, 4);
				vals[ret] += 1;
			}

			foreach (int val in vals)
			{
				Assert.AreNotEqual(0, val);
			}
		}

		void RunLSFR(int degree)
		{
			SortedSet<uint> results = new SortedSet<uint>();

			long end = ((long)1 << degree) - 1;
			var g = new LSFR(degree, 1);

			for (long i = 0; i < end; ++i)
			{
				uint val = g.Next();
				bool added = results.Add(val);
				if (!added)
					Assert.Fail("'{0}' already added. Iteration: {1}, Degree: {2}", val, i, degree);
			}
		}

		[Test]
		public void TestLSFR()
		{
			for (int i = 2; i <= 20; ++i)
				RunLSFR(i);
		}

		public void DoTestSequence(uint max)
		{
			List<uint> vals = new List<uint>();
			Dictionary<uint, object> pass1 = new Dictionary<uint, object>();
			Dictionary<uint, object> pass2 = new Dictionary<uint, object>();

			SequenceGenerator g = new SequenceGenerator(max);
			for (uint i = 1; i <= max; ++i)
			{
				uint val = g.Get(i);
				vals.Add(val);
				if (pass1.ContainsKey(val))
					Assert.Fail("Duplicate on iteration {0} of max {1}".Fmt(i, max));
				pass1.Add(val, null);
			}

			for (uint i = 1; i <= max; ++i)
			{
				uint val = g.Get(vals[(int)(i-1)]);
				if (pass2.ContainsKey(val))
					Assert.Fail("Duplicate on jumping iteration {0} ({1}) of max {2}".Fmt(i, vals[(int)(i-1)], max));
				pass2.Add(val, null);
			}

			Assert.AreEqual(pass1.Count, pass2.Count);
		}

		[Test]
		public void TestSequence()
		{
			for (uint i = 1; i < 1000; ++i)
				DoTestSequence(i);
		}

		[Test]
		public void TestRandomSequence()
		{
			uint max = (uint)new Random((uint)Environment.TickCount).Next(1000, 10000);
			DoTestSequence(max);
		}
	}
}
