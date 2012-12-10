
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
using System.Reflection;

using Mono.Posix;

namespace PeachLinuxCrashHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program(args);
        }

        protected string corePattern = "|{0} {1} -p=%p -u=%u -g=%g -s=%s -t=%t -h=%h -e=%e";
        protected string monoExecutable = "/usr/bin/mono";
        protected string origionalCorePattern = null;
        protected string linuxCrashHandlerExe = "/PeachLinuxCrashHandler.exe";
        protected string logFolder = "/var/peachcrash";

        public Program(string[] args)
        {
            try
            {
                if (args.Length == 0)
                    syntax();

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
					{ "register", v => register() },
				};

                p.Parse(args);

                if (exe == null)
                {
                    syntax();
                }

                if (!Directory.Exists(logFolder))
                {
                    Directory.CreateDirectory(logFolder);
                }

                // World RWX!
                Mono.Unix.Native.Syscall.chmod(logFolder, Mono.Unix.Native.FilePermissions.S_IRWXO);

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
                psi.FileName = "/usr/bin/gdb";
                psi.Arguments = exe + " -c " + coreFilename;
                psi.UseShellExecute = false;
                psi.RedirectStandardError = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true;

                using (var gdb = new Process())
                {
                    gdb.StartInfo = psi;
                    gdb.ErrorDataReceived += new DataReceivedEventHandler(gdb_ErrorDataReceived);
                    gdb.OutputDataReceived += new DataReceivedEventHandler(gdb_OutputDataReceived);
                    gdb.Start();
                    gdb.EnableRaisingEvents = true;
                    gdb.BeginErrorReadLine();
                    gdb.BeginOutputReadLine();
                    gdb.StandardInput.AutoFlush = true;

                    gdb.WaitForInputIdle();

                    // Dump current frame information
                    gdb.StandardInput.WriteLine("info frame");
                    gdb.WaitForInputIdle();

                    // Backtrace on all threads
                    gdb.StandardInput.WriteLine("thread apply all backtrace");
                    gdb.WaitForInputIdle();

                    // Output registers
                    gdb.StandardInput.WriteLine("info registers");
                    gdb.WaitForInputIdle();

                    /// CERT Exploitable ///////////////////////////////
                    
                    //// Load CERT code
                    //gdb.StandardInput.WriteLine("source /PeachGdb/exploitable/exploitable.py");
                    //gdb.WaitForInputIdle();
                    //gdb.StandardInput.WriteLine("exploitable");
                    //gdb.WaitForInputIdle();

                    // Exit GDB
                    gdb.StandardInput.WriteLine("quit");
                    gdb.WaitForExit();
                }

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

GDB Output
----------

{7}

",
                    pid, exe, uid, gid, sig, host, time, stdout));

                // World RWX
                Mono.Unix.Native.Syscall.chmod(coreFilename, Mono.Unix.Native.FilePermissions.S_IRWXO);
                Mono.Unix.Native.Syscall.chmod(infoFilename, Mono.Unix.Native.FilePermissions.S_IRWXO);

                // Done
            }
            catch (SyntaxException)
            {
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(logFolder, "error"), ex.ToString());
            }
        }

        protected string stdout = "";

        void gdb_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            stdout += e.Data + "\n";
            //Console.WriteLine(e.Data);
        }

        protected string stderr = "";

        void gdb_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            stderr += e.Data + "\n";
            //Console.WriteLine(e.Data);
        }

        public void syntax()
        {
            Console.WriteLine("\n");
            Console.WriteLine("[[ Peach 3 Linux Crash Handler");
            Console.WriteLine("[[ Copyright (c) Michael Eddington\n");

            Console.WriteLine(@"

This program is registered with the Linux kernel and called to collect 
the core file generated when a process crashes.  This is the main
method of crash detection on Linux systems.

Syntax: PeachLinuxCrashHandler.exe --register

  --register   Register as crash handler.  Requires root privs.

");
            throw new SyntaxException();
        }

        public void register()
        {
            Console.WriteLine("\n");
            Console.WriteLine("[[ Peach 3 Linux Crash Handler");
            Console.WriteLine("[[ Copyright (c) Michael Eddington\n");

            Console.WriteLine(" - Registering with kernel\n");

            // Register our crash handler via proc file system

            var corePat = string.Format(corePattern,
                                        monoExecutable,
                                        linuxCrashHandlerExe);

            File.WriteAllText(
                "/proc/sys/kernel/core_pattern",
                corePat,
                Encoding.ASCII);

            var checkWrite = File.ReadAllText("/proc/sys/kernel/core_pattern", Encoding.ASCII);
            if (checkWrite.IndexOf(linuxCrashHandlerExe) == -1)
                Console.WriteLine("Error, LinuxCrashMonitor was unable to update /proc/sys/kernel/core_pattern.");

            throw new SyntaxException();
        }
    }

    public class SyntaxException : Exception
    {
    }
}
