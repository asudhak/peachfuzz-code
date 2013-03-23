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
	class IcmpV6ChecksumFixupTests : DataModelCollector
	{
		[Test]
		public void Test1()
		{
			// standard test (Even length string)

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"     <Number name=\"ICMPv6Checksum\" endian=\"big\" size=\"16\">" +
				"           <Fixup class=\"IcmpV6ChecksumFixup\">" +
				"               <Param name=\"ref\" value=\"TheDataModel\"/>" +
				"               <Param name=\"src\" value=\"::1\"/>" +
				"               <Param name=\"dst\" value=\"::1\"/>" +
				"           </Fixup>" +
				"     </Number>" +
				"     <Number name=\"Type\" size=\"8\" valueType=\"hex\" value=\"80\"/>" +
				"     <Number name=\"Code\" size=\"8\"/>" +

				"     <Number name=\"Identifier\" endian=\"big\" size=\"16\" valueType=\"hex\" value=\"08 69\" />" +
				"     <Number name=\"Sequence\" endian=\"big\" size=\"16\" valueType=\"hex\" value=\"00 05\" />" +
				"     <Blob name=\"Data\" valueType=\"hex\" value=\"d6a8f05000000000b5c40c0000000000101112131415161718191a1b1c1d1e1f202122232425262728292a2b2c2d2e2f3031323334353637\"/>" +
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
			// -- this is the pre-calculated checksum from Peach2.3 on the blob: "Hello"
			byte[] precalcChecksum = new byte[] { 0x2f, 0x84 };
			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(precalcChecksum, values[0].Value);
		}

	}
}

// end