
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

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NLog;
using SuperWebSocket;
using Newtonsoft.Json.Linq;
using SuperSocket.SocketBase;

using Peach.Core;
using Peach.Core.IO;
using Peach.Core.Publishers;

namespace Peach.Core.Publishers
{
	[Publisher("WebSocket", true)]
	[Description("WebSocket Publisher")]
	[Parameter("Port", typeof(int), "Port to listen for connections on", "8080")]
	[Parameter("Template", typeof(string), "Data template for publishing")]
	[Parameter("Publish", typeof(string), "How to publish data, base64 or url.", "base64")]
	[Parameter("DataToken", typeof(string), "Token to replace with data in template", "##DATA##")]
	[Parameter("Timeout", typeof(int), "Time in milliseconds to wait for client response", "60000")]
	public class WebSocketPublisher : Publisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		WebSocketServer _socketServer;
		WebSocketSession _session;
		AutoResetEvent _evaluated = new AutoResetEvent(false);
		AutoResetEvent _msgReceived = new AutoResetEvent(false);

		public int Port { get; set; }
		public string Template { get; set; }
		public string Publish { get; set; }
		public string DataToken { get; set; }
		public int Timeout { get; set; }

		string _template;

		public WebSocketPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
			_socketServer = new WebSocketServer();
			_socketServer.Setup(Port);
			_socketServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(appServer_NewMessageReceived);
			_socketServer.NewSessionConnected += _socketServer_NewSessionConnected;

			_template = System.IO.File.ReadAllText(Template);
		}

		void _socketServer_NewSessionConnected(WebSocketSession session)
		{
			logger.Debug("NewSessionConnection");
			_session = session;
		}

		protected override void OnStart()
		{
			base.OnStart();

			logger.Debug("Starting WebSocketServer");
			if (!_socketServer.Start())
				throw new PeachException("Error, web socket server failed to start.");

			logger.Debug("Waiting for WebSocket connection");
			_msgReceived.WaitOne();
			logger.Debug("Connection was received");
		}

		protected override void OnStop()
		{
			base.OnStop();

			_socketServer.Stop();
		}

		protected override void OnOutput(BitwiseStream data)
		{
			try
			{
				logger.Debug(">> OnOutput");
				logger.Debug("Waiting for evaluated or client ready msg");
				_evaluated.WaitOne(Timeout);
				_evaluated.Reset();
				_session.Send(BuildMessage(data));
				logger.Debug("<< OnOutput");
			}
			catch (Exception ex)
			{
				logger.Debug(ex.ToString());
				throw;
			}
		}

		protected string BuildTemplate(BitwiseStream data)
		{
			var value = Publish;

			if (Publish == "base64")
			{
				data.Seek(0, SeekOrigin.Begin);
				var buf = new BitReader(data).ReadBytes((int)data.Length);
				value = Convert.ToBase64String(buf);
			}

			return _template.Replace(DataToken, value);
		}

		protected string BuildMessage(BitwiseStream data)
		{
			var ret = new StringBuilder();
			var msg = new JObject();

			msg["type"] = "template";
			msg["content"] = BuildTemplate(data);

			ret.Append(msg.ToString(Newtonsoft.Json.Formatting.None));
			ret.Append("\n");

			// Compatability with older usage
			msg["type"] = "msg";
			msg["content"] = "evaluate";

			ret.Append(msg.ToString(Newtonsoft.Json.Formatting.None));
			ret.Append("\n");

			return ret.ToString();
		}

		void appServer_NewMessageReceived(WebSocketSession session, string message)
		{
			logger.Debug("NewMessageReceived: " + message);

			_msgReceived.Set();

			var json = JObject.Parse(message);
			if (((string)json["msg"]) == "Evaluation complete" || ((string)json["msg"]) == "Client ready")
				_evaluated.Set();
		}
	}
}

// end
