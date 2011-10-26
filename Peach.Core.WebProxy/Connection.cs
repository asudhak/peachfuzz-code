using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Peach.Core.WebProxy
{
    /// <summary>
    /// A socker par (client/server)
    /// </summary>
    public class Connection
    {
        Socket ClientSocket = null;
        Socket ServerSocket = null;

        public Connection(Socket clientSocket, Socket serverSocket)
        {
            ClientSocket = clientSocket;
            ServerSocket = serverSocket;
        }
    }
}
