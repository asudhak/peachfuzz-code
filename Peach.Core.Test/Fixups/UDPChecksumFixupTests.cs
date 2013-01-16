using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.Fixups
{
	[TestFixture]
	class UDPChecksumFixupTests : DataModelCollector
	{
		[Test]
		public void Test1()
		{
			// standard test (Odd length string)

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Number name=\"UDPChecksum\" endian=\"big\" size=\"16\">" +
				"           <Fixup class=\"UDPChecksumFixup\">" +
				"               <Param name=\"ref\" value=\"Data\"/>" +
				"               <Param name=\"src\" value=\"10.0.1.34\"/>" +
				"               <Param name=\"dst\" value=\"10.0.1.30\"/>" +
				"           </Fixup>" +
				"       </Number>" +
				"       <Blob name=\"Data\" value=\"Hello\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action type=\"output\">" +
				"               <DataModel ref=\"TheDataModel\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheState\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"   </Test>" +
				"</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			// verify values
			byte[] precalcChecksum = new byte[] { 0xc5, 0xd7 };
			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(precalcChecksum, values[0].Value);
		}

		[Test]
		public void Test2()
		{
			// IPv6 test (Odd length string)

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Number name=\"UDPChecksum\" endian=\"big\" size=\"16\">" +
				"           <Fixup class=\"UDPChecksumFixup\">" +
				"               <Param name=\"ref\" value=\"TheDataModel\"/>" +
				"               <Param name=\"src\" value=\"::1\"/>" +
				"               <Param name=\"dst\" value=\"::1\"/>" +
				"           </Fixup>" +
				"       </Number>" +
				"       <Number name=\"SrcPort\" size=\"16\" valueType=\"hex\" value=\"86 d6\"/>" +
                "       <Number name=\"DestPort\" size=\"16\" valueType=\"hex\" value=\"00 01\"/>" +
                "       <Number name=\"Length\" size=\"16\" valueType=\"hex\" value=\"00 0d\"/>"+
				"       <Blob name=\"Data\" value=\"hello\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action type=\"output\">" +
				"               <DataModel ref=\"TheDataModel\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheState\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"   </Test>" +
				"</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			// verify values
			byte[] precalcChecksum = new byte[] { 0x35, 0x29 };
			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(precalcChecksum, values[0].Value);
		}
	}
}

// end