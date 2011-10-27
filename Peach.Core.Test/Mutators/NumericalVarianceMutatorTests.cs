using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.IO;

namespace Peach.Core.Test.Mutators
{
    [TestFixture]
    class NumericalVarianceMutatorTests
    {
		int? testValue = null;

        [Test]
        public void Test1()
        {            
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Peach>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Number name=\"num1\" size=\"32\" value=\"100\" signed=\"false\"/>" +
                "   </DataModel>" +

                "   <StateModel name=\"TheState\" initialState=\"Initial\">" +
                "       <State name=\"Initial\">" +
                "           <Action type=\"output\">" +
                "               <DataModel ref=\"TheDataModel\"/>" +
                "           </Action>" +
                "       </State>" +
                "   </StateModel>" +

                "   <Test name=\"TheTest\">" +
                "       <StateModel ref=\"TheState\"/>" +
                "       <Publisher class=\"Stdout\"/>" +
                "   </Test>" +

                "   <Run name=\"DefaultRun\">" +
                "       <Test ref=\"TheTest\"/>" +
                "   </Run>" +
                "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(new Dictionary<string, string>(), new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
            RunConfiguration config = new RunConfiguration();
            config.rangeStart = 1;
            config.rangeStop = 2;

			Dom.Action.Finished += new ActionFinishedEventHandler(Action_Finished);

			Engine e = new Engine(null);
            e.config = config;
			e.startFuzzing(dom, config);
            RunContext context = e.context;
            Assert.AreEqual(1,testValue);
        }

		void Action_Finished(Dom.Action action)
		{
			testValue = (int)action.dataModel[0].InternalValue;
		}
    }
}
