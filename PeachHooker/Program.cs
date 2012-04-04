
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;

using EasyHook;

namespace PeachHooker
{
	class Program
	{
		static string ChannelName = null;
		public static Program Context = null;

		public bool recv = false;
		public bool send = false;

		static void Main(string[] args)
		{
			Context = new Program();
			Context.Run(args);
		}

		public Program()
		{
		}


		public void Run(string[] args)
		{
			try
			{
				Console.WriteLine();
				Console.WriteLine("| Peach 3 - Hooker");
				Console.WriteLine("| Copyright (c) Deja vu Security\n");

				bool fileMode = false;
				bool networkMode = false;

				string command = null;
				string executable = null;
				int TargetPid = -1;
				string onlyfile = null;

				var p = new OptionSet()
				{
					{ "h|?|help", v => syntax() },
					{ "n|network", v => networkMode = true },
					{ "f|file", v => fileMode = true },
					{ "c|command=", v => command = v},
					{ "p|pid=", v => TargetPid = int.Parse(v) },
					{ "onlyfile=", v => onlyfile = v},
					{ "r|recv", v => recv = true},
					{ "s|send", v => send = true},
					{ "e|executable=", v => executable = v },
				};

				List<string> extra = p.Parse(args);

				if (extra.Count != 0 || (!networkMode && !fileMode))
					syntax();

				if (TargetPid == -1 && command == null)
					syntax();

				if (command != null && executable == null)
					syntax();

				if (recv == false && send == false && networkMode)
				{
					recv = true;
					send = true;
				}

				try
				{
					//try
					//{
					//    Config.Register(
					//        "Hook System Calls for InProc Fuzzing!",
					//        "PeachHooker.exe",
					//        "PeachHooker.Network.dll");
					//}
					//catch (ApplicationException ex)
					//{
					//    Console.WriteLine("Exception while calling Config.Register!");
					//    Console.WriteLine(ex.ToString());
					//    System.Diagnostics.Process.GetCurrentProcess().Kill();
					//}

					RemoteHooking.IpcCreateServer<NetworkInterface>(ref ChannelName, WellKnownObjectMode.SingleCall);

					if (command != null)
					{
						RemoteHooking.CreateAndInject(
							executable,
							command,
							0,
							"PeachHooker.Network.dll",	// 32bit
							"PeachHooker.Network.dll",	// 64bit
							out TargetPid,
							ChannelName);
					}
					else
					{
						RemoteHooking.Inject(
							TargetPid,
							"PeachHooker.Network.dll",
							"PeachHooker.Network.dll",
							ChannelName);
					}

					Console.WriteLine("Press any key to close");
					Console.ReadLine();
				}
				catch (Exception ExtInfo)
				{
					Console.WriteLine("There was an error while connecting to target:\r\n{0}", ExtInfo.ToString());
				}
			}
			catch (SyntaxException)
			{
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				if(ex.InnerException != null)
					Console.WriteLine(ex.InnerException.ToString());
			}
		}

		void syntax()
		{
			Console.WriteLine(@"

  Peach 3 Hooker will hook methods in a target program to perform
  direct dumb fuzzing of file or network access.  This works by
  taking over control of the actual low-level access methods
  and modifying the data prior to passing it along.

  There are two main modes of operation: network and file.

  In network mode the recv/send methods are hooked and the data
  is fuzzed prior to being received or sent.  Peach 3 uses an
  algorithm to determine if the current call should be fuzzed.

  This program is almost never used by itself.

Syntax: PeachHooker --network -e EXECUTABLE -c COMMAND_LINE
Syntax: PeachHooker --network --pid PID
Syntax: PeachHooker --network --recv --pid PID
Syntax: PeachHooker --network --send --pid PID

Syntax: PeachHooker --file -c COMMAND_LINE
Syntax: PeachHooker --file --pid PID
Syntax: PeachHooker --file --onlyfile FILE -c COMMAND_LINE

Operation Modes

  -n --network	Hook network send/receive functions
  -f --file		Hook file read functions

Common Arguments

  -c --command  Start a process using specified command line
  -p --pid		Attach to existing process by PID

File Hooking Arguments

  --onlyfile	Only activate when reading a specific file

Network Hooking Arguments

  -r --recv     Only hook receive functions
  -s --send     Only hook send functions

Examples

  PeachHooker --network -e c:\netcat.exe -c " + "\"netcat.exe 127.0.0.1 9000\"" + @"

");

			throw new SyntaxException();
		}
	}

	public class SyntaxException : Exception
	{
	}
}
