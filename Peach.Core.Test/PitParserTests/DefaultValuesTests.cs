using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;

using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test.PitParserTests
{
	[TestFixture]
	class DefaultValuesTests
	{
		public void TestEncoding(Encoding enc, bool defaultArgs)
		{
			int codepage = 0;
			string encoding = "";

			if (enc != null)
			{
				codepage = enc.CodePage;
				if (enc is UnicodeEncoding)
					encoding = Encoding.Unicode.HeaderName;
				else
					encoding = enc.HeaderName;
			}

			string val = !string.IsNullOrEmpty(encoding) ? "encoding=\"" + encoding + "\"" : "";
			string xml = "<?xml version=\"1.0\" " + val + "?>\r\n" +
				"<Peach>\r\n" +
				"	<DataModel name=\"##VAR1##\">\r\n" +
				"		<String name=\"##VAR2##\"/>\r\n" +
				"	</DataModel>\r\n" +
				"</Peach>";

			Dictionary<string, object> parserArgs = new Dictionary<string, object>();

			if (defaultArgs)
			{
				var defaultValues = new Dictionary<string, string>();
				defaultValues["VAR1"] = "TheDataModel";
				defaultValues["VAR2"] = "SomeString";

				parserArgs[PitParser.DEFINED_VALUES] = defaultValues;
			}

			string pitFile = Path.GetTempFileName();

			using (FileStream f = File.OpenWrite(pitFile))
			{
				using (StreamWriter sw = new StreamWriter(f, System.Text.Encoding.GetEncoding(codepage)))
				{
					sw.Write(xml);
				}
			}

			Dom.Dom dom = Analyzer.defaultParser.asParser(parserArgs, pitFile);
			dom.evaulateDataModelAnalyzers();

			Assert.AreEqual(1, dom.dataModels.Count);
			Assert.AreEqual(1, dom.dataModels[0].Count);

			if (defaultArgs)
			{
				Assert.AreEqual("TheDataModel", dom.dataModels[0].name);
				Assert.AreEqual("SomeString", dom.dataModels[0][0].name);
			}
			else
			{
				Assert.AreEqual("##VAR1##", dom.dataModels[0].name);
				Assert.AreEqual("##VAR2##", dom.dataModels[0][0].name);
			}
		}

		[Test]
		public void TestDefault()
		{
			TestEncoding(null, true);
			TestEncoding(null, false);
		}

		[Test]
		public void TestUtf8()
		{
			TestEncoding(Encoding.UTF8, true);
			TestEncoding(Encoding.UTF8, false);
		}

		[Test]
		public void TestUtf16()
		{
			TestEncoding(Encoding.Unicode, true);
			TestEncoding(Encoding.Unicode, false);
		}

		[Test]
		public void TestUtf32()
		{
			TestEncoding(Encoding.UTF32, true);
			TestEncoding(Encoding.UTF32, false);
		}

		[Test]
		public void TestUtf16BE()
		{
			TestEncoding(Encoding.BigEndianUnicode, true);
			TestEncoding(Encoding.BigEndianUnicode, false);
		}

		[Test]
		public void DefinesBeforeValidate()
		{
			string xml = @"
<Peach>
	<Include ns='test' src='file:##FILE##'/>
</Peach>
";
			string tempFile = Path.GetTempFileName();
			File.WriteAllText(tempFile, "<Peach><DataModel name='DM'/></Peach>");

			var defines = new Dictionary<string, string>();
			defines["FILE"] = tempFile;

			var args = new Dictionary<string, object>();
			args[PitParser.DEFINED_VALUES] = defines;

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(args, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			Assert.NotNull(dom);
		}
	}
}
