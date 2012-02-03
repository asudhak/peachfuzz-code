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

			Type commonInterfaceType = typeof(Peach.Core.Agent.Monitors.WindowsDebug.DebuggerInstance);

			RemotingConfiguration.RegisterWellKnownServiceType(
				commonInterfaceType, "DebuggerInstance", WellKnownObjectMode.Singleton);

			while (true)
			{
				Thread.Sleep(200);
				if (Peach.Core.Agent.Monitors.WindowsDebug.DebuggerInstance.ExitInstance)
					return;
			}
		}
	}
}
