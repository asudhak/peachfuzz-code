using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peach.Core.Dom;

namespace PeachBuilder.Models
{
	public class DesignPeachModel : DesignModel
	{
		public DesignPeachModel(Dom dom)
		{
			this.Dom = dom;
			IconName = "/Icons/node-peach.png";

			foreach(var datamodel in dom.dataModels.Values)
			{
				DesignDataElementModel item = new DesignDataElementModel(datamodel);
				Children.Add(item);
			}
		}

		public Dom Dom
		{
			get;
			set;
		}
	}

	public class DesignImportModel : DesignModel
	{
		public DesignImportModel()
		{
			IconName = "/Icons/node-import.png";
		}

		public string Import
		{
			get;
			set;
		}

		public string From
		{
			get;
			set;
		}
	}
}
