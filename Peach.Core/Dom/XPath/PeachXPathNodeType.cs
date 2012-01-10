using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;

using NLog;

using Peach.Core;
using Peach.Core.IO;
using Peach.Core.Dom;

namespace Peach.Core.Dom.XPath
{
	public enum PeachXPathNodeType
	{
		Root,
		DataModel,
		StateModel,
		Agent,
		Test,
		Run
	}
}
