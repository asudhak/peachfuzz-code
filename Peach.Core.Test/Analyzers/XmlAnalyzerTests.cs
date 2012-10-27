
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
    class XmlAnalyzerTests
    {
        [Test]
        public void BasicTest()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                "	<DataModel name=\"TheDataModel\">" +
                "       <String value=\"&lt;Root&gt;&lt;Element1 attrib1=&quot;Attrib1Value&quot; /&gt;&lt;/Root&gt;\"> "+
			    "           <Analyzer class=\"Xml\" /> " +
		        "       </String>"+
                "	</DataModel>" +
                "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            Assert.IsTrue(dom.dataModels["TheDataModel"][0] is Dom.XmlElement);

            var elem1 = dom.dataModels["TheDataModel"][0] as Dom.XmlElement;

            Assert.AreEqual("Root", elem1.elementName);
        }

		[Test]
		public void AdvancedTest()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
	<DataModel name=""TheDataModel"">

		<String value=""&lt;Root&gt;
		                &lt;Element1 attrib1=&quot;Attrib1Value&quot;&gt;
		                Hello
		                &lt;/Element1&gt;
		                &lt;/Root&gt;"">
			<Analyzer class=""Xml""/>
		</String>

	</DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.IsTrue(dom.dataModels["TheDataModel"][0] is Dom.XmlElement);

			var elem1 = dom.dataModels["TheDataModel"][0] as Dom.XmlElement;

			Assert.AreEqual("Root", elem1.elementName);

			var result = dom.dataModels[0].Value;
			Assert.NotNull(result);
		}
    }
}

// end
