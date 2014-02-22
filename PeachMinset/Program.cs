
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
using System.Reflection;

using Peach.Core;
using Peach.Core.Analysis;
using Peach.Core.Runtime;

namespace PeachMinset
{
	class Program
	{
		class SyntaxException : Exception
		{
			public SyntaxException()
				: base("")
			{
			}

			public SyntaxException(string message)
				: base(message)
			{
			}
		}

		static void Main(string[] args)
		{
			try
			{
				new Program(args);
			}
			catch (OptionException ex)
			{
				Console.WriteLine(ex.Message + "\n");
			}
			catch (SyntaxException ex)
			{
				if (!string.IsNullOrEmpty(ex.Message))
					Console.WriteLine(ex.Message + "\n");
				else
					Syntax();
			}
			catch (PeachException ex)
			{
				Console.WriteLine("{0}\n", ex.Message);

				if (ex.InnerException != null && ex.InnerException.Message != ex.Message)
					Console.WriteLine("{0}\n", ex.InnerException.Message);
			}
			finally
			{
				// HACK - Required on Mono with NLog 2.0
				Peach.Core.Runtime.Program.ConfigureLogging(-1);
			}
		}

		public Program(string[] args)
		{
			Console.WriteLine();
			Console.WriteLine("] Peach 3 -- Minset");
			Console.WriteLine("] Copyright (c) Deja vu Security\n");

			int verbose = 0;
			string samples = null;
			string traces = null;
			bool kill = false;
			string executable = null;
			string arguments = null;
			string minset = null;

			var p = new OptionSet()
				{
					{ "h|?|help", v => Syntax() },
					{ "k", v => kill = true },
					{ "v", v => verbose = 1 },
					{ "s|samples=", v => samples = v },
					{ "t|traces=", v => traces = v},
					{ "m|minset=", v => minset = v }
				};

			var extra = p.Parse(args);

			executable = extra.FirstOrDefault();
			arguments = string.Join(" ", extra.Skip(1));

			if (args.Length == 0)
				throw new SyntaxException();

			if (samples == null)
				throw new SyntaxException("Error, 'samples' argument is required.");

			if (traces == null)
				throw new SyntaxException("Error, 'traces' argument is required.");

			if (minset == null && executable == null)
				throw new SyntaxException("Error, 'minset' or command argument is required.");

			if (executable != null && arguments.IndexOf("%s") == -1)
				throw new SyntaxException("Error, command argument missing '%s'.");

			Peach.Core.Runtime.Program.ConfigureLogging(verbose);

			var sampleFiles = GetFiles(samples, "sample");

			// If we are generating traces, ensure we can write to the traces folder
			if (executable != null)
				VerifyDirectory(traces);

			// If we are generating minset, ensure we can write to the minset folder
			if (minset != null)
				VerifyDirectory(minset);

			var ms = new Minset();

			if (verbose == 0)
			{
				ms.TraceCompleted += new TraceEventHandler(ms_TraceCompleted);
				ms.TraceStarting += new TraceEventHandler(ms_TraceStarting);
				ms.TraceFailed += new TraceEventHandler(ms_TraceFailed);
			}

			if (minset != null && executable != null)
				Console.WriteLine("[*] Running both trace and coverage analysis\n");

			if (executable != null)
			{
				Console.WriteLine("[*] Running trace analysis on " + sampleFiles.Length + " samples...");

				ms.RunTraces(executable, arguments, traces, sampleFiles, kill);

				Console.WriteLine("\n[*] Finished\n");
			}

			if (minset != null)
			{
				var traceFiles = GetFiles(traces, "trace");

				Console.WriteLine("[*] Running coverage analysis...");

				var minsetFiles = ms.RunCoverage(sampleFiles, traceFiles);

				Console.WriteLine("[-]   {0} files were selected from a total of {1}.", minsetFiles.Length, sampleFiles.Length);
				Console.WriteLine("[*] Copying over selected files...");

				foreach (string fileName in minsetFiles)
				{
					var file = Path.GetFileName(fileName);
					var src = Path.Combine(samples, file);
					var dst = Path.Combine(minset, file);

					Console.WriteLine("[-]   {0} -> {1}", src, dst);

					File.Copy(src, dst, true);
				}

				Console.WriteLine("\n[*] Finished\n");
			}
		}

		void ms_TraceStarting(Minset sender, string fileName, int count, int totalCount)
		{
			Console.Write("[{0}:{1}]   Converage trace of {2}...", 
				count, totalCount, fileName);
		}

		void ms_TraceCompleted(Minset sender, string fileName, int count, int totalCount)
		{
			Console.WriteLine(" Completed");
		}

		void ms_TraceFailed(Minset sender, string fileName, int count, int totalCount)
		{
			Console.WriteLine(" Failed");
		}

		static string[] GetFiles(string path, string what)
		{
			string[] fileNames;

			try
			{
				if (path.IndexOf("*") > -1)
				{
					fileNames = Directory.GetFiles(Path.GetDirectoryName(path), Path.GetFileName(path));
				}
				else
				{
					fileNames = Directory.GetFiles(path);
				}
			}
			catch (IOException ex)
			{
				var err = "Error, unable to get the list of {0} files.".Fmt(what);
				throw new PeachException(err, ex);
			}
			catch (UnauthorizedAccessException ex)
			{
				var err = "Error, unable to get the list of {0} files.".Fmt(what);
				throw new PeachException(err, ex);
			}

			Array.Sort(fileNames);

			return fileNames;
		}

		static void VerifyDirectory(string path)
		{
			try
			{
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
			}
			catch (IOException ex)
			{
				throw new PeachException(ex.Message, ex);
			}
			catch (UnauthorizedAccessException ex)
			{
				throw new PeachException(ex.Message, ex);
			}
		}

		static void Syntax()
		{
			Console.WriteLine(@"

Peach Minset is used to locate the minimum set of sample data with 
the best code coverage metrics to use while fuzzing.  This process 
can be distributed out across multiple machines to decrease the run 
time.

There are two steps to the process:

  1. Collect traces       [long process]
  2. Compute minimum set  [short process]

The first step, collecting traces, can be distributed and the results
collected for analysis by step #2.

Collect Traces
--------------

Perform code coverage using all files in the 'samples' folder.  Collect
the .trace files in the 'traces' folder for later analysis.

Syntax:
  PeachMinset [-k -v] -s samples -t traces command.exe args %s

Note:
  %s will be replaced by sample filename.
  -k will terminate command.exe when CPU becomes idle.
  -v will enable debug log messages.


Compute Minimum Set
-------------------

Analyzes all .trace files in the 'traces' folder to determin the minimum
set of samples to use during fuzzing. The minimum set of samples will
be copied from the 'samples' folder to the 'minset' folder.

Syntax:
  PeachMinset -s samples -t traces -m minset


All-In-One
----------

Both tracing and computing can be performed in a single step.

Syntax:
  PeachMinset [-k -v] -s samples -t traces -m minset command.exe args %s

Note:
  %s will be replaced by sample filename.
  -k will terminate command.exe when CPU becomes idle.
  -v will enable debug log messages.


Distributing Minset
-------------------

Minset can be distributed by splitting up the sample files and 
distributing the collecting of traces to multiple machines.  The
final compute minimum set cannot be distributed.

");
		}
	}
}
