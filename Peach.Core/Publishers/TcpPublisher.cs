using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net.Sockets;

using Peach.Core.Dom;

using NLog;
using System.Net;

namespace Peach.Core.Publishers
{
	public abstract class TcpPublisher : BufferedStreamPublisher
	{
		public ushort Port { get; set; }
		
		protected TcpClient _tcp = null;
		protected EndPoint _localEp = null;
		protected EndPoint _remoteEp = null;

		public TcpPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override void StartClient()
		{
			System.Diagnostics.Debug.Assert(_tcp != null);
			System.Diagnostics.Debug.Assert(_client == null);
			System.Diagnostics.Debug.Assert(_localEp == null);
			System.Diagnostics.Debug.Assert(_remoteEp == null);

			_client = _tcp.GetStream();
			_localEp = _tcp.Client.LocalEndPoint;
			_remoteEp = _tcp.Client.RemoteEndPoint;
			_clientName = _remoteEp.ToString();

			base.StartClient();
		}

		protected override void CloseClient()
		{
			base.CloseClient();

			_tcp = null;
			_remoteEp = null;
			_localEp = null;
		}

		protected override IAsyncResult ClientBeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return _tcp.Client.BeginReceive(buffer, offset, count, SocketFlags.None, callback, state);
		}

		protected override int ClientEndRead(IAsyncResult asyncResult)
		{
			return _tcp.Client.EndReceive(asyncResult);
		}

		protected override void ClientShutdown()
		{
			_tcp.Client.Shutdown(SocketShutdown.Send);
		}

		protected override void ClientClose()
		{
			_tcp.Close();
		}
	}
}

// end
