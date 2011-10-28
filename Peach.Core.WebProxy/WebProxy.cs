using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using NLog;

namespace Peach.Core.WebProxy
{
	public delegate void HttpRequestEventHandler(HttpRequest request);
	public delegate void HttpResponseEventHandler(HttpResponse response);

	public class WebProxy
	{
		NLog.Logger logger = LogManager.GetLogger("Peach.Core.WebProxy.WebProxy");
		Proxy proxy = null;

		public event HttpRequestEventHandler NewHttpRequest;
		public event HttpResponseEventHandler NewHttpResponse;

		public WebProxy(Proxy proxy)
		{
			this.proxy = proxy;
			Connection.ClientDataReceived += new DataReceivedEventHandler(Connection_ClientDataReceived);
			Connection.ServerDataReceived += new DataReceivedEventHandler(Connection_ServerDataReceived);
		}

		void Connection_ClientDataReceived(Connection conn)
		{
			var req = HttpRequest.Parse(conn.ClientInputStream);
			if (req == null)
				return;

			req.Connection = conn;

			if (req.Connection.ServerTcpClient == null)
			{
				string host = req.Headers["host"].Value;
				int port = 80;

				var match = Regex.Match(host, "(.*):(.*)");
				if (match != null && match.Groups.Count == 3)
				{
					host = match.Groups[1].Value;
					port = int.Parse(match.Groups[2].Value);
				}

				logger.Info("Creating server connection to " + host + ":" + port);

				TcpClient server = new TcpClient();
				server.Connect(host, port);
				if (!server.Connected)
					logger.Error("Connection failed :(");

				req.Connection.ServerTcpClient = server;
				proxy.connections.Add(server.GetStream(), conn);
			}

			OnNewHttpRequest(req);
		}

		void Connection_ServerDataReceived(Connection conn)
		{
			var res = HttpResponse.Parse(conn.ServerInputStream);
			if (res == null)
				return;

			res.Connection = conn;

			OnNewHttpResponse(res);
		}

		void OnNewHttpRequest(HttpRequest request)
		{
			if (NewHttpRequest != null)
				NewHttpRequest(request);
		}

		void OnNewHttpResponse(HttpResponse response)
		{
			if (NewHttpResponse != null)
				NewHttpResponse(response);
		}
	}
}

// end
