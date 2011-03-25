using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peach.Core;
using System.Windows.Forms;

namespace PeachFileFuzzer
{
		public class ConsoleWatcher : Watcher
		{
			FormMain _form = null;
			public ConsoleWatcher(FormMain form)
			{
				_form = form;
			}

			protected override void RunContext_Debug(DebugLevel level, RunContext context, string from, string msg)
			{
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
				//_form.progressBarOuputFuzzing.Increment(1);
				
				if (totalIterations == null)
				{
					_form.textBoxOutput.Invoke(new DeligateAppendToText(AppendToText),
						new object[] { _form.textBoxOutput, string.Format("\n[{0},-,-] Performing iteration\n", currentIteration) });
				}
				else
				{
					//if(_form.progressBarOuputFuzzing.Maximum != (int)totalIterations)
					//	_form.progressBarOuputFuzzing.Maximum = (int)totalIterations;

					_form.textBoxOutput.Invoke(new DeligateAppendToText(AppendToText),
						new object[] { _form.textBoxOutput, string.Format("\n[{0},{1},?] Performing iteration\n", currentIteration, totalIterations) });
				}
			}

			protected override void Engine_TestError(RunContext context, Exception e)
			{
				_form.textBoxOutput.Invoke(new DeligateAppendToText(AppendToText),
					new object[] { _form.textBoxOutput, "\n[!] Test '" + context.test.name + "' error: " + e.Message + "\n" });
			}

			protected override void Engine_TestFinished(RunContext context)
			{
				_form.textBoxOutput.Invoke(new DeligateAppendToText(AppendToText),
					new object[] { _form.textBoxOutput, "\n[*] Test '" + context.test.name + "' finished.\n" });
			}

			protected override void Engine_TestStarting(RunContext context)
			{
				_form.textBoxOutput.Invoke(new DeligateAppendToText(AppendToText),
					new object[] { _form.textBoxOutput, "[*] Test '" + context.test.name + "' starting.\n" });
			}

			protected override void Engine_RunError(RunContext context, Exception e)
			{
				_form.textBoxOutput.Invoke(new DeligateAppendToText(AppendToText),
					new object[] { _form.textBoxOutput, "\n[!] Run '" + context.run.name + "' error: " + e.Message + "\n" });
			}

			protected override void Engine_RunFinished(RunContext context)
			{
				_form.textBoxOutput.Invoke(new DeligateAppendToText(AppendToText),
					new object[] { _form.textBoxOutput, "[*] Run '" + context.run.name + "' finished.\n" });
			}

			public delegate void DeligateAppendToText(TextBox cltr, String text);
			public void AppendToText(TextBox ctrl, string text)
			{
				ctrl.Text += text;
			}

			protected override void Engine_RunStarting(RunContext context)
			{
				_form.textBoxOutput.Invoke(new DeligateAppendToText(AppendToText), 
					new object[] { _form.textBoxOutput, "[*] Run '" + context.run.name + "' starting.\n"});
			}
		}
}
