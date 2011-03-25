
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
using System.Collections.Generic;
using System.Text;
using Peach.Core;

namespace Peach
{
	public class ConsoleWatcher : Watcher
	{
		protected override void RunContext_Debug(DebugLevel level, RunContext context, string from, string msg)
		{
			Console.WriteLine(string.Format("DBG[{0}] {1}: {2}", level.ToString(), from, msg));
		}

		protected override void Engine_Fault(RunContext context, uint currentIteration, object[] stateModelData, object[] faultData)
		{
			throw new NotImplementedException();
		}

		protected override void Engine_IterationFinished(RunContext context, uint currentIteration)
		{
		}

		protected override void Engine_IterationStarting(RunContext context, uint currentIteration, uint? totalIterations)
		{
			if (totalIterations == null)
			{
				Console.WriteLine(string.Format("\n[{0},-,-] Performing iteration", currentIteration));
			}
			else
			{
				Console.WriteLine(string.Format("\n[{0},{1},-] Performing iteration", currentIteration, totalIterations));
			}
		}

		protected override void Engine_TestError(RunContext context, Exception e)
		{
			Console.WriteLine("\n[!] Test '" + context.test.name + "' error: " + e.Message);
		}

		protected override void Engine_TestFinished(RunContext context)
		{
			Console.WriteLine("\n[*] Test '" + context.test.name + "' finished.");
		}

		protected override void Engine_TestStarting(RunContext context)
		{
			Console.WriteLine("[*] Test '" + context.test.name + "' starting.");
		}

		protected override void Engine_RunError(RunContext context, Exception e)
		{
			Console.WriteLine("\n[!] Run '" + context.run.name + "' error: " + e.Message);
		}

		protected override void Engine_RunFinished(RunContext context)
		{
			Console.WriteLine("[*] Run '" + context.run.name + "' finished.");
		}

		protected override void Engine_RunStarting(RunContext context)
		{
			Console.WriteLine("[*] Run '" + context.run.name + "' starting.");
		}
	}
}

// end
