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

namespace Peach.Core.Test.StateModel
{
	[TestFixture]
	class InputTests : TestBase
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

				"   <StateModel name=\"TheStateModel\" initialState=\"InitialState\">" +
				"       <State name=\"InitialState\">" +
				"           <Action name=\"Action1\" type=\"Input\">" +
				"               <DataModel ref=\"TheDataModel1\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheStateModel\"/>" +
				"       <Publisher class=\"Stdout\"/>" +
				"   </Test>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			MemoryStream stream = new MemoryStream(ASCIIEncoding.ASCII.GetBytes("Hello World!"));
			dom.tests[0].publishers[0] = new MemoryStreamPublisher(stream);

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.config = config;
			e.startFuzzing(dom, config);

			var stateModel = dom.tests[0].stateModel;
			var state = stateModel.initialState;

			Assert.AreEqual(ASCIIEncoding.ASCII.GetBytes("Hello World!"), state.actions[0].dataModel.Value.Value);
		}
	}
}
