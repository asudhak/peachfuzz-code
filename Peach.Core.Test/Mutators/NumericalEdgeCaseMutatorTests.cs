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
    class NumericalEdgeCaseMutatorTests
    {
        bool firstPass = true;

        long? testValue = null;
        List<long?> listVals = new List<long?>();

        ulong? ulongTestValue = null;
        List<ulong?> ulongListVals = new List<ulong?>();

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
				"		<Strategy class=\"Sequencial\"/>" +
				"   </Test>" +

                "   <Run name=\"DefaultRun\">" +
                "       <Test ref=\"TheTest\"/>" +
                "   </Run>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("NumericalEdgeCaseMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            for (int i = 0; i < listVals.Count; ++i)
            {
                Assert.IsTrue(listVals[i] >= sbyte.MinValue);
                Assert.IsTrue(listVals[i] <= sbyte.MaxValue);
            }

            // reset
            firstPass = true;
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
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("NumericalEdgeCaseMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            for (int i = 0; i < listVals.Count; ++i)
            {
                Assert.IsTrue(listVals[i] >= sbyte.MinValue);
                Assert.IsTrue(listVals[i] <= sbyte.MaxValue);
            }

            // reset
            firstPass = true;
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
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("NumericalEdgeCaseMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            for (int i = 0; i < listVals.Count; ++i)
            {
                Assert.IsTrue(listVals[i] >= byte.MinValue);
                Assert.IsTrue(listVals[i] <= byte.MaxValue);
            }

            // reset
            firstPass = true;
            testValue = null;
            listVals.Clear();
        }

        [Test]
        public void Test4()
        {
            // testing larger sized numbers
            // - int16

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"16\" signed=\"true\"/>" +
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
            dom.tests[0].includedMutators.Add("NumericalEdgeCaseMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            for (int i = 0; i < listVals.Count; ++i)
            {
                Assert.IsTrue(listVals[i] >= short.MinValue);
                Assert.IsTrue(listVals[i] <= short.MaxValue);
            }

            // reset
            firstPass = true;
            testValue = null;
            listVals.Clear();
        }

        [Test]
        public void Test5()
        {
            // testing larger sized numbers
            // - uint16

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"16\" signed=\"false\"/>" +
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
            dom.tests[0].includedMutators.Add("NumericalEdgeCaseMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            for (int i = 0; i < listVals.Count; ++i)
            {
                Assert.IsTrue(listVals[i] >= ushort.MinValue);
                Assert.IsTrue(listVals[i] <= ushort.MaxValue);
            }

            // reset
            firstPass = true;
            testValue = null;
            listVals.Clear();
        }

        [Test]
        public void Test6()
        {
            // testing larger sized numbers
            // - int32

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"32\" signed=\"true\"/>" +
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
            dom.tests[0].includedMutators.Add("NumericalEdgeCaseMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            for (int i = 0; i < listVals.Count; ++i)
            {
                Assert.IsTrue(listVals[i] >= int.MinValue);
                Assert.IsTrue(listVals[i] <= int.MaxValue);
            }

            // reset
            firstPass = true;
            testValue = null;
            listVals.Clear();
        }

        [Test]
        public void Test7()
        {
            // testing larger sized numbers
            // - uint32

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"32\" signed=\"false\"/>" +
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
            dom.tests[0].includedMutators.Add("NumericalEdgeCaseMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            for (int i = 0; i < listVals.Count; ++i)
            {
                Assert.IsTrue(listVals[i] >= uint.MinValue);
                Assert.IsTrue(listVals[i] <= uint.MaxValue);
            }

            // reset
            firstPass = true;
            testValue = null;
            listVals.Clear();
        }

        [Test]
        public void Test8()
        {
            // testing larger sized numbers
            // - int64

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"64\" signed=\"true\"/>" +
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
            dom.tests[0].includedMutators.Add("NumericalEdgeCaseMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            for (int i = 0; i < listVals.Count; ++i)
            {
                Assert.IsTrue(listVals[i] >= long.MinValue);
                Assert.IsTrue(listVals[i] <= long.MaxValue);
            }

            // reset
            firstPass = true;
            testValue = null;
            listVals.Clear();
        }

        [Test]
        public void Test9()
        {
            // testing larger sized numbers
            // - uint64

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"64\" signed=\"false\"/>" +
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
            dom.tests[0].includedMutators.Add("NumericalEdgeCaseMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTestULong);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            for (int i = 0; i < ulongListVals.Count; ++i)
            {
                Assert.IsTrue(ulongListVals[i] >= ulong.MinValue);
                Assert.IsTrue(ulongListVals[i] <= ulong.MaxValue);
            }

            // reset
            firstPass = true;
            ulongTestValue = null;
            ulongListVals.Clear();
        }

        void Action_FinishedTest(Dom.Action action)
        {
            if (firstPass)
            {
                firstPass = false;
            }
            else
            {
                // handle numbers
                if (action.dataModel[0] is Number)
                {
                    testValue = (long)action.dataModel[0].InternalValue;
                    listVals.Add(testValue);
                }
                // handle numerical strings
                else if (action.dataModel[0] is Dom.String)
                {
                    long test = 0;
                    if (Int64.TryParse((string)action.dataModel[0].InternalValue, out test))
                    {
                        testValue = test;
                        listVals.Add(testValue);
                    }
                }
            }
        }

        void Action_FinishedTestULong(Dom.Action action)
        {
            if (firstPass)
            {
                firstPass = false;
            }
            else
            {
                ulongTestValue = (ulong)action.dataModel[0].InternalValue;
                ulongListVals.Add(ulongTestValue);
            }
        }
    }
}

// end
