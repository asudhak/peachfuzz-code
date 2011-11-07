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
using Peach.Core.Cracker;
using Peach.Core.IO;

namespace Peach.Core.Test.Mutators
{
    [TestFixture]
    class StringMutatorTests
    {
        string testString = null;
        List<string> testResults = new List<string>();

        [Test]
        public void Test1()
        {
            // standard test generating odd unicode strings for each <String> element

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

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // remove starting default string ("Hello, World!")
            testResults.RemoveAt(0);

            // verify first two values, last two values, and count (= 2379)
            string val1 = "Peach";
            string val2 = "abcdefghijklmnopqrstuvwxyz";
            string val3 = "18446744073709551664";
            string val4 = "10";

            Assert.AreEqual(2379, testResults.Count);
            Assert.AreEqual(val1, testResults[0]);
            Assert.AreEqual(val2, testResults[1]);
            Assert.AreEqual(val3, testResults[testResults.Count - 2]);
            Assert.AreEqual(val4, testResults[testResults.Count - 1]);

            // reset
            testString = null;
            testResults.Clear();
        }

        void Action_FinishedTest(Dom.Action action)
        {
            testString = (string)action.dataModel[0].InternalValue;
            testResults.Add(testString);
        }
    }
}
