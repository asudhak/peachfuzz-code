using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core.Agent.Monitors
{
    [Monitor("Replay", true)]
    public class ReplayMonitor : Monitor
    {
        bool alreadyPaused = false;
        bool replay = true;
        bool firstIteration = true;

        public ReplayMonitor(IAgent agent, string name, Dictionary<string, Variant> args) : base(agent, name, args)
		{
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			alreadyPaused = false;
		}

		public override bool DetectedFault()
		{
			// Method will get called multiple times
			// we only want to pause the first time.
			if (!alreadyPaused)
			{
				alreadyPaused = true;
			}

			return false;
		}

		public override Fault GetMonitorData()
		{
			if (!DetectedFault())
				return null;

            Fault fault = new Fault();
            fault.detectionSource = "ReplayMonitor";
            fault.title = "Fake ReplayMonitor Finding";
            fault.folderName = "ReplayMonitor";
            fault.type = FaultType.Fault;

            return fault;
		}

		public override bool MustStop()
		{
			return false;
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

        public override bool IterationFinished()
        {
            if (firstIteration)
            {
                firstIteration = false;
                return false;
            }
            else if (replay)
            {
                replay = false;
                throw new ReplayTestException();
            }
            else
            {
                replay = true;
                return false;
            }
        }

		public override Variant Message(string name, Variant data)
		{
			return null;
		}
    }
}

// end //
