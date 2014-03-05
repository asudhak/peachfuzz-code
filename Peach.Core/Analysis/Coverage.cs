
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
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using NLog;
using Peach.Core;
using System.Collections;
using System.Threading;

namespace Peach.Core.Analysis
{
	/// <summary>
	/// Abstract base class for performing code coverage via basic blocks
	/// for native binaries.  Each architecture implements this class.
	/// </summary>
	/// <remarks>
	/// So far only Windows has an implementation.
	/// </remarks>
	public class Coverage
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		static string Quote(string str)
		{
			if (str.Contains(' '))
				return "\"" + str + "\"";

			return str;
		}

		static void VerifyExists(string file, string type)
		{
			if (!File.Exists(file))
				throw new FileNotFoundException("Error, can not locate the {0} '{1}'.".Fmt(type, file));
		}

		protected ProcessStartInfo StartInfo { get; private set; }
		protected bool NeedsKilling { get; private set; }

		public Coverage(string executable, string arguments, bool needsKilling)
		{
			VerifyExists(executable, "target executable");

			// Set 1st since it is used by Setup functions
			NeedsKilling = needsKilling;

			if (!arguments.Contains("%s"))
				throw new ArgumentException("Error, arguments must contain a '%s'.");

			var pwd = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			switch (Platform.GetOS())
			{
				case Platform.OS.Windows:
					StartInfo = SetupWindows(pwd, executable, arguments);
					break;
				case Platform.OS.Linux:
					StartInfo = SetupLinux(pwd, executable, arguments);
					break;
				case Platform.OS.OSX:
					StartInfo = SetupOSX(pwd, executable, arguments);
					break;
				default:
					throw new NotSupportedException("Error, coverage is not supported on this platform.");
			}

			StartInfo.RedirectStandardError = true;
			StartInfo.RedirectStandardOutput = true;
			StartInfo.UseShellExecute = false;
			StartInfo.CreateNoWindow = true;

			logger.Debug("Using: {0} {1}", StartInfo.FileName, StartInfo.Arguments);
		}

		#region Platform Setup Functions

		private ProcessStartInfo SetupWindows(string pwd,string executable,string arguments)
		{
			var arch = FileInfo.Instance.GetArch(executable);

			logger.Debug("Target Architecture: {0}", arch);

			string pinPath;
			string pinTool;

			if (arch == Platform.Architecture.x86)
			{
				pinPath = "ia32";
				pinTool = "bblocks32.dll";
			}
			else
			{
				pinPath = "intel64";
				pinTool = "bblocks64.dll";
			}

			pinPath = Path.Combine(pwd, "pin", pinPath, "bin", "pin.exe");
			VerifyExists(pinPath, "pin binary");

			pinTool = Path.Combine(pwd, pinTool);
			VerifyExists(pinTool, "pin tool");

			var psi = new ProcessStartInfo();
			psi.FileName = pinPath;
			psi.Arguments = "-t {0} -cpukill {1} -debug {2} -- {3} {4}".Fmt(
				Quote(pinTool),
				NeedsKilling ? "1" : "0",
				logger.IsDebugEnabled ? "1" : "0",
				Quote(executable),
				arguments);

			return psi;
		}

		private ProcessStartInfo SetupLinux(string pwd, string executable, string arguments)
		{
			var arch = FileInfo.Instance.GetArch(executable);

			logger.Debug("Target Architecture: {0}", arch);

			string pinPath;
			string pinTool;

			if (arch == Platform.Architecture.x86)
			{
				pinPath = "ia32";
				pinTool = "bblocks32.so";
			}
			else
			{
				pinPath = "intel64";
				pinTool = "bblocks64.so";
			}

			pinPath = Path.Combine(pwd, "pin", pinPath, "bin", "pinbin");
			VerifyExists(pinPath, "pin binary");

			pinTool = Path.Combine(pwd, pinTool);
			VerifyExists(pinTool, "pin tool");

			var psi = new ProcessStartInfo();
			psi.FileName = pinPath;
			psi.Arguments = "-t {0} -cpukill {1} -debug {2} -- {3} {4}".Fmt(
				Quote(pinTool),
				NeedsKilling ? "1" : "0",
				logger.IsDebugEnabled ? "1" : "0",
				Quote(executable),
				arguments);

			foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
				psi.EnvironmentVariables[de.Key.ToString()] = de.Value.ToString();

			var origin = Path.Combine(pwd, "pin");

			var elf_libs = "{0}/ia32/runtime:{0}/intel64/runtime:".Fmt(origin);
			var glibc_libs = "{0}/ia32/runtime/glibc:{0}/intel64/runtime/glibc:".Fmt(origin);
			var cpp_libs = "{0}/ia32/runtime/cpplibs:{0}/intel64/runtime/cpplibs:".Fmt(origin);

			var libs = "";
			if (psi.EnvironmentVariables.ContainsKey("LD_LIBRARY_PATH"))
				libs = psi.EnvironmentVariables["LD_LIBRARY_PATH"].ToString();

			psi.EnvironmentVariables["LD_LIBRARY_PATH"] = elf_libs + cpp_libs + libs;
			psi.EnvironmentVariables["PIN_VM_LD_LIBRARY_PATH"] = elf_libs + cpp_libs + glibc_libs + libs;

			return psi;
		}

