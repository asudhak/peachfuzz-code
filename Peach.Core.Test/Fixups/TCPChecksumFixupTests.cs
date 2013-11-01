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
			Assert.AreEqual(precalcChecksum, values[0].ToArray());
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
			Assert.AreEqual(precalcChecksum, values[0].ToArray());
		}

		[Test]
		public void Test3()
		{
			/* Sample TCP Packet from Wireshark
0000   f0 de f1 e3 1b 6a 00 1b 21 75 7a 40 08 00 45 20  .....j..!uz@..E 
0010   00 34 7d de 00 00 2a 06 a8 a5 4a 7d 14 65 0a 00  .4}...*...J}.e..
0020   01 3f 01 bb d1 dc 0b d8 60 51 19 66 92 c7 80 10  .?......`Q.f....
0030   02 95 c8 be 00 00 01 01 05 0a 19 66 92 c6 19 66  ...........f...f
0040   92 c7                                            ..
			*/

			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Blob name='TcpPkt1' valueType='hex' value='01bbd1dc0bd86051196692c780100295'/>

		<Number name='Checksum' endian='big' size='16'>
			<Fixup class='TCPChecksumFixup'>
				<Param name='ref' value='DM'/>
				<Param name='src' value='74.125.20.101'/>
				<Param name='dst' value='10.0.1.63'/>
			</Fixup>
		</Number>

		<Blob name='TcpPkt2' valueType='hex' value='00000101050a196692c6196692c7'/>
	</DataModel>
</Peach>
";
			// From Packet, Checksum is: c8 be

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var val = dom.dataModels[0].Value;
			val = dom.dataModels[0][1].Value;

			// verify values
			byte[] precalcChecksum = new byte[] { 0xc8, 0xbe };
			Assert.AreEqual(precalcChecksum, val.ToArray());

		}

	}
}

// end