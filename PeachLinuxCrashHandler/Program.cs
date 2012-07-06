using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeachLinuxCrashHandler
{
	class Program
	{
		static void Main(string[] args)
		{
			new Program(args);
		}

		public Program(string[] args)
		{
			try
			{
				Console.Write("\n");
				Console.Write("[[ Peach Linux Crash Handler");
				Console.Write("[[ Copyright (c) Michael Eddington\n");

				if (args.Length == 0)
					syntax();

				bool register = false;
				string uid = null;
				string gid = null;
				string sig = null;
				string time = null;
				string host = null;
				string exe = null;

				var p = new OptionSet()
				{
					{ "h|?|help", v => syntax() },
					{ "register", v => register = true },

					{ "u|id=", v => uid = v },
					{ "g|id=", v => gid = v},
					{ "s|ig=", v => sig = v },
					{ "t|ime=", v => time = v},
					{ "h|ost=", v => host = v},
					{ "e|xe=", v => exe = v},
				};

				List<string> extra = p.Parse(args);

				if (register)
				{
					throw new NotImplementedException();
				}

				// Handle incoming core file!
			}
			catch (SyntaxException)
			{
			}
		}

		public void syntax()
		{
			Console.WriteLine(@"

This program is registered with the Linux kernel and called to collect 
the core file generated when a process crashes.  This is the main
method of crash detection on Linux systems.

Usage:

  1. Verify program is world readable/executable
  2. Register handler with linux kernel

     as root

" + "echo \"|/usr/bin/mono /peach3/PeachLinuxCrashHandler.exe u=%u g=%g s=%s t=%t h=%h e=%e\" > /proc/sys/kernel/core_pattern" + @"

     or as root

mono PeachCoreLinux.exe --register

");
			throw new SyntaxException();
		}
	}

	public class SyntaxException : Exception
	{
	}
}
