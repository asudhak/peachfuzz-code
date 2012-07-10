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
//using Peach.Core.MutationStrategies;

namespace Peach.Core.Test.Monitors
{
    [TestFixture]
    class ReplayMonitorTests
    {
        bool firstPass = true;
        string testString = null;
        List<Variant> testResults = new List<Variant>();

        [Test]
        public void Test1()
        {
            // Test that the repeated iterations are producing the same values.

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"str1\" value=\"Hello, World!\"/>" +
                "   </DataModel>" +

                "   <StateModel name=\"TheState\" initialState=\"Initial\">" +
                "       <State name=\"Initial\">" +
                "           <Action type=\"output\">" +
                "               <DataModel ref=\"TheDataModel\"/>" +
                "           </Action>" +
                "       </State>" +
                "   </StateModel>" +

                "   <Agent name=\"LocalAgent\">" +
                "       <Monitor class=\"ReplayMonitor\">" +
                "           <Param name=\"Name\" value=\"ReplayMonitor1\"/>" +
                "       </Monitor>" +
                "   </Agent>" +

                "   <Test name=\"TheTest\">" +
                "       <Agent ref=\"LocalAgent\"/>" +
                "       <StateModel ref=\"TheState\"/>" +
                "       <Publisher class=\"Stdout\"/>" +
                "   </Test>" +

                "   <Run name=\"DefaultRun\">" +
                "       <Test ref=\"TheTest\"/>" +
                "   </Run>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("StringCaseMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.AreEqual(testResults[0], testResults[1]);
            Assert.AreEqual(testResults[2], testResults[3]);
            Assert.AreEqual(testResults[4], testResults[5]);

            // reset
            firstPass = true;
            testString = null;
            testResults.Clear();
        }

        void Action_FinishedTest(Dom.Action action)
        {
            if (firstPass)
            {
                firstPass = false;
            }
            else
            {
                testResults.Add(action.dataModel[0].InternalValue);
            }
        }
    }
}

// end
