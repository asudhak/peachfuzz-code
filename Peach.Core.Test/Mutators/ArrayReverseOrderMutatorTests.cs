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

            var myArray = (Dom.Array)dom.runs[0].tests[0].stateModel.initialState.actions[0].dataModel[0];
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
            Assert.IsTrue(listVals[1].Length == 5);
            Assert.AreEqual(listVals[1][0], (byte)('4'));
            Assert.AreEqual(listVals[1][1], (byte)('3'));
            Assert.AreEqual(listVals[1][2], (byte)('2'));
            Assert.AreEqual(listVals[1][3], (byte)('1'));
            Assert.AreEqual(listVals[1][4], (byte)('0'));

            // reset
            testValue = null;
            listVals.Clear();
        }

        void Action_FinishedTest(Dom.Action action)
        {
            testValue = action.dataModel[0].Value.Value;
            listVals.Add(testValue);
        }
    }
}

// end
