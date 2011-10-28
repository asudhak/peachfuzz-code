using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Peach.Core.WebProxy
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
