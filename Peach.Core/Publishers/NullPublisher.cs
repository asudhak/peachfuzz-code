using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NLog;
using Peach.Core.IO;

namespace Peach.Core.Publishers
{
	[Publisher("Null", true)]
	public class NullPublisher : Publisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public NullPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override void OnOutput(BitwiseStream data)
		{
		}

		protected override Variant OnCall(string method, List<Dom.ActionParameter> args)
		{
			return null;
		}
	}
}
