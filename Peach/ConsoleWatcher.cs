
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
		protected override void Engine_Fault(RunContext context, uint currentIteration, Peach.Core.Dom.StateModel stateModel, Fault [] faultData)
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

			if (totalIterations == null || totalIterations == int.MaxValue)
			{
				var color = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write("\n[");
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.Write(string.Format("{1}{0},-,-", currentIteration, controlIteration));
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
				if(totalIterations == uint.MaxValue)
					Console.Write(string.Format("{2}{0},-,-", currentIteration, totalIterations, controlIteration));
				else
					Console.Write(string.Format("{2}{0},{1},-", currentIteration, totalIterations, controlIteration));
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
