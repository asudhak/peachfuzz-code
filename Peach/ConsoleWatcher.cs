using System;
using System.Collections.Generic;
using System.Text;
using PeachCore;

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
				Console.WriteLine(string.Format("[{0},-,-] Performing iteration", currentIteration));
			}
			else
			{
				Console.WriteLine(string.Format("[{0},{1},?] Performing iteration", currentIteration, totalIterations));
			}
		}

		protected override void Engine_TestError(RunContext context, Exception e)
		{
			Console.WriteLine("[!] Test '" + context.test.name + "' error: " + e.Message);
		}

		protected override void Engine_TestFinished(RunContext context)
		{
			Console.WriteLine("[*] Test '" + context.test.name + "' finished.");
		}

		protected override void Engine_TestStarting(RunContext context)
		{
			Console.WriteLine("[*] Test '" + context.test.name + "' starting.");
		}

		protected override void Engine_RunError(RunContext context, Exception e)
		{
			Console.WriteLine("[!] Run '" + context.run.name + "' error: " + e.Message);
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
