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
    class FiniteRandomNumbersMutatorTests
    {
        bool firstPass = true;

        int? testValue = null;
        List<int?> listVals = new List<int?>();

        uint? testValueUInt = null;
        List<uint?> listValsUInt = new List<uint?>();

        long? testValueLong = null;
        List<long?> listValsLong = new List<long?>();

        ulong? testValueULong = null;
        List<ulong?> listValsULong = new List<ulong?>();

        [Test]
        public void Test1()
        {
            // standard test of generating 5000 random values for each <Number> element

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
				"		<Strategy class=\"Sequencial\"/>" +
				"   </Test>" +

                "   <Run name=\"DefaultRun\">" +
                "       <Test ref=\"TheTest\"/>" +
                "   </Run>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("FiniteRandomNumbersMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.IsTrue(listVals.Count == 5000);

            // reset
            firstPass = true;
            testValue = null;
            listVals.Clear();
        }

        [Test]
        public void Test2()
        {
            // testing N-hint
            // : N = 5, generating 5 random values for each <Number> element

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"true\">" +
                "           <Hint name=\"FiniteRandomNumbersMutator-N\" value=\"5\"/>" +
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
            dom.tests[0].includedMutators.Add("FiniteRandomNumbersMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.IsTrue(listVals.Count == 5);

            // reset
            firstPass = true;
            testValue = null;
            listVals.Clear();
        }

        [Test]
        public void Test3()
        {
            // testing numerical string with N = 10
            // -- will produce [0, UInt32.Max]

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"numStr1\" value=\"100\">" +
                "           <Hint name=\"NumericalString\" value=\"true\"/>" +
                "           <Hint name=\"FiniteRandomNumbersMutator-N\" value=\"10\"/>" +
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
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("FiniteRandomNumbersMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTestUInt);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.IsTrue(listValsUInt.Count == 10);

            // reset
            firstPass = true;
            testValueUInt = null;
            listValsUInt.Clear();
        }

        [Test]
        public void Test4()
        {
            // testing generating 100 Int32's

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"32\" signed=\"true\">" +
                "           <Hint name=\"FiniteRandomNumbersMutator-N\" value=\"100\"/>" +
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
            dom.tests[0].includedMutators.Add("FiniteRandomNumbersMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.IsTrue(listVals.Count == 100);
            for (int i = 0; i < listVals.Count; ++i)
            {
                Assert.GreaterOrEqual(listVals[i], int.MinValue);
                Assert.LessOrEqual(listVals[i], int.MaxValue);
            }

            // reset
            firstPass = true;
            testValue = null;
            listVals.Clear();
        }

        [Test]
        public void Test5()
        {
            // testing generating 100 UInt32's

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"32\" signed=\"false\">" +
                "           <Hint name=\"FiniteRandomNumbersMutator-N\" value=\"100\"/>" +
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
            dom.tests[0].includedMutators.Add("FiniteRandomNumbersMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTestUInt);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.IsTrue(listValsUInt.Count == 100);
            for (int i = 0; i < listValsUInt.Count; ++i)
            {
                Assert.GreaterOrEqual(listValsUInt[i], uint.MinValue);
                Assert.LessOrEqual(listValsUInt[i], uint.MaxValue);
            }

            // reset
            firstPass = true;
            testValueUInt = null;
            listValsUInt.Clear();
        }

        [Test]
        public void Test6()
        {
            // testing generating 100 Int64's

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"64\" signed=\"true\">" +
                "           <Hint name=\"FiniteRandomNumbersMutator-N\" value=\"100\"/>" +
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
            dom.tests[0].includedMutators.Add("FiniteRandomNumbersMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTestLong);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.IsTrue(listValsLong.Count == 100);
            for (int i = 0; i < listValsLong.Count; ++i)
            {
                Assert.GreaterOrEqual(listValsLong[i], long.MinValue);
                Assert.LessOrEqual(listValsLong[i], long.MaxValue);
            }

            // reset
            firstPass = true;
            testValueLong = null;
            listValsLong.Clear();
        }

        [Test]
        public void Test7()
        {
            // testing generating 100 UInt64's

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"64\" signed=\"false\">" +
                "           <Hint name=\"FiniteRandomNumbersMutator-N\" value=\"100\"/>" +
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
            dom.tests[0].includedMutators.Add("FiniteRandomNumbersMutator");

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTestULong);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.IsTrue(listValsULong.Count == 100);
            for (int i = 0; i < listValsULong.Count; ++i)
            {
                Assert.GreaterOrEqual(listValsULong[i], ulong.MinValue);
                Assert.LessOrEqual(listValsULong[i], ulong.MaxValue);
            }

            // reset
            firstPass = true;
            testValueULong = null;
            listValsULong.Clear();
        }

        void Action_FinishedTest(Dom.Action action)
        {
            if (firstPass)
            {
                firstPass = false;
            }
            else
            {
                testValue = (int)action.dataModel[0].InternalValue;
                listVals.Add(testValue);
            }
        }

        void Action_FinishedTestUInt(Dom.Action action)
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
                    testValueUInt = (uint)action.dataModel[0].InternalValue;
                    listValsUInt.Add(testValueUInt);
                }
                // handle numerical strings
                else if (action.dataModel[0] is Dom.String)
                {
                    uint test = 0;
                    if (UInt32.TryParse((string)action.dataModel[0].InternalValue, out test))
                    {
                        testValueUInt = test;
                        listValsUInt.Add(testValueUInt);
                    }
                }
            }
        }

        void Action_FinishedTestLong(Dom.Action action)
        {
            if (firstPass)
            {
                firstPass = false;
            }
            else
            {
                testValueLong = (long)action.dataModel[0].InternalValue;
                listValsLong.Add(testValueLong);
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
                testValueULong = (ulong)action.dataModel[0].InternalValue;
                listValsULong.Add(testValueULong);
            }
        }
    }
}

// end
