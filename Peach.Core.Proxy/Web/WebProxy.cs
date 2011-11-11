
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using NLog;

namespace Peach.Core.Proxy.Web
{
	public delegate void HttpRequestEventHandler(HttpRequest request);
	public delegate void HttpResponseEventHandler(HttpResponse response);

	public class WebProxy
	{
		NLog.Logger logger = LogManager.GetLogger("Peach.Core.Proxy.Web");
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
