
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
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Xml.Serialization;
using Peach.Core;
using Peach.Core.Dom;
using NLog;
using Peach.Core.Agent;
using ZeroMQ;

namespace Peach.Core.Agent.Channels
{
	[Serializable]
	public class AgentMessageZeroMq
	{
		public string Method = null;
		public object[] Arguments = null;
		public SerializableDictionary<string, Variant> Parameters = null;
	}

	[Agent("zmq", true)]
	public class AgentClientZeroMq : AgentClient
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		ZmqContext context = null;
		ZmqSocket client = null;

		public AgentClientZeroMq(string name, string uri, string password)
		{
		}

		public override bool SupportedProtocol(string protocol)
		{
			logger.Trace("SupportedProtocol");
			OnSupportedProtocolEvent(protocol);

			protocol = protocol.ToLower();
			if (protocol == "zmq")
				return true;

			return false;
		}

		public override void AgentConnect(string name, string url, string password)
		{
			logger.Trace("AgentConnect");

			url = url.Replace("zmq://", "tcp://");

			OnAgentConnectEvent(name, url, password);

			if (client != null)
				client.Dispose();
			if (context != null)
				context.Dispose();

			context = ZmqContext.Create();
			client = context.CreateSocket(SocketType.REQ);
			client.Connect(url);
			Send("AgentConnect");
		}

		public override void AgentDisconnect()
		{
			logger.Trace("AgentDisconnect");
			OnAgentDisconnectEvent();

			if (client != null)
			{
				Send("AgentDisconnect");
				client.Dispose();
			}
			if (context != null)
				context.Dispose();

			client = null;
			context = null;
		}

		public override Publisher CreatePublisher(string cls, SerializableDictionary<string, Variant> args)
		{
			logger.Trace("CreatePublisher: {0}", cls);
			OnCreatePublisherEvent(cls, args);
			throw new NotImplementedException();
		}

		public override void StartMonitor(string name, string cls, SerializableDictionary<string, Variant> args)
		{
			logger.Trace("StartMonitor: {0}, {1}", name, cls);
			OnStartMonitorEvent(name, cls, args);
			Send("StartMonitor", args, name, cls);
		}

		public override void StopMonitor(string name)
		{
			logger.Trace("AgentConnect: {0}", name);
			OnStopMonitorEvent(name);
			Send("StopMonitor", name);
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
			Send("IterationStarting", iterationCount, isReproduction);
		}

		public override bool IterationFinished()
		{
			logger.Trace("IterationFinished");
			OnIterationFinishedEvent();
			return (bool)Send("IterationFinished").Arguments[0];
		}

		public override bool DetectedFault()
		{
			logger.Trace("DetectedFault");
			OnDetectedFaultEvent();
			return (bool)Send("DetectedFault").Arguments[0];
		}

		public override Fault[] GetMonitorData()
		{
			logger.Trace("GetMonitorData");
			OnGetMonitorDataEvent();
			return (Fault[])Send("GetMonitorData").Arguments[0];
		}

		public override bool MustStop()
		{
			logger.Trace("MustStop");
			OnMustStopEvent();
			return (bool) Send("MustStop").Arguments[0];
		}

		public override Variant Message(string name, Variant data)
		{
			logger.Trace("Message: {0}", name);
			OnMessageEvent(name, data);
			return (Variant)Send("Message", name, data).Arguments[0];
		}

		XmlSerializer serializer = new XmlSerializer(typeof(AgentMessageZeroMq));

		public AgentMessageZeroMq Send(string method, SerializableDictionary<string, Variant> args, params object[] arguments)
		{
			AgentMessageZeroMq msg = new AgentMessageZeroMq();
			msg.Method = method;
			msg.Arguments = arguments;
			msg.Parameters = args;

			var outWriter = new StringWriter();
			serializer.Serialize(outWriter, msg);
			byte[] buff = Encoding.UTF8.GetBytes(outWriter.ToString());
			List<byte[]> frames = new List<byte[]>();
			frames.Add(buff);

			client.SendMessage(new ZmqMessage(frames));

			return Receive();
		}

