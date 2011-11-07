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
    class XmlW3CMutatorTests
    {
        string testString = null;
        byte[] testBytes = new byte[] { };
        List<string> testResults = new List<string>();
        List<byte[]> testResults2 = new List<byte[]>();

        int ctr = 0;

        [Test]
        public void Test1()
        {
            // standard test performing the W3C parser tests for each <String> element, only works with hints: "XMLHint" / "xml"

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"str1\" value=\"Hello, World!\">" +
                "           <Hint name=\"XMLhint\" value=\"xml\"/>" +
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

            // reset
            testString = null;
            testResults.Clear();
        }

        void Action_FinishedTest(Dom.Action action)
        {
            //if (ctr == 0)
            //    testString = (string)action.dataModel[0].InternalValue;
            //else
            //    testBytes = (byte[])action.dataModel[0].InternalValue;
            //ctr++;
                        
            //int wat = 0;
        }
    }
}
