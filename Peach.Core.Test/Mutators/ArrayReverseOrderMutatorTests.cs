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
    class ArrayReverseOrderMutatorTests
    {
        bool firstPass = true;
        byte[] testValue;
        List<byte[]> listVals = new List<byte[]>();

        [Test]
        public void Test1()
        {
            // standard test - will reverse the order of the array
            // 01234 -> 43210

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"a0\" value=\"0\" maxOccurs=\"100\"/>" +
                "   </DataModel>" +

                "   <StateModel name=\"TheState\" initialState=\"Initial\">" +
                "       <State name=\"Initial\">" +
                "           <Action type=\"output\">" +
                "               <DataModel ref=\"TheDataModel\"/>" +
                "           </Action>" +
                "       </State>" +
                "   </StateModel>" +

                "   <Test name=\"Default\">" +
                "       <StateModel ref=\"TheState\"/>" +
                "       <Publisher class=\"Null\"/>" +
				"		<Strategy class=\"Sequencial\"/>" +
				"   </Test>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("ArrayReverseOrderMutator");

            var myArray = (Dom.Array)dom.tests[0].stateModel.initialState.actions[0].dataModel[0];
            myArray.origionalElement = myArray[0];
            myArray.hasExpanded = true;
            myArray.Add(new Dom.String("a1", "1"));
            myArray.Add(new Dom.String("a2", "2"));
            myArray.Add(new Dom.String("a3", "3"));
            myArray.Add(new Dom.String("a4", "4"));

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            Assert.IsTrue(listVals[0].Length == 5);
            Assert.AreEqual(listVals[0][0], (byte)('4'));
            Assert.AreEqual(listVals[0][1], (byte)('3'));
            Assert.AreEqual(listVals[0][2], (byte)('2'));
            Assert.AreEqual(listVals[0][3], (byte)('1'));
            Assert.AreEqual(listVals[0][4], (byte)('0'));

            // reset
            firstPass = true;
            testValue = null;
            listVals.Clear();
			Dom.Action.Finished -= Action_FinishedTest;
        }

        void Action_FinishedTest(Dom.Action action)
        {
            if (firstPass)
            {
                firstPass = false;
            }
            else
            {
                testValue = action.dataModel[0].Value.Value;
                listVals.Add(testValue);
            }
        }
    }
}

// end
