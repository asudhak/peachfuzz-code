using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using NLog;

namespace Peach.Core.Publishers
{
	[Publisher("TcpListener", true)]
	[Publisher("tcp.TcpListener")]
	[Parameter("Interface", typeof(IPAddress), "IP of interface to bind to")]
	[Parameter("Port", typeof(ushort), "Local port to listen on")]
	[Parameter("Timeout", typeof(int), "How many milliseconds to wait for data (default 3000)", "3000")]
	[Parameter("AcceptTimeout", typeof(int), "How many milliseconds to wait for a connection (default 3000)", "3000")]
	public class TcpListenerPublisher : TcpPublisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public IPAddress Interface { get; set; }
		public int AcceptTimeout { get; set; }

		protected TcpListener _listener = null;

		public TcpListenerPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override void OnOpen()
		{
			System.Diagnostics.Debug.Assert(_listener == null);

			try
			{
				_listener = new TcpListener(Interface, Port);
				_listener.Start();
			}
			catch (Exception ex)
			{
				throw new PeachException("Error, unable to bind to interface " +
					Interface + " on port " + Port + ": " + ex.Message, ex);
			}

			base.OnOpen();
		}

		protected override void OnClose()
		{
			if (_listener != null)
			{
				_listener.Stop();
				_listener = null;
			}

			base.OnClose();
		}

		protected override void OnAccept()
		{
			// Ensure any open stream is closed...
			base.OnClose();

			try
			{
				var ar = _listener.BeginAcceptTcpClient(null, null);
				if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(AcceptTimeout)))
					throw new TimeoutException();
				_tcp = _listener.EndAcceptTcpClient(ar);
			}
			catch (Exception ex)
			{
				throw new SoftException(ex);
			}

			// Start receiving on the client
			StartClient();
		}
	}
}
