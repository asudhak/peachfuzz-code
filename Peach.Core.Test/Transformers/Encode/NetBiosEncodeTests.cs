using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.Transformers.Encode
{
    [TestFixture]
    class NetBiosEncodeTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Block name=\"TheBlock\">" +
                "           <Transformer class=\"NetBiosEncode\"/>" +
                "           <Blob name=\"Data\" value=\"abcdef\"/>" +
                "       </Block>" +
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

                "   <Run name=\"DefaultRun\">" +
                "       <Test ref=\"TheTest\"/>" +
                "   </Run>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            RunConfiguration config = new RunConfiguration();
            config.singleIteration = true;

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            // -- this is the pre-calculated result from Peach2.3 on the blob: "abcdef" (this becomes "GBGCGDGEGFGG")
            byte[] precalcResult = new byte[] { 0x47, 0x42, 0x47, 0x43, 0x47, 0x44, 0x47, 0x45, 0x47, 0x46, 0x47, 0x47 };
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(precalcResult, values[0].Value);
        }
    }
}

// end
