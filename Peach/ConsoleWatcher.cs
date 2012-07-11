
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Peach.Core;
using Peach.Core.Agent;

namespace Peach
{
	public class ConsoleWatcher : Watcher
	{
		protected override void RunContext_Debug(DebugLevel level, RunContext context, string from, string msg)
		{
			Console.WriteLine(string.Format("DBG[{0}] {1}: {2}", level.ToString(), from, msg));
		}

		protected override void Engine_Fault(RunContext context, uint currentIteration, Dictionary<string, Variant> stateModelData, Dictionary<AgentClient, Hashtable> faultData)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(string.Format("\n -- Caught fault at iteration {0} --\n", currentIteration));
			Console.ForegroundColor = color;

		}

		protected override void Engine_HaveCount(RunContext context, uint totalIterations)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("\n -- A total of " + totalIterations + " iterations will be performed --\n");
			Console.ForegroundColor = color;
		}

		protected override void Engine_IterationFinished(RunContext context, uint currentIteration)
		{
		}

		protected override void Engine_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			if (totalIterations == null)
			{
				var color = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write("\n[");
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.Write(string.Format("{0},-,-", currentIteration));
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write("] ");
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("Performing iteration");
				Console.ForegroundColor = color;
			}
			else
			{
				var color = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write("\n[");
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.Write(string.Format("{0},{1},-", currentIteration, totalIterations));
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write("] ");
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("Performing iteration");
				Console.ForegroundColor = color;
			}
		}

		protected override void Engine_TestError(RunContext context, Exception e)
		{
			Console.Write("\n");
			WriteErrorMark();
			Console.WriteLine("Test '" + context.test.name + "' error: " + e.Message);
		}

		protected override void Engine_TestFinished(RunContext context)
		{
			Console.Write("\n");
			WriteInfoMark();
			Console.WriteLine("Test '" + context.test.name + "' finished.");
		}

		protected override void Engine_TestStarting(RunContext context)
		{
			if (context.config.countOnly)
			{
				WriteInfoMark();
				Console.WriteLine("Calculating total iterations by running single iteration.");
			}

			WriteInfoMark();
			Console.WriteLine("Test '" + context.test.name + "' starting.");
		}

		public static void WriteInfoMark()
		{
			var foregroundColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write("[");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("*");
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write("] ");
			Console.ForegroundColor = foregroundColor;
		}

		public static void WriteErrorMark()
		{
			var foregroundColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write("[");
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("!");
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write("] ");
			Console.ForegroundColor = foregroundColor;
		}
	}
}

// end
