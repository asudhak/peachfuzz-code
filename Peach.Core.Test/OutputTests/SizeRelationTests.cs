
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

namespace Peach.Core.Test.OutputTests
{
	[TestFixture]
	class SizeRelationTests
	{
		[Test]
		public void OutputSizeOf1()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Number size=\"8\">" +
				"			<Relation type=\"size\" of=\"Data\" />" +
				"		</Number>" +
				"		<Blob name=\"Data\" />" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.WriteInt8((sbyte)"Hello World".Length);
			data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			byte[] newData = ASCIIEncoding.ASCII.GetBytes("This is a much longer value than before!");
			dom.dataModels[0][1].DefaultValue = new Variant(newData);

			Assert.AreEqual(newData.Length, (int)dom.dataModels[0][0].InternalValue);
		}

		void RunRelation(string len, string value, string encoding, string lengthType, byte[] expected, bool throws)
		{
			// Use expressionGet/expressionSet so we can have negative numbers
			string xml = @"
<Peach>
	<DataModel name='TheDataModel'>
		<String name='Length' length='{0}' lengthType='{1}' type='{2}' padCharacter='0'>
			<Relation type='size' of='Data' expressionSet='size - 3' expressionGet='size + 3'/>
			<Hint name='NumericalString' value='true' />
		</String>
		<String name='Data' value='{3}'/>
	</DataModel>
</Peach>".Fmt(len, lengthType, encoding, value);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			try
			{
				var final = dom.dataModels[0].Value.Value;
				Assert.AreEqual(expected, final);
			}
			catch (SoftException se)
			{
				Assert.True(throws);
				string msg = "Error, String 'TheDataModel.Length' numeric value '{0}' could not be converted to a {1}-{2} {3} string.".Fmt(
					value.Length - 3, len, lengthType.TrimEnd('s'), encoding);
				Assert.AreEqual(msg, se.Message);
			}
		}

		[Test]
		public void TestStringSize()
		{
			// For 0-9, utf7, utf8 and utf9 are all single byte per char
			var expected = Encoding.ASCII.GetBytes("01013 Byte Len *");

			RunRelation("3", "13 Byte Len *", "ascii", "bytes", expected, false);
			RunRelation("1", "13 Byte Len *", "ascii", "bytes", expected, true);
			RunRelation("3", "13 Byte Len *", "ascii", "chars", expected, false);
			RunRelation("1", "13 Byte Len *", "ascii", "chars", expected, true);
			RunRelation("24", "13 Byte Len *", "ascii", "bits", expected, false);
			RunRelation("8", "13 Byte Len *", "ascii", "bits", expected, true);

			RunRelation("3", "13 Byte Len *", "utf7", "bytes", expected, false);
			RunRelation("1", "13 Byte Len *", "utf7", "bytes", expected, true);
			RunRelation("3", "13 Byte Len *", "utf7", "chars", expected, false);
			RunRelation("1", "13 Byte Len *", "utf7", "chars", expected, true);
			RunRelation("24", "13 Byte Len *", "utf7", "bits", expected, false);
			RunRelation("8", "13 Byte Len *", "utf7", "bits", expected, true);

			RunRelation("3", "13 Byte Len *", "utf8", "bytes", expected, false);
			RunRelation("1", "13 Byte Len *", "utf8", "bytes", expected, true);
			RunRelation("3", "13 Byte Len *", "utf8", "chars", expected, false);
			RunRelation("1", "13 Byte Len *", "utf8", "chars", expected, true);
			RunRelation("24", "13 Byte Len *", "utf8", "bits", expected, false);
			RunRelation("8", "13 Byte Len *", "utf8", "bits", expected, true);

			var bs = new BitStream();
			bs.WriteBytes(Encoding.Unicode.GetBytes("010"));
			bs.WriteBytes(Encoding.ASCII.GetBytes("13 Byte Len *"));
			bs.SeekBits(0, SeekOrigin.Begin);
			expected = bs.Value;

			RunRelation("6", "13 Byte Len *", "utf16", "bytes", expected, false);
			RunRelation("2", "13 Byte Len *", "utf16", "bytes", expected, true);
			RunRelation("3", "13 Byte Len *", "utf16", "chars", expected, false);
			RunRelation("1", "13 Byte Len *", "utf16", "chars", expected, true);
			RunRelation("48", "13 Byte Len *", "utf16", "bits", expected, false);
			RunRelation("16", "13 Byte Len *", "utf16", "bits", expected, true);

			bs = new BitStream();
			bs.WriteBytes(Encoding.BigEndianUnicode.GetBytes("010"));
			bs.WriteBytes(Encoding.ASCII.GetBytes("13 Byte Len *"));
			bs.SeekBits(0, SeekOrigin.Begin);
			expected = bs.Value;

			RunRelation("6", "13 Byte Len *", "utf16be", "bytes", expected, false);
			RunRelation("2", "13 Byte Len *", "utf16be", "bytes", expected, true);
			RunRelation("3", "13 Byte Len *", "utf16be", "chars", expected, false);
			RunRelation("1", "13 Byte Len *", "utf16be", "chars", expected, true);
			RunRelation("48", "13 Byte Len *", "utf16be", "bits", expected, false);
			RunRelation("16", "13 Byte Len *", "utf16be", "bits", expected, true);

			bs = new BitStream();
			bs.WriteBytes(Encoding.UTF32.GetBytes("010"));
			bs.WriteBytes(Encoding.ASCII.GetBytes("13 Byte Len *"));
			bs.SeekBits(0, SeekOrigin.Begin);
			expected = bs.Value;

			RunRelation("12", "13 Byte Len *", "utf32", "bytes", expected, false);
			RunRelation("4", "13 Byte Len *", "utf32", "bytes", expected, true);
			RunRelation("3", "13 Byte Len *", "utf32", "chars", expected, false);
			RunRelation("1", "13 Byte Len *", "utf32", "chars", expected, true);
			RunRelation("96", "13 Byte Len *", "utf32", "bits", expected, false);
			RunRelation("32", "13 Byte Len *", "utf32", "bits", expected, true);
		}

		[Test]
		public void TestNegative()
		{
			var expected = Encoding.ASCII.GetBytes("-02.");

			RunRelation("3", ".", "ascii", "bytes", expected, false);
			RunRelation("1", ".", "ascii", "bytes", expected, true);
			RunRelation("3", ".", "ascii", "chars", expected, false);
			RunRelation("1", ".", "ascii", "chars", expected, true);
			RunRelation("24", ".", "ascii", "bits", expected, false);
			RunRelation("8", ".", "ascii", "bits", expected, true);

			RunRelation("3", ".", "utf7", "bytes", expected, false);
			RunRelation("1", ".", "utf7", "bytes", expected, true);
			RunRelation("3", ".", "utf7", "chars", expected, false);
			RunRelation("1", ".", "utf7", "chars", expected, true);
			RunRelation("24", ".", "utf7", "bits", expected, false);
			RunRelation("8", ".", "utf7", "bits", expected, true);

			RunRelation("3", ".", "utf8", "bytes", expected, false);
			RunRelation("1", ".", "utf8", "bytes", expected, true);
			RunRelation("3", ".", "utf8", "chars", expected, false);
			RunRelation("1", ".", "utf8", "chars", expected, true);
			RunRelation("24", ".", "utf8", "bits", expected, false);
			RunRelation("8", ".", "utf8", "bits", expected, true);
		}


	}
}

// end

