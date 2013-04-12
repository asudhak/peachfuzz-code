
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
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml;

using Peach.Core.Dom;
using Peach.Core;
using Peach.Core.Agent;
using Peach.Core.Analyzers;

using SharpPcap;
using NLog;
using NLog.Targets;
using NLog.Config;

namespace Peach.Core.Runtime
{
	/// <summary>
	/// Command line interface for Peach 3.  Mostly backwards compatable with
	/// Peach 2.3.
	/// </summary>
	public class Program
	{
		// PUT THIS INTO YOUR PROGRAM
		////public static int Run(string[] args)
		////{
		////    Peach.Core.AssertWriter.Register();

		////    return new Program(args).exitCode;
		////}

		static ConsoleColor DefaultForground = Console.ForegroundColor;

		public Dictionary<string, string> DefinedValues = new Dictionary<string,string>();
		public Peach.Core.Dom.Dom dom;
		RunConfiguration config = new RunConfiguration();

		public int exitCode = 1;

		public Program()
		{
		}

		protected void ParseRange(RunConfiguration config, string v)
		{
			string[] parts = v.Split(',');
			if (parts.Length != 2)
				throw new PeachException("Invalid range: " + v);

			try
			{
				config.rangeStart = Convert.ToUInt32(parts[0]);
			}
			catch (Exception ex)
			{
				throw new PeachException("Invalid range start iteration: " + parts[0], ex);
			}

			try
			{
				config.rangeStop = Convert.ToUInt32(parts[1]);
			}
			catch (Exception ex)
			{
				throw new PeachException("Invalid range stop iteration: " + parts[1], ex);
			}

			if (config.parallel)
				throw new PeachException("--range is not supported when --parallel is specified");

			config.range = true;
		}

		protected void ParseParallel(RunConfiguration config, string v)
		{
			string[] parts = v.Split(',');
			if (parts.Length != 2)
				throw new PeachException("Invalid parallel value: " + v);

			try
			{
				config.parallelTotal = Convert.ToUInt32(parts[0]);

				if (config.parallelTotal == 0)
					throw new ArgumentOutOfRangeException();
			}
			catch (Exception ex)
			{
				throw new PeachException("Invalid parallel machine total: " + parts[0], ex);
			}

			try
			{
				config.parallelNum = Convert.ToUInt32(parts[1]);
				if (config.parallelNum == 0 || config.parallelNum > config.parallelTotal)
					throw new ArgumentOutOfRangeException();
			}
			catch (Exception ex)
			{
				throw new PeachException("Invalid parallel machine number: " + parts[1], ex);
			}

			if (config.range)
				throw new PeachException("--parallel is not supported when --range is specified");

			config.parallel = true;
		}

		protected static int stopCount = 0;
		protected static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			Console.WriteLine();
			Console.WriteLine(" --- Ctrl+C Detected --- ");

			if (++stopCount == 1)
				e.Cancel = true;
		}

		public void AddNewDefine(string value)
		{
			if(value.IndexOf("=") < 0)
				throw new PeachException("Error, defined values supplied via -D/--define must have an equals sign providing a key-pair set.");

			var kv = value.Split('=');
			DefinedValues[kv[0]] = kv[1];
		}

		public void ShowDevices()
		{
			Console.WriteLine();
			Console.WriteLine("The following devices are available on this machine:");
			Console.WriteLine("----------------------------------------------------");
			Console.WriteLine();

			int i = 0;

			var devices = CaptureDeviceList.Instance;

			// Print out all available devices
			foreach (ICaptureDevice dev in devices)
			{
				Console.WriteLine("Name: {0}\nDescription: {1}\n\n", dev.Name, dev.Description);
				i++;
			}

			throw new SyntaxException();
		}

		public void ShowEnvironment()
		{
			Peach.Core.Usage.Print();
			throw new SyntaxException();
		}

		protected virtual Watcher GetUIWatcher()
		{
			return null;
		}

		protected virtual Analyzer GetParser()
		{
			return Analyzer.defaultParser;
		}
	}


	public class SyntaxException : Exception
	{
	}
}

// end
