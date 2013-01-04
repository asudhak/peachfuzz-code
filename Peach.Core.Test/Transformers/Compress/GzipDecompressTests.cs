using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.Transformers.Compress
{
    [TestFixture]
    class GzipDecompressTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test
			var valueData = new MemoryStream(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			var data = new MemoryStream();
			using (GZipStream zip = new GZipStream(data, CompressionMode.Compress))
			{
				valueData.CopyTo(zip);
			}

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "			<Transformer class=\"GzipDecompress\"/>" +
                "           <Blob name=\"Data\"/>" +
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
			var dataModel = dom.tests[0].stateModel.states["Initial"].actions[0].dataModel;
			dataModel["Data"].DefaultValue = new Variant(data.ToArray());

            RunConfiguration config = new RunConfiguration();
            config.singleIteration = true;

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

			dataModel = dom.tests[0].stateModel.states["Initial"].actions[0].dataModel;
			Assert.AreEqual("Hello World", ASCIIEncoding.ASCII.GetString(dataModel.Value.Value));
        }
    }
}

// end
