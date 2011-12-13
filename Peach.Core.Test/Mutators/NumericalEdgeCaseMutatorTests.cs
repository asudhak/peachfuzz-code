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
    class NumericalEdgeCaseMutatorTests
    {
        int? testValue = null;
        List<int?> listVals = new List<int?>();

        [Test]
        public void Test1()
        {
            // standard test of generating values of +/- 50 around numerical edge cases
            // - testing with a number size of 8, and signed, so edge cases are [0, -128, 127, 255]
            // - if the value produced is out of range, the default value of '0' is returned

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"8\" signed=\"true\"/>" +
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

            // verify values
            Assert.IsTrue(listVals.Count == 405);
            for (int i = 0; i < listVals.Count; ++i)
            {
                Assert.IsTrue(listVals[i] >= sbyte.MinValue);
                Assert.IsTrue(listVals[i] <= sbyte.MaxValue);
            }

            // reset
            testValue = null;
            listVals.Clear();
        }

        [Test]
        public void Test2()
        {
            // testing N-hint
            // : N = 5, generating values of +/- 5 around numerical edge cases
            // - testing with a number size of 8, and signed, so edge cases are [0, -128, 127, 255]
            // - if the value produced is out of range, the default value of '0' is returned

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"8\" signed=\"true\">" +
                "           <Hint name=\"NumericalEdgeCaseMutator-N\" value=\"5\"/>" +
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

            // verify values
            Assert.IsTrue(listVals.Count == 45);
            for (int i = 0; i < listVals.Count; ++i)
            {
                Assert.IsTrue(listVals[i] >= sbyte.MinValue);
                Assert.IsTrue(listVals[i] <= sbyte.MaxValue);
            }

            // reset
            testValue = null;
            listVals.Clear();
        }

        [Test]
        public void Test3()
        {
            // testing unsigned
            // : N = 5, generating values of +/- 5 around numerical edge cases
            // - testing with a number size of 8, and UNsigned, so edge cases are [0, -128, 127, 255], but no negatives
            // - if the value produced is out of range, the default value of '0' is returned

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"8\" signed=\"false\">" +
                "           <Hint name=\"NumericalEdgeCaseMutator-N\" value=\"5\"/>" +
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

            // verify values
            Assert.IsTrue(listVals.Count == 45);
            for (int i = 0; i < listVals.Count; ++i)
            {
                Assert.IsTrue(listVals[i] >= byte.MinValue);
                Assert.IsTrue(listVals[i] <= byte.MaxValue);
            }

            // reset
            testValue = null;
            listVals.Clear();
        }

        //[Test]
        //public void Test3()
        //{
        //    // testing numerical string (strings default to size 32)

        //    string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
        //        "<Peach>" +
        //        "   <DataModel name=\"TheDataModel\">" +
        //        "       <String name=\"numStr1\" value=\"100\">" +
        //        "           <Hint name=\"NumericalString\" value=\"true\"/>" +
        //        "       </String>" +
        //        "   </DataModel>" +

        //        "   <StateModel name=\"TheState\" initialState=\"Initial\">" +
        //        "       <State name=\"Initial\">" +
        //        "           <Action type=\"output\">" +
        //        "               <DataModel ref=\"TheDataModel\"/>" +
        //        "           </Action>" +
        //        "       </State>" +
        //        "   </StateModel>" +

        //        "   <Test name=\"TheTest\">" +
        //        "       <StateModel ref=\"TheState\"/>" +
        //        "       <Publisher class=\"Stdout\"/>" +
        //        "   </Test>" +

        //        "   <Run name=\"DefaultRun\">" +
        //        "       <Test ref=\"TheTest\"/>" +
        //        "   </Run>" +
        //        "</Peach>";

        //    PitParser parser = new PitParser();

        //    Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

        //    RunConfiguration config = new RunConfiguration();

        //    Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

        //    Engine e = new Engine(null);
        //    e.config = config;
        //    e.startFuzzing(dom, config);

        //    // remove start default value (100)
        //    //listVals.RemoveAt(0);

        //    // verify values
        //    //for (int i = 0; i < listVals.Count; ++i)
        //    //    Assert.AreEqual(82 - i, listVals[i]);

        //    // reset
        //    testValue = null;
        //    listVals.Clear();
        //}

        //[Test]
        //public void Test4()
        //{
        //    // testing INVALID use of numerical string, this should produce 0 results

        //    string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
        //        "<Peach>" +
        //        "   <DataModel name=\"TheDataModel\">" +
        //        "       <String name=\"numStr1\" value=\"abc\">" +
        //        "           <Hint name=\"NumericalString\" value=\"true\"/>" +
        //        "       </String>" +
        //        "   </DataModel>" +

        //        "   <StateModel name=\"TheState\" initialState=\"Initial\">" +
        //        "       <State name=\"Initial\">" +
        //        "           <Action type=\"output\">" +
        //        "               <DataModel ref=\"TheDataModel\"/>" +
        //        "           </Action>" +
        //        "       </State>" +
        //        "   </StateModel>" +

        //        "   <Test name=\"TheTest\">" +
        //        "       <StateModel ref=\"TheState\"/>" +
        //        "       <Publisher class=\"Stdout\"/>" +
        //        "   </Test>" +

        //        "   <Run name=\"DefaultRun\">" +
        //        "       <Test ref=\"TheTest\"/>" +
        //        "   </Run>" +
        //        "</Peach>";

        //    PitParser parser = new PitParser();

        //    Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

        //    RunConfiguration config = new RunConfiguration();

        //    Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

        //    Engine e = new Engine(null);
        //    e.config = config;
        //    e.startFuzzing(dom, config);

        //    // listVals should be empty!!
        //    Assert.IsEmpty(listVals);
        //}

        //[Test]
        //public void Test5()
        //{
            // testing odd sizes, they should round up to the next power of 2
            // : size = 10 should become 16, and then generating [0, 16 + 50]
            // : - something currently crashes this in the PitParser?

            //string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            //    "<Peach>" +
            //    "   <DataModel name=\"TheDataModel\">" +
            //    "       <Number name=\"num1\" size=\"10\" value=\"100\" signed=\"false\"/>" +
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

            //PitParser parser = new PitParser();

            //Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            //RunConfiguration config = new RunConfiguration();

            //Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            //Engine e = new Engine(null);
            //e.config = config;
            //e.startFuzzing(dom, config);

            //// remove start default value (100)
            //listVals.RemoveAt(0);

            //// verify values
            //for (int i = 0; i < listVals.Count; ++i)
            //    Assert.AreEqual(66 - i, listVals[i]);

            //// reset
            //testValue = null;
            //listVals.Clear();
        //}

        //[Test]
        //public void Test6()
        //{
        //    // standard test of generating values size + 50 through size - 50
        //    // - signed = "true" so we will receive negative results

        //    string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
        //        "<Peach>" +
        //        "   <DataModel name=\"TheDataModel\">" +
        //        "       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"true\"/>" +
        //        "   </DataModel>" +

        //        "   <StateModel name=\"TheState\" initialState=\"Initial\">" +
        //        "       <State name=\"Initial\">" +
        //        "           <Action type=\"output\">" +
        //        "               <DataModel ref=\"TheDataModel\"/>" +
        //        "           </Action>" +
        //        "       </State>" +
        //        "   </StateModel>" +

        //        "   <Test name=\"TheTest\">" +
        //        "       <StateModel ref=\"TheState\"/>" +
        //        "       <Publisher class=\"Stdout\"/>" +
        //        "   </Test>" +

        //        "   <Run name=\"DefaultRun\">" +
        //        "       <Test ref=\"TheTest\"/>" +
        //        "   </Run>" +
        //        "</Peach>";

        //    PitParser parser = new PitParser();

        //    Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

        //    RunConfiguration config = new RunConfiguration();

        //    Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

        //    Engine e = new Engine(null);
        //    e.config = config;
        //    e.startFuzzing(dom, config);

        //    // remove start default value (100)
        //    //listVals.RemoveAt(0);

        //    // verify values
        //    //for (int i = 0; i < listVals.Count; ++i)
        //    //    Assert.AreEqual(82 - i, listVals[i]);
        //}

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
