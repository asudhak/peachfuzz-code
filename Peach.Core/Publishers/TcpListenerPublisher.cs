using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Peach.Core.Publishers
{
	[Publisher("TcpListener")]
	[Publisher("tcp.TcpListener")]
	[ParameterAttribute("Interface", typeof(string), "Interface to bind to (0.0.0.0 for all)", true)]
	[ParameterAttribute("Port", typeof(int), "Local port to listen on", true)]
	[ParameterAttribute("Timeout", typeof(int), "How long to wait for data/connection", false)]
	public class TcpListenerPublisher : TcpClientPublisher
	{
		string _interface = "0.0.0.0";
		short _port = 0;
		int _timeout = 0;

		TcpListener _listener = null;

		public TcpListenerPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
			if (args.ContainsKey("Interface"))
				_interface = (string)args["Interface"];

			_port = (short)args["Port"];

			if (args.ContainsKey("Timeout"))
				_timeout = (int)args["Timeout"];
		}

		public override void accept(Dom.Action action)
		{
			try
			{
				TcpClient = _listener.AcceptTcpClient();
			}
			catch (SocketException e)
			{
				throw new PeachException("Error, unable to accept incoming connection: " + e.Message);
			}
		}

		public override void close(Dom.Action action)
		{
			base.close(action);
		}

		public override void start(Dom.Action action)
		{
			base.start(action);

			try
			{
				_listener = new TcpListener(IPAddress.Parse(_interface), _port);
				_listener.Start();
			}
			catch (SocketException e)
			{
				throw new PeachException("Error, unable to bind to interface " + 
					_interface + " on port " + _port + ": " + e.Message);
			}
		}

		public override void stop(Dom.Action action)
		{
			base.stop(action);

			_listener.Stop();
			_listener = null;
		}

		public override void open(Dom.Action action)
		{
			throw new PeachException("Error, TcpListener publisher does not support open action type.");
		}
	}
}

// end
