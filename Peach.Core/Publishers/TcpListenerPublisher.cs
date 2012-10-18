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
	[Parameter("Interface", typeof(IPAddress), "IP of interface to bind to", true)]
	[Parameter("Port", typeof(ushort), "Local port to listen on", true)]
	[Parameter("Timeout", typeof(int), "How many milliseconds to wait for data/connection (default 3000)", "3000")]
	public class TcpListenerPublisher : TcpPublisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public IPAddress Interface { get; set; }

		protected TcpListener _listener = null;

		public TcpListenerPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override void OnStart()
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
					Interface + " on port " + Port + ": " + ex.Message);
			}

			base.OnStart();
		}

		protected override void OnStop()
		{
			if (_listener != null)
			{
				_listener.Stop();
				_listener = null;
			}

			base.OnStop();
		}

		protected override void OnAccept()
		{
			// Ensure any open stream is closed...
			OnClose();

			try
			{
				var ar = _listener.BeginAcceptTcpClient(null, null);
				if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(Timeout)))
					throw new TimeoutException();
				_client = _listener.EndAcceptTcpClient(ar);
			}
			catch (Exception ex)
			{
				throw new PeachException("Error, unable to accept incoming connection: " + ex.Message);
			}

			// Start receiving on the client
			StartClient();
		}
	}
}
