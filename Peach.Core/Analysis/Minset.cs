
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

namespace Peach.Core.Analysis
{
	public delegate void TraceStartingEventHandler(Minset sender, string fileName, int count, int totalCount);
	public delegate void TraceCompletedEventHandler(Minset sender, string fileName, int count, int totalCount);

	/// <summary>
	/// Perform analysis on sample sets to identify the smallest sample set
	/// that provides the largest code coverage.
	/// </summary>
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

			// Strip the .trace and path off
			for (int cnt = 0; cnt < minset.Count; cnt++)
			{
				minset[cnt] = Path.GetFileNameWithoutExtension(minset[cnt]);
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
		/// <param name="executable">Executable to run.</param>
		/// <param name="arguments">Executable arguments.  Must contain a "%s" placeholder for the sampe filename.</param>
		/// <param name="tracesFolder">Where to write trace files</param>
		/// <param name="sampleFiles">Collection of sample files</param>
		/// <param name="needsKilling">Does this command requiring forcefull killing to exit?</param>
		/// <returns>Returns a collection of trace files</returns>
		public string[] RunTraces(string executable, string arguments, string tracesFolder, string[] sampleFiles, bool needsKilling = false)
		{
			if (!Directory.Exists(tracesFolder))
				Directory.CreateDirectory(tracesFolder);

			using (Coverage coverage = Coverage.CreateInstance())
			{
				int count = 0;
				string traceFilename = null;
				List<string> traces = new List<string>();
				List<ulong> basicBlocks = coverage.BasicBlocksForExecutable(executable, needsKilling);

				foreach (string fileName in sampleFiles)
				{
					count++;
					OnTraceStarting(fileName, count, sampleFiles.Length);

					// Output trace into the specified tracesFolder
					traceFilename = Path.Combine(tracesFolder, Path.GetFileName(fileName) + ".trace");

					if (RunSingleTrace(coverage,
						traceFilename,
						executable,
						arguments.Replace("%s", fileName),
						basicBlocks,
						needsKilling))
					{
						traces.Add(traceFilename);
					}

					OnTraceCompleted(fileName, count, sampleFiles.Length);
				}

				return traces.ToArray();
			}
		}

		/// <summary>
		/// Create a single trace file based on code coverage stats for fileName.
		/// </summary>
		/// <param name="cov">Coverage stats</param>
		/// <param name="traceFile">Output trace to this filename</param>
		/// <param name="executable">Command to execute.</param>
		/// <param name="arguments">Command arguments.</param>
		/// <param name="basicBlocks">List of basic blocks to trap on</param>
		/// <param name="needsKilling">Does this process require killing?</param>
		/// <returns>True on success, false if a failure occured.</returns>
		public bool RunSingleTrace(Coverage cov, string traceFile, string executable, string arguments, List<ulong> basicBlocks, bool needsKilling = false)
		{
			List<ulong> coverage = cov.CodeCoverageForExecutable(executable, arguments, needsKilling, basicBlocks);

			// Delete existing trace file
			if (File.Exists(traceFile))
				File.Delete(traceFile);

			// Create trace file
			using(FileStream fout = File.Create(traceFile))
			{
				using(StreamWriter sout = new StreamWriter(fout))
				{
					foreach(ulong addr in coverage)
					{
						sout.WriteLine(addr.ToString());
					}
				}
			}

			return true;
		}
	}
}
