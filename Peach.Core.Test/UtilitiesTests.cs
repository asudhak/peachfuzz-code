using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using Peach.Core;

namespace Peach.Core.Test
{
	[TestFixture]
	class UtilitiesTests
	{
		[Test]
		public void TestBadSlices()
		{
			// begin > end
			Assert.Throws<ArgumentOutOfRangeException>(delegate()
			{
				try
				{
					Utilities.SliceRange(1, 0, 1, 10);
				}
				catch (Exception ex)
				{
					Assert.True(ex.Message.Contains("Parameter name: begin"));
					throw;
				}
			});

			// curSlice > numSlices
			Assert.Throws<ArgumentOutOfRangeException>(delegate()
			{
				try
				{
					Utilities.SliceRange(0, 10, 5, 4);
				}
				catch (Exception ex)
				{
					Assert.True(ex.Message.Contains("Parameter name: curSlice"));
					throw;
				}
			});

			// curSlice = 0
			Assert.Throws<ArgumentOutOfRangeException>(delegate()
			{
				try
				{
					Utilities.SliceRange(0, 10, 0, 4);
				}
				catch (Exception ex)
				{
					Assert.True(ex.Message.Contains("Parameter name: curSlice"));
					throw;
				}
			});

			// numSlices > (end - begin + 1)
			Assert.Throws<ArgumentOutOfRangeException>(delegate()
			{
				try
				{
					Utilities.SliceRange(0, 10, 1, 14);
				}
				catch (Exception ex)
				{
					Assert.True(ex.Message.Contains("Parameter name: numSlices"));
					throw;
				}
			});
		}

		[Test]
		public void TestGoodSlices()
		{
			Tuple<uint, uint> ret;

			for (uint i = 1; i <= 10; ++i)
			{
				ret = Utilities.SliceRange(1, 10, i, 10);
				Assert.AreEqual(i, ret.Item1);
				Assert.AreEqual(i, ret.Item2);
			}

			ret = Utilities.SliceRange(1, 10, 1, 1);
			Assert.AreEqual(1, ret.Item1);
			Assert.AreEqual(10, ret.Item2);

			ret = Utilities.SliceRange(1, 10, 1, 2);
			Assert.AreEqual(1, ret.Item1);
			Assert.AreEqual(5, ret.Item2);

			ret = Utilities.SliceRange(1, 10, 2, 2);
			Assert.AreEqual(6, ret.Item1);
			Assert.AreEqual(10, ret.Item2);

			for (uint i = 1; i <= 9; ++i)
			{
				ret = Utilities.SliceRange(1, 10, i, 9);

				if (i == 9)
				{
					Assert.AreEqual(9, ret.Item1);
					Assert.AreEqual(10, ret.Item2);
				}
				else
				{
					Assert.AreEqual(i, ret.Item1);
					Assert.AreEqual(i, ret.Item2);
				}
			}

			ret = Utilities.SliceRange(1, uint.MaxValue, 1, 1);
			Assert.AreEqual(1, ret.Item1);
			Assert.AreEqual(uint.MaxValue, ret.Item2);

			ret = Utilities.SliceRange(1, uint.MaxValue, 1, 2);
			Assert.AreEqual(1, ret.Item1);
			Assert.AreEqual(uint.MaxValue / 2, ret.Item2);

			ret = Utilities.SliceRange(1, uint.MaxValue, 2, 2);
			Assert.AreEqual(uint.MaxValue / 2 + 1, ret.Item1);
			Assert.AreEqual(uint.MaxValue, ret.Item2);

			ret = Utilities.SliceRange(1, 5907588, 1, 38);
			Assert.AreEqual(1, ret.Item1);
			Assert.AreEqual(155462, ret.Item2);
			ret = Utilities.SliceRange(1, 5907588, 2, 38);
			Assert.AreEqual(155463, ret.Item1);
			Assert.AreEqual(310924, ret.Item2);
			ret = Utilities.SliceRange(1, 5907588, 37, 38);
			Assert.AreEqual(5596633, ret.Item1);
			Assert.AreEqual(5752094, ret.Item2);
			ret = Utilities.SliceRange(1, 5907588, 38, 38);
			Assert.AreEqual(5752095, ret.Item1);
			Assert.AreEqual(5907588, ret.Item2);
		}

		[Test]
		public void TestHexDump()
		{
			var output = new MemoryStream();
			var ms = new MemoryStream(Encoding.ASCII.GetBytes("0Hello World"));
			ms.Position = 1;
			Utilities.HexDump(ms, output);
			Assert.AreEqual(1, ms.Position);
			Assert.AreEqual(output.Position, output.Length);
			output.Seek(0, SeekOrigin.Begin);
			var str = Encoding.ASCII.GetString(output.GetBuffer(), 0, (int)output.Length);
			string expected = "00000000   48 65 6C 6C 6F 20 57 6F  72 6C 64                  Hello World     " + Environment.NewLine;
			Assert.AreEqual(expected, str);

			str = Utilities.HexDump(ms);
			Assert.AreEqual(1, ms.Position);
			Assert.AreEqual(expected, str);

		}
	}
}
