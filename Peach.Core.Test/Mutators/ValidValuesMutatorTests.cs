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
    class ValidValuesMutatorTests
    {
        string testString = null;
        List<string> testResults = new List<string>();

        [Test]
        public void Test1()
        {
            // standard tests generating specified valid values for each <String> element

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"str1\" value=\"Hello, World!\">" +
                "           <Hint name=\"ValidValues\" value=\"one;two;three;four;five;abc;123\"/>" +
                "       </String>" +
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

            // verify values
            Assert.IsTrue(testResults[0] == "one");
            Assert.IsTrue(testResults[1] == "two");
            Assert.IsTrue(testResults[2] == "three");
            Assert.IsTrue(testResults[3] == "four");
            Assert.IsTrue(testResults[4] == "five");
            Assert.IsTrue(testResults[5] == "abc");
            Assert.IsTrue(testResults[6] == "123");

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
