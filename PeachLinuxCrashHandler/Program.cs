
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
using System.Diagnostics;
using System.IO;
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

		protected string logFolder = "/var/peachcrash";

		public Program(string[] args)
		{
			try
			{
				if (args.Length == 0)
					syntax();

				bool register = false;
				string uid = null;
				string gid = null;
				string sig = null;
				string time = null;
				string host = null;
				string exe = null;
				string pid = null;

				var p = new OptionSet()
				{
					{ "u|uid=", v => uid = v },
					{ "g|gid=", v => gid = v},
					{ "s|sig=", v => sig = v },
					{ "t|time=", v => time = v},
					{ "h|host=", v => host = v},
					{ "e|exe=", v => exe = v},
					{ "p|pid=", v => pid = v},
					{ "l|logfolder=", v => logFolder = v},
				};

				List<string> extra = p.Parse(args);

				if (register)
				{
					throw new NotImplementedException();
				}

				if (exe == null)
				{
					syntax();
				}

				if (!Directory.Exists(logFolder))
				{
					Directory.CreateDirectory(logFolder);
				}

				// Handle incoming core file!


				var coreFilename = Path.Combine(logFolder, "peach_" + Path.GetFileName(exe) + "_" + pid + ".core");
				var infoFilename = Path.Combine(logFolder, "peach_" + Path.GetFileName(exe) + "_" + pid + ".info");

				using (var sout = File.Create(coreFilename))
				using (var stdin = Console.OpenStandardInput())
				{
					var buff = new byte[1024];
					int count;

					while (true)
					{
						count = stdin.Read(buff, 0, buff.Length);
						if (count == 0)
							break;

						sout.Write(buff, 0, count);
					}
				}

				// Open core file and pull backtrace data from it

				// stacktrace from all threads:
				// gdb thread apply all backtrace
				// gdb info registers

				var psi = new ProcessStartInfo();
				psi.FileName = "gdb";
				psi.Arguments = exe + " -c " + coreFilename;
				psi.UseShellExecute = false;
				psi.RedirectStandardError = true;
				psi.RedirectStandardInput = true;
				psi.RedirectStandardOutput = true;
				psi.CreateNoWindow = true;

				var gdb = new Process();
				gdb.StartInfo = psi;
				gdb.ErrorDataReceived += new DataReceivedEventHandler(gdb_ErrorDataReceived);
				gdb.OutputDataReceived += new DataReceivedEventHandler(gdb_OutputDataReceived);
				gdb.Start();

				gdb.WaitForInputIdle();
				stdout = "";
				stderr = "";

				gdb.StandardInput.WriteLine("thread apply all backtrace");
				gdb.WaitForInputIdle();
				var backtrace = stdout;
				stdout = "";
				stderr = "";

				gdb.StandardInput.WriteLine("info registers");
				gdb.WaitForInputIdle();
				var registers = stdout;
				stdout = "";
				stderr = "";

				gdb.StandardInput.WriteLine("quit");
				gdb.WaitForExit();

				// Write out information file

				File.WriteAllText(infoFilename, string.Format(@"

Linux Crash Handler -- Crash information
========================================

PID: {0}
EXE: {1}
UID: {2}
GID: {3}
SIG: {4}
Host: {5}
Time/date: {6}

Registers
---------

{7}

Backtrace
---------

{8}

",
					pid, exe, uid, gid, sig, host, time, registers, backtrace));

				// Done
			}
			catch (SyntaxException)
			{
			}
		}

		protected string stdout = "";

		void gdb_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			throw new NotImplementedException();
		}

		protected string stderr = "";

		void gdb_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			throw new NotImplementedException();
		}

		public void syntax()
		{
			Console.Write("\n");
			Console.Write("[[ Peach Linux Crash Handler");
			Console.Write("[[ Copyright (c) Michael Eddington\n");

			Console.WriteLine(@"

This program is registered with the Linux kernel and called to collect 
the core file generated when a process crashes.  This is the main
method of crash detection on Linux systems.

This program is not intended to be run outside of Peach.

");
			throw new SyntaxException();
		}
	}

	public class SyntaxException : Exception
	{
	}
}
