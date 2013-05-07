
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
//   Michael Eddington (mike@dejavusecurity.com)

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
    class BinaryAnalyzerTests
    {
        [Test]
        public void BasicTest()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                "	<DataModel name=\"TheDataModel\">" +
                "       <Blob name=\"TheBlob\">" +
                "           <Analyzer class=\"Binary\"> "+
                "               <Param name=\"AnalyzeStrings\" value=\"false\"/> "+
                "           </Analyzer> "+
                "       </Blob>"+
                "	</DataModel>" +
                "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            Random rnd = new Random(123);

            BitStream data = new BitStream();
            data.LittleEndian();

            for (int cnt = 0; cnt < 100; cnt++)
                data.WriteInt32(rnd.NextInt32());

            data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));

            for (int cnt = 0; cnt < 100; cnt++)
                data.WriteInt32(rnd.NextInt32());

            data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Peach Fuzzer"));

            for (int cnt = 0; cnt < 100; cnt++)
                data.WriteInt32(rnd.NextInt32());
            
            data.SeekBits(0, SeekOrigin.Begin);

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);
            data.SeekBytes(0, SeekOrigin.Begin);

            Assert.IsTrue(dom.dataModels["TheDataModel"][0] is Block);
            Assert.AreEqual("TheBlob", dom.dataModels["TheDataModel"][0].name);
            Assert.AreEqual(data.Value, dom.dataModels["TheDataModel"].Value.Value);

            var block = dom.dataModels["TheDataModel"][0] as Block;
            Assert.IsTrue(block[5] is Dom.String);
            Assert.AreEqual("Hello WorldYY&", (string)block[5].InternalValue);

            Assert.IsTrue(block[11] is Dom.String);
            Assert.AreEqual("Peach Fuzzer|", (string)block[11].InternalValue);
        }
    }
}

// end
