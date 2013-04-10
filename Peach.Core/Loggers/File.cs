
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

using Peach.Core;
using Peach.Core.Agent;
using Peach.Core.Dom;

using NLog;

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
		string logpath = null;
		string ourpath = null;
		TextWriter log = null;
		string reproPath = null;

		public FileLogger(Dictionary<string, Variant> args)
		{
			logpath = (string)args["Path"];
		}

		public string Path
		{
			get { return logpath; }
		}

		protected override void Engine_ReproFault(RunContext context, uint currentIteration, Peach.Core.Dom.StateModel stateModel, Fault[] faults)
		{
			reproPath = saveFaults("Reproducing", context, currentIteration, stateModel, faults);
		}

		protected override void Engine_ReproFailed(RunContext context, uint currentIteration)
		{
			string baseName = System.IO.Path.Combine(ourpath, "Reproducing") + System.IO.Path.DirectorySeparatorChar;
			string subdir = reproPath.Substring(baseName.Length);
			string dest = System.IO.Path.Combine(ourpath, "NonReproducable", System.IO.Path.GetDirectoryName(subdir));
			if (!Directory.Exists(dest))
				Directory.CreateDirectory(dest);
			dest = System.IO.Path.Combine(dest, System.IO.Path.GetFileName(subdir));
			Directory.Move(reproPath, dest);
			Directory.Delete(System.IO.Path.Combine(ourpath, "Reproducing"), true);
			reproPath = null;
		}

		protected override void Engine_Fault(RunContext context, uint currentIteration, StateModel stateModel, Fault[] faults)
		{
			string dir = saveFaults("Faults", context, currentIteration, stateModel, faults);
			if (reproPath != null)
			{
				string dirName = "Initial";
				int i = 1;
				while (Directory.Exists(System.IO.Path.Combine(dir, dirName)))
					dirName = "Initial_" + i++;
				Directory.Move(reproPath, System.IO.Path.Combine(dir, dirName));
				Directory.Delete(System.IO.Path.Combine(ourpath, "Reproducing"), true);
				reproPath = null;
			}
		}

		private string saveFaults(string root, RunContext context, uint currentIteration, StateModel stateModel, Fault[] faults)
		{
			Fault coreFault = null;
			List<Fault> dataFaults = new List<Fault>();

			log.WriteLine("! Fault detected at iteration {0} : {1}", currentIteration, DateTime.Now.ToString());

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
				throw new ApplicationException("Error, we should always have a fault with type = Fault!");

			string faultPath = System.IO.Path.Combine(ourpath, root);
			if (!Directory.Exists(faultPath))
				Directory.CreateDirectory(faultPath);

			if (coreFault.folderName != null)
				faultPath = System.IO.Path.Combine(faultPath, coreFault.folderName);

			else if (coreFault.majorHash == null && coreFault.minorHash == null && coreFault.exploitability == null)
			{
				faultPath = System.IO.Path.Combine(faultPath, "Unknown");
			}
			else
			{
				faultPath = System.IO.Path.Combine(faultPath,
					string.Format("{0}_{1}_{2}", coreFault.exploitability, coreFault.majorHash, coreFault.minorHash));
			}

			if (!Directory.Exists(faultPath))
				Directory.CreateDirectory(faultPath);

			faultPath = System.IO.Path.Combine(faultPath, currentIteration.ToString());
			if (!Directory.Exists(faultPath))
				Directory.CreateDirectory(faultPath);

			int cnt = 0;
			foreach (Dom.Action action in stateModel.dataActions)
			{
				logger.Debug("Writing action: " + action.name);

				cnt++;
				if (action.dataModel != null)
				{
					string fileName = System.IO.Path.Combine(faultPath, string.Format("action_{0}_{1}_{2}.txt",
								  cnt, action.type.ToString(), action.name));

					File.WriteAllBytes(fileName, action.dataModel.Value.Value);
				}
				else if (action.parameters.Count > 0)
				{
					int pcnt = 0;
					foreach (Dom.ActionParameter param in action.parameters)
					{
						pcnt++;
						string fileName = System.IO.Path.Combine(faultPath, string.Format("action_{0}-{1}_{2}_{3}.txt",
										cnt, pcnt, action.type.ToString(), action.name));

						File.WriteAllBytes(fileName, param.dataModel.Value.Value);
					}
				}
			}

			// Write out all data information
			foreach (Fault fault in faults)
			{
				logger.Debug("Writing fault: " + fault.title);

				foreach (string key in fault.collectedData.Keys)
				{
					string fileName = System.IO.Path.Combine(faultPath,
						fault.detectionSource + "_" + key);
					File.WriteAllBytes(fileName, fault.collectedData[key]);
				}

				if (fault.description != null)
				{
					string fileName = System.IO.Path.Combine(faultPath,
						fault.detectionSource + "_" + "description.txt");
					File.WriteAllText(fileName, fault.description);
				}
			}

			log.Flush();
			return faultPath;
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

		protected override void Engine_TestStarting(RunContext context)
		{
			if (log != null)
			{
				log.Flush();
				log.Close();
				log.Dispose();
				log = null;
			}

			if (!Directory.Exists(logpath))
				Directory.CreateDirectory(logpath);

			ourpath = System.IO.Path.Combine(logpath, System.IO.Path.GetFileName(context.config.pitFile));
			if (context.config.runName == "DefaultRun")
				ourpath += "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
			else
				ourpath += "_" + context.config.runName + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now);

			try
			{
				Directory.CreateDirectory(ourpath);
			}
			catch (Exception e)
			{
				throw new PeachException(e.Message, e);
			}

			log = File.CreateText(System.IO.Path.Combine(ourpath, "status.txt"));

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
	}
}

// end

