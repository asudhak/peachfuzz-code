
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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Peach.Core;
using Peach.Core.IO;
using Peach.Core.Dom;
using Peach.Core.Cracker;
using Peach.Core.Analyzers;

using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Peach.Core.Test
{
	[TestFixture]
	class ReportedTests
	{
		/// <summary>
		/// From Sirus.
		/// </summary>
		/// <remarks>
		/// input data attached.   For some reason stalls in the ObjectCopier clone method 
		/// trying to deserialze a 120 megabyte (!) memory stream of a Dom.Number... Wonder 
		/// what the heck the serializer is doing..
		/// </remarks>
		[Test]
		public void ObjectCopierExplode()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
<DataModel name=""GeneratedModel"">
            <Block name=""0"">
                <Number name=""1"" signed=""false"" size=""32""/>
                <Block name=""2"">
                    <Number name=""3"" signed=""false"" size=""32""/>
                    <Number name=""4"" maxOccurs=""9999"" minOccurs=""0"" signed=""false"" size=""32"">
                        <Relation type=""count"" of=""5""/>
                    </Number>
                    <Blob name=""5""/>
                </Block>
            </Block>
    </DataModel>
</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			Assert.NotNull(dom);

			var newDom = ObjectCopier.Clone<Dom.Dom>(dom);

			Assert.NotNull(newDom);
		}

		/// <summary>
		/// Reported by Sirus
		/// </summary>
		[Test]
		public void CrackExplode()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
<DataModel name=""GeneratedModel"">
            <Block name=""0"">
                <Number name=""1"" size=""32""/>
                <Block name=""2"">
                    <Relation type=""size"" from=""4""/>
                    <Number name=""3"" size=""32""/>
                    <Number name=""4"" size=""32"">
                        <Relation type=""size"" of=""2""/>
                    </Number>
                </Block>
            </Block>
    </DataModel>
</Peach>
";
			byte[] dataBytes = new byte[] { 
						0x5f,						0xfa,
						0x8a,						0x68,
						0x09,						0x00,
						0x00,						0x00,
						0x9a,						0x3d,
						0x19,						0x28,
						0xc7,						0x1c,
						0x03,						0x9a,
						0xa6 };

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(dataBytes);
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.NotNull(dom);

			var newDom = ObjectCopier.Clone<Dom.Dom>(dom);

			Assert.NotNull(newDom);
		}

		/// <summary>
		/// Reported by Sirus
		/// </summary>
		[Test]
		public void CrackExplode2()
		{
			string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Peach>
<DataModel name=""GeneratedModel"">
            <Block name=""0"">
                <Block name=""1"">
                    <Relation type=""size"" from=""4""/>
                    <Number name=""3"" size=""32"">
                        <Relation type=""size"" of=""2""/>
                    </Number>
                    <Number name=""4"" size=""32"">
                        <Relation type=""size"" of=""1""/>
                    </Number>
                </Block>
                <Blob name=""2"">
                    <Relation type=""size"" from=""3""/>
                </Blob>
            </Block>
    </DataModel>
</Peach>";
			byte[] dataBytes = new byte[] { 
						0x1b,
						0x53,    0xcb,
						0x22,    0x01,
						0x00,    0x00,
						0x00,    0x05 };

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(dataBytes);
			data.SeekBits(0, SeekOrigin.Begin);

			try
			{
				DataCracker cracker = new DataCracker();
				cracker.CrackData(dom.dataModels[0], data);
				Assert.IsTrue(false);
			}
			catch (CrackingFailure)
			{
				Assert.IsTrue(true);
			}
		}
	}
}
