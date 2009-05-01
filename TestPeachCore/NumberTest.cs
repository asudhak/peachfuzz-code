using PeachCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace TestPeachCore
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

		public bool CompareByteArray(byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
				return false;

			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] != b[i])
					return false;
			}

			return true;
		}


		/// <summary>
		///A test for InternalValueToByteArray
		///</summary>
		[TestMethod()]
		[DeploymentItem("PeachCore.dll")]
		public void InternalValueToByteArrayTest8()
		{
			Number_Accessor target = new Number_Accessor(); // TODO: Initialize to an appropriate value
			Variant b = new Variant(100);
			byte[] expected = new byte[] { 0x64 };
			byte[] actual;
			target._size = 8;
			actual = target.InternalValueToByteArray(b);
			Assert.IsTrue(CompareByteArray(expected, actual));
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for InternalValueToByteArray
		///</summary>
		[TestMethod()]
		[DeploymentItem("PeachCore.dll")]
		public void InternalValueToByteArrayTest16()
		{
			Number_Accessor target = new Number_Accessor(); // TODO: Initialize to an appropriate value
			Variant b = new Variant(100);
			byte[] expected = new byte[] { 0x64 };
			byte[] actual;
			target._size = 8;
			actual = target.InternalValueToByteArray(b);
			Assert.IsTrue(CompareByteArray(expected, actual));
			Assert.Inconclusive("Verify the correctness of this test method.");
		}
		/// <summary>
		///A test for InternalValueToByteArray
		///</summary>
		[TestMethod()]
		[DeploymentItem("PeachCore.dll")]
		public void InternalValueToByteArrayTest24()
		{
			Number_Accessor target = new Number_Accessor(); // TODO: Initialize to an appropriate value
			Variant b = new Variant(100);
			byte[] expected = new byte[] { 0x64 };
			byte[] actual;
			target._size = 8;
			actual = target.InternalValueToByteArray(b);
			Assert.IsTrue(CompareByteArray(expected, actual));
			Assert.Inconclusive("Verify the correctness of this test method.");
		}
		/// <summary>
		///A test for InternalValueToByteArray
		///</summary>
		[TestMethod()]
		[DeploymentItem("PeachCore.dll")]
		public void InternalValueToByteArrayTest32()
		{
			Number_Accessor target = new Number_Accessor(); // TODO: Initialize to an appropriate value
			Variant b = new Variant(100);
			byte[] expected = new byte[] { 0x64 };
			byte[] actual;
			target._size = 8;
			actual = target.InternalValueToByteArray(b);
			Assert.IsTrue(CompareByteArray(expected, actual));
			Assert.Inconclusive("Verify the correctness of this test method.");
		}
		/// <summary>
		///A test for InternalValueToByteArray
		///</summary>
		[TestMethod()]
		[DeploymentItem("PeachCore.dll")]
		public void InternalValueToByteArrayTest64()
		{
			Number_Accessor target = new Number_Accessor(); // TODO: Initialize to an appropriate value
			Variant b = new Variant(100);
			byte[] expected = new byte[] { 0x64 };
			byte[] actual;
			target._size = 8;
			actual = target.InternalValueToByteArray(b);
			Assert.IsTrue(CompareByteArray(expected, actual));
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for Number Constructor
		///</summary>
		[TestMethod()]
		public void NumberConstructorTest()
		{
			Number target = new Number();
		}
	}
}
