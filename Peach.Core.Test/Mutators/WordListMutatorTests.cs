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
    class WordListMutatorTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard tests generating specified valid values from wordlist for each <String> element

            string[] expected = new string[] { "one", "two", "three", "four", "five", "abc", "123" };
            string tempFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";

            try
            {
                File.AppendAllLines(tempFileName, expected);
            }
            catch (IOException ex)
            {
                throw new IOException("No unique temporary file name is available.", ex);
            }

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <String name=\"str1\" value=\"Hello, World!\">" +
                "           <Hint name=\"WordList\" value=\""+tempFileName+"\"/>" +
                "       </String>" +
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
                "       <Strategy class=\"Sequential\"/>" +
                "   </Test>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            dom.tests[0].includedMutators = new List<string>();
            dom.tests[0].includedMutators.Add("WordListMutator");

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            e.startFuzzing(dom, config);

            // verify values
            
            Assert.AreEqual(expected.Length, mutations.Count);
            for (int i = 0; i < mutations.Count; ++i)
            {
                Assert.AreEqual(Variant.VariantType.String, mutations[i].GetVariantType());
                Assert.AreEqual(expected[i], (string)mutations[i]);
            }
        }
    }
}

// end
