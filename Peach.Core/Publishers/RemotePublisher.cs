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

		public override Test Test
		{
			get { return base.Test; }
			set { base.Test = value; }
		}

		public override uint Iteration
		{
			get
			{
				return base.Iteration;
			}
			set
			{
				base.Iteration = value;

				if (_publisher != null)
					_publisher.Iteration = value;
			}
		}

		public override bool IsControlIteration
		{
			get
			{
				return base.IsControlIteration;
			}
			set
			{
				base.IsControlIteration = value;

				if (_publisher != null)
					_publisher.IsControlIteration = value;
			}
		}

		public override string Result
		{
			get
			{
				return _publisher.Result;
			}
			set
			{
				_publisher.Result = value;
			}
		}

		protected override void OnStart()
		{
			_publisher = Context.agentManager.CreatePublisher(Agent, Class, Args);
			_publisher.Iteration = Iteration;
			_publisher.IsControlIteration = IsControlIteration;
			_publisher.start();
		}

		protected override void OnStop()
		{
			_publisher.stop();
			_publisher = null;
		}

		protected override void OnOpen()
		{
			_publisher.open();
		}

		protected override void OnClose()
		{
			_publisher.close();
		}

		protected override void OnAccept()
		{
			_publisher.accept();
		}

		protected override Variant OnCall(string method, List<ActionParameter> args)
		{
			return _publisher.call(method, args);
		}

		protected override void OnSetProperty(string property, Variant value)
		{
			_publisher.setProperty(property, value);
		}

		protected override Variant OnGetProperty(string property)
		{
			return _publisher.getProperty(property);
		}

		protected override void OnOutput(Stream data)
		{
			byte[] buffer = new byte[data.Length];
			data.Read(buffer, 0, buffer.Length);
			_publisher.output(buffer, 0, buffer.Length);
		}

		protected override void OnInput()
		{
			_publisher.input();
		}

		public override void WantBytes(long count)
		{
			_publisher.WantBytes(count);
		}

		public override bool CanRead
		{
			get { return _publisher.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _publisher.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _publisher.CanWrite; }
		}

		public override void Flush()
		{
			_publisher.Flush();
		}

		public override long Length
		{
			get { throw new NotImplementedException(); }
		}

		public override long Position
		{
			get { return _publisher.Position; }
			set { _publisher.Position = value; }
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _publisher.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _publisher.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_publisher.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_publisher.Write(buffer, offset, count);
		}
	}
}
