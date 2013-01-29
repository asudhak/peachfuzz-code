using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Peach.Core.Dom;
using Peach.Core.IO;

using NLog;

namespace Peach.Core.Publishers
{
	[Publisher("Remote", true)]
	[Parameter("agent", typeof(string), "Name of agent to host the publisher")]
	[Parameter("class", typeof(string), "Publisher to host")]
	public class RemotePublisher : Publisher
	{
		public string _agent { get; set; }
		public string _class { get; set; }

		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public RemotePublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}
	}
}
