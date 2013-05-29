using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.Monitors
{
	[TestFixture]
	class CleanupFolderMonitorTests
	{
		bool madeTempFiles;
		string tmp;

		[SetUp]
		public void SetUp()
		{
			madeTempFiles = false;
			tmp = Path.GetTempFileName();
			File.Delete(tmp);
			Directory.CreateDirectory(tmp);
		}

		[TearDown]
		public void TearDown()
		{
			Directory.Delete(tmp, true);
		}

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
		<Monitor class='CleanupFolder'>
			<Param name='Folder' value='{0}'/>
		</Monitor>
	</Agent>

	<Test name='Default' replayEnabled='false'>
		<Agent ref='LocalAgent'/>
		<StateModel ref='TheState'/>
		<Publisher class='Null'/>
		<Strategy class='RandomDeterministic'/>
	</Test>
</Peach>";

			var ret = string.Format(template, folder);
			return ret;
		}

		private void Run(string folder, Engine.IterationStartingEventHandler OnIterStart = null)
		{
			string xml = MakeXml(folder);

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

		[Test]
		public void TestBadFolder()
		{
			// Should run even if the folder does not exist
			Run("some_unknown_folder");
		}

		[Test]
		public void TestNoNewFiles()
		{
			// Should not delete the folder being monotired or any files/directories that already exist
			string file1 = Path.Combine(tmp, "file");
			string sub = Path.Combine(tmp, "sub");
			string file2 = Path.Combine(sub, "file");

			Directory.CreateDirectory(sub);
			File.Create(file1).Close();
			File.Create(file2).Close();

			Run(tmp);

			Assert.True(Directory.Exists(tmp));
			Assert.True(Directory.Exists(sub));
			Assert.True(File.Exists(file1));
			Assert.True(File.Exists(file2));
		}

		[Test]
		public void TestCleanup()
		{
			// Should not delete the folder being monotired or any files/directories that already exist
			string file1 = Path.Combine(tmp, "file");
			string sub = Path.Combine(tmp, "sub");
			string file2 = Path.Combine(sub, "file");

			Directory.CreateDirectory(sub);
			File.Create(file1).Close();
			File.Create(file2).Close();

			Run(tmp, IterationStarting);

			Assert.True(madeTempFiles);

			Assert.True(Directory.Exists(tmp));
			Assert.True(Directory.Exists(sub));
			Assert.True(File.Exists(file1));
			Assert.True(File.Exists(file2));

			Assert.False(Directory.Exists(Path.Combine(tmp, "newsub")));
			Assert.False(File.Exists(Path.Combine(tmp, "newfile")));
		}

		void IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			string file1 = Path.Combine(tmp, "newfile");
			string sub = Path.Combine(tmp, "newsub");
			string file2 = Path.Combine(sub, "newfile");

			Directory.CreateDirectory(sub);
			File.Create(file1).Close();
			File.Create(file2).Close();

			madeTempFiles = true;
		}
	}
}
