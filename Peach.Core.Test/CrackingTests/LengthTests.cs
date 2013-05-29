using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.IO;

namespace Peach.Core.Test.CrackingTests
{
	[TestFixture]
	public class LengthTests
	{
		string cont_template = @"
<Peach>
	<DataModel name=""TheDataModel"">
		<{0} lengthType=""{1}"" {2}=""{3}"">
			<Blob/>
		</{0}>
		<!-- ensure we don't fall into the isLastUnsizedElement case -->
		<Blob />
	</DataModel>
</Peach>";

		string elem_template = @"
<Peach>
	<DataModel name=""TheDataModel"">
		<{0} lengthType=""{1}"" {2}=""{3}""/>
		<!-- ensure we don't fall into the isLastUnsizedElement case -->
		<Blob />
	</DataModel>
</Peach>";

		BitStream Crack(string template, string elem, string units, string lengthType, string length)
		{
			string xml = string.Format(template, elem, units, lengthType, length);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(1, dom.dataModels.Count);
			Assert.AreEqual(2, dom.dataModels[0].Count);
			var de = dom.dataModels[0][0];

			var cont = de as DataElementContainer;
			if (cont != null)
			{
				Assert.AreEqual(1, cont.Count);
				de = cont[0];
			}
			
			var value = de.Value;
			return value;
		}

		BitStream CrackElement(string elem, string units, string lengthType, string length)
		{
			return Crack(elem_template, elem, units, lengthType, length);
		}

		BitStream CrackContainer(string elem, string units, string lengthType, string length)
		{
			return Crack(cont_template, elem, units, lengthType, length);
		}

		[Test, ExpectedException(typeof(PeachException))]
		public void BlobChars()
		{
			CrackElement("Blob", "chars", "length", "5");
		}

		[Test]
		public void BlobBytes()
		{
			var bs = CrackElement("Blob", "bytes", "length", "5");
			Assert.AreEqual(new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55 }, bs.Value);
		}

		[Test]
		public void BlobBits()
		{
			var bs = CrackElement("Blob", "bits", "length", "36");
			Assert.AreEqual(new byte[] { 0x11, 0x22, 0x33, 0x44, 0x50 }, bs.Value);
		}

		[Test, ExpectedException(typeof(PeachException))]
		public void BlockChars()
		{
			CrackContainer("Block", "chars", "length", "5");
		}

		[Test]
		public void BlockBytes()
		{
			var bs = CrackContainer("Block", "bytes", "length", "5");
			Assert.AreEqual(new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55 }, bs.Value);
		}

		[Test]
		public void BlockBits()
		{
			var bs = CrackContainer("Block", "bits", "length", "36");
			Assert.AreEqual(new byte[] { 0x11, 0x22, 0x33, 0x44, 0x50 }, bs.Value);
		}

		[Test, ExpectedException(typeof(PeachException))]
		public void ChoiceChars()
		{
			CrackContainer("Choice", "chars", "length", "5");
		}

		[Test]
		public void ChoiceBytes()
		{
			var bs = CrackContainer("Choice", "bytes", "length", "5");
			Assert.AreEqual(new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55 }, bs.Value);
		}

		[Test]
		public void ChoiceBits()
		{
			var bs = CrackContainer("Choice", "bits", "length", "36");
			Assert.AreEqual(new byte[] { 0x11, 0x22, 0x33, 0x44, 0x50 }, bs.Value);
		}

		[Test, ExpectedException(typeof(PeachException))]
		public void FlagsChars()
		{
			CrackContainer("Flags", "chars", "length", "2");
		}

		[Test, ExpectedException(typeof(PeachException))]
		public void FlagsBytes()
		{
			CrackElement("Flags", "bytes", "length", "2");
		}

		[Test, ExpectedException(typeof(PeachException))]
		public void FlagsBits()
		{
			CrackElement("Flags", "bits", "length", "16");
		}

		[Test]
		public void StringChars()
		{
			var bs = CrackElement("String type=\"utf16\"", "chars", "length", "2");
			Assert.AreEqual(new byte[] { 0x11, 0x22, 0x33, 0x44 }, bs.Value);
		}

		[Test]
		public void StringBytes()
		{
			var bs = CrackElement("String type=\"utf16\"", "bytes", "length", "4");
			Assert.AreEqual(new byte[] { 0x11, 0x22, 0x33, 0x44 }, bs.Value);
		}

		[Test]
		public void StringBits()
		{
			var bs = CrackElement("String type=\"utf16\"", "bits", "length", "32");
			Assert.AreEqual(new byte[] { 0x11, 0x22, 0x33, 0x44 }, bs.Value);
		}
	}
}