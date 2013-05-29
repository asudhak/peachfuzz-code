
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
using System.Diagnostics;

using Peach.Core;
using Peach.Core.Agent;

using NLog;

namespace Peach.Core.Runtime
{
	public class ConsoleWatcher : Watcher
	{
		Stopwatch timer = new Stopwatch();
		uint startIteration = 0;
		bool reproducing = false;

		protected override void Engine_ReproFault(RunContext context, uint currentIteration, Peach.Core.Dom.StateModel stateModel, Fault [] faultData)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(string.Format("\n -- Caught fault at iteration {0}, trying to reproduce --\n", currentIteration));
			Console.ForegroundColor = color;
			reproducing = true;
		}

		protected override void Engine_ReproFailed(RunContext context, uint currentIteration)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(string.Format("\n -- Could not reproduce fault at iteration {0} --\n", currentIteration));
			Console.ForegroundColor = color;
			reproducing = false;
		}

		protected override void Engine_Fault(RunContext context, uint currentIteration, Peach.Core.Dom.StateModel stateModel, Fault[] faultData)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(string.Format("\n -- {1} fault at iteration {0} --\n", currentIteration,
				reproducing ? "Reproduced" : "Caught"));
			Console.ForegroundColor = color;
			reproducing = false;
		}

		protected override void Engine_HaveCount(RunContext context, uint totalIterations)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("\n -- A total of " + totalIterations + " iterations will be performed --\n");
			Console.ForegroundColor = color;
		}

		protected override void Engine_HaveParallel(RunContext context, uint startIteration, uint stopIteration)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("\n -- Machine {0} of {1} will run iterations {2} to {3} --\n",
				context.config.parallelNum, context.config.parallelTotal, startIteration, stopIteration);
			Console.ForegroundColor = color;
		}

		protected override void Engine_IterationFinished(RunContext context, uint currentIteration)
		{
		}

		protected override void Engine_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			string controlIteration = "";
			if (context.controlIteration && context.controlRecordingIteration)
				controlIteration = "R";
			else if (context.controlIteration)
				controlIteration = "C";

			string strTotal = "-";
			string strEta = "-";


			if (!timer.IsRunning)
			{
				timer.Start();
				startIteration = currentIteration;
			}

			if (totalIterations != null && totalIterations < uint.MaxValue)
			{
				strTotal = totalIterations.ToString();

				var done = currentIteration - startIteration;
				var total = totalIterations.Value - startIteration + 1;
				var elapsed = timer.ElapsedMilliseconds;
				TimeSpan remain;

				if (done == 0)
				{
					remain = TimeSpan.FromMilliseconds(elapsed * total);
				}
				else
				{
					remain = TimeSpan.FromMilliseconds((total * elapsed / done) - elapsed);
				}

				strEta = remain.ToString("g");
			}


			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write("\n[");
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write(string.Format("{0}{1},{2},{3}", controlIteration, currentIteration, strTotal, strEta));
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write("] ");
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine("Performing iteration");
			Console.ForegroundColor = color;
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
			Console.WriteLine("Test '" + context.test.name + "' starting with random seed " + context.config.randomSeed + ".");
		}

		protected override void MutationStrategy_Mutating(string elementName, string mutatorName)
		{
			WriteInfoMark();
			Console.WriteLine("Fuzzing: {0}", elementName);
			WriteInfoMark();
			Console.WriteLine("Mutator: {0}", mutatorName);
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
