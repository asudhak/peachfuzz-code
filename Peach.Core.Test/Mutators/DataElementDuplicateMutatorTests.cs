using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;

namespace Peach.Core.Test.Mutators
{
    [TestFixture]
    class DataElementDuplicateMutatorTests
    {
        bool firstPass = true;
        List<DataModel> results = new List<DataModel>();

        [Test]
        public void Test1()
        {
            // standard test of duplicating elements from the data model (2x - 50x)

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num0\" size=\"16\" signed=\"false\"/>" +
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
            dom.tests[0].includedMutators.Add("DataElementDuplicateMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            for (int i = 0; i < 49; ++i)
                Assert.AreEqual(i + 2, results[i].Count);

            // reset
            firstPass = true;
            results.Clear();
        }

        [Test]
        public void Test2()
        {
            //string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            //    "<Peach>" +
            //    "   <DataModel name=\"TheDataModel\">" +
            //    "       <Number name=\"num0\" size=\"16\" signed=\"false\"/>" +
            //    "       <Number name=\"num1\" size=\"16\" signed=\"false\"/>" +
            //    "   </DataModel>" +

            //    "   <StateModel name=\"TheState\" initialState=\"Initial\">" +
            //    "       <State name=\"Initial\">" +
            //    "           <Action type=\"output\">" +
            //    "               <DataModel ref=\"TheDataModel\"/>" +
            //    "           </Action>" +
            //    "       </State>" +
            //    "   </StateModel>" +

            //    "   <Test name=\"TheTest\">" +
            //    "       <StateModel ref=\"TheState\"/>" +
            //    "       <Publisher class=\"Stdout\"/>" +
            //    "   </Test>" +

            //    "   <Run name=\"DefaultRun\">" +
            //    "       <Test ref=\"TheTest\"/>" +
            //    "   </Run>" +
            //    "</Peach>";

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"Png\">" +
                "       <Blob/>" +
                "   </DataModel>" +

                "   <Agent name=\"LocalAgent\">" +
                "       <Monitor class=\"WindowsDebugEngine\">" +
                "           <Param name=\"CommandLine\" value=\"C:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe fuzzed.png\"/>" +
                "           <Param name=\"WinDbgPath\" value=\"C:\\Program Files (x86)\\Debugging Tools for Windows (x86)\"/>" +
                "           <Param name=\"StartOnCall\" value=\"ScoobySnacks\"/>" +
                "       </Monitor>" +
                "   </Agent>" +

                "   <StateModel name=\"TheState\" initialState=\"Initial\">" +
                "       <State name=\"Initial\">" +
                "           <Action type=\"output\">" +
                "               <DataModel ref=\"Png\"/>" +
                "               <Data fileName=\"c:\\sample.png\"/>" +
                "           </Action>" +
                "           <Action type=\"close\"/>" +
                "           <Action type=\"call\" method=\"ScoobySnacks\" publisher=\"Peach.Agent\"/>" +
                "       </State>" +
                "   </StateModel>" +

                "   <Test name=\"TheTest\">" +
                "       <Agent ref=\"LocalAgent\"/>" +
                "       <StateModel ref=\"TheState\"/>" +
                "       <Publisher class=\"File\"/>" +
                "           <Param name=\"FileName\" value=\"fuzzed.png\"/>" +
                "       </Publisher>" +
                "   </Test>" +

                "   <Run name=\"DefaultRun\">" +
                "       <Test ref=\"TheTest\"/>" +
                "   </Run>" +
                "</Peach>";
            
            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("DataElementDuplicateMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values

            // reset
            firstPass = true;
            results.Clear();
        }

        void Action_FinishedTest(Dom.Action action)
        {
            if (firstPass)
            {
                firstPass = false;
            }
            else
            {
                results.Add(action.dataModel);
            }
        }
    }
}

// end
