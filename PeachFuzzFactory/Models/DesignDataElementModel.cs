using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peach.Core;
using Peach.Core.Dom;

namespace PeachFuzzFactory.Models
{
	public class DesignDataElementModel : DesignModel
	{
		public DesignDataElementModel(DataElement elem)
		{
			this.DataElement = elem;

			IconName = "/Icons/node-" + elem.GetType().Name.ToLower() + ".png";

			if (elem is DataElementContainer)
			{
				foreach (var item in ((DataElementContainer)elem))
				{
					var child = new DesignDataElementModel(item);
					Children.Add(child);
				}
			}
			if (elem is Choice)
			{
				foreach (var item in ((Choice)elem).choiceElements.Values)
				{
					var child = new DesignDataElementModel(item);
					Children.Add(child);
				}
			}
		}

		public DataElement DataElement
		{
			get;
			set;
		}
	}
}

// end
