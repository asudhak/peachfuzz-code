using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;
using Peach.Core.Agent;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Peach.Core.Test.Monitors
{
	[TestFixture]
	class RunCommandTests
	{
		string MakeXml(string[] options)
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
					<Monitor class='RunCommand'>
						<Param name='Command' value='{0}'/>
						<Param name='Arguments' value='{1}'/>
						<Param name='When' value='{2}'/>
						<Param name='UseShell' value='true'/>
					</Monitor>

					{3}
				</Agent>

				<Test name='Default' replayEnabled='false'>
					<Agent ref='LocalAgent'/>
					<StateModel ref='TheState'/>
					<Publisher class='Null'/>
					<Strategy class='RandomDeterministic'/>
				</Test>
			</Peach>";

			var ret = string.Format(template, options);
			return ret;
		}

		void Run(string testName, string arguments="", string faultAgent = "")
		{
			string tempFile = Path.GetTempFileName();
			string testFile = createScript(testName, tempFile);
			string xml = MakeXml(new string[] {testFile , arguments, testName, faultAgent});

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			string[] output = File.ReadAllLines(tempFile);

			Assert.AreEqual(1, output.Length);
			Assert.AreEqual(testName, output[0]);
		}

		private string createScript(string testName, string tempFile)
		{
			string extension = "";
			if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
				extension = "bat";
			else
				extension = "sh";

			string command = "echo " + testName + ">" + tempFile;
			string path = Path.GetTempPath();
			string fileName = Guid.NewGuid().ToString() + "." + extension;
			string file = Path.Combine(path, fileName);
			System.IO.File.WriteAllText(@file, command);

			return file;
		}

		[Test]
		public void TestOnStart()
		{
			Run("OnStart");
		}

		[Test]
		public void TestOnEnd()
		{
			Run("OnEnd");
		}

		[Test]
		public void TestOnIterationStart()
		{
			Run("OnIterationStart");
		}

		[Test]
		public void TestOnIterationEnd()
		{
			Run("OnIterationEnd");
		}

		[Test, ExpectedException(typeof(PeachException), ExpectedMessage = "Could not start monitor \"RunCommand\".  Monitor 'RunCommand' could not set value type parameter 'When' to 'null'.")]
        public void TestNoWhen()
		{
			Run("");
		}

		[Test]
		public void TestOnFault()
		{
			string faultAgent = @"
			<Monitor class='FaultingMonitor'>
				<Param name='Iteration' value='1'/>
			</Monitor>";

			try
			{
				Run("OnFault", "", faultAgent);
				Assert.Fail("Should throw.");
			}
			catch (PeachException ex)
			{
				Assert.AreEqual("Fault detected on control iteration.", ex.Message);
			}
		}
	}
}
