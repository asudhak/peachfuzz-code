
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
    class XmlAnalyzerTests : DataModelCollector
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

		[Test]
		public void UnicodeTest()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
	<DataModel name=""TheDataModel"">

		<String type=""utf8"" value=""&lt;Root&gt;
		                &lt;Element1 attrib1=&quot;{0} Attrib1Value&quot;&gt;
		                Hello {1}
		                &lt;/Element1&gt;
		                &lt;/Root&gt;"">
			<Analyzer class=""Xml""/>
		</String>

	</DataModel>
</Peach>".Fmt("\u0134", "\x0298");

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.UTF8.GetBytes(xml)));

			Assert.IsTrue(dom.dataModels["TheDataModel"][0] is Dom.XmlElement);

			var elem1 = dom.dataModels["TheDataModel"][0] as Dom.XmlElement;

			Assert.AreEqual("Root", elem1.elementName);

			var result = dom.dataModels[0].Value;
			var str = Encoding.UTF8.GetString(result.ToArray());
			Assert.NotNull(result);
			Assert.NotNull(str);
		}

		[Test]
		public void CrackXml1()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String>
			<Analyzer class='Xml'/>
		</String>
	</DataModel>
</Peach>
";

			string payload = @"<element>&lt;foo&gt;</element>";

			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", payload);

			var cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(payload, dom.dataModels[0].InternalValue.BitsToString());

		}

		[Test]
		public void CrackXml2()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String>
			<Analyzer class='Xml'/>
		</String>
	</DataModel>
</Peach>
";

			string payload = @"<Peach xmlns=""http://peachfuzzer.com/2012/Peach"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><Foo xsi:type=""Bar"" /></Peach>";

			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", payload);

			var cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			var actual = dom.dataModels[0].InternalValue.BitsToString();
			Assert.AreEqual(payload, actual);
		}

		[Test]
		public void CrackXml3()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String>
			<Analyzer class='Xml'/>
		</String>
	</DataModel>
</Peach>
";

			string payload = @"<?xml version=""1.0"" encoding=""utf-8""?><Peach xmlns=""http://peachfuzzer.com/2012/Peach"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><Foo xsi:type=""Bar"" /></Peach>";

			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", payload);

			var cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			var actual = dom.dataModels[0].InternalValue.BitsToString();
			Assert.AreEqual(payload, actual);
		}

		[Test]
		public void CrackXml4()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String>
			<Analyzer class='Xml'/>
		</String>
	</DataModel>
</Peach>
";

			string payload = @"<?xml version=""1.0"" encoding=""utf-16"" standalone=""yes""?><Peach xmlns=""http://peachfuzzer.com/2012/Peach"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><Foo xsi:type=""Bar"" /></Peach>";

			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", payload);

			var cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			var bytes = dom.dataModels[0].Value.ToArray();
			var actual = Encoding.Unicode.GetString(bytes);
			Assert.AreEqual(payload, actual);
		}

		[Test]
		public void Fuzz1()
		{
			// Trying to emit xmlns="" is invalid, have to remove xmlns attr
			// Swap attribute with element neighbor == no change

			string tmp = Path.GetTempFileName();

			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String type='utf8'>
			<Analyzer class='Xml'/>
		</String>
	</DataModel>

	<StateModel name='SM' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='DM'/>
				<Data fileName='{0}'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<Strategy class='Sequential'/>
		<StateModel ref='SM'/>
		<Publisher class='Null'/>
	</Test>
</Peach>
".Fmt(tmp);

			string payload = @"<Peach xmlns=""http://peachfuzzer.com/2012/Peach"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><Foo xsi:type=""Bar"">Text</Foo></Peach>";
			File.WriteAllText(tmp, payload);

			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			uint count = 0;

			var config = new RunConfiguration();
			var e = new Engine(null);
			e.IterationStarting += (ctx, curr, total) => ++count;
			e.startFuzzing(dom, config);

			Assert.Greater(count, 12000);
		}

		[Test]
		public void Fuzz2()
		{
			// Trying to emit xmlns="" is invalid, have to remove xmlns attr
			// Swap attribute with element neighbor == no change

			string tmp = Path.GetTempFileName();

			string xml = @"
<Peach>
	<DataModel name='DM'>
		<String>
			<Analyzer class='Xml'/>
		</String>
	</DataModel>

	<StateModel name='SM' initialState='Initial'>
		<State name='Initial'>
			<Action type='output'>
				<DataModel ref='DM'/>
				<Data fileName='{0}'/>
			</Action>
		</State>
	</StateModel>

	<Test name='Default'>
		<Strategy class='Sequential'/>
		<StateModel ref='SM'/>
		<Publisher class='Null'/>
	</Test>
</Peach>
".Fmt(tmp);

			string payload = @"<Peach xmlns=""http://peachfuzzer.com/2012/Peach"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><Foo xsi:type=""Bar"">Text</Foo></Peach>";
			File.WriteAllText(tmp, payload);

			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			uint count = 0;

			var config = new RunConfiguration();
			var e = new Engine(null);
			e.IterationStarting += (ctx, curr, total) => ++count;
			e.startFuzzing(dom, config);

			int same = 0;

			for (int i = 0; i < dataModels.Count; ++i)
			{
				var final = Encoding.ISOLatin1.GetString(dataModels[i].Value.ToArray());
				if (final == payload)
					++same;
			}

			Assert.Greater(count, 0);
			Assert.AreEqual(count, dataModels.Count);
			Assert.AreEqual(3, same);
		}
	}
}

// end
