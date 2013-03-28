using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Peach.Core;
using Peach.Core.Agent.Monitors;
using NUnit.Framework;
using System.Threading;
using Peach.Core.Analyzers;
using System.IO;
using System.Text;

namespace Peach.Core.Test.Agent.Monitors
{
	[TestFixture]
	public class PopupWatcherTest
	{
		class Params : Dictionary<string, string> { }

		private Fault[] faults;
		private uint faultIteration;

		[SetUp]
		public void SetUp()
		{
			faults = null;
			faultIteration = 0;
		}

		void _Fault(RunContext context, uint currentIteration, Dom.StateModel stateModel, Fault[] faults)
		{
			Assert.Null(this.faults);
			this.faults = faults;
		}

		string MakeXml(Params parameters)
		{
			string fmt = "<Param name='{0}' value='{1}'/>";

			string template = @"
<Peach>
	<DataModel name='TheDataModel'>
		<String value='Hello' mutable='false'/>
	</DataModel>

	<StateModel name='TheState' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='TheDataModel'/>
			</Action>
		</State>
	</StateModel>

	<Agent name='LocalAgent'>
		<Monitor class='FaultingMonitor'>
			<Param name='Iteration' value='{0}'/>
		</Monitor>
		<Monitor class='PopupWatcher'>
{1}
		</Monitor>
	</Agent>

	<Test name='Default' replayEnabled='false'>
		<Agent ref='LocalAgent'/>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>";

			var items = parameters.Select(kv => string.Format(fmt, kv.Key, kv.Value));
			var joined = string.Join(Environment.NewLine, items);
			var ret = string.Format(template, faultIteration, joined);

			return ret;
		}

		void Run(Params parameters, bool shouldFault)
		{
			string xml = MakeXml(parameters);

			faults = null;

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("StringCaseMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.Fault += _Fault;

			if (!shouldFault)
			{
				e.startFuzzing(dom, config);
				return;
			}

			try
			{
				e.startFuzzing(dom, config);
				Assert.Fail("Should throw.");
			}
			catch (PeachException ex)
			{
				Assert.AreEqual("Fault detected on control iteration.", ex.Message);
			}
		}

		[Test, ExpectedException(ExpectedException = typeof(PeachException), ExpectedMessage = "Could not start monitor \"PopupWatcher\".  Monitor 'PopupWatcher' is missing required parameter 'WindowNames'.")]
		public void TestNoWindow()
		{
			Run(new Params(), false);
			Assert.Null(faults);
		}

		class LameWindow : System.Windows.Forms.NativeWindow, IDisposable
		{
			public LameWindow(string windowTitle)
			{
				var cp = new System.Windows.Forms.CreateParams();
				cp.Caption = windowTitle;
				CreateHandle(cp);
			}

			public void Dispose()
			{
				DestroyHandle();
			}
		}

		private AutoResetEvent evt = new AutoResetEvent(false);

		void ThreadProc(object windowTitle)
		{
			evt.Reset();
			using (var wnd = new LameWindow(windowTitle.ToString()))
			{
				System.Windows.Forms.Application.DoEvents();
				evt.Set();
				System.Windows.Forms.Application.Run();
				Console.WriteLine("Done!");
			}
		}

		[Test]
		public void TestWindow()
		{
			string windowName = "PopupWatcherTest - " + System.Diagnostics.Process.GetCurrentProcess().Id;
			var th = new Thread(ThreadProc);
			th.Start(windowName);
			evt.WaitOne();

			faultIteration = 1;

			try
			{
				Run(new Params { { "WindowNames", windowName } }, true);
			}
			finally
			{
				System.Windows.Forms.Application.Exit();
				th.Join();
			}

			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual("PopupWatcher", faults[1].detectionSource);
			Assert.AreEqual("Closed 1 popup window.", faults[1].title);
			Assert.True(faults[1].description.Contains("PopupWatcherTest"));
			Assert.AreEqual(FaultType.Data, faults[1].type);
		}

		[Test]
		public void TestWindowList()
		{
			string windowName = "PopupWatcherTest - " + System.Diagnostics.Process.GetCurrentProcess().Id;
			var th = new Thread(ThreadProc);
			th.Start(windowName);
			evt.WaitOne();

			faultIteration = 1;

			try
			{
				Run(new Params { { "WindowNames", "Window1,Window2," + windowName } }, true);
			}
			finally
			{
				System.Windows.Forms.Application.Exit();
				th.Join();
			}

			Assert.NotNull(faults);
			Assert.AreEqual(2, faults.Length);
			Assert.AreEqual("FaultingMonitor", faults[0].detectionSource);
			Assert.AreEqual("PopupWatcher", faults[1].detectionSource);
			Assert.AreEqual("Closed 1 popup window.", faults[1].title);
			Assert.True(faults[1].description.Contains(windowName));
			Assert.AreEqual(FaultType.Data, faults[1].type);
		}

		[Test]
		public void TestFault()
		{
			string windowName = "PopupWatcherTest - " + System.Diagnostics.Process.GetCurrentProcess().Id;

			var th = new Thread(ThreadProc);
			th.Start(windowName);
			evt.WaitOne();

			try
			{
				Run(new Params { { "WindowNames", windowName }, { "Fault", "true" } }, true);
			}
			finally
			{
				System.Windows.Forms.Application.Exit();
				th.Join();
			}

			Assert.NotNull(faults);
			Assert.AreEqual(1, faults.Length);
			Assert.AreEqual("PopupWatcher", faults[0].detectionSource);
			Assert.AreEqual("Closed 1 popup window.", faults[0].title);
			Assert.True(faults[0].description.Contains(windowName));
		}
	}
}
