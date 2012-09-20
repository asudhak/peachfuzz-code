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
			((DataElementContainer)dm[0]).Add(new Block("block1.1"));
			((DataElementContainer)dm[0]).Add(new Block("block1.2"));
			((DataElementContainer)dm[1]).Add(new Block("block2.1"));
			((DataElementContainer)dm[1]).Add(new Block("block2.2"));

			((DataElementContainer)((DataElementContainer)dm[0])[0]).Add(new Dom.String("string1.1.1"));
			((DataElementContainer)((DataElementContainer)dm[0])[0]).Add(new Dom.String("string1.1.2"));
			((DataElementContainer)((DataElementContainer)dm[0])[1]).Add(new Dom.String("string1.2.1"));
			((DataElementContainer)((DataElementContainer)dm[0])[1]).Add(new Dom.String("string1.2.2"));

			((DataElementContainer)((DataElementContainer)dm[1])[0]).Add(new Dom.String("string2.1.1"));
			((DataElementContainer)((DataElementContainer)dm[1])[0]).Add(new Dom.String("string2.1.2"));
			((DataElementContainer)((DataElementContainer)dm[1])[1]).Add(new Dom.String("string2.2.1"));
			((DataElementContainer)((DataElementContainer)dm[1])[1]).Add(new Dom.String("string2.2.2"));

			var dmCopy = ObjectCopier.Clone<DataModel>(dm);
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
			((DataElementContainer)dm[0]).Add(new Block("block1.1"));
			((DataElementContainer)dm[0]).Add(new Block("block1.2"));
			((DataElementContainer)dm[1]).Add(new Block("block2.1"));
			((DataElementContainer)dm[1]).Add(new Block("block2.2"));

			((DataElementContainer)((DataElementContainer)dm[0])[0]).Add(new Dom.String("string1.1.1"));
			((DataElementContainer)((DataElementContainer)dm[0])[0]).Add(new Dom.String("string1.1.2"));
			((DataElementContainer)((DataElementContainer)dm[0])[1]).Add(new Dom.String("string1.2.1"));
			((DataElementContainer)((DataElementContainer)dm[0])[1]).Add(new Dom.String("string1.2.2"));

			((DataElementContainer)((DataElementContainer)dm[1])[0]).Add(new Dom.String("string2.1.1"));
			((DataElementContainer)((DataElementContainer)dm[1])[0]).Add(new Dom.String("string2.1.2"));
			((DataElementContainer)((DataElementContainer)dm[1])[1]).Add(new Dom.String("string2.2.1"));
			((DataElementContainer)((DataElementContainer)dm[1])[1]).Add(new Dom.String("string2.2.2"));

			((DataElementContainer)((DataElementContainer)dm[0])[0])[0].relations.Add(new SizeRelation());

			((DataElementContainer)((DataElementContainer)dm[0])[0])[0].relations[0].OfName = "string1.1.2";
			((DataElementContainer)((DataElementContainer)dm[0])[0])[0].relations[0].FromName = "string1.1.1";

			var dmCopy = ObjectCopier.Clone<DataModel>(dm);
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
			((DataElementContainer)dm[0]).Add(new Block("block1.1"));
			((DataElementContainer)dm[0]).Add(new Block("block1.2"));
			((DataElementContainer)dm[1]).Add(new Block("block2.1"));
			((DataElementContainer)dm[1]).Add(new Block("block2.2"));

			((DataElementContainer)((DataElementContainer)dm[0])[0]).Add(new Dom.String("string1.1.1"));
			((DataElementContainer)((DataElementContainer)dm[0])[0]).Add(new Dom.String("string1.1.2"));
			((DataElementContainer)((DataElementContainer)dm[0])[1]).Add(new Dom.String("string1.2.1"));
			((DataElementContainer)((DataElementContainer)dm[0])[1]).Add(new Dom.String("string1.2.2"));

			((DataElementContainer)((DataElementContainer)dm[1])[0]).Add(new Dom.String("string2.1.1"));
			((DataElementContainer)((DataElementContainer)dm[1])[0]).Add(new Dom.String("string2.1.2"));
			((DataElementContainer)((DataElementContainer)dm[1])[1]).Add(new Dom.String("string2.2.1"));
			((DataElementContainer)((DataElementContainer)dm[1])[1]).Add(new Dom.String("string2.2.2"));

			((DataElementContainer)((DataElementContainer)dm[0])[0])[0].relations.Add(new SizeRelation());

			((DataElementContainer)((DataElementContainer)dm[0])[0])[0].relations[0].OfName = "string1.1.2";
			((DataElementContainer)((DataElementContainer)dm[0])[0])[0].relations[0].FromName = "string1.1.1";

			((DataElementContainer)((DataElementContainer)dm[0])[0])[1].relations.Add(((DataElementContainer)((DataElementContainer)dm[0])[0])[0].relations[0], false);

			var dmCopy = ObjectCopier.Clone<DataModel>(dm);
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
					Assert.AreEqual(rel.parent.GetHashCode(), countItem.GetHashCode(), countItem.fullName);
			}

			foreach (var child in elem)
			{
				if (child is DataElementContainer)
					ValidateListVsDictionary(child as DataElementContainer, elem);
			}
		}
	}
}
