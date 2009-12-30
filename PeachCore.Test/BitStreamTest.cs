using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
		public void DWORDTest()
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
	}
}
