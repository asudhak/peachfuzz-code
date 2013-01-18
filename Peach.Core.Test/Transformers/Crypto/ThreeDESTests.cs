using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.Transformers.Crypto
{
    [TestFixture]
    class ThreeDESTests : DataModelCollector
    {

        [Test]
        public void Test1()
        {
            // standard test

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" + 
                "        <Blob name=\"Data\" value=\"Hello\">" +
                "           <Transformer class=\"ThreeDES\">" +
                "               <Param name=\"Key\" value=\"c89de1a4237def182afec153\"/>" +
                "               <Param name=\"IV\" value=\"password\"/>" +           
                "           </Transformer>" +
                "        </Blob>" +
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
                "   </Test>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            RunConfiguration config = new RunConfiguration();
            config.singleIteration = true;

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            // -- this is the pre-calculated result on the blob: "Hello"
            byte[] precalcResult = new byte[] { 0xf6, 0xb6, 0x18, 0x6a, 0x4d, 0x6d, 0x4f, 0xa0 };
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(precalcResult, values[0].Value);
        }
    }
}
