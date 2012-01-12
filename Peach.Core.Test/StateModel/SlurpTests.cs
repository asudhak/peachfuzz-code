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

namespace Peach.Core.Test.StateModel
{
	[TestFixture]
	class SlurpTests : TestBase
	{
		//NLog.Logger logger = LogManager.GetLogger("Peach.Core.Test.StateModel.SlurpTests");

		//[SetUp]
		//public void Initialize()
		//{
		//    // Step 1. Create configuration object 
		//    LoggingConfiguration config = new LoggingConfiguration();

		//    // Step 2. Create targets and add them to the configuration 
		//    ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
		//    config.AddTarget("console", consoleTarget);

		//    OutputDebugStringTarget fileTarget = new OutputDebugStringTarget();
		//    config.AddTarget("DbWin", fileTarget);

		//    // Step 3. Set target properties 
		//    consoleTarget.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";
		//    fileTarget.Layout = "Log4JXmlEventLayout";

		//    // Step 4. Define rules
		//    LoggingRule rule1 = new LoggingRule("*", LogLevel.Trace, consoleTarget);
		//    config.LoggingRules.Add(rule1);

		//    //LoggingRule rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
		//    //config.LoggingRules.Add(rule2);

		//    // Step 5. Activate the configuration
		//    LogManager.Configuration = config;
		//    //LogManager.EnableLogging();

		//    logger.Info("Logs Initialized! -- Hello world!");
		//}

		[Test]
		public void Test1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Peach>" +
				"   <DataModel name=\"TheDataModel1\">" +
				"       <String value=\"Hello\"/>" +
				"   </DataModel>" +
				"   <DataModel name=\"TheDataModel2\">" +
				"       <String value=\"Hello World!\"/>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheStateModel\" initialState=\"InitialState\">" +
				"       <State name=\"InitialState\">" +
				"           <Action name=\"Action1\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel1\"/>" +
				"           </Action>" +
				"			<Action name=\"Action2\" type=\"slurp\" valueXpath=\"//InitialState//TheDataModel1\" setXpath=\"//TheDataModel2\"/>" +
				"           <Action name=\"Action3\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel2\"/>" +
				"           </Action>" +
				"           <Action name=\"Action4\" type=\"output\">" +
				"               <DataModel ref=\"TheDataModel2\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"TheTest\">" +
				"       <StateModel ref=\"TheStateModel\"/>" +
				"       <Publisher class=\"Stdout\"/>" +
				"   </Test>" +

				"   <Run name=\"DefaultRun\">" +
				"       <Test ref=\"TheTest\"/>" +
				"   </Run>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);

			var stateModel = dom.runs[0].tests[0].stateModel;
			var state = stateModel.initialState;

			Assert.AreEqual(state.actions[0].dataModel.Value.Value, state.actions[2].dataModel.Value.Value);
			Assert.AreEqual(state.actions[0].dataModel.Value.Value, state.actions[3].dataModel.Value.Value);
		}
	}
}
