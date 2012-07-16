
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

namespace Peach.Core.WindowsDebugInstance
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("> Peach.Core.WindowsDebugInstance");
				Console.WriteLine("> Copyright (c) Deja vu Security\n");

				Console.WriteLine("Syntax:");
				Console.WriteLine(" Peach.Core.WindowsDebugInstance.exe IPC_CHANNEL_NAME\n\n");

				return;
			}

			IpcChannel ipcChannel = new IpcChannel(args[0]);
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
						return;

					// Timebomb!
					if ((DateTime.Now - Peach.Core.Agent.Monitors.WindowsDebug.DebuggerInstance.LastHeartBeat).TotalSeconds > 30)
						return;
				}
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

