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
	[Parameter("Agent", typeof(string), "Name of agent to host the publisher")]
	[Parameter("Class", typeof(string), "Publisher to host")]
	[InheritParameter("Class")]
	public class RemotePublisher : Publisher
	{
		public string Agent { get; protected set; }
		public string Class { get; protected set; }
		public SerializableDictionary<string, Variant> Args { get; protected set; }

		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		private Publisher _publisher;

		public RemotePublisher(Dictionary<string, Variant> args)
			: base(args)
		{
			this.Args = new SerializableDictionary<string,Variant>();

			foreach (var kv in args)
				this.Args.Add(kv.Key, kv.Value);
		}

		protected RunContext Context
		{
			get
			{
				Dom.Dom dom = this.Test.parent as Dom.Dom;
				return dom.context;
			}
		}

		protected override void OnStart()
		{
			_publisher = Context.agentManager.CreatePublisher(Agent, Class, Args);
		}

		protected override void OnStop()
		{
			_publisher = null;
		}

		protected override void OnOpen()
		{
		}

		protected override void OnClose()
		{
		}

		protected override void OnOutput(Stream data)
		{
		}
	}
}
