using Peach.Core.Dom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Peach.Core.Dom;
using System.Reflection;
using System.Collections.Generic;

namespace Peach.Core.Test
{
    
    
    /// <summary>
    ///This is a test class for DataElementTest and is intended
    ///to contain all DataElementTest Unit Tests
    ///</summary>
	[TestClass()]
	public class DataElementTest
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
		///A test for Value
		///</summary>
		[TestMethod()]
		public void ValueTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			BitStream actual;
			actual = target.Value;
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for transformer
		///</summary>
		[TestMethod()]
		public void transformerTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			Transformer expected = null; // TODO: Initialize to an appropriate value
			Transformer actual;
			target.transformer = expected;
			actual = target.transformer;
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for relations
		///</summary>
		[TestMethod()]
		public void relationsTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for parent
		///</summary>
		[TestMethod()]
		public void parentTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			DataElementContainer expected = null; // TODO: Initialize to an appropriate value
			DataElementContainer actual;
			target.parent = expected;
			actual = target.parent;
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for MutatedValue
		///</summary>
		[TestMethod()]
		public void MutatedValueTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			Variant expected = null; // TODO: Initialize to an appropriate value
			Variant actual;
			target.MutatedValue = expected;
			actual = target.MutatedValue;
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for isLeafNode
		///</summary>
		[TestMethod()]
		public void isLeafNodeTest()
		{
			Block target = new Block();
			Block t1 = new Block();
			Block t2 = new Block();

			Assert.IsTrue(target.isLeafNode, "Target initially");

			target.Add(t1);
			Assert.IsFalse(target.isLeafNode, "target with single child");
			Assert.IsTrue(t1.isLeafNode, "targets child t1");

			t2.Add(target);
			Assert.IsFalse(t2.isLeafNode, "t2 with target as child");
			Assert.IsFalse(target.isLeafNode, "target as child of t2 with child t1");
			Assert.IsTrue(t1.isLeafNode, "t1 as child to target child to t2");

			target.Remove(t1);
			Assert.IsFalse(t2.isLeafNode, "t2 after removing t1 from target");
			Assert.IsTrue(target.isLeafNode, "target after removing t1, parent t2");
			Assert.IsTrue(t1.isLeafNode, "t1 removed from target");
		}

		/// <summary>
		///A test for InternalValue
		///</summary>
		[TestMethod()]
		public void InternalValueTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			Variant actual;
			actual = target.InternalValue;
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for fullName
		///</summary>
		[TestMethod()]
		public void fullNameTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			string actual;
			actual = target.fullName;
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for fixup
		///</summary>
		[TestMethod()]
		public void fixupTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			Fixup expected = null; // TODO: Initialize to an appropriate value
			Fixup actual;
			target.fixup = expected;
			actual = target.fixup;
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for DefaultValue
		///</summary>
		[TestMethod()]
		public void DefaultValueTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			Variant expected = null; // TODO: Initialize to an appropriate value
			Variant actual;
			target.DefaultValue = expected;
			actual = target.DefaultValue;
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for previousSibling
		///</summary>
		[TestMethod()]
		public void previousSiblingTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			DataElement expected = null; // TODO: Initialize to an appropriate value
			DataElement actual;
			actual = target.previousSibling();
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for OnMutatedValueChanged
		///</summary>
		[TestMethod()]
		[DeploymentItem("PeachCore.dll")]
		public void OnMutatedValueChangedTest()
		{
			// Creation of the private accessor for 'Microsoft.VisualStudio.TestTools.TypesAndSymbols.Assembly' failed
			Assert.Inconclusive("Creation of the private accessor for \'Microsoft.VisualStudio.TestTools.TypesAndSy" +
					"mbols.Assembly\' failed");
		}

		/// <summary>
		///A test for OnInvalidated
		///</summary>
		[TestMethod()]
		[DeploymentItem("PeachCore.dll")]
		public void OnInvalidatedTest()
		{
			// Creation of the private accessor for 'Microsoft.VisualStudio.TestTools.TypesAndSymbols.Assembly' failed
			Assert.Inconclusive("Creation of the private accessor for \'Microsoft.VisualStudio.TestTools.TypesAndSy" +
					"mbols.Assembly\' failed");
		}

		/// <summary>
		///A test for OnDefaultValueChanged
		///</summary>
		[TestMethod()]
		[DeploymentItem("PeachCore.dll")]
		public void OnDefaultValueChangedTest()
		{
			// Creation of the private accessor for 'Microsoft.VisualStudio.TestTools.TypesAndSymbols.Assembly' failed
			Assert.Inconclusive("Creation of the private accessor for \'Microsoft.VisualStudio.TestTools.TypesAndSy" +
					"mbols.Assembly\' failed");
		}

		/// <summary>
		///A test for nextSibling
		///</summary>
		[TestMethod()]
		public void nextSiblingTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			DataElement expected = null; // TODO: Initialize to an appropriate value
			DataElement actual;
			actual = target.nextSibling();
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for loadDataElements
		///</summary>
		[TestMethod()]
		public void loadDataElementsTest()
		{
			Assembly assembly = null; // TODO: Initialize to an appropriate value
			DataElement.loadDataElements(assembly);
			Assert.Inconclusive("A method that does not return a value cannot be verified.");
		}

		/// <summary>
		///A test for Invalidate
		///</summary>
		[TestMethod()]
		public void InvalidateTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			target.Invalidate();
			Assert.Inconclusive("A method that does not return a value cannot be verified.");
		}

		/// <summary>
		///A test for InternalValueToByteArray
		///</summary>
		[TestMethod()]
		[DeploymentItem("PeachCore.dll")]
		public void InternalValueToByteArrayTest()
		{
			// Creation of the private accessor for 'Microsoft.VisualStudio.TestTools.TypesAndSymbols.Assembly' failed
			Assert.Inconclusive("Creation of the private accessor for \'Microsoft.VisualStudio.TestTools.TypesAndSy" +
					"mbols.Assembly\' failed");
		}

		/// <summary>
		///A test for getRoot
		///</summary>
		[TestMethod()]
		public void getRootTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			DataElement expected = null; // TODO: Initialize to an appropriate value
			DataElement actual;
			actual = target.getRoot();
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for GenerateValue
		///</summary>
		[TestMethod()]
		public void GenerateValueTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			BitStream expected = null; // TODO: Initialize to an appropriate value
			BitStream actual;
			actual = target.GenerateValue();
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		/// <summary>
		///A test for GenerateInternalValue
		///</summary>
		[TestMethod()]
		public void GenerateInternalValueTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			Variant expected = null; // TODO: Initialize to an appropriate value
			Variant actual;
			actual = target.GenerateInternalValue();
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}

		class MyDataElement : DataElement
		{
		}

		internal virtual DataElement CreateDataElement()
		{
			// TODO: Instantiate an appropriate concrete class.
			DataElement target = new MyDataElement();
			return target;
		}

		/// <summary>
		///A test for find
		///</summary>
		[TestMethod()]
		public void findTest()
		{
			DataElement target = CreateDataElement(); // TODO: Initialize to an appropriate value
			string name = string.Empty; // TODO: Initialize to an appropriate value
			DataElement expected = null; // TODO: Initialize to an appropriate value
			DataElement actual;
			actual = target.find(name);
			Assert.AreEqual(expected, actual);
			Assert.Inconclusive("Verify the correctness of this test method.");
		}
	}
}
