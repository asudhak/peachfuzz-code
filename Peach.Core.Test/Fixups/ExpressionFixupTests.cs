using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core.IO;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.Fixups
{
    [TestFixture]
    class ExpressionFixupTests
    {
       

        [Test]
        public void IntTest()
        {
            

            // standard test

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"CRC\" size=\"32\" signed=\"false\">" +
                "           <Fixup class=\"ExpressionFixup\">" +
                "               <Param name=\"ref\" value=\"Data\"/>" +
                "               <Param name=\"expression\" value=\"42\"/>" +
                "           </Fixup>" +
                "       </Number>" +
                "       <Blob name=\"Data\" value=\"12345\"/>" +
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
                "       <Publisher class=\"Stdout\"/>" +
                "   </Test>" +

                "   <Run name=\"DefaultRun\">" +
                "       <Test ref=\"TheTest\"/>" +
                "   </Run>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            RunConfiguration config = new RunConfiguration();
            config.singleIteration = true;
            
            byte[] testValue = null;
            Dom.Action.Finished += delegate(Dom.Action action) { testValue = action.dataModel[0].Value.Value; };

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values
            
            Assert.AreEqual(testValue, new byte[] { 42, 0x00, 0x00, 0x00 });

            
        }

        [Test]
        public void StringTest()
        {


            // standard test

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"CRC\" length='4' signed=\"false\">" +
                "           <Fixup class=\"ExpressionFixup\">" +
                "               <Param name=\"ref\" value=\"Data\"/>" +
                "               <Param name=\"expression\" value=\"'AABB'\"/>" +
                "           </Fixup>" +
                "       </Blob>" +
                "       <Blob name=\"Data\" value=\"12345\"/>" +
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
                "       <Publisher class=\"Stdout\"/>" +
                "   </Test>" +

                "   <Run name=\"DefaultRun\">" +
                "       <Test ref=\"TheTest\"/>" +
                "   </Run>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            RunConfiguration config = new RunConfiguration();
            config.singleIteration = true;

            byte[] testValue = null;
            Dom.Action.Finished += delegate(Dom.Action action) { testValue = action.dataModel[0].Value.Value; };

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values

            Assert.AreEqual(new byte[] { 0x41, 0x41, 0x42, 0x42 }, testValue);


        }


        [Test]
        public void ByteTest()
        {


            // standard test

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"CRC\" length=\"4\" signed=\"false\">" +
                "           <Fixup class=\"ExpressionFixup\">" +
                "               <Param name=\"ref\" value=\"Data\"/>" +
                "               <Param name=\"expression\" value=\"'\\x00\\x01\\xff\\x00'\"/>" +
                "           </Fixup>" +
                "       </Blob>" +
                "       <Blob name=\"Data\" value=\"12345\"/>" +
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
                "       <Publisher class=\"Stdout\"/>" +
                "   </Test>" +

                "   <Run name=\"DefaultRun\">" +
                "       <Test ref=\"TheTest\"/>" +
                "   </Run>" +
                "</Peach>";

            PitParser parser = new PitParser();

            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            RunConfiguration config = new RunConfiguration();
            config.singleIteration = true;

            byte[] testValue = null;
            Dom.Action.Finished += delegate(Dom.Action action) { testValue = action.dataModel[0].Value.Value; };

            Engine e = new Engine(null);
            e.config = config;
            e.startFuzzing(dom, config);

            // verify values

            Assert.AreEqual(new byte[] { 0x00, 0x01, 0xff, 0x00 },testValue);


        }

    }
}

// end
