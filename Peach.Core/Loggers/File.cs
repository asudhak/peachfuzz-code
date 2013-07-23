
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Peach.Core;
using Peach.Core.Agent;
using Peach.Core.Dom;

using NLog;
using Peach.Core.IO;

namespace Peach.Core.Loggers
{
	/// <summary>
	/// Standard file system logger.
	/// </summary>
	[Logger("File")]
	[Logger("Filesystem", true)]
	[Logger("logger.Filesystem")]
	[Parameter("Path", typeof(string), "Log folder")]
	public class FileLogger : Logger
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		Fault reproFault = null;
		TextWriter log = null;

		public FileLogger(Dictionary<string, Variant> args)
		{
			Path = (string)args["Path"];
		}

		/// <summary>
		/// The user configured base path for all the logs
		/// </summary>
		public string Path
		{
			get;
			private set;
		}

		/// <summary>
		/// The specific path used to log faults for a given test.
		/// </summary>
		protected string RootDir
		{
			get;
			private set;
		}

		protected enum Category { Faults, Reproducing, NonReproducable }

		protected void SaveFault(Category category, Fault fault)
		{
			log.WriteLine("! Fault detected at iteration {0} : {1}", fault.iteration, DateTime.Now.ToString());

			// root/category/bucket/iteration
			var subDir = System.IO.Path.Combine(RootDir, category.ToString(), fault.folderName, fault.iteration.ToString());

			var files = new List<string>();

			foreach (var kv in fault.collectedData)
			{
				var fileName = System.IO.Path.Combine(subDir, kv.Key);
				SaveFile(category, fileName, kv.Value);
				files.Add(fileName);
			}

			OnFaultSaved(category, fault, files.ToArray());

			log.Flush();
		}

		protected override void Engine_ReproFault(RunContext context, uint currentIteration, Peach.Core.Dom.StateModel stateModel, Fault[] faults)
		{
			System.Diagnostics.Debug.Assert(reproFault == null);

			reproFault = combineFaults(context, currentIteration, stateModel, faults);
			SaveFault(Category.Reproducing, reproFault);
		}

		protected override void Engine_ReproFailed(RunContext context, uint currentIteration)
		{
			System.Diagnostics.Debug.Assert(reproFault != null);

			SaveFault(Category.NonReproducable, reproFault);
			reproFault = null;
		}

		protected override void Engine_Fault(RunContext context, uint currentIteration, StateModel stateModel, Fault[] faults)
		{
			var fault = combineFaults(context, currentIteration, stateModel, faults);

			if (reproFault != null)
			{
				// Save reproFault collectedData in fault
				foreach (var kv in reproFault.collectedData)
				{
					var key = System.IO.Path.Combine("Initial", reproFault.iteration.ToString(), kv.Key);
					fault.collectedData.Add(key, kv.Value);
				}

				reproFault = null;
			}

			SaveFault(Category.Faults, fault);
		}

		// TODO: Figure out how to not do this!
		private static byte[] ToByteArray(BitwiseStream data)
		{
			var length = (data.LengthBits + 7) / 8;
			var buffer = new byte[length];
			var offset = 0;
			var count = buffer.Length;

			data.Seek(0, System.IO.SeekOrigin.Begin);

			int nread;
			while ((nread = data.Read(buffer, offset, count)) != 0)
			{
				offset += nread;
				count -= nread;
			}

			if (count != 0)
			{
				System.Diagnostics.Debug.Assert(count == 1);

				ulong bits;
				nread = data.ReadBits(out bits, 64);

				System.Diagnostics.Debug.Assert(nread > 0);
				System.Diagnostics.Debug.Assert(nread < 8);

				buffer[offset] = (byte)(bits << (8 - nread));
			}

			return buffer;
		}

