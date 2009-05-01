using PeachCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace TestPeachCore
{
    
    
    /// <summary>
    ///This is a test class for StringTest and is intended
    ///to contain all StringTest Unit Tests
    ///</summary>
	[TestClass()]
	public class StringTest
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
		public void InternalValueToByteArrayTest()
		{
			String_Accessor target = new String_Accessor(); // TODO: Initialize to an appropriate value
			Variant v = new Variant("Hello World!"); // TODO: Initialize to an appropriate value
			byte[] expected = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 
				0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x21 };
			byte[] actual;
			target._type = StringType.Ascii;
			actual = target.InternalValueToByteArray(v);
			Assert.IsTrue(CompareByteArray(expected, actual));
		}

		/// <summary>
		///A test for String Constructor
		///</summary>
		[TestMethod()]
		public void StringConstructorTest()
		{
			String target = new String();
		}
	}
}
