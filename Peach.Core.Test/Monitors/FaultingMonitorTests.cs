using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;
using Peach.Core.Agent;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Peach.Core.Test.Monitors
{
    /// <summary>
    /// Faults on iterations in the 'Iteration' parameter.
    /// Reproduces faults on iterations in the 'Repro' parameter.
    /// If Iteration is omitted, faults on all non-control iterations.
    /// If Repro is omitted, reproduces all faults.
    /// If 'C' is in the 'Iteration' parameter, faults on a control iteration.
    /// If 'Iteration' parameter is '0', no faults will occur.
    /// If 'Repro' parameter is '0', no faults will reproduce.
    /// </summary>
    [Monitor("FaultingMonitor", true)]
    [Parameter("Iteration", typeof(string), "List of iterations to fault on", "")]
    [Parameter("Repro", typeof(string), "List of iterations to repro fault on", "")]
    public class FaultingMonitor : Peach.Core.Agent.Monitor
    {
        protected string[] iters = new string[0];
        protected string[] reproIters = new string[0];
        protected int curIter = 0;
        protected bool replay = false;
        protected bool fault = false;
        protected bool control = true;

        public FaultingMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
            : base(agent, name, args)
        {
            if (args.ContainsKey("Iteration"))
                iters = ((string)args["Iteration"]).Split(',');
            if (args.ContainsKey("Repro"))
                reproIters = ((string)args["Repro"]).Split(',');
        }

        public override void StopMonitor()
        {
            SessionFinished();
        }

        public override void SessionStarting() { }

        public override void SessionFinished() { }


        public override void IterationStarting(uint iterationCount, bool isReproduction) 
        {
            curIter = (int)iterationCount;
            replay = isReproduction;
        }

        public override bool IterationFinished()
        {
            var iter = control ? "C" : curIter.ToString();

            if (replay)
                fault = reproIters.Length == 0 || reproIters.Contains(iter);
            else if (iters.Length == 0)
                fault = !control;
            else
                fault = iters.Contains(iter);

            control = false;
            return false;
        }

        public override bool DetectedFault()
        {
            return fault;
        }

        public override Fault GetMonitorData()
        {
            if (!DetectedFault())
                return null;

            Fault fault = new Fault();
            fault.detectionSource = "FaultingMonitor";
            fault.folderName = "FaultingMonitor";
            fault.type = FaultType.Fault;

            fault.collectedData.Add(new Fault.Data("Output", Encoding.ASCII.GetBytes("Faulted on Iteration: "+curIter.ToString())));
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
    [TestFixture]
    class FaultingMonitorTests
    {
        List<string> testResults = new List<string>();

        private static string pre_xml =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<Peach>" +
            "	<DataModel name=\"TheDataModel\">" +
            "		<String value=\"Hello World\" />" +
            "	</DataModel>";

        private static string post_xml =
            "	<StateModel name=\"TheState\" initialState=\"Initial\">" +
            "		<State name=\"Initial\">" +
            "			<Action type=\"output\">" +
            "				<DataModel ref=\"TheDataModel\"/>" +
            "			</Action>" +
            "		</State>" +
            "	</StateModel>" +
            "	" +
            "	<Test name=\"Default\">" +
            "		<Agent ref=\"LocalAgent\"/>" +
            "		<StateModel ref=\"TheState\"/>" +
            "		<Publisher class=\"Null\" />" +
            "		<Strategy class=\"RandomDeterministic\"/>" +
            "	</Test>" +
            "</Peach>";

        private void RunTest(string mid_xml, uint iterations, Engine.FaultEventHandler OnFault)
        {
            testResults.Clear();

            string xml = pre_xml + mid_xml + post_xml;

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            RunConfiguration config = new RunConfiguration();

            Engine e = new Engine(null);
            config.range = true;
            config.rangeStart = 0;
            config.rangeStop = iterations;

            if (OnFault != null)
            {
                e.Fault += OnFault;
            }

            e.startFuzzing(dom, config);

            if (OnFault != null)
            {
                Assert.AreEqual(expectedFaults, testResults.Count);
            }
        }

        uint expectedFaultIteration;
        uint expectedFaults;

        [Test]
        public void ControlIterTest()
        {
            string agent_xml =
                "	<Agent name=\"LocalAgent\">" +
                "		<Monitor class=\"FaultingMonitor\">" +
                "			<Param name=\"Iteration\" value=\"C\"/>" +
                "		</Monitor>" +
                "	</Agent>";
            expectedFaultIteration = 1;

            // Faults on iteration 1 cause peach exceptions
            try
            {
                RunTest(agent_xml, 10, new Engine.FaultEventHandler(_Fault));
                Assert.Fail("Should throw.");
            }
            catch (PeachException ex)
            {
                Assert.AreEqual(1, testResults.Count);
                Assert.AreEqual("Fault detected on control iteration.", ex.Message);
            }
        }

        void _Fault(RunContext context, uint currentIteration, Dom.StateModel stateModel, Fault[] faults)
        {
            Assert.AreEqual(expectedFaultIteration, currentIteration);
            Assert.AreEqual(1, faults.Length);
            Assert.AreEqual(expectedFaultIteration, faults[0].iteration);

            testResults.Add("Success");
        }

        [Test]
        public void FirstIterTest()
        {
            string agent_xml =
                "	<Agent name=\"LocalAgent\">" +
                "		<Monitor class=\"FaultingMonitor\">" +
                "			<Param name=\"Iteration\" value=\"1\"/>" +
                "		</Monitor>" +
                "	</Agent>";
            expectedFaultIteration = 1;
            expectedFaults = 1;
            RunTest(agent_xml, 10, new Engine.FaultEventHandler(_Fault));
        }

        [Test]
        public void SecondIterTest()
        {
            string agent_xml =
                "	<Agent name=\"LocalAgent\">" +
                "		<Monitor class=\"FaultingMonitor\">" +
                "			<Param name=\"Iteration\" value=\"2\"/>" +
                "		</Monitor>" +
                "	</Agent>";
            expectedFaultIteration = 2;
            expectedFaults = 1;
            RunTest(agent_xml, 10, new Engine.FaultEventHandler(_Fault));
        }
    }
}
