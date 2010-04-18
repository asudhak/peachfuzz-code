using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using PeachCore.Dom;
using PeachCore;

namespace PeachCore.Test
{
	/// <summary>
	/// Summary description for BitStreamTest
	/// </summary>
	[TestClass]
	public class BitStreamTest
	{
		public BitStreamTest()
		{
			//
			// TODO: Add constructor logic here
			//
		}

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
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void BasicTest()
		{
			BitStream bits = new BitStream();

			bits.LittleEndian();
			bits.WriteDWORD(0x7fffffff);
			bits.BigEndian();
			bits.WriteDWORD(0x7fffffff);

			Assert.IsTrue(bits.TellBits() == 64, "Post write position is inccorect");

			bits.SeekBits(0, System.IO.SeekOrigin.Begin);
			Assert.IsTrue(bits.TellBits() == 0, "Post seek position is inccorect");

			bits.LittleEndian();
			Assert.IsTrue(bits.ReadDWORD() == 0x7fffffff, "Read/write of little endian DWORD missmatch");
			bits.BigEndian();
			Assert.IsTrue(bits.ReadDWORD() == 0x7fffffff, "Read/write of big endian DWORD missmatch");

			Assert.IsTrue(bits.TellBits() == 64, "Post read position is inccorect");
		}

		[TestMethod]
		public void SignedWriteReadTest()
		{
			BitStream bits = new BitStream();

			bits.LittleEndian();
			bits.WriteSByte(-15);
			bits.WriteShort(-12);
			bits.WriteInt(-1);
			bits.WriteLong(-12312312232);

			bits.BigEndian();
			bits.WriteSByte(-15);
			bits.WriteShort(-12);
			bits.WriteInt(-1);
			bits.WriteLong(-12312312232);

			bits.SeekBits(0, System.IO.SeekOrigin.Begin);

			bits.LittleEndian();
			Assert.IsTrue(bits.ReadSByte() == -15, "Little ReadSByte != -15");
			Assert.IsTrue(bits.ReadShort() == -12, "Little ReadShort != -12");
			Assert.IsTrue(bits.ReadInt() == -1, "Little ReadInt != -1");
			Assert.IsTrue(bits.ReadLong() == -12312312232, "Little ReadLong != -12312312232");

			bits.BigEndian();
			Assert.IsTrue(bits.ReadSByte() == -15, "Big ReadSByte != -15");
			Assert.IsTrue(bits.ReadShort() == -12, "Big ReadShort != -12");
			Assert.IsTrue(bits.ReadInt() == -1, "Big ReadInt != -1");
			Assert.IsTrue(bits.ReadLong() == -12312312232, "Big ReadLong != -12312312232");
		}

		[TestMethod]
		public void UnsignedWriteReadTest()
		{
			BitStream bits = new BitStream();

			bits.LittleEndian();
			bits.WriteByte(15);
			bits.WriteUShort(12);
			bits.WriteUInt(1);
			bits.WriteULong(12312312232);

			bits.BigEndian();
			bits.WriteByte(15);
			bits.WriteUShort(12);
			bits.WriteUInt(1);
			bits.WriteULong(12312312232);

			bits.SeekBits(0, System.IO.SeekOrigin.Begin);

			bits.LittleEndian();
			Assert.IsTrue(bits.ReadByte() == 15, "Little ReadByte != 15");
			Assert.IsTrue(bits.ReadUShort() == 12, "Little ReadUShort != 12");
			Assert.IsTrue(bits.ReadUInt() == 1, "Little ReadUInt != 1");
			Assert.IsTrue(bits.ReadULong() == 12312312232, "Little ReadULong != 12312312232");

			bits.BigEndian();
			Assert.IsTrue(bits.ReadSByte() == 15, "Big ReadSByte != 15");
			Assert.IsTrue(bits.ReadShort() == 12, "Big ReadUShort != 12");
			Assert.IsTrue(bits.ReadInt() == 1, "Big ReadUInt != 1");
			Assert.IsTrue(bits.ReadLong() == 12312312232, "Big ReadULong != 12312312232");
		}

		class TestDataElement : DataElement
		{
			public TestDataElement(string name)
			{
				this.name = name;
			}
		}

