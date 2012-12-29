using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;
using Proc = System.Diagnostics.Process;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("Memory", true)]
	[Parameter("MemoryLimit", typeof(uint), "Fault when memory usage surpasses limit.", "0")]
	[Parameter("StopOnFault", typeof(bool), "Stop when a fault is detected.", "false")]
	[Parameter("Pid", typeof(int?), "ID of process to monitor.", "")]
	[Parameter("ProcessName", typeof(string), "Name of process to monitor.", "")]
	public class MemoryMonitor : Peach.Core.Agent.Monitor
	{
		private Fault fault = null;

		public uint   MemoryLimit { get; private set; }
		public bool   StopOnFault { get; private set; }
		public int?   Pid         { get; private set; }
		public string ProcessName { get; private set; }

		private ProcessInfo GetProcessInfo()
		{
			Proc[] procs = new Proc[0];

			try
			{
				if (Pid.HasValue)
					procs = new Proc[] { Proc.GetProcessById(Pid.Value) };
				else
					procs = Proc.GetProcessesByName(ProcessName);

				var p = procs.FirstOrDefault();

				if (p != null)
					return ProcessInfo.Instance.Snapshot(p);
			}
			catch
			{
			}
			finally
			{
				foreach (var p in procs)
					p.Close();
			}

			return null;
		}

		public MemoryMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);

			if (!Pid.HasValue && string.IsNullOrEmpty(ProcessName))
				throw new PeachException("Either pid or process name is required.");

			if (Pid.HasValue && !string.IsNullOrEmpty(ProcessName))
				throw new PeachException("Only specify pid or process name, not both.");
		}

		public override void StopMonitor()
		{
		}

		public override void SessionStarting()
		{
		}

		public override void SessionFinished()
		{
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			fault = null;
		}

		public override bool IterationFinished()
		{
			fault = new Fault();
			fault.detectionSource = "MemoryMonitor";

			ProcessInfo pi = GetProcessInfo();
			if (pi == null)
			{
				if (Pid.HasValue)
					fault.description = "Unable to locate process with Pid " + Pid.Value + ".";
				else
					fault.description = "Unable to locate process \"" + ProcessName + "\".";

				fault.type = FaultType.Fault;
				fault.title = fault.description;
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("PrivateMemorySize: {0}\n", pi.PrivateMemorySize64);
				sb.AppendFormat("WorkingSet: {0}\n", pi.WorkingSet64);
				sb.AppendFormat("PeakWorkingSet: {0}\n", pi.PeakWorkingSet64);
				sb.AppendFormat("VirtualMemorySize: {0}\n", pi.VirtualMemorySize64);
				sb.AppendFormat("PeakVirtualMemorySize: {0}\n", pi.PeakVirtualMemorySize64);

				fault.type = MemoryLimit > 0 && MemoryLimit <= pi.WorkingSet64 ? FaultType.Fault : FaultType.Data;
				fault.description = sb.ToString();
				fault.title = string.Format("{0} (pid: {1}) memory usage", pi.ProcessName, pi.Id);
			}

			return false;
		}

		public override bool DetectedFault()
		{
			System.Diagnostics.Debug.Assert(fault != null);
			return fault.type == FaultType.Fault;
		}

		public override Fault GetMonitorData()
		{
			return fault;
		}

		public override bool MustStop()
		{
			return StopOnFault && DetectedFault();
		}

		public override Variant Message(string name, Variant data)
		{
			return null;
		}
	}
}
