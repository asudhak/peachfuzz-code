using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.IO;

namespace Peach.Core.Test.Transformers.Encode
{
    [TestFixture]
    class Ipv6StringToOctetTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test

			string xml = @"
				<Peach>
					<DataModel name='TheDataModel'>
						<Block name='TheBlock'>
							<Transformer class='Ipv6StringToOctet'/>
							<Blob name='Data' value='3ffe:1900:4545:3:200:f8ff:fe21:67cf'/>
						 </Block>
					</DataModel>

					<StateModel name='TheState' initialState='Initial'>
						<State name='Initial'>
							<Action type='output'>
								<DataModel ref='TheDataModel'/>
							</Action>
						</State>
					</StateModel>

					<Test name='Default'>
						<StateModel ref='TheState'/>
						<Publisher class='Null'/>
					</Test>
				</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            RunConfiguration config = new RunConfiguration();
            config.singleIteration = true;

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            // -- this is the pre-calculated result from Peach2.3 on the blob: "3ffe:1900:4545:3:200:f8ff:fe21:67cf"
            byte[] precalcResult = new byte[] { 0x3F, 0xFE, 0x19, 0x00, 0x45, 0x45, 0x00, 0x03, 0x02, 0x00, 0xF8, 0xFF, 0xFE, 0x21, 0x67, 0xCF };
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(precalcResult, values[0].Value);
        }

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, can't transform IP to bytes, '3ffe:1900:4545:3:200:f8ff:fe21' is not a valid IP address.")]
		public void InvalidIPAdressTest()
		{

			string xml =@"
				<Peach>
					<DataModel name='TheDataModel'>
						<Block name='TheBlock'>
							<Transformer class='Ipv6StringToOctet'/>
							<Blob name='Data' value='3ffe:1900:4545:3:200:f8ff:fe21'/>
						 </Block>
					</DataModel>

					<StateModel name='TheState' initialState='Initial'>
						<State name='Initial'>
							<Action type='output'>
								<DataModel ref='TheDataModel'/>
							</Action>
						</State>
					</StateModel>

					<Test name='Default'>
						<StateModel ref='TheState'/>
						<Publisher class='Null'/>
					</Test>
				</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);
		}

		[Test]
		public void CrackingTest1()
		{
			string xml = @"
						<Peach>
							<DataModel name='TheDataModel'>
								<String name='Data' value='3ffe:1900:4545:3:200:f8ff:fe21:67cf'>
									<Transformer class='Ipv6StringToOctet'/>
								</String>
							</DataModel>
						 </Peach>";
			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x3F, 0xFE, 0x19, 0x00, 0x45, 0x45, 0x00, 0x03, 0x02, 0x00, 0xF8, 0xFF, 0xFE, 0x21, 0x67, 0xCF });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("3ffe:1900:4545:3:200:f8ff:fe21:67cf", (string)dom.dataModels[0][0].DefaultValue);
		}

		[Test]
		public void CrackingTest2()
		{
			string xml = @"
						<Peach>
							<DataModel name='DM'>
								<Block name='blk' length='16'>
									<String name='IP'>
										<Transformer class='Ipv6StringToOctet'/>
									</String>
								</Block>
								<String name='Payload'/>
							</DataModel>
						</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x3F, 0xFE, 0x19, 0x00, 0x45, 0x45, 0x00, 0x03, 0x02, 0x00, 0xF8, 0xFF, 0xFE, 0x21, 0x67, 0xCF });
			data.WriteBytes(Encoding.ASCII.GetBytes("Hello"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("3ffe:1900:4545:3:200:f8ff:fe21:67cf", (string)dom.dataModels[0].find("blk.IP").DefaultValue);
			Assert.AreEqual("Hello", (string)dom.dataModels[0][1].DefaultValue);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, can't transform bytes to IP, expected 16 bytes but got 13 bytes.")]
		public void NotEnoughDataCrackingTest()
		{
			string xml = @"
						<Peach>
							<DataModel name='DM'>
									<String name='IP'>
										<Transformer class='Ipv6StringToOctet'/>
									</String>
							</DataModel>
						</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x3F, 0xFE, 0x19, 0x00, 0x45, 0x45, 0x00, 0x03, 0x02, 0x00, 0xF8, 0xFF, 0xFE });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, can't transform bytes to IP, expected 16 bytes but got 18 bytes.")]
		public void TooMuchDataCrackingTest()
		{
			string xml = @"
						<Peach>
							<DataModel name='DM'>
									<String name='IP'>
										<Transformer class='Ipv6StringToOctet'/>
									</String>
							</DataModel>
						</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(new byte[] { 0x3F, 0xFE, 0x19, 0x00, 0x45, 0x45, 0x00, 0x03, 0x02, 0x00, 0xF8, 0xFF, 0xFE, 0x21, 0x67, 0xCF, 0xFF, 0xFF });
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);
		}
    }
}

// end
