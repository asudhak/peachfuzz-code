using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;
using Peach.Core.Cracker;

namespace Peach.Core.Test.Fixups
{
	[TestFixture]
	class CiscoFixupTests : DataModelCollector
	{
		[Test]
		public void OddLengthHighBitSetTest()
		{
			// standard test

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Number name=\"Cdp\" size=\"16\" signed=\"false\" endian=\"little\">" +
				"           <Fixup class=\"CiscoFixup\">" +
				"               <Param name=\"ref\" value=\"Data1\"/>" +
				"           </Fixup>" +
				"       </Number>" +
				"       <Blob name=\"Data1\" valueType=\"hex\" value=\"02b400000001000c6d7973776974636800020011000000010101cc0004c0a800fd000300134661737445746865726e6574302f31000400080000002800050114436973636f20496e7465726e6574776f726b204f7065726174696e672053797374656d20536f667477617265200a494f532028746d2920433239353020536f667477617265202843323935302d49364b324c3251342d4d292c2056657273696f6e2031322e3128323229454131342c2052454c4541534520534f4654574152452028666331290a546563686e6963616c20537570706f72743a20687474703a2f2f7777772e636973636f2e636f6d2f74656368737570706f72740a436f707972696768742028632920313938362d3230313020627920636973636f2053797374656d732c20496e632e0a436f6d70696c6564205475652032362d4f63742d31302031303a3335206279206e627572726100060015636973636f2057532d43323935302d31320008002400000c011200000000ffffffff010220ff000000000000000bbe189a40ff00000009000c4d59444f4d41494e000a00060001000b0005010012000500001300050000160011000000010101cc0004c0a800fd\"/>" +
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

			Engine e = new Engine(this);
			e.startFuzzing(dom, config);

			// verify values
			byte[] precalcChecksum = new byte[] { 0x0a, 0x09 };
			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(precalcChecksum, values[0].ToArray());
		}

		[Test]
		public void EvenLengthTest()
		{
			// standard test

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Number name=\"Cdp\" size=\"16\" signed=\"false\" endian=\"little\">" +
				"           <Fixup class=\"CiscoFixup\">" +
				"               <Param name=\"ref\" value=\"Data1\"/>" +
				"           </Fixup>" +
				"       </Number>" +
				"       <Blob name=\"Data1\" valueType=\"hex\" value=\"01b4000000010006523100020011000000010101cc0004c0a80a010003000d45746865726e6574300004000800000001000500d8436973636f20496e7465726e6574776f726b204f7065726174696e672053797374656d20536f667477617265200a494f532028746d29203136303020536f667477617265202843313630302d4e592d4c292c2056657273696f6e2031312e3228313229502c2052454c4541534520534f4654574152452028666331290a436f707972696768742028632920313938362d3139393820627920636973636f2053797374656d732c20496e632e0a436f6d70696c6564205475652030332d4d61722d39382030363a33332062792064736368776172740006000e636973636f2031363031\"/>" +
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

			Engine e = new Engine(this);
			e.startFuzzing(dom, config);

			// verify values
			byte[] precalcChecksum = new byte[] { 0xf0, 0xdf };
			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(precalcChecksum, values[0].ToArray());
		}

		[Test]
		public void OddLengthHighBitNotSetTest()
		{
			// standard test

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Number name=\"Cdp\" size=\"16\" signed=\"false\" endian=\"little\">" +
				"           <Fixup class=\"CiscoFixup\">" +
				"               <Param name=\"ref\" value=\"Data1\"/>" +
				"           </Fixup>" +
				"       </Number>" +
				"       <Blob name=\"Data1\" valueType=\"hex\" value=\"02b40000000100065231000500fd436973636f20494f5320536f6674776172652c203238303020536f667477617265202843323830304e4d2d414456495053455256494345534b392d4d292c2056657273696f6e2031322e3428323429542c2052454c4541534520534f4654574152452028666331290a546563686e6963616c20537570706f72743a20687474703a2f2f7777772e636973636f2e636f6d2f74656368737570706f72740a436f707972696768742028632920313938362d3230303920627920436973636f2053797374656d732c20496e632e0a436f6d70696c6564205765642032352d4665622d30392031373a35352062792070726f645f72656c5f7465616d0006000e436973636f203238313100020011000000010101cc00040a760a01000300134661737445746865726e6574302f300004000800000029000700130a760a00180a761400180a761e001800090004000b000501\"/>" +
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

			Engine e = new Engine(this);
			e.startFuzzing(dom, config);

			// verify values
			byte[] precalcChecksum = new byte[] { 0xe2, 0xba };
			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(precalcChecksum, values[0].ToArray());
		}
	}
}

// end
