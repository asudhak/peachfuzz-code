using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core.Analyzers;
using Proc = System.Diagnostics.Process;
using System.Diagnostics;

namespace Peach.Core.Test.Monitors
{
	[TestFixture]
	class ProcessMonitorTests
	{
		string MakeXml(string folder)
		{
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
		<Monitor class='Process'>
			<Param name='Executable' value='{0}'/>
		</Monitor>
	</Agent>

	<Test name='Default'>
		<Agent ref='LocalAgent'/>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
	</Test>
</Peach>";

			var ret = string.Format(template, folder);
			return ret;
		}

		void Run(string proccessNames, Engine.IterationStartingEventHandler OnIterStart = null)
		{
			string xml = MakeXml(proccessNames);

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("StringCaseMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			if (OnIterStart != null)
				e.IterationStarting += OnIterStart;
			e.startFuzzing(dom, config);
		}

		[Test, ExpectedException(typeof(PeachException))]
		public void TestBadProcss()
		{
			// Specify a process name that is not running
			Run("some_invalid_process");
		}
	}
}