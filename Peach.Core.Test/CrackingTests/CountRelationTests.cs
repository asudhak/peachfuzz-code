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
	class CountRelationTests
	{
        [Test]
        public void CrackCountOf1()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                "	<DataModel name=\"TheDataModel\">" +
                "		<Number size=\"8\">" +
                "			<Relation type=\"count\" of=\"numbers\" />" +
                "		</Number>" +
                "		<Number size=\"8\" name=\"numbers\" minOccurs=\"0\" maxOccurs=\"-1\"/>" +
                "	</DataModel>" +
                "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            BitStream data = new BitStream();

            data.WriteBytes(new byte[] { 0x05, 0x01, 0x02, 0x03, 0x04, 0x05 });

            data.SeekBits(0, SeekOrigin.Begin);

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual(5, (int)dom.dataModels[0][0].DefaultValue);
            Assert.IsInstanceOf<Dom.Array>(dom.dataModels[0][1]);
            Assert.AreEqual(5, ((Dom.Array)dom.dataModels[0][1]).Count);
			Assert.IsTrue(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 }.SequenceEqual((byte[])dom.dataModels[0][1].InternalValue));
        }

        [Test]
        public void CrackCountOf2()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                "	<DataModel name=\"TheDataModel\">" +
                "		<Number size=\"8\" name=\"numbers\" occurs=\"5\"/>" +
                "	</DataModel>" +
                "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            BitStream data = new BitStream();

            data.WriteBytes(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0xFF, 0xFF, 0xFF, 0xFF });

            data.SeekBits(0, SeekOrigin.Begin);

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.IsInstanceOf<Dom.Array>(dom.dataModels[0][0]);
            Assert.AreEqual(5, ((Dom.Array)dom.dataModels[0][0]).Count);
            Assert.IsTrue(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 }.SequenceEqual((byte[])dom.dataModels[0][0].InternalValue));
        }


        [Test]
        public void CrackCountOf3()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                "	<DataModel name=\"TheDataModel\">" +
                "		<Number size=\"8\" name=\"numbers\" minOccurs=\"5\" maxOccurs=\"5\"/>" +
                "	</DataModel>" +
                "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            BitStream data = new BitStream();

            data.WriteBytes(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0xFF, 0xFF, 0xFF, 0xFF });

            data.SeekBits(0, SeekOrigin.Begin);

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.IsInstanceOf<Dom.Array>(dom.dataModels[0][0]);
            Assert.AreEqual(5, ((Dom.Array)dom.dataModels[0][0]).Count);
			Assert.IsTrue(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 }.SequenceEqual((byte[])dom.dataModels[0][0].InternalValue));
        }

        [Test]
        public void CrackCountOf4()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                "	<DataModel name=\"TheDataModel\">" +
                "		<Number size=\"8\">" +
                "			<Relation type=\"count\" of=\"numbers\" expressionGet=\"count/2\"/>" +
                "		</Number>" +
                "		<Number size=\"8\" name=\"numbers\" minOccurs=\"0\" maxOccurs=\"-1\"/>" +
                "	</DataModel>" +
                "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            BitStream data = new BitStream();

            data.WriteBytes(new byte[] { 0x0A, 0x01, 0x02, 0x03, 0x04, 0x05 });

            data.SeekBits(0, SeekOrigin.Begin);

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual(10, (int)dom.dataModels[0][0].DefaultValue);
            Assert.AreEqual(5, (int)dom.dataModels[0][0].InternalValue);
            Assert.IsInstanceOf<Dom.Array>(dom.dataModels[0][1]);
            Assert.AreEqual(5, ((Dom.Array)dom.dataModels[0][1]).Count);
			Assert.IsTrue(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 }.SequenceEqual((byte[])dom.dataModels[0][1].InternalValue));
        }

		[Test]
		public void CrackLeftovers()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number name=\"count\" size=\"8\">" +
				"			<Relation type=\"count\" of=\"num\" />" +
				"		</Number>" +
				"		<Number name=\"num\" size=\"8\" minOccurs=\"2\"/>" +
				"		<String name=\"str\" length=\"9\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(Encoding.ASCII.GetBytes("\x06QWERTYleftoversextrajunk"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(3, dom.dataModels[0].Count);
			Dom.Array array = (Dom.Array)dom.dataModels[0][1];
			Assert.AreEqual(6, array.Count);
			Assert.AreEqual("leftovers", (string)dom.dataModels[0][2].DefaultValue);
		}
	}
}
