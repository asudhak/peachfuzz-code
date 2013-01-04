using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;
using Peach.Core.Mutators;

namespace Peach.Core.Test.Mutators
{
	[TestFixture]
	class BlobMutatorTests : DataModelCollector
	{
		static string getXml(string strategy, int len, byte min = 0, byte max = 255, string hint = "")
		{
			string template = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Blob name=\"blob1\" length=\"{0}\" valueType=\"hex\" value=\"{1}\">" +
				"           {3}" +
				"       </Blob>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action type=\"output\">" +
				"               <DataModel ref=\"TheDataModel\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheState\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"       <Strategy class=\"{2}\"/>" +
				"   </Test>" +
				"</Peach>";

			StringBuilder sb = new StringBuilder();
			byte val = min;

			for (int i = 0; i < len; ++i)
			{
				sb.Append(val.ToString("X2"));
				sb.Append(" ");

				if (val == max)
					val = min;
				else
					++val;
			}

			if (hint.Length > 0)
				hint = string.Format("<Hint name=\"BlobMutator-How\" value=\"{0}\"/>", hint);

			return string.Format(template, len, sb.ToString(), strategy, hint);
		}


		private void RunSequential(int expected, string hint = "")
		{
			ResetContainers();

			// Test all variations are hit when using the sequential strategy
			string xml = getXml("Sequential", 4, 0, 255, hint);

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("BlobMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			// verify values
			Assert.AreEqual(expected, mutations.Count);

			foreach (var mutation in mutations)
			{
				Assert.AreEqual(Variant.VariantType.ByteString, mutation.GetVariantType());
			}
		}

		private void RunRandom(uint iterations, string hint, int len, byte val)
		{
			ResetContainers();

			// Test all variations are hit when using the sequential strategy
			string xml = getXml("Random", len, val, val, hint);

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("BlobMutator");

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 0;
			config.rangeStop = iterations;
			config.randomSeed = 12345;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			// verify values
			Assert.AreEqual(iterations, mutations.Count);
		}

		[Test]
		public void TestSequential()
		{
			RunSequential(Enum.GetValues(typeof(BlobMutator.How)).Length);
		}

		[Test]
		public void TestSequentialHints()
		{
			RunSequential(1, "NullRange");
			RunSequential(1, "ExpandZero");
			RunSequential(2, "ExpandZero,ExpandAllRandom");
			RunSequential(2, "ExpandAllRandom,UnNullRange");
			RunSequential(3, "ChangeRange,Reduce,RangeSpecial");
			Assert.Throws(typeof(PeachException), delegate() { RunSequential(0, "badhint"); }, "Unexpected value for Hint named: BlobMutator-How");
		}

		[Test]
		public void TestExpandSingleRandom()
		{
			RunRandom(2500, "ExpandSingleRandom", 4, 0);

			// Should expand by [0,255] bytes
			// When expanding, will stick len bytes somewhere in the blob
			// and populate with the same value

			int minLen = int.MaxValue;
			int maxLen = int.MinValue;
			int[] count = new int[256];

			foreach (var item in mutations)
			{
				Assert.AreEqual(Variant.VariantType.ByteString, item.GetVariantType());
				byte[] val = (byte[])item;

				if (val.Length < minLen)
					minLen = val.Length;
				if (val.Length > maxLen)
					maxLen = val.Length;

				int expanded = val.Length - 4;

				byte max = val.Max();
				byte min = val.Min();

				Assert.AreEqual(0, min);
				Assert.LessOrEqual(max, 256);

				count[max] += 1;

				if (max != 0)
				{
					int numNonZero = val.Count(n => n == max);
					Assert.AreEqual(expanded, numNonZero);
					int numZero = val.Count(n => n == 0);
					Assert.AreEqual(4, numZero);
				}
			}

			Assert.AreEqual(4, minLen);
			Assert.AreEqual(4 + 255, maxLen);

			int numMissed = count.Count(n => n == 0);
			Assert.AreEqual(0, numMissed);
		}

		[Test]
		public void TestExpandIncrementing()
		{
			RunRandom(100000, "ExpandIncrementing", 4, 0);

			// Should expand by len bytes [0,255]
			// When expanding, will stick len bytes somewhere in the blob
			// and populate with values incrementing from [x,255]
			// where x is a random number from [0,len]

			int minLen = int.MaxValue;
			int maxLen = int.MinValue;
			int[] minCount = new int[256];
			int[] maxCount = new int[256];

			foreach (var item in mutations)
			{
				Assert.AreEqual(Variant.VariantType.ByteString, item.GetVariantType());
				byte[] val = (byte[])item;

				if (val.Length < minLen)
					minLen = val.Length;
				if (val.Length > maxLen)
					maxLen = val.Length;

				int expanded = val.Length - 4;

				byte max = val.Max();
				byte min = (max == 0) ? (byte)0 : val.First(n => n != 0);

				if (min == 1 && (max - min + 1 == expanded - 1))
					min = 0;

				if (max == 0 && min == 0)
				{
					Assert.True(expanded == 0 || expanded == 1);
				}
				else
				{
					Assert.AreEqual(expanded, max - min + 1);
				}

				minCount[min] += 1;
				maxCount[max] += 1;

			}

			int numMinMissed = minCount.Count(n => n == 0);
			int numMaxMissed = maxCount.Count(n => n == 0);

			Assert.AreEqual(4, minLen);
			Assert.AreEqual(4 + 255, maxLen);
			Assert.LessOrEqual(numMinMissed, 1);
			Assert.AreEqual(0, numMaxMissed);
		}

		[Test]
		public void TestExpandZero()
		{
			RunRandom(2000, "ExpandZero", 4, 1);

			// Should expand by [0,255] bytes
			// When expanding, will stick len bytes somewhere in the blob
			// and populate with 0s

			int[] count = new int[256];

			foreach (var item in mutations)
			{
				Assert.AreEqual(Variant.VariantType.ByteString, item.GetVariantType());
				byte[] val = (byte[])item;

				int expanded = val.Length - 4;
				Assert.GreaterOrEqual(expanded, 0);
				Assert.LessOrEqual(expanded, 255);

				int nonZero = val.Count(n => n != 0);
				int zero = val.Count(n => n == 0);

				Assert.AreEqual(4, nonZero);
				Assert.AreEqual(expanded, zero);

				count[expanded] += 1;
			}

			int numMissed = count.Count(n => n == 0);
			Assert.AreEqual(0, numMissed);
		}

		[Test]
		public void TestExpandAllRandom()
		{
			RunRandom(2000, "ExpandAllRandom", 4, 1);

			// Should expand by [0,255] bytes
			// When expanding, will stick len bytes somewhere in the blob
			// and populate with random bytes

			int[] count = new int[256];
			int[] bytes = new int[256];

			foreach (var item in mutations)
			{
				Assert.AreEqual(Variant.VariantType.ByteString, item.GetVariantType());
				byte[] val = (byte[])item;

				int expanded = val.Length - 4;
				Assert.GreaterOrEqual(expanded, 0);
				Assert.LessOrEqual(expanded, 255);

				count[expanded] += 1;

				bytes[0] -= 4;
				foreach (byte b in val)
					bytes[b] += 1;

			}

			int numLenMissed = count.Count(n => n == 0);
			int numBytesMissed = bytes.Count(n => n == 0);
			Assert.AreEqual(0, numLenMissed);
			Assert.AreEqual(0, numBytesMissed);
		}

		[Test]
		public void TestReduce()
		{
			RunRandom(5000, "Reduce", 100, 1);

			// Should reduce by [0,len] bytes

			int[] count = new int[100];

			foreach (var item in mutations)
			{
				Assert.AreEqual(Variant.VariantType.ByteString, item.GetVariantType());
				byte[] val = (byte[])item;

				int reduced = 100 - val.Length;
				Assert.GreaterOrEqual(reduced, 0);
				Assert.LessOrEqual(reduced, 100);

				count[reduced] += 1;
			}

			int numMissed = count.Count(n => n == 0);
			Assert.AreEqual(0, numMissed);
		}

		[Test]
		public void TestChangeRange()
		{
			RunRandom(2000, "ChangeRange", 256, 0);

			// Should change a range of [0,100] bytes to a random value

			int[] bytes = new int[256];
			int[] count = new int[101];

			foreach (var item in mutations)
			{
				Assert.AreEqual(Variant.VariantType.ByteString, item.GetVariantType());
				byte[] val = (byte[])item;

				int nonZero = val.Count(n => n != 0);
				int numChanged = nonZero;

				Assert.GreaterOrEqual(numChanged, 0);
				Assert.LessOrEqual(numChanged, 100);

				count[numChanged] += 1;

				foreach (byte b in val)
					bytes[b] += 1;
			}

			int numLenMissed = count.Count(n => n == 0);
			int numBytesMissed = bytes.Count(n => n == 0);
			Assert.AreEqual(0, numLenMissed);
			Assert.AreEqual(0, numBytesMissed);
		}

		[Test]
		public void TestChangeRangeSpecial()
		{
			RunRandom(2000, "RangeSpecial", 256, 0xaa);

			// Should change a range of [0,100] bytes to a random value
			// from 0x00, 0x01, 0xfe, 0xff

			int[] vals = new int[5];
			int[] count = new int[101];

			foreach (var item in mutations)
			{
				Assert.AreEqual(Variant.VariantType.ByteString, item.GetVariantType());
				byte[] val = (byte[])item;

				int num00 = val.Count(n => n == 0x00);
				int num01 = val.Count(n => n == 0x01);
				int numFE = val.Count(n => n == 0xfe);
				int numFF = val.Count(n => n == 0xff);
				int numAA = val.Count(n => n == 0xaa);

				int numChanged = num00 + num01 + numFE + numFF;

				Assert.GreaterOrEqual(numChanged, 0);
				Assert.LessOrEqual(numChanged, 100);

				count[numChanged] += 1;

				vals[0] += num00;
				vals[1] += num01;
				vals[2] += numFE;
				vals[3] += numFF;
				vals[4] += numAA;
			}

			int numLenMissed = count.Count(n => n == 0);
			int numValsMissed = vals.Count(n => n == 0);
			Assert.AreEqual(0, numLenMissed);
			Assert.AreEqual(0, numValsMissed);
		}

		[Test]
		public void TestNullRange()
		{
			RunRandom(2000, "NullRange", 256, 0xaa);

			// Should change a range of [0,100] bytes to 0x00

			int[] count = new int[101];

			foreach (var item in mutations)
			{
				Assert.AreEqual(Variant.VariantType.ByteString, item.GetVariantType());
				byte[] val = (byte[])item;

				int changed = val.Count(n => n == 0x00);
				int unchanged = val.Count(n => n == 0xaa);

				Assert.AreEqual(val.Length, changed + unchanged);

				Assert.GreaterOrEqual(changed, 0);
				Assert.LessOrEqual(changed, 100);

				count[changed] += 1;
			}

			int numLenMissed = count.Count(n => n == 0);
			Assert.AreEqual(0, numLenMissed);
		}

		[Test]
		public void TestUnNullRange()
		{
			RunRandom(2000, "UnNullRange", 256, 0);

			// Should change a range of [0,100] bytes to values between [1,255]

			int[] count = new int[101];
			int[] bytes = new int[256];

			foreach (var item in mutations)
			{
				Assert.AreEqual(Variant.VariantType.ByteString, item.GetVariantType());
				byte[] val = (byte[])item;

				int changed = val.Count(n => n != 0);
				int unchanged = val.Count(n => n == 0);

				Assert.AreEqual(val.Length, changed + unchanged);

				Assert.GreaterOrEqual(changed, 0);
				Assert.LessOrEqual(changed, 100);

				count[changed] += 1;

				bytes[0] -= unchanged;
				foreach (byte b in val)
					bytes[b] += 1;
			}

			int numLenMissed = count.Count(n => n == 0);
			int numValsMissed = bytes.Count(n => n == 0);
			Assert.AreEqual(0, numLenMissed);
			Assert.AreEqual(0, bytes[0]);
			Assert.AreEqual(1, numValsMissed);
		}
	}
}

// end
