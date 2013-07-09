
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
	class NumberTests
	{
		[Test]
		public void CrackNumber1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\" signed=\"true\"/>" +
				"		<Number size=\"16\" signed=\"true\"/>" +
				"		<Number size=\"8\" signed=\"true\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0:L8}{1:L16}{2:L8}", 0x10, 0xbb8, 0x19);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(0x10, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(0xbb8, (int)dom.dataModels[0][1].DefaultValue);
			Assert.AreEqual(0x19, (int)dom.dataModels[0][2].DefaultValue);
		}

		[Test]
		public void CrackNumber2()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"2\" signed=\"true\"/>" +
				"		<Number size=\"2\" signed=\"false\"/>" +
				"		<Number size=\"3\" signed=\"true\"/>" +
				"		<Number size=\"9\" signed=\"false\"/>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			var data = Bits.Fmt("{0}", (ushort)0xffff);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual(-1, (int)dom.dataModels[0][0].DefaultValue);
			Assert.AreEqual(3, (int)dom.dataModels[0][1].DefaultValue);
			Assert.AreEqual(-1, (int)dom.dataModels[0][2].DefaultValue);
			Assert.AreEqual(511, (int)dom.dataModels[0][3].DefaultValue);
		}
	}
}

// end
