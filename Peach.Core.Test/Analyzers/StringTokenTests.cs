
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//  Michael Eddington (mike@dejavusecurity.com)
//	Adam Cecchetti (adam@dejavusecurity.com) 

// $Id$

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

namespace Peach.Core.Test.Analyzers
{
    [TestFixture]
    internal class StringTokenTests
    {
        [Test]
        public void BasicTest()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                         "	<DataModel name=\"TheDataModel\">" +
                         "		<String name=\"TheString\">" +
                         "			<Analyzer class=\"StringToken\" />" +
                         "		</String>" +
                         "	</DataModel>" +
                         "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            var data = Bits.Fmt("{0}", "Hello World");

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual("Hello World", dom.dataModels[0][0].InternalValue.BitsToString());
            Assert.AreEqual(3, ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0]).Count);
        }

        [Test]
        public void NoTokens()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                         "	<DataModel name=\"TheDataModel\">" +
                         "		<String name=\"TheString\">" +
                         "			<Analyzer class=\"StringToken\" />" +
                         "		</String>" +
                         "	</DataModel>" +
                         "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            var data = Bits.Fmt("{0}", "HelloWorld");

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual("HelloWorld", (string)dom.dataModels[0][0].InternalValue);
            Assert.AreEqual(1, ((DataElementContainer) dom.dataModels[0]).Count);
        }

        [Test]
        public void JustTokens()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                         "	<DataModel name=\"TheDataModel\">" +
                         "		<String name=\"TheString\">" +
                         "			<Analyzer class=\"StringToken\" />" +
                         "		</String>" +
                         "	</DataModel>" +
                         "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            var data = Bits.Fmt("{0}", "\r\n\"'[]{}<>` \t.,~!@#$%^?&*_=+-|\\:;/");

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual("\r\n\"'[]{}<>` \t.,~!@#$%^?&*_=+-|\\:;/", dom.dataModels[0][0].InternalValue.BitsToString());
            Assert.AreEqual(3, ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0]).Count);
        }

        [Test]
        public void SingleTokenMiddle()
        {
             string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                         "	<DataModel name=\"TheDataModel\">" +
                         "		<String name=\"TheString\">" +
                         "			<Analyzer class=\"StringToken\" />" +
                         "		</String>" +
                         "	</DataModel>" +
                         "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            var data = Bits.Fmt("{0}", "AAAA:AAAA");

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual("AAAA:AAAA", dom.dataModels[0][0].InternalValue.BitsToString());
            Assert.AreEqual("AAAA",((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0])[0].InternalValue.ToString());           
            Assert.AreEqual(":", ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0])[1].InternalValue.ToString());           
            Assert.AreEqual("AAAA", ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0])[2].InternalValue.ToString());           
            Assert.AreEqual(3, ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0]).Count);           
        }

        [Test]
        public void SingleTokenEnd()
        {
             string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                         "	<DataModel name=\"TheDataModel\">" +
                         "		<String name=\"TheString\">" +
                         "			<Analyzer class=\"StringToken\" />" +
                         "		</String>" +
                         "	</DataModel>" +
                         "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            var data = Bits.Fmt("{0}", "AAAAAAAA:");

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual("AAAAAAAA:", dom.dataModels[0][0].InternalValue.BitsToString());
            Assert.AreEqual("AAAAAAAA",((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0])[0].InternalValue.ToString());           
            Assert.AreEqual(":", ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0])[1].InternalValue.ToString());           
            Assert.AreEqual(3, ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0]).Count);           
        }

        [Test]
        public void SingleTokenBegin()
        {
             string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                         "	<DataModel name=\"TheDataModel\">" +
                         "		<String name=\"TheString\">" +
                         "			<Analyzer class=\"StringToken\" />" +
                         "		</String>" +
                         "	</DataModel>" +
                         "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            var data = Bits.Fmt("{0}", ":AAAAAAAA");

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual(":AAAAAAAA" ,dom.dataModels[0][0].InternalValue.BitsToString());
            Assert.AreEqual(":", ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0])[1].InternalValue.ToString());           
            Assert.AreEqual("AAAAAAAA",((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0])[2].InternalValue.ToString());           
            Assert.AreEqual(3, ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0]).Count);           
        }

        [Test]
        public void StringTokenAllWhiteSpaceTokens()
        {
              string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                         "	<DataModel name=\"TheDataModel\">" +
                         "		<String name=\"TheString\">" +
                         "			<Analyzer class=\"StringToken\" />" +
                         "		</String>" +
                         "	</DataModel>" +
                         "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            var data = Bits.Fmt("{0}", @"\t\n\r ");

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual(@"\t\n\r ", dom.dataModels[0][0].InternalValue.BitsToString());
            Assert.AreEqual(3, ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0]).Count);                      
        }

		[Test]
		public void StringTokenSizeTracking()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String>
			<Analyzer class='StringToken'/>
		</String>
	</DataModel>
</Peach>";

			var history = new List<string>();
			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			var data = Bits.Fmt("{0}", @"<Peach>");
			var cracker = new DataCracker();

			cracker.EnterHandleNodeEvent += (elem, pos, bs) => { history.Add("Enter: " + pos.ToString()); };
			cracker.ExitHandleNodeEvent += (elem, pos, bs) => { history.Add("Exit: " + pos.ToString()); };
			cracker.AnalyzerEvent += (elem, bs) => { history.Add("Analyze Elem"); };

			cracker.CrackData(dom.dataModels[0], data);

			var expected = new string[]
			{
				"Enter: 0", // Begin DataModel
				"Enter: 0", // Begin String
				"Exit: 56", // End String
				"Exit: 56", // End Data Model
				"Analyze Elem",
				"Enter: 0", // Top level block
				    "Enter: 0",     // '<' Token Block
				       "Enter: 0",
				       "Exit: 0",
				       "Enter: 0",  // '<' Token begin
				       "Exit: 8",   // '<' Token end
				       "Enter: 8",
				           "Enter: 8", // 'Peach' begin
				           "Exit: 48", // 'Peach' end
				           "Enter: 48", // '>' Token begin
				           "Exit: 56",  // '>' Token end
				           "Enter: 56",
				           "Exit: 56",
				       "Exit: 56",
				    "Exit: 56",
				"Exit: 56",
			};

			Assert.AreEqual(expected, history.ToArray());
		}
    }
}

// end
