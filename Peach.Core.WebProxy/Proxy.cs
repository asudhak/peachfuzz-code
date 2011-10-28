using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using NLog;

namespace Peach.Core.WebProxy
{

	/// <summary>
	/// Delagete for consumer to perform work items.  Client should not perform
	/// long running tasks in this handler as it will block proxy operation.
	/// </summary>
	/// <returns>Return false to exit proxy.</returns>
	public delegate bool WorkHandler();

	public class Proxy
	{
		NLog.Logger logger = LogManager.GetLogger("Peach.Core.WebProxy.Proxy");
		byte[] buff = new byte[1024 * 1024];
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

				Connection [] connList = new Connection[connections.Values.Count];
				connections.Values.CopyTo(connList, 0);
				foreach (Connection conn in connList)
				{
					ProcessRead(conn);
				}

				if (WorkHandler != null)
					if (!WorkHandler())
						break;
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
					conn.ClientStream.ReadTimeout = 1;
					len = conn.ClientStream.Read(buff, 0, buff.Length);
				}
				catch (Exception)
				{
					connections.Remove(conn.ClientStream);
					connections.Remove(conn.ServerStream);
					conn.ServerTcpClient.Close();
					conn.ClientTcpClient.Close();
					return;
				}

				if (len > 0)
				{
					long pos = conn.ClientInputStream.Position;
					conn.ClientInputStream.Seek(0, System.IO.SeekOrigin.End);
					conn.ClientInputStream.Write(buff, 0, len);
					conn.ClientInputStream.Position = pos;

					logger.Info(string.Format("Read {0} from client socket {1}", len, conn.ClientTcpClient.Client.RemoteEndPoint.ToString()));

					conn.OnClientDataReceived();
				}
			}
			if (conn.ServerStream != null)
			{
				int len = 0;

				try
				{
					conn.ServerStream.ReadTimeout = 1;
					len = conn.ServerStream.Read(buff, 0, buff.Length);
				}
				catch (Exception)
				{
					connections.Remove(conn.ClientStream);
					connections.Remove(conn.ServerStream);
					conn.ServerTcpClient.Close();
					conn.ClientTcpClient.Close();
					return;
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
