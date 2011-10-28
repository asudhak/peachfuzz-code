using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Peach.Core.WebProxy;

namespace Peach.Core.WebProxy.Tester
{
	class Program
	{
		static NLog.Logger logger = LogManager.GetLogger("Peach.Core.WebProxy.Tester.Program");

		static void Main(string[] args)
		{
			logger.Info("WebProxy Tester Starting Up!!");

			Proxy proxy = new Proxy();
			WebProxy webProxy = new WebProxy(proxy);
			webProxy.NewHttpRequest += new HttpRequestEventHandler(webProxy_NewHttpRequest);
			webProxy.NewHttpResponse += new HttpResponseEventHandler(webProxy_NewHttpResponse);

			proxy.Run();
		}

		static void webProxy_NewHttpRequest(HttpRequest request)
		{
			logger.Info(">> Request: " + request.Uri);
			string data = request.ToString();
			byte[] buff = ASCIIEncoding.ASCII.GetBytes(data);

			try
			{
				logger.Info(">> Sending request along");
				request.Connection.ServerStream.Write(buff, 0, buff.Length);
				logger.Info(">> Sent request!");
			}
			catch(Exception e)
			{
				logger.Error(e.ToString());
			}
		}

		static void webProxy_NewHttpResponse(HttpResponse response)
		{
			logger.Info("<< Response: " + response.Status);
			string data = response.ToString();
			byte[] buff = ASCIIEncoding.ASCII.GetBytes(data);
			response.Connection.ClientStream.Write(buff, 0, buff.Length);
		}

	}
}
