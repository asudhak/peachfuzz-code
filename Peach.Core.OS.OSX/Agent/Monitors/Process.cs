using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Peach.Core.Dom;

using NLog;

namespace Peach.Core.Agent.Monitors
{
	/// <summary>
	/// Start a process
	/// </summary>
	[Monitor("Process", true)]
	[Monitor("process.Process")]
	public class Process : BaseProcess
	{
		public Process(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
		}

		protected override ulong GetTotalCpuTime(System.Diagnostics.Process process)
		{
			return CrashWrangler.GetTotalCputime(process.Id);
		}

	}
}
