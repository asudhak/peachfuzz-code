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
//   Adam Cecchetti (adam@dejavusecurity.com)

// $Id$

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Agent.Channels
{
	#region RestProxyPublisher

	public class RestProxyPublisher : Publisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public string Url = null;
		public string Agent { get; set; }
		public string Class { get; set; }
		public Dictionary<string, string> Args { get; set; }

		[Serializable]
		public class CreatePublisherRequest
		{
			public uint iteration = 0;
			public bool isControlIteration = false;
			public string Cls = "";
			public Dictionary<string, string> args = null;
		}

		[Serializable]
		public class RestProxyPublisherResponse
		{
			public bool error = false;
			public string errorString = null;
		}

		public RestProxyPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
			this.Args = new Dictionary<string,string>();

			foreach (var kv in args)
			{
				// Note: Cast to string rather than ToString()
				// since ToString can include debugging information.
				this.Args.Add(kv.Key, (string)kv.Value);
			}
		}

		public string Send(string query)
		{
			return Send(query, "");
		}

		public string Send(string query, Dictionary<string, Variant> args)
		{
			var newArg = new Dictionary<string, string>();

			foreach (var kv in args)
			{
				// NOTE: cast to string, rather than .ToString() since
				// .ToString() can include debugging information.
				newArg.Add(kv.Key, (string)kv.Value);
			}

			JsonArgsRequest request = new JsonArgsRequest();
			request.args = newArg;

			return Send(query, JsonConvert.SerializeObject(request));
		}

		public string Send(string query, string json, bool restart = true)
		{
			try
			{
				var httpWebRequest = (HttpWebRequest)WebRequest.Create(Url + "/Publisher/" + query);
				httpWebRequest.ContentType = "text/json";

				if (string.IsNullOrEmpty(json))
				{
					httpWebRequest.Method = "GET";
				}
				else
				{
					httpWebRequest.Method = "POST";
					using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
					{
						streamWriter.Write(json);
					}
				}

				var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

				if (httpResponse.GetResponseStream() != null)
				{
					using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
					{
						var jsonResponse = streamReader.ReadToEnd();
						var response = JsonConvert.DeserializeObject<RestProxyPublisherResponse>(jsonResponse);

						if (response.error)
						{
							logger.Warn("Query \"" + query + "\" error: " + response.errorString);
							RestartRemotePublisher();

							jsonResponse = Send(query, json, false);
							response = JsonConvert.DeserializeObject<RestProxyPublisherResponse>(jsonResponse);

							if (response.error)
							{
								logger.Warn("Unable to restart connection");
								throw new SoftException("Query \"" + query + "\" error: " + response.errorString);
							}
						}

						return jsonResponse;
					}
				}
				else
				{
					return "";
				}
			}
			catch (SoftException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new SoftException("Failure communicating with REST Agent", e);
			}
		}

		protected void RestartRemotePublisher()
		{
			logger.Debug("Restarting remote publisher");

			CreatePublisherRequest request = new CreatePublisherRequest();
			request.iteration = Iteration;
			request.isControlIteration = IsControlIteration;
			request.Cls = Class;
			request.args = Args;

			Send("CreatePublisher", JsonConvert.SerializeObject(request));
		}

		[Serializable]
		public class IterationRequest
		{
			public uint iteration = 0;
		}

		public override uint Iteration
		{
			get
			{
				return base.Iteration;
			}

			set
			{
				base.Iteration = value;

				IterationRequest request = new IterationRequest();
				request.iteration = value;

				Send("Set_Iteration", JsonConvert.SerializeObject(request));
			}
		}

		[Serializable]
		public class IsControlIterationRequest
		{
			public bool isControlIteration = false;
		}

		public override bool IsControlIteration
		{
			get
			{
				return base.IsControlIteration;
			}
			set
			{
				base.IsControlIteration = value;

				var request = new IsControlIterationRequest();
				request.isControlIteration = value;

				Send("Set_IsControlIteration", JsonConvert.SerializeObject(request));
			}
		}

		[Serializable]
		public class ResultRequest
		{
			public string result = null;
		}
		[Serializable]
		public class ResultResponse: RestProxyPublisherResponse
		{
			public string result = null;
		}

		public override string Result
		{
			get
			{
				return JsonConvert.DeserializeObject<ResultRequest>(Send("Get_Result")).result;
			}
			set
			{
				var request = new ResultResponse();
				request.result = value;

				Send("Set_Result", JsonConvert.SerializeObject(request));
			}
		}

		protected override void OnStart()
		{
			IsControlIteration = IsControlIteration;
			Iteration = Iteration;

			Send("start");
		}

		protected override void OnStop()
		{
			Send("stop");
		}

		protected override void OnOpen()
		{
			Send("open");
		}

		protected override void OnClose()
		{
			Send("close");
		}

		protected override void OnAccept()
		{
			Send("accept");
		}

		[Serializable]
		public class OnCallArgument
		{
			public string name;
			public byte[] data;
			public ActionParameter.Type type;
		}

		[Serializable]
		public class OnCallRequest
		{
			public string method = null;
			public OnCallArgument[] args;
		}

		[Serializable]
		public class OnCallResponse : RestProxyPublisherResponse
		{
			public Variant value = null;
		}

		protected override Variant OnCall(string method, List<ActionParameter> args)
		{
			var request = new OnCallRequest();

			request.method = method;
			request.args = new OnCallArgument[args.Count];

			for (int cnt = 0; cnt < args.Count; cnt++)
			{
				request.args[cnt] = new OnCallArgument();
				request.args[cnt].name = args[cnt].name;
				request.args[cnt].type = args[cnt].type;
				request.args[cnt].data = new byte[args[cnt].dataModel.Value.Length];
				args[cnt].dataModel.Value.Read(request.args[cnt].data, 0, (int)args[cnt].dataModel.Value.Length);
			}

			var json = Send("call", JsonConvert.SerializeObject(request));
			var response = JsonConvert.DeserializeObject<OnCallResponse>(json);

			return response.value;
		}

		[Serializable]
		public class OnSetPropertyRequest
		{
			public string property;
			public byte[] data;
		}

		protected override void OnSetProperty(string property, Variant value)
		{
			// The Engine always gives us a BitStream but we can't remote that

			var request = new OnSetPropertyRequest();

			request.property = property;
			request.data = (byte[])value;

			Send("setProperty", JsonConvert.SerializeObject(request));
		}

		[Serializable]
		public class OnGetPropertyResponse : RestProxyPublisherResponse
		{
			public Variant value;
		}

		protected override Variant OnGetProperty(string property)
		{
			var json = Send("getProperty");
			var response = JsonConvert.DeserializeObject<OnGetPropertyResponse>(json);
			return response.value;
		}

		[Serializable]
		public class OnOutputRequest
		{
			public byte[] data;
		}

		protected override void OnOutput(BitwiseStream data)
		{
			var request = new OnOutputRequest();
			request.data = new byte[data.Length];
			data.Read(request.data, 0, (int)data.Length);

			data.Position = 0;

			Send("output", JsonConvert.SerializeObject(request));
		}

		protected override void OnInput()
		{
			Send("input");
			ReadAllBytes();
		}

		[Serializable]
		public class WantBytesRequest
		{
			public long count;
		}

		public override void WantBytes(long count)
		{
			var request = new WantBytesRequest();
			request.count = count;

			Send("WantBytes", JsonConvert.SerializeObject(request));
			ReadAllBytes();
		}

		[Serializable]
		public class ReadBytesRequest
		{
			public int count = 0;
		}

		[Serializable]
		public class ReadBytesResponse : RestProxyPublisherResponse
		{
			public byte[] data;
		}

		public byte[] ReadBytes(int count)
		{
			var request = new ReadBytesRequest();
			request.count = count;

			var json = Send("ReadBytes", JsonConvert.SerializeObject(request));
			var response = JsonConvert.DeserializeObject<ReadBytesResponse>(json);

			return response.data;
		}

		public byte[] ReadAllBytes()
		{
			var json = Send("ReadAllBytes");
			var response = JsonConvert.DeserializeObject<ReadBytesResponse>(json);

			return response.data;
		}
	}

	#endregion

	[Serializable]
	public class AgentMessageRest
	{
		public string Method = null;
		public object[] Arguments = null;
		public Dictionary<string, Variant> Parameters = null;
	}

	[Serializable]
	public class JsonResponse
	{
		public string Status { get; set; }
		public string Data { get; set; }
		public Dictionary<string, object> Results { get; set; }
	}

	[Serializable]
	public class JsonFaultResponse
	{
		public string Status { get; set; }
		public string Data { get; set; }
		public Fault[] Results { get; set; }
	}

	[Serializable]
	public class JsonArgsRequest
	{
		public Dictionary<string, string> args { get; set; }
	}

	[Agent("http", true)]
	public class AgentClientRest : AgentClient
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		private static string _url;
		public AgentClientRest(string name, string uri, string password)
		{
			this.name = name;

			_url = uri + "/Agent";
			if (string.IsNullOrEmpty(uri))
				throw new PeachException("Uri for rest agent cannot be empty");
		}

		public override bool SupportedProtocol(string protocol)
		{
			logger.Trace("SupportedProtocol");
			OnSupportedProtocolEvent(protocol);

			protocol = protocol.ToLower();
			if (protocol == "http")
				return true;

			return false;
		}

		private JsonResponse ParseResponse(string json)
		{
			if (string.IsNullOrEmpty(json))
				throw new PeachException("Agent Response Empty");

			try
			{
				return JsonConvert.DeserializeObject<JsonResponse>(json);
			}
			catch (Exception e)
			{
				throw new PeachException("Failed to deserialize JSON response from Agent", e);
			}
		}

		public override void AgentConnect(string name, string url, string password)
		{
			logger.Trace("AgentConnect");
			OnAgentConnectEvent(name, url, password);

			Send("AgentConnect");
		}

		public override void AgentDisconnect()
		{
			logger.Trace("AgentDisconnect");
			OnAgentDisconnectEvent();

			Send("AgentDisconnect");
		}

		public override Publisher CreatePublisher(string cls, Dictionary<string, Variant> args)
		{
			logger.Trace("CreatePublisher: {0}", cls);
			OnCreatePublisherEvent(cls, args);
			var pub = new RestProxyPublisher(args);
			pub.Class = cls;
			//pub.Args = newargs;
			pub.Url = _url;

			return pub;
		}

		public override BitwiseStream CreateBitwiseStream()
		{
			logger.Trace("BitwiseStream");
			OnCreateBitwiseStreamEvent();
			return new BitStream();
		}

		public override void StartMonitor(string name, string cls, Dictionary<string, Variant> args)
		{
			logger.Trace("StartMonitor: {0}, {1}", name, cls);
			OnStartMonitorEvent(name, cls, args);
			Send("StartMonitor?name=" + name + "&cls=" + cls, args);
		}

		public override void StopMonitor(string name)
		{
			logger.Trace("AgentConnect: {0}", name);
			OnStopMonitorEvent(name);
			Send("StopMonitor?name=" + name);
		}

		public override void StopAllMonitors()
		{
			logger.Trace("StopAllMonitors");
			OnStopAllMonitorsEvent();
			Send("StopAllMonitors");
		}

		public override void SessionStarting()
		{
			logger.Trace("SessionStarting");
			OnSessionStartingEvent();
			Send("SessionStarting");
		}

		public override void SessionFinished()
		{
			logger.Trace("SessionFinished");
			OnSessionFinishedEvent();
			Send("SessionFinished");
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			logger.Trace("IterationStarting: {0}, {1}", iterationCount, isReproduction);
			OnIterationStartingEvent(iterationCount, isReproduction);
			Send("IterationStarting?iterationCount=" + iterationCount.ToString() + "&" + "isReproduction=" + isReproduction.ToString());
		}

		public override bool IterationFinished()
		{
			logger.Trace("IterationFinished");
			OnIterationFinishedEvent();
			string json = Send("IterationFinished");
			JsonResponse response = ParseResponse(json);
			return Convert.ToBoolean(response.Status);
		}

		public override bool DetectedFault()
		{
			logger.Trace("DetectedFault");
			OnDetectedFaultEvent();
			string json = Send("DetectedFault");
			JsonResponse response = ParseResponse(json);
			return Convert.ToBoolean(response.Status);
		}

		public override Fault[] GetMonitorData()
		{
			logger.Trace("GetMonitorData");
			OnGetMonitorDataEvent();

			try
			{
				string json = Send("GetMonitorData");
				JsonFaultResponse response = JsonConvert.DeserializeObject<JsonFaultResponse>(json);

				return response.Results;
			}
			catch (Exception e)
			{
				logger.Debug(e.ToString());
				throw new PeachException("Failed to get Monitor Data", e);
			}
		}

		public override bool MustStop()
		{
			logger.Trace("MustStop");
			OnMustStopEvent();
			string json = Send("MustStop");
			JsonResponse response = ParseResponse(json);
			return Convert.ToBoolean(response.Status);
		}

		public override Variant Message(string name, Variant data)
		{
			logger.Trace("Message: {0}", name);
			OnMessageEvent(name, data);
			throw new NotImplementedException();
		}

		public string Send(string query)
		{
			return Send(query, "");
		}

		public string Send(string query, Dictionary<string, Variant> args)
		{
			var newArg = new Dictionary<string, string>();

			foreach (var kv in args)
			{
				// Note: Cast rather than call .ToString() since
				// ToString() can include debugging information
				newArg.Add(kv.Key, (string)kv.Value);
			}

			JsonArgsRequest request = new JsonArgsRequest();
			request.args = newArg;

			return Send(query, JsonConvert.SerializeObject(request));
		}

		public string Send(string query, string json)
		{
			try
			{
				var httpWebRequest = (HttpWebRequest)WebRequest.Create(_url + "/" + query);
				httpWebRequest.ContentType = "text/json";
				if (string.IsNullOrEmpty(json))
				{
					httpWebRequest.Method = "GET";
				}
				else
				{
					httpWebRequest.Method = "POST";
					using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
					{
						streamWriter.Write(json);
					}
				}
				var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

				if (httpResponse.GetResponseStream() != null)
				{
					using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
					{
						return streamReader.ReadToEnd();
					}
				}
				else
				{
					return "";
				}
			}
			catch (Exception e)
			{
				throw new PeachException("Failure communicating with REST Agent", e);
			}
		}
	}
}
// end
