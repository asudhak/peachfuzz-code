using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Peach.Core.Publishers
{
	[Publisher("Null", true)]
	public class NullPublisher : Publisher
	{
		public NullPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override void OnOutput(Stream data)
		{
		}
	}
}
