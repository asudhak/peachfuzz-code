using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using System.IO;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.Publishers
{
	class TestPublisher : Peach.Core.Publishers.ConsoleHexPublisher
	{
		public TestPublisher()
			: base(new Dictionary<string,Variant>())
		{
			this.stream = new MemoryStream();
			this.BytesPerLine = 16;
		}

		public MemoryStream Stream
		{
			get { return this.stream as MemoryStream; }
		}

		protected override void OnOpen()
		{
		}

		protected override void OnClose()
		{
		}
	}

	[TestFixture]
	class ConsoleHexPublisher
	{
		[Test]
		public void Test1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel1\">" +
				"       <String value=\"Hello World! Hello World!\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheStateModel\" initialState=\"InitialState\">" +
				"       <State name=\"InitialState\">" +
				"           <Action name=\"Action1\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel1\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheStateModel\"/>" +
				"       <Publisher class=\"StdoutHex\"/>" +
				"   </Test>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].publishers[0] = new TestPublisher();

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			TestPublisher eval = dom.tests[0].publishers[0] as TestPublisher;
			eval.Stream.Seek(0, SeekOrigin.Begin);
			string results = Encoding.ASCII.GetString(eval.Stream.ToArray());

			string expected =
				"00000000   48 65 6C 6C 6F 20 57 6F  72 6C 64 21 20 48 65 6C   Hello World! Hel" + Environment.NewLine +
				"00000010   6C 6F 20 57 6F 72 6C 64  21                        lo World!       " + Environment.NewLine;

			Assert.AreEqual(expected, results);
		}
	}
}
