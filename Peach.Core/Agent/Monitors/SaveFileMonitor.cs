using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core.Agent.Monitors
{
	/// <summary>
	/// Save a file when a fault occurs.
	/// </summary>
	[Monitor("SaveFile", true)]
	[Parameter("Filename", typeof(string), "File to save on fault")]
	public class SaveFileMonitor : Monitor
	{
		string _fileName = null;

		public SaveFileMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			if (args.ContainsKey("Filename"))
				_fileName = (string)args["Filename"];
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
		}

		public override bool IterationFinished()
		{
			return false;
		}

		public override bool DetectedFault()
		{
			return false;
		}

		public override Fault GetMonitorData()
		{
			Fault fault = new Fault();
			fault.type = FaultType.Data;
			fault.collectedData["savefile_" + Path.GetFileName(_fileName)] = File.ReadAllBytes(_fileName);
			fault.title = "Save File \"" + _fileName + "\"";
			fault.detectionSource = "SaveFileMonitor";

			return fault;
		}

		public override bool MustStop()
		{
			return false;
		}

		public override Variant Message(string name, Variant data)
		{
			return null;
		}
	}
}
