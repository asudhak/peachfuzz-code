
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

namespace Peach.Core.Test.CrackingTests
{
	[TestFixture]
	public class ConstraintTests
	{
		[Test]
		public void ConstraintChoice1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Choice>" +
				"			<Blob name=\"Blob10\" length=\"5\" constraint=\"len(value) &lt; 3\" />" +
				"			<Blob name=\"Blob5\" length=\"5\" />" +
				"		</Choice>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", new byte[] { 1, 2, 3, 4, 5 });

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.IsTrue(dom.dataModels[0][0] is Choice);
			Assert.AreEqual("Blob5", ((Choice)dom.dataModels[0][0])[0].name);
			Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5 }, ((DataElementContainer)dom.dataModels[0][0])[0].DefaultValue.BitsToArray());
		}

		[Test]
		public void ConstraintChoice2()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Choice>" +
				"			<Blob name=\"Blob10\" length=\"5\" constraint=\"len(value) &gt; 3\" />" +
				"			<Blob name=\"Blob5\" length=\"5\" />" +
				"		</Choice>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", new byte[] { 1, 2, 3, 4, 5 });

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.IsTrue(dom.dataModels[0][0] is Choice);
			Assert.AreEqual("Blob10", ((Choice)dom.dataModels[0][0])[0].name);
			Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5 }, ((DataElementContainer)dom.dataModels[0][0])[0].DefaultValue.BitsToArray());
		}

		[Test]
		public void ConstraintRegex()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<Import import=\"re\"/>" +
				"	<Import import=\"code\"/>" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<String constraint=\"re.search('^\\w+$', value) != None\"/>" +
				"	</DataModel>" +
				"</Peach>";

			try
			{
				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				var data = Bits.Fmt("{0}", "Hello");

				DataCracker cracker = new DataCracker();
				cracker.CrackData(dom.dataModels[0], data);

				Assert.AreEqual("Hello", (string)dom.dataModels[0][0].DefaultValue);
			}
			finally
			{
				Scripting.Imports.Clear();
			}
		}

		[Test]
		public void ConstraintNumberRelation()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Block minOccurs='0'>
			<Number size='8' constraint='int(value) > 0'>
				<Relation type='size' of='value'/>
			</Number>
			<Blob name='value'/>
		</Block>
		<Number size='8'/>
		<Blob/>
	</DataModel>
</Peach>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", new byte[] { 1, 1, 2, 2, 2, 3, 3, 3, 3, 0, 4, 4, 4, 4 });

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(3, dom.dataModels[0].Count);

			var array = dom.dataModels[0][0] as Dom.Array;
			Assert.NotNull(array);
			Assert.AreEqual(3, array.Count);

			var b1 = array[0] as Dom.Block;
			Assert.NotNull(b1);
			Assert.AreEqual(1, (int)b1[0].DefaultValue);
			Assert.AreEqual(new byte[] { 1 }, b1[0].Value.ToArray());

			var b2 = array[1] as Dom.Block;
			Assert.NotNull(b2);
			Assert.AreEqual(2, (int)b2[0].DefaultValue);
			Assert.AreEqual(new byte[] { 2, 2 }, b2[1].Value.ToArray());

			var b3 = array[2] as Dom.Block;
			Assert.NotNull(b3);
			Assert.AreEqual(3, (int)b3[0].DefaultValue);
			Assert.AreEqual(new byte[] { 3, 3, 3 }, b3[1].Value.ToArray());

			var num = dom.dataModels[0][1] as Dom.Number;
			Assert.NotNull(num);
			Assert.AreEqual(0, (int)num.DefaultValue);

			var blob = dom.dataModels[0][2] as Dom.Blob;
			Assert.NotNull(blob);
			Assert.AreEqual(new byte[] { 4, 4, 4, 4 }, blob.Value.ToArray());
		}

		[Test]
		public void ConstraintChoice()
		{
			string xml = @"
<Peach>
	<DataModel name='DM'>
		<Choice>
			<Choice name='choice' constraint='element.SelectedElement.name == ""str10""'>
				<String name='str10' length='10'/>
				<String name='strX' />
			</Choice>
			<String name='unsized' />
		</Choice>
	</DataModel>
</Peach>
";

			var cracker = new DataCracker();
			var parser = new PitParser();

			var dom1 = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			var data1 = Bits.Fmt("{0}", "HelloWorld");
			cracker.CrackData(dom1.dataModels[0], data1);

			Assert.AreEqual(1, dom1.dataModels[0].Count);
			var choice1 = dom1.dataModels[0][0] as Dom.Choice;
			Assert.NotNull(choice1);
			Assert.AreEqual("choice", choice1.SelectedElement.name);

			var dom2 = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			var data2 = Bits.Fmt("{0}", "Hello");
			cracker.CrackData(dom2.dataModels[0], data2);

			Assert.AreEqual(1, dom2.dataModels[0].Count);
			var choice2 = dom2.dataModels[0][0] as Dom.Choice;
			Assert.NotNull(choice2);
			Assert.AreEqual("unsized", choice2.SelectedElement.name);
		}
	}
}

// end