		private ProcessStartInfo SetupOSX(string pwd, string executable, string arguments)
		{
			var pin32 = Path.Combine(pwd, "pin", "ia32", "bin", "pinbin");
			VerifyExists(pin32, "pin binary");

			var pin64 = Path.Combine(pwd, "pin", "intel64", "bin", "pinbin");
			VerifyExists(pin32, "pin binary");

			var pinTool = Path.Combine(pwd, "bblocks.dylib");
			VerifyExists(pinTool, "pin tool");

			var psi = new ProcessStartInfo();
			psi.FileName = pin32;
			psi.Arguments = "-p64 {0} -t {1} -cpukill {2} -debug {3} -- {4} {5}".Fmt(
				Quote(pin64),
				Quote(pinTool),
				NeedsKilling ? "1" : "0",
				logger.IsDebugEnabled ? "1" : "0",
				Quote(executable),
				arguments);

			foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
				psi.EnvironmentVariables[de.Key.ToString()] = de.Value.ToString();

			return psi;
		}

		#endregion

		/// <summary>
		/// Runs code coverage of sample file and saves results in a trace file.
		/// Throws a PeachException on failure.
		/// </summary>
		/// <param name="sampleFile">Name of sample file to use for instrumentation.</param>
		/// <param name="traceFile">Name of result trace file to generate.</param>
		public void Run(string sampleFile, string traceFile)
		{
			var outFile = "bblocks.out";
			var pidFile = "bblocks.pid";

			var psi = new ProcessStartInfo();
			psi.Arguments = StartInfo.Arguments.Replace("%s", Quote(sampleFile));
			psi.FileName = StartInfo.FileName;
			psi.RedirectStandardError = StartInfo.RedirectStandardError;
			psi.RedirectStandardOutput = StartInfo.RedirectStandardOutput;
			psi.UseShellExecute = StartInfo.UseShellExecute;
			psi.CreateNoWindow = StartInfo.CreateNoWindow;

			foreach (DictionaryEntry de in StartInfo.EnvironmentVariables)
				psi.EnvironmentVariables[de.Key.ToString()] = de.Value.ToString();

			logger.Debug("Using sample {0}", sampleFile);
			logger.Debug("{0} {1}", psi.FileName, psi.Arguments);

			try
			{
				if (File.Exists(outFile))
					File.Delete(outFile);
			}
			catch (Exception ex)
			{
				throw new PeachException("Failed to delete old output file '{0}'.".Fmt(outFile), ex);
			}

			try
			{
				if (File.Exists(pidFile))
					File.Delete(pidFile);
			}
			catch (Exception ex)
			{
				throw new PeachException("Failed to delete old pid file '{0}'.".Fmt(pidFile), ex);
			}

			using (var proc = new Process())
			{
				proc.StartInfo = psi;
				proc.OutputDataReceived += proc_OutputDataReceived;
				proc.ErrorDataReceived += proc_ErrorDataReceived;

				try
				{
					proc.Start();
				}
				catch (Exception ex)
				{
					throw new PeachException("Failed to start pin process.", ex);
				}

				proc.BeginErrorReadLine();
				proc.BeginOutputReadLine();

				while (!File.Exists(pidFile) && !proc.HasExited)
					Thread.Sleep(250);

				if (proc.HasExited)
					throw new PeachException("Pin exited without starting the target process.");

				logger.Debug("Waiting for pin process to exit.");

				proc.WaitForExit();

				logger.Debug("Pin process exited.");
			}

			if (!File.Exists(outFile))
				throw new PeachException("Pin exited without creating output file.");

			// Ensure outFile is not zero sized
			var fi = new System.IO.FileInfo(outFile);
			if (fi.Length == 0)
				throw new PeachException("Pin exited without creating any trace file entries. This usually means the target did not run to completion.");

			try
			{
				if (File.Exists(traceFile))
					File.Delete(traceFile);
			}
			catch (Exception ex)
			{
				throw new PeachException("Failed to delete old trace file '{0}'.".Fmt(traceFile), ex);
			}

			try
			{
				// Move bblocks.out to target
				File.Move(outFile, traceFile);
			}
			catch (Exception ex)
			{
				throw new PeachException("Failed to move pin outpout file into destination trace file.", ex);
			}
		}

		void proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data == null)
			{
				((Process)sender).CancelErrorRead();
			}
			else
			{
				logger.Debug(e.Data);
			}
		}

		void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data == null)
			{
				((Process)sender).CancelOutputRead();
			}
			else
			{
				logger.Debug(e.Data);
			}
		}
	}
}
