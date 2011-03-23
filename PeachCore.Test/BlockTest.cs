using Peach.Core.Dom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Peach.Core.Test
{
    
    
    /// <summary>
    ///This is a test class for BlockTest and is intended
    ///to contain all BlockTest Unit Tests
    ///</summary>
	[TestClass()]
	public class BlockTest
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
			// TODO: Verify this is actually correct :)
			byte[] correctValue = {
				0x53,	0x74,	0x72,	0x69,
				0x6e,	0x67,	0x20,	0x31,
				0x53,	0x74,	0x72,	0x69,
				0x6e,	0x67,	0x20,	0x32,
				0x02,	0x01,	0x03,	0x53,
				0x74,	0x72,	0x69,	0x6e,
				0x67,	0x20,	0x33
			   };
			byte[] correctValue2 = {
				0x53,	0x74,	0x72,	0x69,
				0x6e,	0x67,	0x20,	0x31,
				0x53,	0x74,	0x72,	0x69,
				0x6e,	0x67,	0x20,	0x32,
				0x09,	0x01,	0x03,	0x53,
				0x74,	0x72,	0x69,	0x6e,
				0x67,	0x20,	0x33
			   };

			Block block = new Block();
			Block subBlock1 = new Block();
			Block subBlock2 = new Block();
			String str1 = new String();
			String str2 = new String();
			String str3 = new String();
			Number num1 = new Number();
			Number num2 = new Number();
			Number num3 = new Number();

			str1.DefaultValue = new Variant("String 1");
			str2.DefaultValue = new Variant("String 2");
			str3.DefaultValue = new Variant("String 3");
			num1.DefaultValue = new Variant(1);
			num2.DefaultValue = new Variant(2);
			num3.DefaultValue = new Variant(3);

			block.Add(str1);
			block.Add(subBlock1);
			subBlock1.Add(str2);
			subBlock1.Add(num2);
			block.Add(num1);
			block.Add(subBlock2);
			subBlock2.Add(num3);
			subBlock2.Add(str3);

			BitStream bits = block.Value;

			bits.SeekToDataElement(num1);
			Assert.IsTrue(bits.ReadSByte() == 1, "num1 readback failed");
			bits.SeekToDataElement(num2);
			Assert.IsTrue(bits.ReadSByte() == 2, "num2 readback failed");
			bits.SeekToDataElement(num3);
			Assert.IsTrue(bits.ReadSByte() == 3, "num3 readback failed");

			Assert.IsTrue(ByteArrayCompare(correctValue, bits.Value), "correctValue != bits.value");

			// Now lets verify that Invalidation is working

			num2.DefaultValue = new Variant(9);
			Assert.IsTrue(ByteArrayCompare(correctValue2, bits.Value), "correctValue2 != bits.value");
		}

		bool ByteArrayCompare(byte[] a1, byte[] a2)
		{
			if (a1.Length != a2.Length)
				return false;

			for (int i = 0; i < a1.Length; i++)
				if (a1[i] != a2[i])
					return false;

			return true;
		}

		/// <summary>
		///A test for GenerateInternalValue
		///</summary>
		[TestMethod()]
		public void GenerateInternalValueTest()
		{
			Block target = new Block(); // TODO: Initialize to an appropriate value
			Variant expected = null; // TODO: Initialize to an appropriate value
			Variant actual;
			actual = target.GenerateInternalValue();
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for Block Constructor
		///</summary>
		[TestMethod()]
		public void BlockConstructorTest()
		{
			Block target = new Block();
			Assert.Inconclusive("TODO: Implement code to verify target");
		}
	}
}
