using System;
using Peach.Core.Dom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Peach.Core.Test
{
    
    
    /// <summary>
    ///This is a test class for NumberTest and is intended
    ///to contain all NumberTest Unit Tests
    ///</summary>
	[TestClass()]
	public class NumberTest
	{


		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion

		[TestMethod]
		public void SmokeTest()
		{
			Number num = new Number();
			Assert.IsTrue(num.Size == 8);
			Assert.IsTrue(num.Signed);
			Assert.IsTrue(num.LittleEndian);
			Assert.IsTrue(num.MaxValue == 127);
			Assert.IsTrue(num.MinValue == -128);

			num.DefaultValue = new Variant(100);
			Assert.IsTrue((int)num.DefaultValue == 100);
			Assert.IsTrue((int)num.InternalValue == 100);
			Assert.IsTrue(Utils.ByteArrayCompare(num.Value.Value,
				new byte[] { 100 }));

			num.Signed = false;
			Assert.IsTrue(num.MaxValue == 255);
			Assert.IsTrue(num.MinValue == 0);
			
			num.Size = 3;
			Assert.IsTrue(num.MaxValue == 7);
			Assert.IsTrue(num.MinValue == 0);

			try
			{
				num.DefaultValue = new Variant(200);
				Assert.IsTrue(false, "Able to set larger number than MaxValue");
			}
			catch (ApplicationException e)
			{
			}

			try
			{
				num.DefaultValue = new Variant(-12);
				Assert.IsTrue(false, "Able to set smaller # than MinValue");
			}
			catch (ApplicationException e)
			{
			}
		}

		/// <summary>
		///A test for Size
		///</summary>
		[TestMethod()]
		public void SizeTest()
		{
			Number target = new Number(); // TODO: Initialize to an appropriate value
			uint expected = 0; // TODO: Initialize to an appropriate value
			uint actual;
			target.Size = expected;
			actual = target.Size;
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for InternalValueToBitStream
		///</summary>
		[TestMethod()]
		[DeploymentItem("PeachCore.dll")]
		public void InternalValueToBitStreamTest()
		{
			// Creation of the private accessor for 'Microsoft.VisualStudio.TestTools.TypesAndSymbols.Assembly' failed
			Assert.Inconclusive("Creation of the private accessor for \'Microsoft.VisualStudio.TestTools.TypesAndSy" +
					"mbols.Assembly\' failed");
		}
	}
}
