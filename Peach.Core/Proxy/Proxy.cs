
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
using System.Net.Sockets;
using System.Net;
using NLog;

namespace Peach.Core.Proxy
{

	/// <summary>
	/// Delagete for consumer to perform work items.  Client should not perform
	/// long running tasks in this handler as it will block proxy operation.
	/// </summary>
	/// <returns>Return false to exit proxy.</returns>
	public delegate bool WorkHandler();

	public class Proxy
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		byte[] buff = new byte[1024];
		int listenPort;
		string address;
		public TcpListener listener = null;
		public Dictionary<NetworkStream, Connection> connections = new Dictionary<NetworkStream, Connection>();

		public WorkHandler WorkHandler;

		/// <summary>
		/// Create a proxy instance.
		/// </summary>
		/// <param name="address">To listen on all interaces specify 0.0.0.0 as the address (the default)</param>
		/// <param name="port">Port to listen on, default is 8080.</param>
		public Proxy(string address = "0.0.0.0", int port = 8080)
		{
			listenPort = port;
			this.address = address;
		}

		public bool KeepRunning = true;

		public void Run()
		{
			logger.Info("Creating TcpListener");

			KeepRunning = true;

			listener = new TcpListener(IPAddress.Parse(address), listenPort);
			listener.Start();

			while (KeepRunning)
			{
				if (listener.Pending())
					listener.BeginAcceptTcpClient(new AsyncCallback(ProcessAcceptTcpClient), null);

				List<Connection> connList = new List<Connection>();
				lock (connections)
				{
					foreach (Connection conn in connections.Values)
						if (!connList.Contains(conn))
							connList.Add(conn);
				}

				foreach (Connection conn in connList)
					ProcessRead(conn);

				//if (WorkHandler != null)
				//    if (!WorkHandler())
				//        break;

				//Console.Error.Write(".");
			}

			lock (connections)
			{
				foreach (Connection conn in connections.Values)
				{
					conn.ClientTcpClient.Close();
					conn.ServerTcpClient.Close();
				}

				connections = new Dictionary<NetworkStream, Connection>();
			}

			listener = null;
		}

		void ProcessRead(Connection conn)
		{
			logger.Info("ProcessRead");

			if (conn.ClientStream != null)
			{
				int len = 0;
				int b;

				try
				{
					conn.ClientTcpClient.Client.Blocking = false;
					conn.ClientStream.ReadTimeout = 1;

					// Must read one byte at a time for non-blocking to work.
					while (len < buff.Length)
					{
						b = conn.ClientStream.ReadByte();
						if (b == -1)
							break;

						buff[len] = (byte)b;
						len++;
					}
				}
				catch (Exception e)
				{
					if (e.Message.IndexOf("A non-blocking socket operation could not be completed immediately.") == -1)
					{
						logger.Info("! Client read exception, closing down connection from client {0} to server {1}: {2}",
							conn.ClientTcpClient.Client.RemoteEndPoint.ToString(),
							conn.ServerTcpClient.Client.RemoteEndPoint.ToString(),
							e.Message);

						lock (connections)
						{
							connections.Remove(conn.ClientStream);
							connections.Remove(conn.ServerStream);
						}
						conn.ServerTcpClient.Close();
						conn.ClientTcpClient.Close();
						return;
					}
				}
				finally
				{
					// Catch Disposed exception
					try
					{
						conn.ClientTcpClient.Client.Blocking = true;
					}
					catch
					{
					}
				}

				if (len > 0)
				{
					long pos = conn.ClientInputStream.Position;
					conn.ClientInputStream.Seek(0, System.IO.SeekOrigin.End);
					conn.ClientInputStream.Write(buff, 0, len);
					conn.ClientInputStream.Position = pos;

					//using(FileStream sout = File.Open("c:\\client-"+conn.ClientTcpClient.Client.Handle.ToString()+".txt", FileMode.Append))
					//{
					//    sout.Write(buff, 0, len);
					//}					

					logger.Info(string.Format("C: {1}: Read {0}", len, conn.ClientTcpClient.Client.RemoteEndPoint.ToString()));

					conn.OnClientDataReceived();
				}
			}

			if (conn.ServerStream != null)
			{
				int len = 0;
				int b;

				try
				{
					conn.ServerTcpClient.Client.Blocking = false;
					conn.ServerStream.ReadTimeout = 1;

					// Must read one byte at a time for non-blocking to work.
					while (len < buff.Length)
					{
						b = conn.ServerStream.ReadByte();
						if (b == -1)
							break;

						buff[len] = (byte)b;
						len++;
					}
				}
				catch (Exception e)
				{
					if (e.Message.IndexOf("A non-blocking socket operation could not be completed immediately.") == -1)
					{
						logger.Info("! Server read exception, closing down connection from client {0} to server {1}: {2}",
							conn.ClientTcpClient.Client.RemoteEndPoint.ToString(),
							conn.ServerTcpClient.Client.RemoteEndPoint.ToString(),
							e.Message);

						lock (connections)
						{
							connections.Remove(conn.ClientStream);
							connections.Remove(conn.ServerStream);
						}

						conn.ServerTcpClient.Close();
						conn.ClientTcpClient.Close();
						return;
					}
				}
				finally
				{
					// Catch Disposed exception
					try
					{
						conn.ServerTcpClient.Client.Blocking = true;
					}
					catch
					{
					}
				}

				if (len > 0)
				{
					long pos = conn.ServerInputStream.Position;
					conn.ServerInputStream.Seek(0, System.IO.SeekOrigin.End);
					conn.ServerInputStream.Write(buff, 0, len);
					conn.ServerInputStream.Position = pos;

					logger.Info(string.Format("S: {1}: Read {0}", len, conn.ServerTcpClient.Client.RemoteEndPoint.ToString()));

					conn.OnServerDataReceived();
				}
			}
		}

		void ProcessAcceptTcpClient(IAsyncResult result)
		{
			TcpClient client = listener.EndAcceptTcpClient(result);

			logger.Info("Accepted new client: " + client.Client.RemoteEndPoint.ToString());

			var conn = new Connection(client, null);

			lock (connections)
			{
				connections.Add(client.GetStream(), conn);
			}
		}
	}
}

// end
