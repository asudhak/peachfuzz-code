
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

namespace Peach.Core.Test.PitParserTests
{
	[TestFixture]
	class BlockTests
	{
		[Test]
		public void TestOverWrite()
		{
			string xml = @"
<Peach>
	<DataModel name='Base'>
		<Block name='b'>
			<String name='s1' value='Hello'/>
			<String name='s2' value='World'/>
		</Block>
	</DataModel>

	<DataModel name='Derived' ref='Base'>
		<String name='b.s2' value='Hello'/>
	</DataModel>
</Peach>";

			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(2, dom.dataModels.Count);
			Assert.AreEqual("HelloWorld", Encoding.ASCII.GetString(dom.dataModels[0].Value.Value));
			Assert.AreEqual("HelloHello", Encoding.ASCII.GetString(dom.dataModels[1].Value.Value));
		}

		[Test]
		public void TestAddNewChild()
		{
			string xml = @"
<Peach>
	<DataModel name='Base'>
		<Block name='b'>
			<String name='s1' value='Hello'/>
			<String name='s2' value='World'/>
		</Block>
		<String value='.'/>
	</DataModel>

	<DataModel name='Derived' ref='Base'>
		<String name='b.s3' value='Hello'/>
	</DataModel>
</Peach>";

			var parser = new PitParser();
			var dom = parser.asParser(null, new MemoryStream(Encoding.ASCII.GetBytes(xml)));

			Assert.AreEqual(2, dom.dataModels.Count);
			Assert.AreEqual("HelloWorld.", Encoding.ASCII.GetString(dom.dataModels[0].Value.Value));
			Assert.AreEqual("HelloWorldHello.", Encoding.ASCII.GetString(dom.dataModels[1].Value.Value));
		}
	}
}
