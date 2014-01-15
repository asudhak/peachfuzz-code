
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
				Console.WriteLine(ex.Message + "\n");
			}
		}

		public Program(string[] args)
		{
			Console.WriteLine();
			Console.WriteLine("] Peach 3 -- Minset");
			Console.WriteLine("] Copyright (c) Deja vu Security\n");

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


            var ms = new Minset();
            ms.TraceCompleted += new TraceCompletedEventHandler(ms_TraceCompleted);
            ms.TraceStarting += new TraceStartingEventHandler(ms_TraceStarting);
            var both = false;

            if (extra.Count > 0 && minset != null && traces != null && samples != null)
            {
                both = true;
                Console.WriteLine("[*] Running both trace and coverage analysis");
            }

            if (both || (executable != null && minset == null))
            {
                var sampleFiles = GetFiles(samples);

                Console.WriteLine("[*] Running trace analysis on " + sampleFiles.Length + " samples...");
                ms.RunTraces(executable, arguments, traces, sampleFiles, kill);

                Console.WriteLine("\n[*] Finished");
            }

			if (both || (extra.Count == 0 && minset != null && traces != null && samples != null))
			{
				Console.WriteLine("[*] Running coverage analysis...");
				var sampleFiles = GetFiles(samples);
				var minsetFiles = ms.RunCoverage(sampleFiles, GetFiles(traces));

				Console.WriteLine("[-]   " + minsetFiles.Length + " files were selected from a total of " + sampleFiles.Count() + ".");
				Console.WriteLine("[*] Copying over selected files...");

				if (!Directory.Exists(minset))
					Directory.CreateDirectory(minset);

				foreach (string fileName in minsetFiles)
				{
					Console.WriteLine("[-]   " + Path.Combine(samples, Path.GetFileName(fileName)) + " -> " + Path.Combine(minset, Path.GetFileName(fileName)));
					File.Copy(Path.Combine(samples, Path.GetFileName(fileName)), Path.Combine(minset, Path.GetFileName(fileName)), true);
				}

				Console.WriteLine("\n[*] Finished");
			}
		}

		void ms_TraceStarting(Minset sender, string fileName, int count, int totalCount)
		{
			Console.Write("[{0}:{1}]   Converage trace of {2}...", 
				count, totalCount, fileName);
		}

		void ms_TraceCompleted(Minset sender, string fileName, int count, int totalCount)
		{
			Console.WriteLine("done.");
		}

		string[] GetFiles(string path)
		{
			string[] filenames;

			if (path.IndexOf("*") > -1)
			{
				try
				{
					filenames = Directory.GetFiles(Path.GetDirectoryName(path), Path.GetFileName(path));
				}
				catch
				{
					filenames = Directory.GetFiles(".", Path.GetFileName(path));
				}
			}
			else
			{
				filenames = Directory.GetFiles(path);
			}

			Array.Sort(filenames);

			return filenames;
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
  PeachMinset [-k] -s samples -t traces command.exe args %s

Note:
  %s will be replaced by sample filename.
  -k will terminate command.exe when CPU becomes idle.


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
  PeachMinset [-k] -s samples -t traces -m minset command.exe args %s

Note:
  %s will be replaced by sample filename.
  -k will terminate command.exe when CPU becomes idle.


Distributing Minset
-------------------

Minset can be distributed by splitting up the sample files and 
distributing the collecting of traces to multiple machines.  The
final compute minimum set cannot be distributed.

");
		}
	}
}
