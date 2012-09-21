using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Peach.Core.IO;

using Peach.Core.Dom;

namespace Peach.Core.Test
{
	[TestFixture]
	class DomGeneralTests
	{
		[Test]
		public void Find()
		{
			DataModel dm = new DataModel("root");
			dm.Add(new Block("block1"));
			dm.Add(new Block("block2"));
			((DataElementContainer)dm[0]).Add(new Block("block1_1"));
			((DataElementContainer)dm[0]).Add(new Block("block1_2"));
			((DataElementContainer)dm[1]).Add(new Block("block2_1"));
			((DataElementContainer)dm[1]).Add(new Block("block2_2"));

			((DataElementContainer)((DataElementContainer)dm[0])[0]).Add(new Dom.String("string1_1_1"));
			((DataElementContainer)((DataElementContainer)dm[0])[0]).Add(new Dom.String("string1_1_2"));
			((DataElementContainer)((DataElementContainer)dm[0])[1]).Add(new Dom.String("string1_2_1"));
			((DataElementContainer)((DataElementContainer)dm[0])[1]).Add(new Dom.String("string1_2_2"));

			((DataElementContainer)((DataElementContainer)dm[1])[0]).Add(new Dom.String("string2_1_1"));
			((DataElementContainer)((DataElementContainer)dm[1])[0]).Add(new Dom.String("string2_1_2"));
			((DataElementContainer)((DataElementContainer)dm[1])[1]).Add(new Dom.String("string2_2_1"));
			((DataElementContainer)((DataElementContainer)dm[1])[1]).Add(new Dom.String("string2_2_2"));

			Assert.NotNull(dm.find("string1_1_1"));
			Assert.NotNull(dm.find("string1_1_1").find("string2_1_2"));
		}
	}
}