		private Fault combineFaults(RunContext context, uint currentIteration, StateModel stateModel, Fault[] faults)
		{
			Fault ret = new Fault();

			Fault coreFault = null;
			List<Fault> dataFaults = new List<Fault>();

			// First find the core fault.
			foreach (Fault fault in faults)
			{
				if (fault.type == FaultType.Fault)
				{
					coreFault = fault;
					logger.Debug("Found core fault [" + coreFault.title + "]");
				}
				else
					dataFaults.Add(fault);
			}

			if (coreFault == null)
				throw new PeachException("Error, we should always have a fault with type = Fault!");

			// Gather up data from the state model
			foreach (var item in stateModel.dataActions)
			{
				logger.Debug("Saving action: " + item.Key);
				ret.collectedData.Add(item.Key, ToByteArray(item.Value));
			}

			// Write out all collected data information
			foreach (Fault fault in faults)
			{
				logger.Debug("Saving fault: " + fault.title);

				foreach (var kv in fault.collectedData)
				{
					string fileName = fault.detectionSource + "_" + kv.Key;
					ret.collectedData.Add(fileName, kv.Value);
				}

				if (fault.description != null)
				{
					string fileName = fault.detectionSource + "_" + "description.txt";
					ret.collectedData.Add(fileName, Encoding.UTF8.GetBytes(fault.description));
				}
			}

			// Copy over information from the core fault
			if (coreFault.folderName != null)
				ret.folderName = coreFault.folderName;
			else if (coreFault.majorHash == null && coreFault.minorHash == null && coreFault.exploitability == null)
				ret.folderName = "Unknown";
			else
				ret.folderName = string.Format("{0}_{1}_{2}", coreFault.exploitability, coreFault.majorHash, coreFault.minorHash);

			ret.controlIteration = coreFault.controlIteration;
			ret.controlRecordingIteration = coreFault.controlRecordingIteration;
			ret.description = coreFault.description;
			ret.detectionSource = coreFault.detectionSource;
			ret.exploitability = coreFault.exploitability;
			ret.iteration = currentIteration;
			ret.majorHash = coreFault.majorHash;
			ret.minorHash = coreFault.minorHash;
			ret.title = coreFault.title;
			ret.type = coreFault.type;

			return ret;
		}

		protected override void Engine_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			if (currentIteration != 1 && currentIteration % 100 != 0)
				return;

			if (totalIterations != null)
			{
				log.WriteLine(". Iteration {0} of {1} : {2}", currentIteration, (uint)totalIterations, DateTime.Now.ToString());
				log.Flush();
			}
			else
			{
				log.WriteLine(". Iteration {0} : {1}", currentIteration, DateTime.Now.ToString());
				log.Flush();
			}
		}

		protected override void Engine_TestError(RunContext context, Exception e)
		{
			log.WriteLine("! Test error: " + e.ToString());
			log.Flush();
		}

		protected override void Engine_TestFinished(RunContext context)
		{
			if (log != null)
			{
				log.WriteLine(". Test finished: " + context.test.name);
				log.Flush();
				log.Close();
				log.Dispose();
				log = null;
			}
		}

		protected override void MutationStrategy_DataSetChanged(Dom.Action action, Data data)
		{
			if (data.DataType == DataType.File)
			{
				log.WriteLine(". {0}.{1} loaded data file '{2}'", action.parent.name, action.name, data.FileName);
			}
			else
			{
				log.WriteLine(". {0}.{1} applied data fields", action.parent.name, action.name);
				foreach (var item in data.fields)
				{
					log.WriteLine("  {0} = {1}", item.Key, item.Value);
				}
			}

			log.Flush();
		}

		protected override void Engine_TestStarting(RunContext context)
		{
			if (log != null)
			{
				log.Flush();
				log.Close();
				log.Dispose();
				log = null;
			}

			RootDir = GetBasePath(context);

			log = OpenStatusLog();

			log.WriteLine("Peach Fuzzing Run");
			log.WriteLine("=================");
			log.WriteLine("");
			log.WriteLine("Date of run: " + context.config.runDateTime.ToString());
			log.WriteLine("Peach Version: " + context.config.version);

			log.WriteLine("Seed: " + context.config.randomSeed);

			log.WriteLine("Command line: " + context.config.commandLine);
			log.WriteLine("Pit File: " + context.config.pitFile);
			log.WriteLine(". Test starting: " + context.test.name);
			log.WriteLine("");

			log.Flush();
		}

		protected virtual TextWriter OpenStatusLog()
		{
			try
			{
				Directory.CreateDirectory(RootDir);
			}
			catch (Exception e)
			{
				throw new PeachException(e.Message, e);
			}

			return File.CreateText(System.IO.Path.Combine(RootDir, "status.txt"));
		}

		protected virtual string GetBasePath(RunContext context)
		{
			string ret = System.IO.Path.Combine(Path, System.IO.Path.GetFileName(context.config.pitFile));
			if (context.config.runName == "Default")
				ret += "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
			else
				ret += "_" + context.config.runName + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
			return ret;
		}

		protected virtual void OnFaultSaved(Category category, Fault fault, string[] dataFiles)
		{
			if (category != Category.Reproducing)
			{
				// Ensure any past saving of this fault as Reproducing has been cleaned up
				string reproDir = System.IO.Path.Combine(RootDir, Category.Reproducing.ToString());

				if (Directory.Exists(reproDir))
				{
					try
					{
						Directory.Delete(reproDir, true);
					}
					catch (IOException)
					{
						// Can happen if a process has a file/subdirectory open...
					}
				}
			}
		}

		protected virtual void SaveFile(Category category, string fullPath, byte[] contents)
		{
			try
			{
				string dir = System.IO.Path.GetDirectoryName(fullPath);
				Directory.CreateDirectory(dir);
				File.WriteAllBytes(fullPath, contents);
			}
			catch (Exception e)
			{
				throw new PeachException(e.Message, e);
			}
		}
	}
}

// end

