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
    class DataElementRemoveMutatorTests
    {
        bool firstPass = true;
        List<DataModel> results = new List<DataModel>();

        [Test]
        public void Test1()
        {
            // standard test of removing elements from the data model

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num0\" size=\"32\" signed=\"true\" value=\"41\"/>" +
                "       <Number name=\"num1\" size=\"32\" signed=\"true\" value=\"42\"/>" +
                "       <Number name=\"num2\" size=\"32\" signed=\"true\" value=\"43\"/>" +
                "       <Number name=\"num3\" size=\"32\" signed=\"true\" value=\"44\"/>" +
                "       <Number name=\"num4\" size=\"32\" signed=\"true\" value=\"45\"/>" +
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
            dom.tests[0].includedMutators.Add("DataElementRemoveMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.IsTrue(results.Count == 5);

            Assert.IsTrue(results[0].Count == 4);
            Assert.AreEqual(results[0][0].name, "num1");
            Assert.AreEqual(results[0][1].name, "num2");
            Assert.AreEqual(results[0][2].name, "num3");
            Assert.AreEqual(results[0][3].name, "num4");

            Assert.IsTrue(results[1].Count == 4);
            Assert.AreEqual(results[1][0].name, "num0");
            Assert.AreEqual(results[1][1].name, "num2");
            Assert.AreEqual(results[1][2].name, "num3");
            Assert.AreEqual(results[1][3].name, "num4");

            Assert.IsTrue(results[2].Count == 4);
            Assert.AreEqual(results[2][0].name, "num0");
            Assert.AreEqual(results[2][1].name, "num1");
            Assert.AreEqual(results[2][2].name, "num3");
            Assert.AreEqual(results[2][3].name, "num4");

            Assert.IsTrue(results[3].Count == 4);
            Assert.AreEqual(results[3][0].name, "num0");
            Assert.AreEqual(results[3][1].name, "num1");
            Assert.AreEqual(results[3][2].name, "num2");
            Assert.AreEqual(results[3][3].name, "num4");

            Assert.IsTrue(results[4].Count == 4);
            Assert.AreEqual(results[4][0].name, "num0");
            Assert.AreEqual(results[4][1].name, "num1");
            Assert.AreEqual(results[4][2].name, "num2");
            Assert.AreEqual(results[4][3].name, "num3");

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
