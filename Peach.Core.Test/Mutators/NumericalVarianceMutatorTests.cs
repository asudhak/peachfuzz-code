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
    class NumericalVarianceMutatorTests
    {
		int? testValue = null;
        List<int?> listVals = new List<int?>();

        [Test]
        public void Test1()
        {
            // standard test of generating values 50 - 150

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\"/>" +
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

            // remove start / end default values (100)
            listVals.RemoveAt(0);
            listVals.RemoveAt(listVals.Count - 1);

            // verify values
            for (int i = 0; i <= 100; ++i)
                Assert.AreEqual(150 - i, listVals[i]);

            // reset
            testValue = null;
            listVals.Clear();
        }

        [Test]
        public void Test2()
        {
            // testing N-hint
            // : N = 5, generating values 95 - 105

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\">" +
                "           <Hint name=\"NumericalVarianceMutator-N\" value=\"5\"/>" +
                "       </Number>" +
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

            // remove start / end default values (100)
            listVals.RemoveAt(0);
            listVals.RemoveAt(listVals.Count - 1);

            // verify values
            for (int i = 0; i <= 10; ++i)
                Assert.AreEqual(105 - i, listVals[i]);

            // reset
            testValue = null;
            listVals.Clear();
        }

        [Test]
        public void Test3()
        {
            // testing numerical string

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"numStr1\" value=\"100\">" +
                "           <Hint name=\"NumericalString\" value=\"true\"/>" +
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

            // remove start / end default values (100)
            listVals.RemoveAt(0);
            listVals.RemoveAt(listVals.Count - 1);

            // verify values
            for (int i = 0; i <= 100; ++i)
                Assert.AreEqual(150 - i, listVals[i]);

            // reset
            testValue = null;
            listVals.Clear();
        }

        [Test]
        public void Test4()
        {
            // testing INVALID use of numerical string, this should produce 0 results

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"numStr1\" value=\"abc\">" +
                "           <Hint name=\"NumericalString\" value=\"true\"/>" +
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

            // listVals should be empty!!
            Assert.IsEmpty(listVals);
        }

        void Action_FinishedTest(Dom.Action action)
		{
            // handle numbers
            if (action.dataModel[0] is Number)
            {
                testValue = (int)action.dataModel[0].InternalValue;
                listVals.Add(testValue);
            }
            // handle numerical strings
            else if (action.dataModel[0] is Dom.String)
            {
                int test = 0;
                if (Int32.TryParse((string)action.dataModel[0].InternalValue, out test))
                {
                    testValue = test;
                    listVals.Add(testValue);
                }
            }
		}
    }
}

// end
