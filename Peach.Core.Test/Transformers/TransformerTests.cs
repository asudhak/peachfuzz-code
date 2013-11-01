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

namespace Peach.Core.Test.Transformers
{
	[TestFixture]
	class TransformerTests
	{
		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, multiple transformers are defined on element 'str'.")]
		public void TwoTransformers()
		{
			string xml = @"
				<Peach>
					<DataModel name='TheDataModel'>
						<String name='str' value='Hello World'>
							<Transformer class='Null'/>
							<Transformer class='Null'/>
						</String>
					</DataModel>
				</Peach>";

			PitParser parser = new PitParser();

			parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error, multiple nested transformers are defined on element 'str'.")]
		public void BadNestedTransformers()
		{
			string xml = @"
				<Peach>
					<DataModel name='TheDataModel'>
						<String name='str' value='127.0.0.1'>
							<Transformer class='Hex'>
								<Transformer class='Null'/>
								<Transformer class='Ipv4StringToOctet'/>
							</Transformer>
						</String>
					</DataModel>
				</Peach>";

			PitParser parser = new PitParser();

			parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)), false);

		}

		[Test]
		public void NestedTransformers()
		{
			string xml = @"
				<Peach>
					<DataModel name='TheDataModel'>
						<String name='str' value='127.0.0.1'>
							<Transformer class='Null'>
								<Transformer class='Ipv4StringToOctet'>
									<Transformer class='Hex'/>
								</Transformer>
							</Transformer>
						</String>
					</DataModel>
				</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var actual = dom.dataModels[0].Value.ToArray();
			var expected = Encoding.ASCII.GetBytes("7f000001"); // 127.0.0.1
			Assert.AreEqual(expected, actual);

			var data = Bits.Fmt("{0}", Encoding.ASCII.GetBytes("0a01ff02")); //10.1.55.2

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			var value = (string)dom.dataModels[0][0].DefaultValue;
			Assert.AreEqual("10.1.255.2", value);
		}
	}
}
