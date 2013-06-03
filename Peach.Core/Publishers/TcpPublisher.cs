
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

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

			try
			{
				_client = new MemoryStream();
				_localEp = _tcp.Client.LocalEndPoint;
				_remoteEp = _tcp.Client.RemoteEndPoint;
				_clientName = _remoteEp.ToString();
			}
			catch (Exception ex)
			{
				Logger.Error("open: Error, Unable to start tcp client reader. {0}.", ex.Message);
				throw new SoftException(ex);
			}

			base.StartClient();
		}

		protected override void ClientClose()
		{
			_tcp.Close();
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

		protected override void ClientWrite(byte[] buffer, int offset, int count)
		{
			_tcp.Client.Send(buffer, offset, count, SocketFlags.None);
		}
	}
}

// end
