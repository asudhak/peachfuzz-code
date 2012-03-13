using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Peach.Core.Proxy.Web;

namespace Peach.Core.Proxy.Web.Tester
{
	class Program
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		static void Main(string[] args)
		{
			logger.Info("WebProxy Tester Starting Up!!");

			File.WriteAllText("c:\\webproxy.txt", "");

			Proxy proxy = new Proxy();
			WebProxy webProxy = new WebProxy(proxy);
			webProxy.NewHttpRequest += new HttpRequestEventHandler(webProxy_NewHttpRequest);
			webProxy.NewHttpResponse += new HttpResponseEventHandler(webProxy_NewHttpResponse);

			proxy.Run();
		}

		static void webProxy_NewHttpRequest(HttpRequest request)
		{
			logger.Info(">> Request: " + request.Uri);
			byte[] buff = request.ToByteArray();

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
			try
			{
				logger.Info("<< Response: " + response.Status);
				byte[] buff = response.ToByteArray();
				response.Connection.ClientStream.Write(buff, 0, buff.Length);
				logger.Info("<< Sent Response!");

				using (FileStream sout = File.Open("c:\\webproxy.txt", FileMode.Append))
				{
					sout.Seek(0, SeekOrigin.End);
					sout.Write(buff, 0, buff.Length);
				}
			}
			catch (Exception e)
			{
				logger.Error(e.ToString());
			}
		}
	}
}