		public AgentMessageZeroMq Send(string method, params object[] arguments)
		{
			AgentMessageZeroMq msg = new AgentMessageZeroMq();
			msg.Method = method;
			msg.Arguments = arguments;

			var outWriter = new StringWriter();
			serializer.Serialize(outWriter, msg);
			byte[] buff = Encoding.UTF8.GetBytes(outWriter.ToString());
			List<byte[]> frames = new List<byte[]>();
			frames.Add(buff);

			client.SendMessage(new ZmqMessage(frames));

			return Receive();
		}

		public AgentMessageZeroMq Receive()
		{
			using(MemoryStream sin = new MemoryStream())
			{
				var msg = client.ReceiveMessage();
				foreach (Frame frame in msg)
					sin.Write(frame.Buffer, 0, frame.BufferSize);

				sin.Position = 0;
				using(StreamReader reader = new StreamReader(sin, System.Text.Encoding.UTF8))
				{
					return (AgentMessageZeroMq)serializer.Deserialize(reader);
				}
			}
		}
	}

	/// <summary>
	/// Implement agent service running over XML-RPC.
	/// </summary>
	public class AgentServiceZeroMq : IAgent
	{
		public Agent agent = null;
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public AgentServiceZeroMq()
		{
			agent = new Agent("AgentServiceTcpRemote");
		}

		public void AgentConnect(string password)
		{
			logger.Trace("AgentConnect");
			agent.AgentConnect();
		}

		public void AgentDisconnect()
		{
			logger.Trace("AgentDisconnect");
			agent.AgentDisconnect();
		}

		public Publisher CreatePublisher(string cls, SerializableDictionary<string, Variant> args)
		{
			throw new NotImplementedException();
		}

		public void StartMonitor(string name, string cls, SerializableDictionary<string, Variant> args)
		{
			logger.Trace("StartMonitor: {0}, {1}", name, cls);
			agent.StartMonitor(name, cls, args);
		}

		public void StopMonitor(string name)
		{
			logger.Trace("AgentConnect: {0}", name);
			agent.StopMonitor(name);
		}

		public void StopAllMonitors()
		{
			logger.Trace("StopAllMonitors");
			agent.StopAllMonitors();
		}

		public void SessionStarting()
		{
			logger.Trace("SessionStarting");
			agent.SessionStarting();
		}

		public void SessionFinished()
		{
			logger.Trace("SessionFinished");
			agent.SessionFinished();
		}

		public void IterationStarting(uint iterationCount, bool isReproduction)
		{
			logger.Trace("IterationStarting: {0}, {1}", iterationCount, isReproduction);
			agent.IterationStarting(iterationCount, isReproduction);
		}

		public bool IterationFinished()
		{
			logger.Trace("IterationFinished");
			return agent.IterationFinished();
		}

		public bool DetectedFault()
		{
			logger.Trace("DetectedFault");
			return agent.DetectedFault();
		}

		public Fault[] GetMonitorData()
		{
			logger.Trace("GetMonitorData");
			return agent.GetMonitorData();
		}

		public bool MustStop()
		{
			logger.Trace("MustStop");
			return agent.MustStop();
		}

		public Variant Message(string name, Variant data)
		{
			logger.Trace("Message: {0}", name);
			return agent.Message(name, data);
		}

		public object QueryMonitors(string query)
		{
			logger.Trace("QueryMonitors: {0}", query);
			return agent.QueryMonitors(query);
		}
	}

	[AgentServer("zmq")]
	public class AgentServerZeroMq : IAgentServer
	{
		#region IAgentServer Members
		XmlSerializer serializer = new XmlSerializer(typeof(AgentMessageZeroMq));

		public void Run(Dictionary<string, string> args)
		{
			try
			{
				int port = 9001;

				if (args.ContainsKey("port"))
					port = int.Parse(args["port"]);

				AgentMessageZeroMq msg;
				AgentServiceZeroMq agent = null;
				AgentMessageZeroMq ack = new AgentMessageZeroMq();
				ack.Method = "ACK";
				ack.Arguments = null;
				string ackMessage;

				{
					var outStr = new StringWriter();
					serializer.Serialize(outStr, ack);
					ackMessage = outStr.ToString();
				}

				using (ZmqContext context = ZmqContext.Create())
				using (ZmqSocket server = context.CreateSocket(SocketType.REP))
				{
					server.Bind("tcp://*:" + port);

					Console.WriteLine(" -- Press ENTER to quit agent -- ");

					while (true)
					{
						msg = Receive(server);

						if (msg.Method == "AgentConnect")
						{
							if (agent != null)
								agent.AgentDisconnect();

							agent = new AgentServiceZeroMq();
							agent.AgentConnect(null);
							Send(server, ackMessage);
						}
						else if (msg.Method == "StartMonitor")
						{
							try
							{
								agent.StartMonitor((string)msg.Arguments[0], (string)msg.Arguments[1], msg.Parameters);
								Send(server, ackMessage);
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.ToString());

								AgentMessageZeroMq rep = new AgentMessageZeroMq();
								rep.Method = "Exception";
								rep.Arguments = new object[1];
								rep.Arguments[0] = ex;

								var outStr = new StringWriter();
								serializer.Serialize(outStr, rep);
								var reply = outStr.ToString();

								Send(server, reply);
							}
						}
						else
						{
							try
							{
								Type[] types;

								if (msg.Arguments == null)
									types = new Type[0];
								else
								{
									types = new Type[msg.Arguments.Count()];
									for (int cnt = 0; cnt < msg.Arguments.Count(); cnt++)
										types[cnt] = msg.Arguments[cnt].GetType();
								}

								var method = typeof(AgentServiceZeroMq).GetMethod(msg.Method, types);
								object ret = null;

								if(msg.Arguments == null)
									ret = method.Invoke(agent, new object[0]);
								else
									ret = method.Invoke(agent, msg.Arguments);

								if (ret == null)
									Send(server, ackMessage);
								else
								{
									AgentMessageZeroMq rep = new AgentMessageZeroMq();
									rep.Method = "ACK";
									rep.Arguments = new object[1];
									rep.Arguments[0] = ret;

									var outStr = new StringWriter();
									serializer.Serialize(outStr, rep);
									var reply = outStr.ToString();

									Send(server, reply);
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.ToString());

								AgentMessageZeroMq rep = new AgentMessageZeroMq();
								rep.Method = "Exception";
								rep.Arguments = new object[1];
								rep.Arguments[0] = ex;

								var outStr = new StringWriter();
								serializer.Serialize(outStr, rep);
								var reply = outStr.ToString();

								Send(server, reply);
							}
						}
					}
				}
			}
			catch (Exception eex)
			{
				Console.WriteLine(eex.ToString());
			}
		}

		public void Send(ZmqSocket socket, string message)
		{
			byte[] msg = Encoding.UTF8.GetBytes(message);
			List<byte[]> frames = new List<byte[]>();
			int parts = msg.Length / (1024 * 32);

			if (parts > 0)
			{
				byte[] frame = new byte[1024 * 32];
				int start;

				for (int cnt = 0; cnt < parts; cnt++)
				{
					start = cnt * (1024 * 32);

					for (int i = 0; i < (1024 * 32); i++)
						frame[i] = msg[start + i];

					frames.Add(frame);
				}

				if (msg.Length % (1024 * 32) > 0)
				{
					start = (parts + 1) * (1024 * 32);
					byte[] last = new byte[msg.Length - start];

					for (int cnt = 0; (start + cnt) < msg.Length; cnt++)
					{
						last[cnt] = msg[start + cnt];
					}

					frames.Add(last);
				}

			}
			else
				frames.Add(msg);

			ZmqMessage zmessage = new ZmqMessage(frames);
			socket.SendMessage(zmessage);
		}

		public AgentMessageZeroMq Receive(ZmqSocket socket)
		{
			using (MemoryStream sin = new MemoryStream())
			{
				var msg = socket.ReceiveMessage();
				foreach (Frame frame in msg)
					sin.Write(frame.Buffer, 0, frame.BufferSize);

				sin.Position = 0;
				using (StreamReader reader = new StreamReader(sin, System.Text.Encoding.UTF8))
				{
					return (AgentMessageZeroMq) serializer.Deserialize(reader);
				}
			}
		}


		#endregion
	}
}

// end

