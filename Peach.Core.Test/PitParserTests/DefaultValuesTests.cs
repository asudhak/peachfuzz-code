using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;

using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.PitParserTests
{
	[TestFixture]
	class DefaultValuesTests
	{
		public void TestEncoding(Encoding enc)
		{
			string val = (enc != Encoding.Default) ? "encoding=\"" + enc.HeaderName + "\"" : "";
			string xml = "<?xml version=\"1.0\" " + val + "?>\r\n" +
				"<Peach>\r\n" +
				"	<DataModel name=\"##VAR1##\">\r\n" +
				"		<String name=\"##VAR2##\"/>\r\n" +
				"	</DataModel>\r\n" +
				"</Peach>";

			var defaultValues = new Dictionary<string, string>();
			defaultValues["VAR1"] = "TheDataModel";
			defaultValues["VAR2"] = "SomeString";
			Dictionary<string, object> parserArgs = new Dictionary<string, object>();
			parserArgs[PitParser.DEFINED_VALUES] = defaultValues;

			string pitFile = Path.GetTempFileName();

			using (FileStream f = File.OpenWrite(pitFile))
			{
				using (StreamWriter sw = new StreamWriter(f, enc))
				{
					sw.Write(xml);
				}
			}

			Engine e = new Engine(null);
			Dom.Dom dom = Analyzer.defaultParser.asParser(parserArgs, pitFile);
			dom.evaulateDataModelAnalyzers();

			Assert.AreEqual(1, dom.dataModels.Count);
			Assert.AreEqual("TheDataModel", dom.dataModels[0].name);
			Assert.AreEqual(1, dom.dataModels[0].Count);
			Assert.AreEqual("SomeString", dom.dataModels[0][0].name);
		}

		[Test]
		public void TestDefault()
		{
			TestEncoding(Encoding.Default);
		}

		[Test]
		public void TestUtf8()
		{
			TestEncoding(Encoding.UTF8);
		}

		[Test]
		public void TestUtf16()
		{
			TestEncoding(Encoding.Unicode);
		}

		[Test]
		public void TestUtf32()
		{
			TestEncoding(Encoding.UTF32);
		}

		[Test]
		public void TestUtf16BE()
		{
			TestEncoding(Encoding.BigEndianUnicode);
		}
	}
}
