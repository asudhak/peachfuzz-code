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
	public class RemotePublisher : StreamPublisher
	{
		public string Agent { get; protected set; }
		public string Class { get; protected set; }
		public SerializableDictionary<string, Variant> Args { get; protected set; }

		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		private Publisher _publisher = null;

		public RemotePublisher(Dictionary<string, Variant> args)
			: base(args)
		{
			this.Args = new SerializableDictionary<string,Variant>();

			foreach (var kv in args)
				this.Args.Add(kv.Key, kv.Value);

			stream = new MemoryStream();
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

		protected void RestartRemotePublisher()
		{
			logger.Debug("Restarting remote publisher");

			_publisher = Context.agentManager.CreatePublisher(Agent, Class, Args);
			_publisher.Iteration = Iteration;
			_publisher.IsControlIteration = IsControlIteration;
			_publisher.start();
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
				{
					try
					{
						_publisher.Iteration = value;
					}
					catch (System.Runtime.Remoting.RemotingException)
					{
						RestartRemotePublisher();
					}
				}
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
				{
					try
					{
						_publisher.IsControlIteration = value;
					}
					catch (System.Runtime.Remoting.RemotingException)
					{
						RestartRemotePublisher();
					}
				}
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
			logger.Debug(">> OnStart");
			_publisher = Context.agentManager.CreatePublisher(Agent, Class, Args);
			_publisher.Iteration = Iteration;
			_publisher.IsControlIteration = IsControlIteration;
			_publisher.start();
		}

		protected override void OnStop()
		{
			try
			{
				_publisher.stop();
			}
			catch (System.Runtime.Remoting.RemotingException)
			{
			}

			_publisher = null;
		}

		protected override void OnOpen()
		{
			try
			{
				_publisher.open();
			}
			catch (System.Runtime.Remoting.RemotingException)
			{
				RestartRemotePublisher();
				_publisher.open();
			}
		}

		protected override void OnClose()
		{
			try
			{
				_publisher.close();
			}
			catch (System.Runtime.Remoting.RemotingException)
			{
				try
				{
					RestartRemotePublisher();
					_publisher.close();
				}
				catch (System.Runtime.Remoting.RemotingException)
				{
					logger.Warn("Ignoring remoting exception on OnClose");
				}
			}
		}

		protected override void OnAccept()
		{
			try
			{
				_publisher.accept();
			}
			catch (System.Runtime.Remoting.RemotingException)
			{
				RestartRemotePublisher();
				_publisher.accept();
			}
		}

		protected override Variant OnCall(string method, List<ActionParameter> args)
		{
			try
			{
				return _publisher.call(method, args);
			}
			catch (System.Runtime.Remoting.RemotingException)
			{
				try
				{
					RestartRemotePublisher();
					return _publisher.call(method, args);
				}
				catch (System.Runtime.Remoting.RemotingException)
				{
					logger.Warn("Ignoring remoting exception on OnCall");
					return null;
				}
			}
		}

		protected override void OnSetProperty(string property, Variant value)
		{
			try
			{
				_publisher.setProperty(property, value);
			}
			catch (System.Runtime.Remoting.RemotingException)
			{
				try
				{
					RestartRemotePublisher();
					_publisher.setProperty(property, value);
				}
				catch (System.Runtime.Remoting.RemotingException)
				{
					logger.Warn("Ignoring remoting exception on OnSetProperty");
				}
			}
		}

		protected override Variant OnGetProperty(string property)
		{
			try
			{
				return _publisher.getProperty(property);
			}
			catch (System.Runtime.Remoting.RemotingException)
			{
				try
				{
					RestartRemotePublisher();
					return _publisher.getProperty(property);
				}
				catch (System.Runtime.Remoting.RemotingException)
				{
					logger.Warn("Ignoring remoting exception on OnGetProperty");
					return null;
				}
			}
		}

		protected override void OnOutput(byte[] buffer, int offset, int count)
		{
			try
			{
				_publisher.output(buffer, offset, count);
			}
			catch (System.Runtime.Remoting.RemotingException)
			{
				logger.Warn("Ignoring remoting exception on OnOutput");
			}
		}

		protected override void OnInput()
		{
			try
			{
				_publisher.input();

				stream.Seek(0, SeekOrigin.Begin);
				stream.SetLength(0);

				ReadAllBytes();
			}
			catch (System.Runtime.Remoting.RemotingException)
			{
				logger.Warn("Ignoring remoting exception on OnInput");
			}
		}

		public override void WantBytes(long count)
		{
			try
			{
				long need = count - (stream.Length - stream.Position);
				if (need > 0)
				{
					_publisher.WantBytes(need);
					ReadAllBytes();
				}
			}
			catch (System.Runtime.Remoting.RemotingException)
			{
				logger.Warn("Ignoring remoting exception on WantBytes");
			}
		}

		private void ReadAllBytes()
		{
			long pos = stream.Position;

			try
			{
				for (;;)
				{
					int b = _publisher.ReadByte();
					if (b == -1)
					{
						stream.Seek(pos, SeekOrigin.Begin);
						return;
					}

					stream.WriteByte((byte)b);
				}
			}
			catch (System.Runtime.Remoting.RemotingException)
			{
				logger.Warn("Ignoring remoting exception on ReadAllBytes");
				stream.Seek(pos, SeekOrigin.Begin);
			}
		}
	}
}
