using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using NLog;

namespace Peach.Core.Proxy.Web
{

	/// <summary>
	/// Delagete for consumer to perform work items.  Client should not perform
	/// long running tasks in this handler as it will block proxy operation.
	/// </summary>
	/// <returns>Return false to exit proxy.</returns>
	public delegate bool WorkHandler();

	public class Proxy
	{
		NLog.Logger logger = LogManager.GetLogger("Peach.Core.Proxy.Web.Proxy");
		byte[] buff = new byte[1024];
		int listenPort = 8080;
		public TcpListener listener = null;
		public Dictionary<NetworkStream, Connection> connections = new Dictionary<NetworkStream, Connection>();

		public WorkHandler WorkHandler;

		public void Run()
		{
			logger.Info("Creating TcpListener");
			listener = new TcpListener(IPAddress.Parse("0.0.0.0"), listenPort);
			listener.Start();

			while (true)
			{
				if (listener.Pending())
					listener.BeginAcceptTcpClient(new AsyncCallback(ProcessAcceptTcpClient), null);

				List<Connection> connList = new List<Connection>();
				foreach (Connection conn in connections.Values)
					if (!connList.Contains(conn))
						connList.Add(conn);

				foreach (Connection conn in connList)
					ProcessRead(conn);

				if (WorkHandler != null)
					if (!WorkHandler())
						break;

				Console.Error.Write(".");
			}

			foreach (Connection conn in connections.Values)
			{
				conn.ClientTcpClient.Close();
				conn.ServerTcpClient.Close();
			}

			connections = new Dictionary<NetworkStream, Connection>();
			listener = null;
		}

		void ProcessRead(Connection conn)
		{
			if (conn.ClientStream != null)
			{
				int len = 0;

				try
				{
					conn.ClientTcpClient.Client.Blocking = false;
					conn.ClientStream.ReadTimeout = 1;

					// Must read one byte at a time for non-blocking to work.
					while (len < buff.Length)
					{
						buff[len] = (byte)conn.ClientStream.ReadByte();
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

						connections.Remove(conn.ClientStream);
						connections.Remove(conn.ServerStream);
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

					using(FileStream sout = File.Open("c:\\client-"+conn.ClientTcpClient.Client.Handle.ToString()+".txt", FileMode.Append))
					{
						sout.Write(buff, 0, len);
					}					

					logger.Info(string.Format("Read {0} from client socket {1}", len, conn.ClientTcpClient.Client.RemoteEndPoint.ToString()));

					conn.OnClientDataReceived();
				}
			}

			if (conn.ServerStream != null)
			{
				int len = 0;

				try
				{
					conn.ServerTcpClient.Client.Blocking = false;
					conn.ServerStream.ReadTimeout = 1;

					// Must read one byte at a time for non-blocking to work.
					while (len < buff.Length)
					{
						buff[len] = (byte)conn.ServerStream.ReadByte();
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

						connections.Remove(conn.ClientStream);
						connections.Remove(conn.ServerStream);
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

					logger.Info(string.Format("Read {0} from server socket {1}", len, conn.ServerTcpClient.Client.RemoteEndPoint.ToString()));

					conn.OnServerDataReceived();
				}
			}
		}

		void ProcessAcceptTcpClient(IAsyncResult result)
		{
			TcpClient client = listener.EndAcceptTcpClient(result);

			logger.Info("Accepted new client: " + client.Client.RemoteEndPoint.ToString());

			var conn = new Connection(client, null);
			connections.Add(client.GetStream(), conn);
		}
	}
}
