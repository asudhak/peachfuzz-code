
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using Peach.Core.Runtime;

using NLog;
using NLog.Config;
using NLog.Targets;

namespace Peach.Core.WindowsDebugInstance
{
	class Program
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		static bool debug = false;
		static bool timebomb = true;
		static string channelName = null;

		static void Syntax()
		{
			Console.WriteLine("> Peach.Core.WindowsDebugInstance");
			Console.WriteLine("> Copyright (c) Deja vu Security\n");

			Console.WriteLine("Syntax:");
			Console.WriteLine(" Peach.Core.WindowsDebugInstance.exe [--debug] IPC_CHANNEL_NAME\n\n");

			throw new SyntaxException();
		}

		static void Main(string[] args)
		{
			var DefaultForeground = Console.ForegroundColor;

			try
			{
				if (args.Length == 0)
					Syntax();

				var p = new OptionSet()
				{
					{ "h|?|help", v => Syntax() },
					{ "debug", v => debug = true },
					{ "timebomb=", v => timebomb = Convert.ToBoolean(v) },
				};

				List<string> extra = p.Parse(args);

				if (extra.Count != 1)
					Syntax();

				channelName = extra[0];

				Run();
			}
			catch (SyntaxException)
			{
				// Ignore, thrown by syntax()
			}
			catch (OptionException oe)
			{
					Console.WriteLine(oe.Message +"\n"); 
			}
			finally
			{
				// HACK - Required on Mono with NLog 2.0
				LogManager.Configuration = null;

				// Reset console colors
				Console.ForegroundColor = DefaultForeground;
			}
		}

		static void Run()
		{
			// Enable debugging if asked for
			if (debug)
			{
				var nconfig = new LoggingConfiguration();
				var consoleTarget = new ColoredConsoleTarget();
				nconfig.AddTarget("console", consoleTarget);
				consoleTarget.Layout = "${logger} ${message}";

				var rule = new LoggingRule("*", LogLevel.Debug, consoleTarget);
				nconfig.LoggingRules.Add(rule);

				LogManager.Configuration = nconfig;
			}

			logger.Debug("Starting up IPC listener: {0}", channelName);

			IpcChannel ipcChannel = new IpcChannel(channelName);
			ChannelServices.RegisterChannel(ipcChannel, false);
			Peach.Core.Agent.Monitors.WindowsDebug.DebuggerInstance.LastHeartBeat = DateTime.Now;

			try
			{
				Type commonInterfaceType = typeof(Peach.Core.Agent.Monitors.WindowsDebug.DebuggerInstance);

				RemotingConfiguration.RegisterWellKnownServiceType(
					commonInterfaceType, "DebuggerInstance", WellKnownObjectMode.Singleton);

				while (true)
				{
					Thread.Sleep(200);
					if (Peach.Core.Agent.Monitors.WindowsDebug.DebuggerInstance.ExitInstance)
					{
						logger.Debug("ExitInstance is true!");
						return;
					}

					// Timebomb!
					if (timebomb && (DateTime.Now - Peach.Core.Agent.Monitors.WindowsDebug.DebuggerInstance.LastHeartBeat).TotalSeconds > 30)
					{
						logger.Debug("Last heartbeat over 30 seconds, exiting!");
						return;
					}
				}
			}
			catch(Exception ex)
			{
				logger.Debug(ex.ToString());
			}
			finally
			{
				ChannelServices.UnregisterChannel(ipcChannel);
			}
		}
	}
}

//To work around this issue, you can configure your server so that exclusiveAddressUse = false. This allows your server to create the channel even if existing clients already have a handle on the named pipe.

//Hashtable channelProperties = new Hashtable(); 
// channelProperties.Add("portName", "localhost:8888"); 
// channelProperties.Add("exclusiveAddressUse", false); 
//IChannel _serverChannel = new IpcServerChannel(channelProperties, null); 
// ChannelServices.RegisterChannel(_serverChannel, true); 

