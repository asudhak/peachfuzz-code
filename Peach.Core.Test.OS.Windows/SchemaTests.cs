using System;
using NUnit.Framework;
using Peach.Core.Analyzers;
using System.IO;

namespace Peach.Core.Test.OS.Windows
{
	[TestFixture]
	public class SchemaTests
	{
		[Test]
		public void LineNumbers()
		{
			string xml = @"
<Peach>
	<DataModel bad_attr=''/>

	<StateModel/>

	<Test name='Default'/>
</Peach>
";

			var parser = new PitParser();

			try
			{
				parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
				Assert.Fail("Should throw");
			}
			catch (PeachException ex)
			{
				var lines = ex.Message.Split('\n');
				Assert.AreEqual(6, lines.Length);

				Assert.True(lines[0].Contains("file failed to validate"));
				Assert.True(lines[1].StartsWith("Line: 3, Position: 13"));
				Assert.True(lines[1].Contains("The 'bad_attr' attribute is not declared."));
				Assert.True(lines[2].StartsWith("Line: 5, Position: 3"));
				Assert.True(lines[2].Contains("The required attribute 'name' is missing."));
				Assert.True(lines[3].StartsWith("Line: 5, Position: 3"));
				Assert.True(lines[3].Contains("The required attribute 'initialState' is missing."));
				Assert.True(lines[4].StartsWith("Line: 5, Position: 3"));
				Assert.True(lines[4].Contains("incomplete content"));
			}
		}
	}
}
