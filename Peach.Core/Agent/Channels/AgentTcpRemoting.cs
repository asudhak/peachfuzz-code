
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using Peach.Core;
using Peach.Core.Dom;
using NLog;
using Peach.Core.Agent;
using System.Net.Sockets;

namespace Peach.Core.Agent.Channels
{
	[Agent("tcp", true)]
	public class AgentClientTcpRemoting : AgentClient
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		int _remotingWaitTime = 1000 * 60 * 1;
		TcpClientChannel _channel = null;
		AgentServiceTcpRemote proxy = null;
		string _url = null;

		/// <summary>
		/// This is set to true when fault data is collected
		/// indicating a fualt has occured.  When a fault occures
		/// the agent might get restarted or a virtual machine
		/// reset to a snapshot.
		/// </summary>
		bool _restartAgent = false;

		/// <summary>
		/// Contains information about all created monitors.
		/// </summary>
		/// <remarks>
		/// When restarting the agent connect we will need to recreate monitors.
		/// 
		/// The tuple contains:
		/// 
		///   * name
		///   * cls
		///   * args
		/// 
		/// </remarks>
		List<Tuple<string, string, SerializableDictionary<string, Variant>>> _monitors = new List<Tuple<string, string, SerializableDictionary<string, Variant>>>();

		public AgentClientTcpRemoting(string name, string uri, string password)
		{
		}

		/// <summary>
		/// Perform our remoting call with a forced timeout.
		/// </summary>
		/// <param name="method"></param>
		protected void PerformRemoting(ThreadStart method)
		{
			Exception remotingException = null;

			var thread = new System.Threading.Thread(delegate()
			{
				try
				{
					method();
				}
				catch (Exception ex)
				{
					remotingException = ex;
				}
			});

			thread.Start();
			if (thread.Join(_remotingWaitTime))
			{
				if (remotingException != null)
				{
					if (remotingException is PeachException)
						throw new PeachException(remotingException.Message, remotingException);

					if (remotingException is SoftException)
						throw new SoftException(remotingException.Message, remotingException);

					if (remotingException is RemotingException)
						throw new RemotingException(remotingException.Message, remotingException);

					throw new AgentException(remotingException.Message, remotingException);
				}
			}
			else
			{
				throw new RemotingException("Remoting call timed out.");
			}
		}

		public override bool SupportedProtocol(string protocol)
		{
			logger.Trace("SupportedProtocol");
			OnSupportedProtocolEvent(protocol);

			protocol = protocol.ToLower();
			if (protocol == "tcp")
				return true;

			return false;
		}

		protected void CreateProxy()
		{
			RemoveProxy();

			IDictionary props = new Hashtable() as IDictionary;
			props["timeout"] = (uint)1000*60*1; // wait one minute max
			props["connectionTimeout"] = (uint)1000*60*1; // wait one minute max

			_channel = new TcpClientChannel(props, null);
			ChannelServices.RegisterChannel(_channel, false); // Disable security for speed

			proxy = (AgentServiceTcpRemote)Activator.GetObject(typeof(AgentServiceTcpRemote), _url);
			if (proxy == null)
				throw new PeachException("Error, unable to create proxy for remote agent '" + _url + "'.");
		}

		protected void RemoveProxy()
		{
			proxy = null;

			if (_channel != null)
			{
				ChannelServices.UnregisterChannel(_channel);
				_channel = null;
			}
		}

		public override void AgentConnect(string name, string url, string password)
		{
			logger.Trace("AgentConnect");
			OnAgentConnectEvent(name, url, password);

			if (proxy != null)
				AgentDisconnect();

			if (url.EndsWith("/"))
				url += "PeachAgent";
			else
				url += "/PeachAgent";

			_url = url;

			CreateProxy();

			try
			{
				proxy.AgentConnect(null);
			}
			catch (Exception ex)
			{
				throw new PeachException("Error, unable to connect to remote agent '" + _url + "'.  " + ex.Message, ex);
			}
		}

		public override void AgentDisconnect()
		{
			logger.Trace("AgentDisconnect");
			OnAgentDisconnectEvent();

			PerformRemoting(delegate() { proxy.AgentDisconnect(); });
			RemoveProxy();
		}

		/// <summary>
		/// This method is used to recreate monitors when we restart an agent connection.
		/// </summary>
		protected void RecreateMonitors()
		{
			foreach (var moninfo in _monitors)
			{
				PerformRemoting(delegate() { proxy.StartMonitor(moninfo.Item1, moninfo.Item2, moninfo.Item3); });
			}
		}

		public override Publisher CreatePublisher(string cls, SerializableDictionary<string, Variant> args)
		{
			logger.Trace("CreatePublisher: {0}", cls);

			OnCreatePublisherEvent(cls, args);

			Publisher ret = null;
			PerformRemoting(delegate() { ret = proxy.CreatePublisher(cls, args); });

			return ret;
		}

		public override void StartMonitor(string name, string cls, SerializableDictionary<string, Variant> args)
		{
			logger.Trace("StartMonitor: {0}, {1}", name, cls);

			_monitors.Add(new Tuple<string, string, SerializableDictionary<string, Variant>>(name, cls, args));

			OnStartMonitorEvent(name, cls, args);
			PerformRemoting(delegate() { proxy.StartMonitor(name, cls, args); });
		}

