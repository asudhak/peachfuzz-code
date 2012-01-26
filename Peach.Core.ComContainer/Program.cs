using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

using NLog;

namespace Peach.Core.ComContainer
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Peach 3 COM Container");
			Console.WriteLine("---------------------");
			Console.WriteLine("");

			int port = 9001;

			if (args.Count() > 0)
				port = int.Parse(args[0]);

			Console.WriteLine(" --> Listening on port " + port + " for incoming connection...");
			//select channel to communicate
			TcpChannel chan = new TcpChannel(port);
			ChannelServices.RegisterChannel(chan, false);    //register channel

			//register remote object
			RemotingConfiguration.RegisterWellKnownServiceType(
				typeof(ComContainer),
				"PeachComContainer", WellKnownObjectMode.Singleton);

			//inform console
			Console.WriteLine(" -- Press ENTER to quit agent -- ");
			Console.ReadLine();
		}
	}
}
