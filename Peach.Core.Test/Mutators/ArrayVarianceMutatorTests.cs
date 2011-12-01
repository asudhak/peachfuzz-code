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
    class ArrayVarianceMutatorTests
    {
        //bool firstPass = true;
        //byte[] result = new byte[] { };
        //List<byte[]> testResults = new List<byte[]>();

        [Test]
        public void Test1()
        {
            // standard test of flipping 20% of the bits in a blob
            // : in this case, we'll use 1 byte with a value of 0, so we should get 1 bit flipped.

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"string1\" value=\"Hello, World!\" maxOccurs=\"1024\"/>" +
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

            var myArray = (Dom.Array)dom.dataModels[0][0];
            myArray.hasExpanded = true;
            myArray.occurs = 3;
            myArray.origionalElement = myArray[0];
            myArray.Add(new Dom.String("string1-1", "pos1"));
            myArray.Add(new Dom.String("string1-2", "pos2"));

            RunConfiguration config = new RunConfiguration();

            Dom.Action.Finished += new ActionFinishedEventHandler(Action_FinishedTest);

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            //Assert.AreNotEqual(0, testResults[0]);

            // reset
            //firstPass = true;
            //testResults.Clear();
        }

        void Action_FinishedTest(Dom.Action action)
        {
        }
    }
}
