using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using Peach.Core;
using Peach.Core.IO;

namespace Peach.Core.Test
{
	[TestFixture]
	class VariantTests
	{
		[Test]
		public void TestToString()
		{
			string str;

			str = new Variant((int)5).ToString();
			Assert.AreEqual("5", str);
			str = new Variant((int)-5).ToString();
			Assert.AreEqual("-5", str);
			str = new Variant((long)5).ToString();
			Assert.AreEqual("5", str);
			str = new Variant((long)-5).ToString();
			Assert.AreEqual("-5", str);
			str = new Variant((ulong)5).ToString();
			Assert.AreEqual("5", str);
			str = new Variant("short str").ToString();
			Assert.AreEqual("short str", str);
			str = new Variant(Encoding.ASCII.GetBytes("short str")).ToString();
			Assert.AreEqual("73 68 6f 72 74 20 73 74 72", str);
			str = new Variant(new BitStream(Encoding.ASCII.GetBytes("short str"))).ToString();
			Assert.AreEqual("73 68 6f 72 74 20 73 74 72", str);

			string longStr = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Nam cursus. Morbi ut mi. Nullam enim leo, egestas id, condimentum at, laoreet mattis, massa. Sed eleifend nonummy diam. Praesent mauris ante, elementum et, bibendum at, posuere sit amet, nibh. Duis tincidunt lectus quis dui viverra vestibulum. Suspendisse vulputate aliquam dui. Nulla elementum dui ut augue. Aliquam vehicula mi at mauris. Maecenas placerat, nisl at consequat rhoncus, sem nunc gravida justo, quis eleifend arcu velit quis lacus. Morbi magna magna, tincidunt a, mattis non, imperdiet vitae, tellus. Sed odio est, auctor ac, sollicitudin in, consequat vitae, orci. Fusce id felis. Vivamus sollicitudin metus eget eros.";
			str = new Variant(longStr).ToString();
			Assert.AreEqual("Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Nam cu.. (Len: 692 chars)", str);

			byte[] bytes = Encoding.ASCII.GetBytes(longStr);
			str = new Variant(bytes).ToString();
			Assert.AreEqual("4c 6f 72 65 6d 20 69 70 73 75 6d 20 64 6f 6c 6f 72 20 73 69 74 20 61 6d 65 74 2c 20 63 6f 6e 73.. (Len: 692 bytes)", str);

			BitStream bs = new BitStream(bytes);
			str = new Variant(bs).ToString();
			Assert.AreEqual("4c 6f 72 65 6d 20 69 70 73 75 6d 20 64 6f 6c 6f 72 20 73 69 74 20 61 6d 65 74 2c 20 63 6f 6e 73.. (Len: 5536 bits)", str);
		}
	}
}
