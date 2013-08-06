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
using NLog;

namespace Peach.Core.Test.Publishers
{
	// Only returns input bytes when asked
	class BytePublisher : Peach.Core.Publishers.StreamPublisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		MemoryStream data;

		public BytePublisher()
			: base(new Dictionary<string, Variant>())
		{
			data = new MemoryStream(Encoding.ASCII.GetBytes("Hello World And More Stuff"));
		}

		protected override void OnOpen()
		{
			data.Seek(0, SeekOrigin.Begin);
			this.stream = new MemoryStream();
		}

		public override void WantBytes(long count)
		{
			var buf = new byte[count];
			var len = data.Read(buf, 0, (int)count);
			var pos = stream.Position;
			stream.Seek(0, SeekOrigin.End);
			stream.Write(buf, 0, len);
			stream.Seek(pos, SeekOrigin.Begin);
		}
	}

	class InputTests : DataModelCollector
	{
		[Test]
		public void TestWantBytes()
		{
			// Ensure we successfully read "Hello World" from a publisher that
			// only makes available bytes as they are requested via the WantBytes() API

			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<Block length='5'>
			<Block>
				<String length='2'/>
				<String length='2'/>
				<String/>
			</Block>
		</Block>

		<String value=' ' token='true'/>

		<Block length='5'>
			<Block>
				<String length='2'/>
				<String length='2'/>
				<String/>
			</Block>
		</Block>
	</DataModel>

	<StateModel name='TheStateModel' initialState='InitialState'>
		<State name='InitialState'>
			<Action name='Action1' type='input'>
				<DataModel ref='TheDataModel'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<StateModel ref='TheStateModel'/>
		<Publisher class='Null'/>
	</Test>

</Peach>";

			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].publishers[0] = new BytePublisher();

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			Assert.AreEqual(1, this.actions.Count);
			Assert.AreEqual("Hello World", this.actions[0].dataModel.InternalValue.BitsToString());
		}
	}
}
