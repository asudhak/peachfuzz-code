
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

namespace Peach.Core.Analysis.Minset
{
	public delegate void TraceStartingEventHandler(Minset sender, string fileName, int count, int totalCount);
	public delegate void TraceCompletedEventHandler(Minset sender, string fileName, int count, int totalCount);
	
	public class Minset
	{
		public event TraceStartingEventHandler TraceStarting;
		public event TraceCompletedEventHandler TraceCompleted;

		protected void OnTraceStarting(string fileName, int count, int totalCount)
		{
			if (TraceStarting != null)
				TraceStarting(this, fileName, count, totalCount);
		}

		public void OnTraceCompleted(string fileName, int count, int totalCount)
		{
			if (TraceCompleted != null)
				TraceCompleted(this, fileName, count, totalCount);
		}

		static bool Is64BitProcess
		{
			get { return IntPtr.Size == 8; }
		}

		/// <summary>
		/// Load the blocks
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		static List<int> LoadBlocks(string fileName)
		{
			using (StreamReader sin = File.OpenText(fileName))
			{
				string line;
				List<int> blocks = new List<int>();

				while ((line = sin.ReadLine()) != null)
					blocks.Add(int.Parse(line));

				return blocks;
			}
		}

		/// <summary>
		/// Perform coverage analysis of trace files.
		/// </summary>
		/// <remarks>
		/// Note: The sample and trace collections must have matching indexes.
		/// </remarks>
		/// <param name="sampleFiles">Collection of sample files</param>
		/// <param name="traceFiles">Collection of trace files for sample files</param>
		/// <returns>Returns the minimum set of smaple files.</returns>
		public string[] RunCoverage(string [] sampleFiles, string [] traceFiles)
		{
			// All blocks we are covering
			var coveredBlocks = new List<int>();
			var minset = new List<string>();

			// Number of blocks in trace file
			var blocksInTraceFile = new Dictionary<string, List<int>>();

			// Load blocks
			foreach (string traceFile in traceFiles)
				blocksInTraceFile[traceFile] = LoadBlocks(traceFile);

			// List of items sorted by count of blocks
			var sortedTraceBlocksByCount = new List<KeyValuePair<string, List<int>>>(blocksInTraceFile.ToArray < KeyValuePair<string, List<int>>>());
			sortedTraceBlocksByCount.Sort((firstPair, nextPair) =>
					{
						return firstPair.Value.Count.CompareTo(nextPair.Value.Count);
					}
				);

			foreach(KeyValuePair<string, List<int>> keyValue in sortedTraceBlocksByCount)
			{
				if(coveredBlocks.Count == 0)
				{
					coveredBlocks.AddRange(keyValue.Value);
					minset.Add(keyValue.Key);
					continue;
				}

				var delta = Delta(keyValue.Value, coveredBlocks);

				if(delta.Count == 0)
					continue;

				minset.Add(keyValue.Key);
				coveredBlocks.AddRange(delta);
			}

			return minset.ToArray();
		}

		/// <summary>
		/// Are any of the childs items missing from master.
		/// </summary>
		/// <param name="child"></param>
		/// <param name="master"></param>
		/// <returns></returns>
		protected List<int> Delta(List<int> child, List<int> master)
		{
			List<int> delta = new List<int>();

			foreach (int bblock in child)
			{
				if (!master.Contains(bblock))
					delta.Add(bblock);
			}

			return delta;
		}

