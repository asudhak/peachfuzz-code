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
	class TCPChecksumFixupTests : DataModelCollector
	{
		[Test]
		public void Test1()
		{
			// standard test (Odd length string)

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Number name=\"TCPChecksum\" endian=\"big\" size=\"16\">" +
				"           <Fixup class=\"TCPChecksumFixup\">" +
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
			byte[] precalcChecksum = new byte[] { 0xc5, 0xe2 };
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
				"       <Number name=\"TCPChecksum\" endian=\"big\" size=\"16\">" +
				"           <Fixup class=\"TCPChecksumFixup\">" +
				"               <Param name=\"ref\" value=\"TheDataModel\"/>" +
				"               <Param name=\"src\" value=\"fe80::20c:29ff:feef:2a1b\"/>" +
				"               <Param name=\"dst\" value=\"fe80::20c:29ff:fe65:c6c5\"/>" +
				"           </Fixup>" +
				"       </Number>" +
				"       <Blob name=\"Data\" valueType=\"hex\" value=\"c5 0f 27 0f 9b 4a 38 3e 00 00 00 00 a0 02 38 40\"/>" +
				"       <Blob name=\"Data2\" valueType=\"hex\" value=\"00 00 02 04 05 a0 04 02 08 0a 00 4e 53 e5 00 00 00 00 01 03 03 04\"/>" +
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
			byte[] precalcChecksum = new byte[] { 0xb8, 0xad };
			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(precalcChecksum, values[0].Value);
		}
	}
}

// end