		public override void StopMonitor(string name)
		{
			logger.Trace("AgentConnect: {0}", name);
			OnStopMonitorEvent(name);
			PerformRemoting(delegate() { proxy.StopMonitor(name); });
		}

		public override void StopAllMonitors()
		{
			logger.Trace("StopAllMonitors");
			OnStopAllMonitorsEvent();
			PerformRemoting(delegate() { proxy.StopAllMonitors(); });
		}

		public override void SessionStarting()
		{
			logger.Trace("SessionStarting");
			OnSessionStartingEvent();
			PerformRemoting(delegate() { proxy.SessionStarting(); });
		}

		public override void SessionFinished()
		{
			logger.Trace("SessionFinished");
			OnSessionFinishedEvent();
			PerformRemoting(delegate() { proxy.SessionFinished(); });
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			logger.Trace("IterationStarting: {0}, {1}", iterationCount, isReproduction);

			OnIterationStartingEvent(iterationCount, isReproduction);

			if (_restartAgent)
				_restartAgent = false;

			try
			{
				bool connected = false;
				PerformRemoting(delegate() { connected = proxy.Connected; });
				if (connected)
				{
					PerformRemoting(delegate() { proxy.IterationStarting(iterationCount, isReproduction); });
					return;
				}
			}
			catch (SocketException)
			{
				logger.Debug("IterationStarting: Socket error, recreating proxy");

				proxy = (AgentServiceTcpRemote)Activator.GetObject(typeof(AgentServiceTcpRemote), _url);
				if (proxy == null)
					throw new PeachException("Error, unable to create proxy for remote agent '" + _url + "'.");
			}

			proxy.AgentConnect(null);
			proxy.SessionStarting();
			RecreateMonitors();
			proxy.IterationStarting(iterationCount, isReproduction);
		}

		public override bool IterationFinished()
		{
			logger.Trace("IterationFinished");
			OnIterationFinishedEvent();
			bool ret = false;
			PerformRemoting(delegate() { ret = proxy.IterationFinished(); });
			return ret;
		}

		public override bool DetectedFault()
		{
			logger.Trace("DetectedFault");
			OnDetectedFaultEvent();
			
			bool ret = false;
			PerformRemoting(delegate() { ret = proxy.DetectedFault(); });
			return ret;
		}

		public override Fault[] GetMonitorData()
		{
			logger.Trace("GetMonitorData");

			// On next iteration starting we will reconnect
			// to our remote agent.
			_restartAgent = true;

			OnGetMonitorDataEvent();

			Fault[] ret = null;
			PerformRemoting(delegate() { ret = proxy.GetMonitorData(); });
			return ret;
		}

		public override bool MustStop()
		{
			logger.Trace("MustStop");
			OnMustStopEvent();

			bool ret = false;
			PerformRemoting(delegate() { ret = proxy.MustStop(); });
			return ret;
		}

		public override Variant Message(string name, Variant data)
		{
			logger.Trace("Message: {0}", name);
			OnMessageEvent(name, data);
			
			Variant ret = null;
			PerformRemoting(delegate() { ret = proxy.Message(name, data); });
			return ret;
		}

	}

	/// <summary>
	/// Implement agent service running over XML-RPC.
	/// </summary>
	public class AgentServiceTcpRemote : MarshalByRefObject, IAgent
	{
		public Agent agent = null;
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		public AgentServiceTcpRemote()
		{
			agent = new Agent("AgentServiceTcpRemote");
		}

		public bool Connected
		{
			get; private set;
		}

		public void AgentConnect(string password)
		{
			logger.Trace("AgentConnect");
			Connected = true;
			agent.AgentConnect();
		}

		public void AgentDisconnect()
		{
			logger.Trace("AgentDisconnect");
			agent.AgentDisconnect();
		}

		public Publisher CreatePublisher(string cls, SerializableDictionary<string, Variant> args)
		{
			logger.Trace("CreatePublisher: {0}", cls);
			return agent.CreatePublisher(cls, args);
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

	[AgentServer("tcp")]
	public class AgentServerTcpRemoting : IAgentServer
	{
		#region IAgentServer Members

		public void Run(Dictionary<string, string> args)
		{
			int port = 9001;

			if (args.ContainsKey("port"))
				port = int.Parse(args["port"]);

			//select channel to communicate
			//IDictionary props = new Hashtable() as IDictionary;
			//props["port"] = port;
			//props["exclusiveAddressUse"] = false;
			//var chan = new TcpServerChannel(props, null);
			TcpChannel chan = new TcpChannel(port);
			ChannelServices.RegisterChannel(chan, false);    //register channel

			//register remote object
			RemotingConfiguration.RegisterWellKnownServiceType(
				typeof(AgentServiceTcpRemote),
				"PeachAgent", WellKnownObjectMode.Singleton);

			//inform console
			Console.WriteLine(" -- Press ENTER to quit agent -- ");
			Console.ReadLine();
		}

		#endregion
	}
}

// end
