using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Analyzers;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Peach.Core.Test.Publishers
{

	class SimpleHttpListener
	{
		bool stop = false;

		public SimpleHttpListener()
		{
			Assert.True(HttpListener.IsSupported);
		}

		HttpListener listener;

		public void Stop()
		{
			this.stop = true;
			listener.Stop();
		}

		// This example requires the System and System.Net namespaces. 
		public void Listen(string[] prefixes)
		{
			// URI prefixes are required, 
			// for example "http://localhost:8080/index/".
			if (prefixes == null || prefixes.Length == 0)
				throw new ArgumentException("prefixes");

			// Create a listener.
			listener = new HttpListener();
			// Add the prefixes. 
			foreach (string s in prefixes)
			{
				listener.Prefixes.Add(s);
			}
			listener.Start();

			IAsyncResult ar = null;

			while(!stop)
			{
				try
				{
					if (ar == null)
						 ar = listener.BeginGetContext(null, null);

					if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1)))
						continue;

					// Note: The GetContext method blocks while waiting for a request. 
					HttpListenerContext context = listener.EndGetContext(ar);
					HttpListenerRequest request = context.Request;
					// Obtain a response object.

					if (request.ContentLength64 > 0)
					{
						byte[] buf = new byte[request.ContentLength64];
						request.InputStream.Read(buf, 0, buf.Length);
					}

					HttpListenerResponse response = context.Response;

					// Construct a response. 
					string responseString = request.HttpMethod + " Hello World Too = " + request.ContentLength64;
					byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
					// Get a response stream and write the response to it.
					response.ContentLength64 = buffer.Length;
					System.IO.Stream output = response.OutputStream;
					output.Write(buffer, 0, buffer.Length);
					// You must close the output stream.
					output.Close();
					response.Close();

					ar = null;
				}
				catch
				{
					return;
				}

			}
		}
	}


	[TestFixture]
	public class HttpPublisherTests : DataModelCollector
	{
		public string send_recv_template = @"
<Peach>
	<DataModel name=""TheDataModel"">
		<String name=""str"" value=""Hello World""/>
	</DataModel>

	<DataModel name=""TheDataModel2"">
		<String name=""str""/>
	</DataModel>

	<StateModel name=""ClientState"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Send"" type=""output"">
				<DataModel ref=""TheDataModel""/>
			</Action>
			<Action name=""Recv"" type=""input"">
				<DataModel ref=""TheDataModel2""/>
			</Action>
		</State>
	</StateModel>

<Test name=""Default"">
		<StateModel ref=""ClientState""/>
		<Publisher class=""Http"">
			<Param name=""Method"" value=""{0}""/>
			<Param name=""Url"" value=""{1}""/>
		</Publisher>
	</Test>

</Peach>
";

		public string recv_template = @"
<Peach>
	<DataModel name=""TheDataModel"">
		<String name=""str""/>
	</DataModel>

	<StateModel name=""ClientState"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Recv"" type=""input"">
				<DataModel ref=""TheDataModel""/>
			</Action>
		</State>
	</StateModel>

<Test name=""Default"">
		<StateModel ref=""ClientState""/>
		<Publisher class=""Http"">
			<Param name=""Method"" value=""{0}""/>
			<Param name=""Url"" value=""{1}""/>
		</Publisher>
	</Test>

</Peach>
";
		
		public void HttpClient(bool send_recv, string method)
		{
			ushort port = TestBase.MakePort(56000, 57000);
			string url = "http://localhost:" + port.ToString() + "/";

			SimpleHttpListener listener = new SimpleHttpListener();
			string[] prefixes = new string[1] {url};
			Thread lThread = new Thread(() => listener.Listen(prefixes));

			lThread.Start();
			try
			{
				string xml = string.Format(send_recv ? send_recv_template : recv_template, method, url);

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				RunConfiguration config = new RunConfiguration();
				config.singleIteration = true;

				Engine e = new Engine(null);
				e.startFuzzing(dom, config);

				if (send_recv)
				{
					Assert.AreEqual(2, actions.Count);

					var de1 = actions[0].dataModel.find("TheDataModel.str");
					Assert.NotNull(de1);
					var de2 = actions[1].dataModel.find("TheDataModel2.str");
					Assert.NotNull(de2);

					string send = (string)de1.DefaultValue;
					string recv = (string)de2.DefaultValue;

					Assert.AreEqual("Hello World", send);
					Assert.AreEqual(method + " Hello World Too = 11", recv);
				}
				else
				{
					Assert.AreEqual(1, actions.Count);
					var de1 = actions[0].dataModel.find("TheDataModel.str");
					Assert.NotNull(de1);

					string recv = (string)de1.DefaultValue;

					Assert.AreEqual(method + " Hello World Too = 0", recv);
				}
			}
			finally
			{
				listener.Stop();
				lThread.Join();
			}
		}

		[Test, ExpectedException(typeof(PeachException))]
		public void HttpClientSendGet()
		{
			// Http publisher does not support sending data when the GET method is used
			HttpClient(true, "GET");
		}

		[Test]
		public void HttpClientRecvGet()
		{
			// Http publisher does not support sending data when the GET method is used
			HttpClient(false, "GET");
		}

		[Test]
		public void HttpClientSendPost()
		{
			HttpClient(true, "POST");
		}

		[Test]
		public void HttpClientRecvPost()
		{
			HttpClient(false, "POST");
		}
	}
}
