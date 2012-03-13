
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

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Peach.Core.Proxy
{

	public delegate void DataReceivedEventHandler(Connection conn);

    /// <summary>
    /// A socker par (client/server)
    /// </summary>
    public class Connection
    {
		public static event DataReceivedEventHandler ClientDataReceived;
		public static event DataReceivedEventHandler ServerDataReceived;

		NetworkStream clientStream = null;
		NetworkStream serverStream = null;

		TcpClient clientTcpClient = null;
		TcpClient serverTcpClient = null;

		public Connection(TcpClient clientClient, TcpClient serverClient)
        {
			ClientTcpClient = clientClient;
			ServerTcpClient = serverClient;

			ClientInputStream = new MemoryStream();
			ServerInputStream = new MemoryStream();
        }

		public TcpClient ClientTcpClient
		{
			get
			{
				return clientTcpClient;
			}
			set
			{
				clientTcpClient = value;
				if(clientTcpClient != null)
					clientStream = clientTcpClient.GetStream();
			}
		}

		public TcpClient ServerTcpClient
		{
			get
			{
				return serverTcpClient;
			}

			set
			{
				serverTcpClient = value;
				if (serverTcpClient != null)
					serverStream = serverTcpClient.GetStream();
			}
		}

		public NetworkStream ClientStream { get { return clientStream;} }
		public NetworkStream ServerStream { get { return serverStream; } }

		public MemoryStream ClientInputStream { get; set; }
		public MemoryStream ServerInputStream { get; set; }

		public void OnClientDataReceived()
		{
			if (ClientDataReceived != null)
				ClientDataReceived(this);
		}
		public void OnServerDataReceived()
		{
			if (ServerDataReceived != null)
				ServerDataReceived(this);
		}
	}
}
