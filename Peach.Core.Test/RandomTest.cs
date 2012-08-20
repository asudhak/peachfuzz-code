using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;

namespace Peach.Core.Test
{
	[TestFixture]
	class RandomTest
	{
		[Test]
		public void Test1()
		{
			// Ensure random numbers are determinstic for a given seed
			Random rand = new Random(0);

			Assert.AreEqual(1559595546, rand.Next());
			Assert.AreEqual(1755192844, rand.Next());
			Assert.AreEqual(1649316166, rand.Next());
			Assert.AreEqual(1198642031, rand.Next());
			Assert.AreEqual(442452829, rand.Next());
		}

		[Test]
		public void Test2()
		{
			// Ensure random numbers are determinstic for a given seed
			Random rand = new Random(1);

			Assert.AreNotEqual(1559595546, rand.Next());
			Assert.AreNotEqual(1755192844, rand.Next());
			Assert.AreNotEqual(1649316166, rand.Next());
			Assert.AreNotEqual(1198642031, rand.Next());
			Assert.AreNotEqual(442452829, rand.Next());
		}
	}
}