		[TestMethod]
		public void InsertAlignedTest()
		{
			BitStream bits = new BitStream();
			BitStream b1 = new BitStream();
			BitStream b2 = new BitStream();
			BitStream b3 = new BitStream();

			bits.WriteByte(100, new TestDataElement("Byte100"));
			bits.WriteByte(101, new TestDataElement("Byte101"));
			bits.WriteByte(102, new TestDataElement("Byte102"));
			bits.WriteByte(103, new TestDataElement("Byte103"));
			bits.WriteByte(104, new TestDataElement("Byte104"));

			b1.WriteByte(200, new TestDataElement("Byte200"));
			b1.WriteByte(201, new TestDataElement("Byte201"));
			b1.WriteByte(202, new TestDataElement("Byte202"));
			b1.WriteByte(203, new TestDataElement("Byte203"));
			b1.WriteByte(204, new TestDataElement("Byte204"));

			b2.WriteByte(230, new TestDataElement("Byte300"));
			b2.WriteByte(231, new TestDataElement("Byte301"));
			b2.WriteByte(232, new TestDataElement("Byte302"));
			b2.WriteByte(233, new TestDataElement("Byte303"));
			b2.WriteByte(234, new TestDataElement("Byte304"));

			b3.WriteByte(240, new TestDataElement("Byte400"));
			b3.WriteByte(241, new TestDataElement("Byte401"));
			b3.WriteByte(242, new TestDataElement("Byte402"));
			b3.WriteByte(243, new TestDataElement("Byte403"));
			b3.WriteByte(244, new TestDataElement("Byte404"));

			b2.SeekBits(0, SeekOrigin.End);
			b2.Insert(b3);

			b1.SeekBits(0, SeekOrigin.Begin);
			b1.Insert(b2);

			bits.SeekToDataElement("Byte102");
			bits.Insert(b1);

			Assert.IsTrue(bits.DataElementPosition("Byte100") == 0 * 8, "Byte100 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte101") == 1 * 8, "Byte101 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte102") == 17 * 8, "Byte102 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte103") == 18 * 8, "Byte103 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte104") == 19 * 8, "Byte104 incorrect");

			Assert.IsTrue(bits.DataElementPosition("Byte200") == 12 * 8, "Byte200 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte201") == 13 * 8, "Byte201 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte202") == 14 * 8, "Byte202 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte203") == 15 * 8, "Byte203 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte204") == 16 * 8, "Byte204 incorrect");

			Assert.IsTrue(bits.DataElementPosition("Byte300") == 2 * 8, "Byte300 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte301") == 3 * 8, "Byte301 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte302") == 4 * 8, "Byte302 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte303") == 5 * 8, "Byte303 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte304") == 6 * 8, "Byte304 incorrect");

			Assert.IsTrue(bits.DataElementPosition("Byte400") == 7 * 8, "Byte400 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte401") == 8 * 8, "Byte401 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte402") == 9 * 8, "Byte402 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte403") == 10 * 8, "Byte403 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte404") == 11 * 8, "Byte404 incorrect");

			bits.SeekToDataElement("Byte100");
			Assert.IsTrue(bits.ReadByte() == 100, "Byte100 value incorrect");
			bits.SeekToDataElement("Byte101");
			Assert.IsTrue(bits.ReadByte() == 101, "Byte101 value incorrect");
			bits.SeekToDataElement("Byte102");
			Assert.IsTrue(bits.ReadByte() == 102, "Byte102 value incorrect");
			bits.SeekToDataElement("Byte103");
			Assert.IsTrue(bits.ReadByte() == 103, "Byte103 value incorrect");
			bits.SeekToDataElement("Byte104");
			Assert.IsTrue(bits.ReadByte() == 104, "Byte104 value incorrect");

			bits.SeekToDataElement("Byte200");
			Assert.IsTrue(bits.ReadByte() == 200, "Byte200 value incorrect");
			bits.SeekToDataElement("Byte201");
			Assert.IsTrue(bits.ReadByte() == 201, "Byte201 value incorrect");
			bits.SeekToDataElement("Byte202");
			Assert.IsTrue(bits.ReadByte() == 202, "Byte202 value incorrect");
			bits.SeekToDataElement("Byte203");
			Assert.IsTrue(bits.ReadByte() == 203, "Byte203 value incorrect");
			bits.SeekToDataElement("Byte204");
			Assert.IsTrue(bits.ReadByte() == 204, "Byte204 value incorrect");

			bits.SeekToDataElement("Byte300");
			Assert.IsTrue(bits.ReadByte() == 230, "Byte300 value incorrect");
			bits.SeekToDataElement("Byte301");
			Assert.IsTrue(bits.ReadByte() == 231, "Byte301 value incorrect");
			bits.SeekToDataElement("Byte302");
			Assert.IsTrue(bits.ReadByte() == 232, "Byte302 value incorrect");
			bits.SeekToDataElement("Byte303");
			Assert.IsTrue(bits.ReadByte() == 233, "Byte303 value incorrect");
			bits.SeekToDataElement("Byte304");
			Assert.IsTrue(bits.ReadByte() == 234, "Byte304 value incorrect");

			bits.SeekToDataElement("Byte400");
			Assert.IsTrue(bits.ReadByte() == 240, "Byte400 value incorrect");
			bits.SeekToDataElement("Byte401");
			Assert.IsTrue(bits.ReadByte() == 241, "Byte401 value incorrect");
			bits.SeekToDataElement("Byte402");
			Assert.IsTrue(bits.ReadByte() == 242, "Byte402 value incorrect");
			bits.SeekToDataElement("Byte403");
			Assert.IsTrue(bits.ReadByte() == 243, "Byte403 value incorrect");
			bits.SeekToDataElement("Byte404");
			Assert.IsTrue(bits.ReadByte() == 244, "Byte404 value incorrect");
		}

		[TestMethod]
		public void InsertUnalignedTest()
		{
			BitStream bits = new BitStream();
			BitStream b1 = new BitStream();
			BitStream b2 = new BitStream();
			BitStream b3 = new BitStream();

			bits.WriteBits(0, 2, new TestDataElement("Byte100"));
			bits.WriteBits(1, 5, new TestDataElement("Byte101"));
			bits.WriteBits(2, 7, new TestDataElement("Byte102"));
			ulong bitsSize = 2 + 5 + 7;

			b1.WriteBits(3, 3, new TestDataElement("Byte200"));
			b1.WriteBits(4, 5, new TestDataElement("Byte201"));
			b1.WriteBits(5, 7, new TestDataElement("Byte202"));
			ulong b1Size = 3 + 5 + 7;

			b2.WriteBits(6, 5, new TestDataElement("Byte300"));
			b2.WriteBits(7, 5, new TestDataElement("Byte301"));
			b2.WriteBits(8, 5, new TestDataElement("Byte302"));
			ulong b2Size = 5 + 5 + 5;

			b3.WriteBits(9, 5, new TestDataElement("Byte400"));
			b3.WriteBits(10, 5, new TestDataElement("Byte401"));
			b3.WriteBits(11, 5, new TestDataElement("Byte402"));
			ulong b3Size = 5 + 5 + 5;

			b2.SeekBits(0, SeekOrigin.End);
			b2.Insert(b3);

			b1.SeekBits(0, SeekOrigin.Begin);
			b1.Insert(b2);

			bits.SeekToDataElement("Byte102");
			bits.Insert(b1);

			Assert.IsTrue(bits.DataElementPosition("Byte100") == 0 * 8, "Byte100 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte101") == 2, "Byte101 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte102") == b1Size + b2Size + b3Size + 2 + 5, "Byte102 incorrect");

			Assert.IsTrue(bits.DataElementPosition("Byte200") == 7+b2Size+b3Size, "Byte200 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte201") == 7 + b2Size + b3Size + 5, "Byte201 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte202") == 7 + b2Size + b3Size + 5 + 5, "Byte202 incorrect");

			Assert.IsTrue(bits.DataElementPosition("Byte300") == 7, "Byte300 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte301") == 7+5, "Byte301 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte302") == 7+5+5, "Byte302 incorrect");

			Assert.IsTrue(bits.DataElementPosition("Byte400") == 7+b2Size, "Byte400 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte401") == 7 + b2Size+5, "Byte401 incorrect");
			Assert.IsTrue(bits.DataElementPosition("Byte402") == 7 + b2Size+5+5, "Byte402 incorrect");

			bits.SeekToDataElement("Byte100");
			Assert.IsTrue(bits.ReadBits(2) == 0, "Byte100 value incorrect");
			bits.SeekToDataElement("Byte101");
			Assert.IsTrue(bits.ReadBits(5) == 1, "Byte101 value incorrect");
			bits.SeekToDataElement("Byte102");
			Assert.IsTrue(bits.ReadBits(7) == 2, "Byte102 value incorrect");

			bits.SeekToDataElement("Byte200");
			Assert.IsTrue(bits.ReadBits(5) == 3, "Byte200 value incorrect");
			bits.SeekToDataElement("Byte201");
			Assert.IsTrue(bits.ReadBits(5) == 4, "Byte201 value incorrect");
			bits.SeekToDataElement("Byte202");
			Assert.IsTrue(bits.ReadBits(5) == 5, "Byte202 value incorrect");

			bits.SeekToDataElement("Byte300");
			Assert.IsTrue(bits.ReadBits(5) == 6, "Byte300 value incorrect");
			bits.SeekToDataElement("Byte301");
			Assert.IsTrue(bits.ReadBits(5) == 7, "Byte301 value incorrect");
			bits.SeekToDataElement("Byte302");
			Assert.IsTrue(bits.ReadBits(5) == 8, "Byte302 value incorrect");

			bits.SeekToDataElement("Byte400");
			Assert.IsTrue(bits.ReadBits(5) == 9, "Byte400 value incorrect");
			bits.SeekToDataElement("Byte401");
			Assert.IsTrue(bits.ReadBits(5) == 10, "Byte401 value incorrect");
			bits.SeekToDataElement("Byte402");
			Assert.IsTrue(bits.ReadBits(5) == 11, "Byte402 value incorrect");
		}
	}
}
