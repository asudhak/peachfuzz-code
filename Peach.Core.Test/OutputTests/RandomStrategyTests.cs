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
using Peach.Core.MutationStrategies;

namespace Peach.Core.Test.OutputTests
{
    [TestFixture]
    class RandomStrategyTests
    {
        bool firstPass = true;
        string testString = null;
        string names = null;
        //int? testInt = null;
        List<string> testResults = new List<string>();

        [Test]
        public void Test1()
        {
            Assert.Null("TODO: Fix me!");

            // Test that the repeated iterations are producing the same values.

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"str1\" value=\"Hello, World!\"/>" +
                "       <String name=\"str2\" value=\"Hello, World!\"/>" +
                "       <String name=\"str3\" value=\"Hello, World!\"/>" +
                "       <String name=\"str4\" value=\"Hello, World!\"/>" +
                "       <String name=\"str5\" value=\"Hello, World!\"/>" +
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

                "   <Test name=\"Default\">" +
                "       <Agent ref=\"LocalAgent\"/>" +
                "       <Strategy class=\"Random\">" +
                //"           <Param name=\"Seed\" value=\"10\"/>" +
                "       </Strategy>" +
                "       <StateModel ref=\"TheState\"/>" +
                "       <Publisher class=\"Null\"/>" +
                "   </Test>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("StringCaseMutator");
            dom.tests[0].includedMutators.Add("StringMutator");
            dom.tests[0].includedMutators.Add("UnicodeBomMutator");

            RunConfiguration config = new RunConfiguration();

            MutationStrategies.RandomStrategy.Iterating += new RandomStrategyIterationEventHandler(RandomStrategy_Iterating);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            MutationStrategies.RandomStrategy.Iterating -= RandomStrategy_Iterating;

            // verify values

            // reset
            firstPass = true;
            testString = null;
            testResults.Clear();
        }

        void RandomStrategy_Iterating(string elementName, string mutatorName)
        {
            testResults.Add(mutatorName + " | " + elementName);
        }
    }
}

// end
