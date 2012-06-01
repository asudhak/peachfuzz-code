
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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using NLog;

namespace Peach.Core.Proxy.Net
{
	public delegate void ClientDataReceivedEventHandler(Connection conn);
	public delegate void ServerDataReceivedEventHandler(Connection conn);

	public class NetProxy
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		Proxy proxy = null;
		string _remoteAddress;
		int _remotePort;

		public event ClientDataReceivedEventHandler ClientDataReceived;
		public event ServerDataReceivedEventHandler ServerDataReceived;

		public NetProxy(string listenAddress, int listenPort, string remoteAddress, int remotePort)
		{
			_remoteAddress = remoteAddress;
			_remotePort = remotePort;

			this.proxy = new Proxy(listenAddress, listenPort);

			Connection.ClientDataReceived += new DataReceivedEventHandler(Connection_ClientDataReceived);
			Connection.ServerDataReceived += new DataReceivedEventHandler(Connection_ServerDataReceived);
		}

		public void Stop()
		{
			proxy.KeepRunning = false;
		}

		public void Run()
		{
			proxy.Run();
		}

		void Connection_ClientDataReceived(Connection conn)
		{
			if (conn.ServerTcpClient == null)
			{
				logger.Info("Creating server connection to " + _remoteAddress + ":" + _remotePort);

				TcpClient server = new TcpClient();
				server.Connect(_remoteAddress, _remotePort);
				if (!server.Connected)
				{
					logger.Error("Connection failed :(");
					Stop();
					return;
				}

				conn.ServerTcpClient = server;
				proxy.connections.Add(server.GetStream(), conn);
			}

			OnClientDataReceived(conn);
		}

		void Connection_ServerDataReceived(Connection conn)
		{
			OnServerDataReceived(conn);
		}

		void OnClientDataReceived(Connection conn)
		{
			if (ClientDataReceived != null)
				ClientDataReceived(conn);
			else
			{
				try
				{
					// Otherwise copy data across.

					byte[] buff = new byte[1024];
					int read = 0;

					do
					{
						read = conn.ClientInputStream.Read(buff, 0, buff.Length);
						if (read > 0)
						{
							logger.Debug("S: {1}: Writing {0}",
								read,
								conn.ServerTcpClient.Client.RemoteEndPoint.ToString());
							conn.ServerStream.Write(buff, 0, read);
							conn.ServerStream.Flush();

							using (FileStream sout = File.Open("c:\\server-" + conn.ServerTcpClient.Client.RemoteEndPoint.ToString().Replace(":","_") + ".txt", FileMode.Append))
							{
								sout.Write(buff, 0, read);
							}
						}
					}
					while (read > 0);
				}
				catch
				{
				}
			}
		}

		void OnServerDataReceived(Connection conn)
		{
			if (ServerDataReceived != null)
				ServerDataReceived(conn);
			else
			{
				try
				{
					// Otherwise copy data across.

					byte[] buff = new byte[1024];
					int read = 0;

					do
					{
						read = conn.ServerInputStream.Read(buff, 0, buff.Length);
						if (read > 0)
						{
							logger.Debug("C: {1}: Writing {0}", 
								read, 
								conn.ClientTcpClient.Client.RemoteEndPoint.ToString());
							conn.ClientStream.Write(buff, 0, read);
							conn.ClientStream.Flush();

							using (FileStream sout = File.Open("c:\\client-" + conn.ServerTcpClient.Client.RemoteEndPoint.ToString().Replace(":", "_") + ".txt", FileMode.Append))
							{
								sout.Write(buff, 0, read);
							}
						}
					}
					while (read > 0);
				}
				catch
				{
				}
			}
		}
	}
}

// end
