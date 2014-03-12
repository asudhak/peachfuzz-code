using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Analyzers;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace Peach.Core.Test.Publishers
{
	[TestFixture]
	class TcpListenerPublisherTests : DataModelCollector
	{
		[Test]
		public void TcpListenerTest1()
		{
			ushort port = TestBase.MakePort(51000, 52000);
			string xml = @"
				<Peach>
					<DataModel name='input'>
						<Choice>
							<String length='100'/>
							<String />
						</Choice>
					</DataModel>

					<DataModel name='output'>
						<String name='str'/>
					</DataModel>

					<StateModel name='SM' initialState='InitialState'>
						<State name='InitialState'>
							<Action type='open' publisher='Server'/>

							<Action type='output' publisher='Client'>
								<DataModel ref='output'/>
								<Data>
									<Field name='str' value='Hello'/>
								</Data>
							</Action>
			
							<Action type='accept' publisher='Server'/>

							<Action type='input' publisher='Server'>
								<DataModel ref='input'/>
							</Action>

							<Action type='output' publisher='Client'>
								<DataModel ref='output'/>
								<Data>
									<Field name='str' value='World'/>
								</Data>
							</Action>

							<Action type='input' publisher='Server'>
								<DataModel ref='input'/>
							</Action>
						</State>
					</StateModel>

					<Test name='Default'>
						<StateModel ref='SM'/>

						<Publisher class='TcpListener' name='Server'>
							<Param name='Interface' value='127.0.0.1'/>
							<Param name='Port' value='{0}'/>
							<Param name='Timeout' value='1000'/>
						</Publisher>
		
						<Publisher class='Tcp' name='Client'>
							<Param name='Host' value='127.0.0.1'/>
							<Param name='Port' value='{0}'/>
						</Publisher>
					</Test>
				</Peach>".Fmt(port);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);

			e.startFuzzing(dom, config);

			Assert.AreEqual("Hello", dom.tests[0].stateModel.states["InitialState"].actions[1].dataModel.InternalValue.BitsToString());
			Assert.AreEqual("Hello", dom.tests[0].stateModel.states["InitialState"].actions[3].dataModel.InternalValue.BitsToString());
			Assert.AreEqual("World", dom.tests[0].stateModel.states["InitialState"].actions[4].dataModel.InternalValue.BitsToString());
			Assert.AreEqual("World", dom.tests[0].stateModel.states["InitialState"].actions[5].dataModel.InternalValue.BitsToString());
			
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error on data input, the buffer is not initalized.")]
		public void FailingInputTest1()
		{
			string xml = @"
				<Peach>
					<DataModel name='input'>
						<String />
					</DataModel>

					<StateModel name='SM' initialState='InitialState'>
						<State name='InitialState'>
							<Action type='input' publisher='Server'>
								<DataModel ref='input'/>
							</Action>
						</State>
					</StateModel>

					<Test name='Default'>
						<StateModel ref='SM'/>

						<Publisher class='TcpListener' name='Server'>
							<Param name='Interface' value='127.0.0.1'/>
							<Param name='Port' value='55555'/>
							<Param name='Timeout' value='1000'/>
						</Publisher>
					</Test>
				</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);

			e.startFuzzing(dom, config);
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Error on data output, the client is not initalized.")]
		public void FailingOutputTest1()
		{
			string xml = @"
				<Peach>
					<DataModel name='output'>
						<String name='str'/>
					</DataModel>

					<StateModel name='SM' initialState='InitialState'>
						<State name='InitialState'>
							<Action type='output' publisher='Server'>
								<DataModel ref='output'/>
							</Action>
						</State>
					</StateModel>

					<Test name='Default'>
						<StateModel ref='SM'/>

						<Publisher class='TcpListener' name='Server'>
							<Param name='Interface' value='127.0.0.1'/>
							<Param name='Port' value='0'/>
							<Param name='Timeout' value='1000'/>
						</Publisher>
					</Test>
				</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);

			e.startFuzzing(dom, config);
		}
	}
}
