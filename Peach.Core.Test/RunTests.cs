using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;

namespace Peach.Core.Test
{
	[TestFixture]
	class RunTests
	{
		DateTime iterationStarted;
		double iterationTimeSeconds = -1;

		[Test]
		public void TestWaitTime()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Blob name=\"blob1\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action type=\"output\">" +
				"               <DataModel ref=\"TheDataModel\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\" waitTime=\"5\">" +
				"       <StateModel ref=\"TheState\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"       <Strategy class=\"Sequencial\"/>" +
				"   </Test>" +
				"</Peach>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			dom.tests[0].includedMutators = new List<string>();
			dom.tests[0].includedMutators.Add("BlobMutator");

			RunConfiguration config = new RunConfiguration();

			Engine e = new Engine(null);
			e.config = config;
			e.IterationStarting += new Engine.IterationStartingEventHandler(e_IterationStarting);
			e.IterationFinished += new Engine.IterationFinishedEventHandler(e_IterationFinished);
			e.startFuzzing(dom, config);

			// verify values
			Assert.GreaterOrEqual(5, iterationTimeSeconds);
		}
		void e_IterationFinished(RunContext context, uint currentIteration)
		{
			iterationTimeSeconds = (DateTime.Now - this.iterationStarted).TotalSeconds;
		}

		void e_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			this.iterationStarted = DateTime.Now;
		}


	}
}