		/// <summary>
		/// Collect traces for a collection of sample files.
		/// </summary>
		/// <remarks>
		/// This method will use the TraceStarting and TraceCompleted events
		/// to report progress.
		/// </remarks>
		/// <param name="command">Command to execute.  Must contain a "%s" placeholder for the sampe filename.</param>
		/// <param name="sampleFiles">Collection of sample files</param>
		/// <param name="needsKilling">Does this command requiring forcefull killing to exit?</param>
		/// <returns>Returns a collection of trace files</returns>
		public string[] RunTraces(string command, string[] sampleFiles, bool needsKilling = false)
		{
			try
			{
				int count = 0;
				List<string> traces = new List<string>();

				if (File.Exists("bblocks.out"))
					File.Delete("bblocks.out");
				if (File.Exists("bblocks.existing"))
					File.Delete("bblocks.existing");

				foreach (string fileName in sampleFiles)
				{
					count++;
					OnTraceStarting(fileName, count, sampleFiles.Length);

					if (RunSingleTrace(fileName + ".trace", command, fileName, needsKilling))
						traces.Add(fileName + ".trace");

					OnTraceCompleted(fileName, count, sampleFiles.Length);
				}

				return traces.ToArray();
			}
			finally
			{
				if (File.Exists("bblocks.out"))
					File.Delete("bblocks.out");
				if (File.Exists("bblocks.existing"))
					File.Delete("bblocks.existing");
			}
		}

		/// <summary>
		/// Create a single trace file based on code coverage stats for fileName.
		/// </summary>
		/// <remarks>
		/// To fully clean up make sure the following two files do not exist:
		/// 
		///   * bblocks.out -- Temp file for coverage numbers
		///   * bblocks.existing -- If running multiple coverage numbers this file is the baseline
		///   
		/// </remarks>
		/// <param name="traceFile">Output trace to this filename</param>
		/// <param name="command">Command to execute.  Must contain "%s" which is substituded for fileName.</param>
		/// <param name="fileName">Sample file to get coverage stats on</param>
		/// <param name="needsKilling">Does this process require killing?</param>
		/// <returns>True on success, false if a failure occured.</returns>
		public bool RunSingleTrace(string traceFile, string command, string fileName, bool needsKilling = false)
		{
			string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string pin32 = Path.Combine(assemblyPath, @"pin-msvc10-ia32_intel64-windows\ia32\bin\pin.exe");
			string pin64 = Path.Combine(assemblyPath, @"pin-msvc10-ia32_intel64-windows\intel64\bin\pin.exe");
			string bblocks = Path.Combine(assemblyPath, "bblocks.dll");

			if (!File.Exists(pin32) || !File.Exists(pin64))
				throw new ApplicationException(string.Format("Error, unable to locate 32 or 64bit pin! [{0}] or [{1}]",
					pin32, pin64));

			if (!File.Exists(bblocks))
				throw new ApplicationException(string.Format("Error, unable to locate bblocks at [{0}]",
					bblocks));

			string args = string.Format("-t {0} -- {1}",
				bblocks,
				command.Replace("%s", fileName));

			if (File.Exists("bblocks.out") && !File.Exists("bblocks.existing"))
				File.Move("bblocks.out", "bblocks.existing");
			else if (File.Exists("bblocks.out"))
				File.Delete("bblocks.out");

			System.Diagnostics.Process proc = new System.Diagnostics.Process();
			proc.StartInfo.Arguments = args;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardError = true;
			proc.StartInfo.RedirectStandardOutput = true;
			
			if(Is64BitProcess)
				proc.StartInfo.FileName = pin64;
			else
				proc.StartInfo.FileName = pin32;

			_procOutput.Clear();

			proc.Start();
			proc.WaitForExit();

			// We should always end up with a bblocks.out file
			if (!File.Exists("bblocks.out"))
				return false;

			// If we don't already have an existing file, create
			if (File.Exists("bblocks.out") && !File.Exists("bblocks.existing"))
				File.Copy("bblocks.out", "bblocks.existing");

			// Finally rename bblocks.out to the correct trace file
			if (File.Exists(traceFile))
				File.Delete(traceFile);

			File.Move("bblocks.out", traceFile);

			return true;
		}

		StringBuilder _procOutput = new StringBuilder();

		void proc_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
		{
			_procOutput.Append(e.Data);
			_procOutput.Append("\n");
		}

		void proc_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
		{
			_procOutput.Append(e.Data);
			_procOutput.Append("\n");
		}
	}
}
