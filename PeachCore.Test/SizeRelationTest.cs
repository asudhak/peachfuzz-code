using System.Text;
using PeachCore.Dom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace PeachCore.Test
{
    
    
    /// <summary>
    ///This is a test class for SizeRelationTest and is intended
    ///to contain all SizeRelationTest Unit Tests
    ///</summary>
	[TestClass()]
	public class SizeRelationTest
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
		public void ExpressionGetTest()
		{
			Block left = new Block();
			Block right = new Block();

			left.Add(new String("TheSize"));
			right.Add(new String("Sizer"));

			left["TheSize"].DefaultValue = new Variant("666");
			right["Sizer"].DefaultValue = new Variant("Hello World");

			string valueLeft = ASCIIEncoding.ASCII.GetString((byte[])left.InternalValue);
			string valueRight = ASCIIEncoding.ASCII.GetString((byte[])right.InternalValue);

			left["TheSize"].relations.Add(new SizeRelation());
			left["TheSize"].relations[0].Of = right["Sizer"];
			left["TheSize"].relations[0].ExpressionGet = "size + 1";

			valueLeft = ASCIIEncoding.ASCII.GetString((byte[])left.InternalValue);
			Assert.IsTrue(int.Parse(valueLeft) == "Hello World".Length+1, "left.Value == Length+1");

			// Change of size
			right["Sizer"].DefaultValue = new Variant("1234567890");

			valueLeft = ASCIIEncoding.ASCII.GetString((byte[])left.InternalValue);

			Assert.IsTrue(int.Parse(valueLeft) == "1234567890".Length+1, "left.Value == Length+1");
		}

		[TestMethod]
		public void InvalidationTest()
		{
			Block left = new Block();
			Block right = new Block();

			left.Add(new String("TheSize"));
			right.Add(new String("Sizer"));

			left["TheSize"].DefaultValue = new Variant("666");
			right["Sizer"].DefaultValue = new Variant("Hello World");

			string valueLeft = ASCIIEncoding.ASCII.GetString((byte[]) left.InternalValue);
			string valueRight = ASCIIEncoding.ASCII.GetString((byte[]) right.InternalValue);

			left["TheSize"].relations.Add(new SizeRelation());
			left["TheSize"].relations[0].Of = right["Sizer"];

			valueLeft = ASCIIEncoding.ASCII.GetString((byte[]) left.InternalValue);
			valueRight = ASCIIEncoding.ASCII.GetString((byte[]) right.InternalValue);
			Assert.IsTrue(int.Parse(valueLeft) == "Hello World".Length, "left.Value == Length");
			Assert.IsTrue((string)valueRight == "Hello World", "right.Value == 'Hello World'");

			// Change of size
			right["Sizer"].DefaultValue = new Variant("1234567890");

			valueLeft = ASCIIEncoding.ASCII.GetString((byte[]) left.InternalValue);
			valueRight = ASCIIEncoding.ASCII.GetString((byte[]) right.InternalValue);

			Assert.IsTrue(int.Parse(valueLeft) == "1234567890".Length, "left.Value == Length");
			Assert.IsTrue((string)valueRight == "1234567890", "right.Value == '1234567890'");
		}

		/// <summary>
		///A test for GetValue
		///</summary>
		[TestMethod()]
		public void GetValueTest()
		{
			Block top = new Block();
			top.Add(new String("TheSize"));
			top.Add(new String("Sizer"));

			top["TheSize"].relations.Add(new SizeRelation());
			top["TheSize"].relations[0].Of = top[1];

			Variant value = top["TheSize"].relations[0].GetValue();
			Assert.IsTrue((int)value == "Sizer".Length, "Relation.GetValue() == Length");

			value = top[0].InternalValue;
			Assert.IsTrue((int)value == "Sizer".Length, "Of.InternalValue == Length");

			// Change of size
			top[1].DefaultValue = new Variant("1234567890");

			value = top["TheSize"].relations[0].GetValue();
			Assert.IsTrue((int)value == "1234567890".Length, "Relation.GetValue() == Length");

			value = top[0].InternalValue;
			Assert.IsTrue((int)value == "1234567890".Length, "Of.InternalValue == Length");
		}

		/// <summary>
		///A test for SetValue
		///</summary>
		[TestMethod()]
		public void SetValueTest()
		{
			SizeRelation target = new SizeRelation(); // TODO: Initialize to an appropriate value
			Variant value = null; // TODO: Initialize to an appropriate value
			target.SetValue(value);
			Assert.Inconclusive("A method that does not return a value cannot be verified.");
		}

		/// <summary>
		///A test for SizeRelation Constructor
		///</summary>
		[TestMethod()]
		public void SizeRelationConstructorTest()
		{
			SizeRelation target = new SizeRelation();
			Assert.Inconclusive("TODO: Implement code to verify target");
		}
	}
}
