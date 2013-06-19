using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NLog;
using NLog.Targets;
using NLog.Config;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.IO;
using Peach.Core.Publishers;
using Peach.Core.Loggers;

namespace Peach.Core.Test
{
	[TestFixture]
	class LoggerTests
	{
		[Test]
		public void Test3()
		{
			string tmp = Path.GetTempFileName();
			File.Delete(tmp);

			string xml = @"
<Peach>
	<DataModel name='RequestModel'>
		<Number name='Sequence' size='8'/>
		<String name='Method' length='1'/>
	</DataModel>

	<DataModel name='XModel'>
		<Number name='Sequence' size='8'/>
		<String name='Response' value='X Response'/>
	</DataModel>

	<DataModel name='YModel'>
		<Number name='Sequence' size='8'/>
		<String name='Response' value='Y Response'/>
	</DataModel>

	<StateModel name='SM' initialState='Request'>
		<State name='Request'>
			<Action name='RecvReq' type='input'>
				<DataModel ref='RequestModel'/>
			</Action>

			<Action type='changeState' ref='XResponse' when=""str(getattr(StateModel.states['Request'].actions[0].dataModel.find('Method'), 'DefaultValue', None)) == 'X'"" />

			<Action type='changeState' ref='YResponse' when=""str(getattr(StateModel.states['Request'].actions[0].dataModel.find('Method'), 'DefaultValue', None)) == 'Y'"" />
		</State>

		<State name='XResponse'>
			<Action type='slurp' valueXpath='//Request//RecvReq//RequestModel//Sequence' setXpath='//Sequence' />

			<Action type='output' name='OutputX'>
				<DataModel ref='XModel' />
			</Action>

			<Action type='changeState' ref='Request' />
		</State>

		<State name='YResponse'>
			<Action type='slurp' valueXpath='//Request//RecvReq//RequestModel//Sequence' setXpath='//Sequence' />

			<Action type='output' name='OutputY'>
				<DataModel ref='YModel' />
			</Action>
		</State>
	</StateModel>

	<Agent name='Agent'>
		<Monitor class='FaultingMonitor'>
			<Param name='Iteration' value='2'/>
		</Monitor>
	</Agent>

	<Test name='Default'>
		<Publisher class='Null'/>
		<StateModel ref='SM'/>
		<Agent ref='Agent'/>
		<Logger class='File'>
			<Param name='Path' value='{0}'/>
		</Logger>
	</Test>
</Peach>".Fmt(tmp);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].publishers[0] = new TestPub();

			RunConfiguration config = new RunConfiguration();
			config.range = true;
			config.rangeStart = 1;
			config.rangeStop = 2;
			config.pitFile = "LoggerTest";

			Engine e = new Engine(null);
			e.IterationStarting += new Engine.IterationStartingEventHandler(e_IterationStarting);

			e.startFuzzing(dom, config);
		}

		void e_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			context.engine.IterationStarting -= e_IterationStarting;
			context.engine.Fault += new Engine.FaultEventHandler(e_Fault);
			context.engine.ReproFault += new Engine.ReproFaultEventHandler(e_ReproFault);
		}

		void e_ReproFault(RunContext context, uint currentIteration, Dom.StateModel stateModel, Fault[] faultData)
		{
			VerifyFaults("Reproducing", context, currentIteration);
		}

		void e_Fault(RunContext context, uint currentIteration, Dom.StateModel stateModel, Fault[] faultData)
		{
			VerifyFaults("Faults", context, currentIteration);
		}

		void VerifyFaults(string dir, RunContext context, uint currentIteration)
		{
			var pub = context.dom.tests[0].publishers[0] as TestPub;
			Assert.NotNull(pub);
			Assert.AreEqual(3, pub.outputs.Count);

			var logger = context.dom.tests[0].loggers[0] as FileLogger;
			Assert.NotNull(logger);

			var subdir = Directory.EnumerateDirectories(logger.Path).FirstOrDefault();
			Assert.NotNull(subdir);

			var fullPath = Path.Combine(logger.Path, subdir, dir, "FaultingMonitor", currentIteration.ToString());

			var files = Directory.EnumerateFiles(fullPath, "action*.txt").ToList();
			Assert.AreEqual(6, files.Count);

			var actual = File.ReadAllBytes(Path.Combine(fullPath, "action_1_Input_RecvReq.txt"));
			Assert.AreEqual(new byte[] { 1, (byte)'X' }, actual);

			actual = File.ReadAllBytes(Path.Combine(fullPath, "action_2_Output_OutputX.txt"));
			Assert.AreEqual(pub.outputs[0], actual);

			actual = File.ReadAllBytes(Path.Combine(fullPath, "action_3_Input_RecvReq.txt"));
			Assert.AreEqual(new byte[] { 2, (byte)'X' }, actual);

			actual = File.ReadAllBytes(Path.Combine(fullPath, "action_4_Output_OutputX.txt"));
			Assert.AreEqual(pub.outputs[1], actual);

			actual = File.ReadAllBytes(Path.Combine(fullPath, "action_5_Input_RecvReq.txt"));
			Assert.AreEqual(new byte[] { 3, (byte)'Y' }, actual);

			actual = File.ReadAllBytes(Path.Combine(fullPath, "action_6_Output_OutputY.txt"));
			Assert.AreEqual(pub.outputs[2], actual);
		}

		class TestPub : StreamPublisher
		{
			private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

			protected override NLog.Logger Logger { get { return logger; } }

			private int cnt;

			public List<byte[]> outputs = new List<byte[]>();

			public TestPub()
				: base(new Dictionary<string, Variant>())
			{
				this.stream = new MemoryStream();
			}

			protected override void OnOpen()
			{
				cnt = 0;
				outputs.Clear();
			}

			protected override void OnInput()
			{
				++cnt;

				this.stream.SetLength(0);
				this.stream.WriteByte((byte)cnt);

				if (cnt == 3)
					this.stream.WriteByte((byte)'Y');
				else
					this.stream.WriteByte((byte)'X');

				this.stream.Position = 0;
			}

			protected override void OnOutput(byte[] buffer, int offset, int count)
			{
				var buf = new byte[count];
				Buffer.BlockCopy(buffer, offset, buf, 0, count);
				outputs.Add(buf);
			}
		}
	}
}
