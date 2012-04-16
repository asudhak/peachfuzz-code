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

namespace Peach.Core.Test.Monitors
{
    [TestFixture]
    class ReplayMonitorTests
    {
        [Test]
        public void Test1()
        {
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

                "   <Test name=\"TheTest\">" +
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
            //Assert.IsTrue(testResults[0] == "hello, world!");
            //Assert.IsTrue(testResults[1] == "HELLO, WORLD!");
            //Assert.IsFalse(testResults[2] == "hello, world!");
            //Assert.IsFalse(testResults[2] == "HELLO, WORLD!");

            // reset
            //firstPass = true;
            //testString = null;
            //testResults.Clear();
        }

        void Action_FinishedTest(Dom.Action action)
        {
            //if (firstPass)
            //{
            //    firstPass = false;
            //}
            //else
            //{
            //    testString = (string)action.dataModel[0].InternalValue;
            //    testResults.Add(testString);
            //}
        }
    }
}

// end
