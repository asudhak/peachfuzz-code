using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.IO;
using Peach.Core.Agent;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Peach.Core.Test.Monitors
{
    [Monitor("FaultingMonitor", true)]
    [Parameter("Iteration", typeof(int), "Iteration to Fault on", true)]
    public class FaultingMonitor : Peach.Core.Agent.Monitor
    {
        protected int Iter = 0;
        protected int curIter = 0;

        public FaultingMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
            : base(agent, name, args)
        {
            if (args.ContainsKey("Iteration"))
                Iter = (int)args["Iteration"];
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
        }

        public override bool IterationFinished()
        {
            return false;
        }

        public override bool DetectedFault()
        {
            bool _FaultResponse = false;
            try
            {
                _FaultResponse = (curIter == Iter);

            }
            catch (Exception e)
            {
                throw new PeachException(e.Message);
            }

            //TODO change to regex
            return _FaultResponse;

        }

        public override Fault GetMonitorData()
        {
            Fault fault = new Fault();
            fault.detectionSource = "FaultingMonitor";
            fault.folderName = "FaultingMonitor";
            fault.type = FaultType.Fault;

            fault.collectedData["Output"] = Encoding.ASCII.GetBytes("Faulted on Iteration: "+curIter.ToString());
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
            "	</Test>" +
            "</Peach>";

        private void RunTest(string mid_xml, uint iterations, Engine.FaultEventHandler OnFault)
        {
            string xml = pre_xml + mid_xml + post_xml;

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            RunConfiguration config = new RunConfiguration();
            config.singleIteration = true;

            Engine e = new Engine(null);
            e.config = config;
            e.config.range = true;
            e.config.rangeStart = 0;
            e.config.rangeStop = iterations;

            if (OnFault != null)
            {
                e.Fault += OnFault;
            }

            e.startFuzzing(dom, config);

            if (OnFault != null)
            {
                Assert.AreEqual(1, testResults.Count);
                testResults.Clear();
            }

            Assert.AreEqual(0, testResults.Count);

        }

        uint expectedFaultIteration;

        [Test]
        public void FirstIterTest()
        {
            string agent_xml =
                "	<Agent name=\"LocalAgent\">" +
                "		<Monitor class=\"FaultingMonitor\">" +
                "			<Param name=\"Iteration\" value=\"0\"/>" +
                "		</Monitor>" +
                "	</Agent>";
            expectedFaultIteration = 0;
            RunTest(agent_xml, 10, new Engine.FaultEventHandler(_Fault));
        }
        void _Fault(RunContext context, uint currentIteration, Dom.StateModel stateModel, Fault[] faults)
        {
            Assert.AreEqual(expectedFaultIteration, currentIteration);
            Assert.AreEqual(1, faults.Length);
            Assert.AreEqual(expectedFaultIteration, faults[0].iteration);

            testResults.Add("Success");
        }


        [Test]
        public void SecondIterTest()
        {
            string agent_xml =
                "	<Agent name=\"LocalAgent\">" +
                "		<Monitor class=\"FaultingMonitor\">" +
                "			<Param name=\"Iteration\" value=\"1\"/>" +
                "		</Monitor>" +
                "	</Agent>";
            expectedFaultIteration = 1;
            RunTest(agent_xml, 10, new Engine.FaultEventHandler(_Fault));
        }
    }
}
