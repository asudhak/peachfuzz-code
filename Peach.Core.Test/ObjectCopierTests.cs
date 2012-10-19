using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Peach.Core.IO;

using Peach.Core.Dom;
using Peach.Core.Analyzers;

namespace Peach.Core.Test
{
	[TestFixture]
	class ObjectCopierTests
	{
		[Test]
		public void ValidateNonDiverganceListVsDictionary()
		{
			// Our ordered dictionary class contains both a 
			// dictionary and list of the same objects.
			// When copied do both sides match?

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

			var dmCopy = dm.Clone() as DataModel;
			ValidateListVsDictionary(dmCopy, null);
		}

		[Test]
		public void ValidateRelations()
		{
			// Our ordered dictionary class contains both a 
			// dictionary and list of the same objects.
			// When copied do both sides match?

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

			((DataElementContainer)((DataElementContainer)dm[0])[0])[0].relations.Add(new SizeRelation());

			((DataElementContainer)((DataElementContainer)dm[0])[0])[0].relations[0].OfName = "string1_1_2";
			((DataElementContainer)((DataElementContainer)dm[0])[0])[0].relations[0].FromName = "string1_1_1";

			var dmCopy = dm.Clone() as DataModel;
			ValidateListVsDictionary(dmCopy, null);
		}

		[Test]
		public void ValidateRelations2()
		{
			// Our ordered dictionary class contains both a 
			// dictionary and list of the same objects.
			// When copied do both sides match?

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

			((DataElementContainer)((DataElementContainer)dm[0])[0])[0].relations.Add(new SizeRelation());

			((DataElementContainer)((DataElementContainer)dm[0])[0])[0].relations[0].OfName = "string2_1_2";
			((DataElementContainer)((DataElementContainer)dm[0])[0])[0].relations[0].FromName = "string1_1_1";

			((DataElementContainer)((DataElementContainer)dm[1])[0])[1].relations.Add(((DataElementContainer)((DataElementContainer)dm[0])[0])[0].relations[0], false);

			dm.find("string1_1_1").DefaultValue = new Variant("10");
			dm.find("string2_1_2").DefaultValue = new Variant("1234567890");

			// Generate the value
			var value = dm.Value;
			Assert.NotNull(value);

			var dmCopy = dm.Clone() as DataModel;
			for (int count = 0; count < 10; count++)
				dmCopy = dmCopy.Clone() as DataModel;

			ValidateListVsDictionary(dmCopy, null);
		}

		[Test]
		public void ValidateRelations3()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
				"	<DataModel name=\"TheDataModel\">" +
				"		<Blob name=\"Data\" value=\"12345\"/>" +
				"		<Number name=\"TheNumber\" size=\"8\">" +
				"			<Relation type=\"size\" of=\"Data\" />" +
				"		</Number>" +
				"	</DataModel>" +
				"</Peach>";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			DataModel dmCopy = dom.dataModels[0].Clone() as DataModel;
			ValidateListVsDictionary(dmCopy, null);
		}

		public void ValidateListVsDictionary(DataElementContainer elem, DataElementContainer parent)
		{
			Assert.NotNull(elem);

			if(parent != null)
				Assert.AreEqual(elem.parent.GetHashCode(), parent.GetHashCode(), elem.fullName);

			for (int count = 0; count < elem.Count; count++)
			{
				var countItem = elem[count];
				var dictItem = elem[countItem.name];

				Assert.AreEqual(countItem.GetHashCode(), dictItem.GetHashCode(), countItem.fullName);

				foreach (var rel in countItem.relations)
					Assert.AreEqual(rel.parent.getRoot().GetHashCode(), countItem.getRoot().GetHashCode(), countItem.fullName);
			}

			foreach (var child in elem)
			{
				if (child is DataElementContainer)
					ValidateListVsDictionary(child as DataElementContainer, elem);
			}
		}
	}
}
