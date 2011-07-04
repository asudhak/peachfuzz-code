
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Peach.Core;
using Peach.Core.Agent;
using Peach.Core.Dom;

namespace Peach.Core.Loggers
{
	/// <summary>
	/// Standard file system logger.
	/// </summary>
	[Logger("File")]
	[Parameter("Path", typeof(string), "Log folder", true)]
	public class FileLogger : Logger
	{
		string logpath = null;
		string ourpath = null;
		TextWriter log = null;

		public FileLogger(Dictionary<string, string> args)
		{
			logpath = args["Path"];
		}

		protected override void Engine_Fault(RunContext context, uint currentIteration, Dictionary<string, Variant> stateModelData, Dictionary<AgentClient, Hashtable> faultData)
		{
			string bucketKey = null;
			string bucketData = null;

			log.WriteLine("! Fault detected at iteration {0}", currentIteration);

			foreach (var data in faultData.Values)
			{
				foreach (var key in data.Keys)
				{
					if (((string)key).IndexOf("Bucket") > -1)
					{
						bucketKey = (string)key;
						bucketData = (string)data[key];
						break;
					}
				}

				if (bucketKey != null)
					break;
			}

			string faultPath = Path.Combine(ourpath, "Faults");
			if (!Directory.Exists(faultPath))
				Directory.CreateDirectory(faultPath);

			if (bucketKey != null)
			{
				faultPath = Path.Combine(faultPath, bucketData);
			}
			else
			{
				faultPath = Path.Combine(faultPath, "Unknown");
			}

			if(!Directory.Exists(faultPath))
				Directory.CreateDirectory(faultPath);

			faultPath = Path.Combine(faultPath, currentIteration.ToString());
			if (!Directory.Exists(faultPath))
				Directory.CreateDirectory(faultPath);

		
		//# Expand actionValues
		
		//for i in range(len(actionValues)):
		//    fileName = os.path.join(path, "data_%d_%s_%s.txt" % (i, actionValues[i][1], actionValues[i][0]))
			
		//    if len(actionValues[i]) > 2:
		//        fout = open(fileName, "w+b")
		//        fout.write(actionValues[i][2])
				
		//        if len(actionValues[i]) > 3 and actionValues[i][1] != 'output':
		//            fout.write(repr(actionValues[i][3]))
				
		//        fout.close()
				
		//        # Output filename from data set if we have it.
		//        if len(actionValues[i]) > 3 and actionValues[i][1] == 'output':
		//            self._writeMsg("Origional file name: "+actionValues[i][3])
					
		//            fileName = os.path.join(path, "data_%d_%s_%s_fileName.txt" % (i, actionValues[i][1], actionValues[i][0]))
		//            fout = open(fileName, "w+b")
		//            fout.write(actionValues[i][3])
		//            fout.close()
		
		//for key in monitorData.keys():
		//    if key.find("_Bucket") == -1:
		//        fout = open(os.path.join(path,key), "wb")
		//        fout.write(monitorData[key])
		//        fout.close()

		}

		protected override void Engine_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			if (currentIteration % 100 != 0)
				return;

			if (totalIterations != null)
				log.WriteLine(". Iteration {0} of {1}", currentIteration, (uint)totalIterations);
			else
				log.WriteLine(". Iteration {0}", currentIteration);
		}

		protected override void Engine_TestError(RunContext context, Exception e)
		{
			log.WriteLine("! Test error: " + e.ToString());
		}

		protected override void Engine_TestFinished(RunContext context)
		{
			log.WriteLine(". Test finished: " + context.run.name + "." + context.test.name);
		}

		protected override void Engine_TestStarting(RunContext context)
		{
			log.WriteLine(". Test starting: " + context.run.name + "." + context.test.name);
		}

		protected override void Engine_RunError(RunContext context, Exception e)
		{
			log.WriteLine("! Run error: " + e.ToString());
		}

		protected override void Engine_RunFinished(RunContext context)
		{
			if (log != null)
			{
				log.Flush();
				log.Close();
				log.Dispose();
				log = null;
			}
		}

		protected override void Engine_RunStarting(RunContext context)
		{
			if (log != null)
			{
				log.Flush();
				log.Close();
				log.Dispose();
				log = null;
			}

			ourpath = Path.Combine(logpath, context.config.pitFile);

			if (context.config.runName == "DefaultRun")
				ourpath += "_" + string.Format("{0:yyyyMMddhhmmss}", DateTime.Now);
			else
				ourpath += "_" + context.config.runName + "_" + string.Format("{0:yyyyMMddhhmmss}", DateTime.Now);

			Directory.CreateDirectory(ourpath);

			log = File.CreateText(Path.Combine(ourpath, "status.txt"));

			log.WriteLine("Peach Fuzzing Run");
			log.WriteLine("=================");
			log.WriteLine("");
			log.WriteLine("Date of run: ");
			log.WriteLine("Peach Version: ");
			log.WriteLine("Seed: ");
			log.WriteLine("Command line: ");
			log.WriteLine("Pit File: ");
			log.WriteLine("Run name: ");
			log.WriteLine("");
		}
	}
}

// end

