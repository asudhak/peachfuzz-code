using Peach.Core.Dom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Peach.Core.Test
{
    
    
    /// <summary>
    ///This is a test class for DataElementContainerTest and is intended
    ///to contain all DataElementContainerTest Unit Tests
    ///</summary>
	[TestClass()]
	public class DataElementContainerTest
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


		/// <summary>
		///A test for Item
		///</summary>
		[TestMethod()]
		public void ItemIndexTest()
		{
			Block block = new Block();
			String str1 = new String();
			String str2 = new String();
			String str3 = new String();

			block.Add(str1);
			block.Add(str2);
			block.Add(str3);

			Assert.IsTrue(str1 == block[0], "block[0] == str1 failed");
			Assert.IsTrue(str2 == block[1], "block[1] == str2 failed");
			Assert.IsTrue(str3 == block[2], "block[2] == str3 failed");

			block[str2.name] = str2;

			Assert.IsTrue(str1 == block[0], "block[0] == str1 failed");
			Assert.IsTrue(str2 == block[1], "block[1] == str2 failed");
			Assert.IsTrue(str3 == block[2], "block[2] == str3 failed");
		}

		/// <summary>
		///A test for Item
		///</summary>
		[TestMethod()]
		public void ItemKeyTest()
		{
			Block block = new Block();
			String str1 = new String();
			String str2 = new String();
			String str3 = new String();

			block.Add(str1);
			block.Add(str2);
			block.Add(str3);

			Assert.IsTrue(str1 == block[str1.name], "block[str1.name] == str1 failed");
			Assert.IsTrue(str2 == block[str2.name], "block[str2.name] == str2 failed");
			Assert.IsTrue(str3 == block[str3.name], "block[str3.name] == str3 failed");

			block[str2.name] = str2;

			Assert.IsTrue(str1 == block[str1.name], "block[str1.name] == str1 failed");
			Assert.IsTrue(str2 == block[str2.name], "block[str2.name] == str2 failed");
			Assert.IsTrue(str3 == block[str3.name], "block[str3.name] == str3 failed");
		}

		/// <summary>
		///A test for isLeafNode
		///</summary>
		[TestMethod()]
		public void isLeafNodeTest()
		{
			Block block = new Block();
			Block b1 = new Block();

			Assert.IsTrue(block.isLeafNode);
			Assert.IsTrue(b1.isLeafNode);

			block.Add(b1);

			Assert.IsFalse(block.isLeafNode);
			Assert.IsTrue(b1.isLeafNode);

			block.Remove(b1);

			Assert.IsTrue(block.isLeafNode);
			Assert.IsTrue(b1.isLeafNode);
		}

		/// <summary>
		///A test for Count
		///</summary>
		[TestMethod()]
		public void CountTest()
		{
			Block block = new Block();
			String str1 = new String();
			String str2 = new String();
			String str3 = new String();

			block.Add(str1);
			block.Add(str2);
			block.Add(str3);

			Assert.IsTrue(block.Count == 3);
		}

		/// <summary>
		///A test for RemoveAt
		///</summary>
		[TestMethod()]
		public void RemoveAtTest()
		{
			Assert.Inconclusive("A method that does not return a value cannot be verified.");
		}

		/// <summary>
		///A test for Remove
		///</summary>
		[TestMethod()]
		public void RemoveTest()
		{
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for Insert
		///</summary>
		[TestMethod()]
		public void InsertTest()
		{
			Assert.Inconclusive("A method that does not return a value cannot be verified.");
		}

		/// <summary>
		///A test for IndexOf
		///</summary>
		[TestMethod()]
		public void IndexOfTest()
		{
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for Contains
		///</summary>
		[TestMethod()]
		public void ContainsTest()
		{
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for Clear
		///</summary>
		[TestMethod()]
		public void ClearTest()
		{
			Assert.Inconclusive("A method that does not return a value cannot be verified.");
		}

		/// <summary>
		///A test for Add
		///</summary>
		[TestMethod()]
		public void AddTest()
		{
			Assert.Inconclusive("A method that does not return a value cannot be verified.");
		}
	}
}
