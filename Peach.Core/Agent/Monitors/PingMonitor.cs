using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace Peach.Core.Agent.Monitors
{
    [Monitor("PingMonitor")]
    [Parameter("Host", typeof(string), "Host to ping : default 127.0.0.1")]
    [Parameter("Timeout", typeof(int), "Ping timeout in milliseconds default : 1000", "1000")] 
    [Parameter("Data", typeof(string), "Data to send : default none", "")]
    [Parameter("FaultOnSuccess", typeof(bool), "Fault if ping is successful : default is false", "false")]
    public class PingMonitor : Peach.Core.Agent.Monitor
    {
        protected string Host = "127.0.0.1";
        protected bool FaultOnSuccess = false;
        protected int Timeout = 1000;
        protected string Data = ""; 

        private Ping _ping;
 
        public PingMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
            : base(agent, name, args)
        {

            if(args.ContainsKey("Host"))
                Host = (string) args["Host"];

            if(args.ContainsKey("FaultOnSuccess"))
                FaultOnSuccess = ((string) args["FaultOnSuccess"]).ToLower() == "true";

            if (args.ContainsKey("Data"))
                Data = (string) args["Data"]; 

            if (args.ContainsKey("Timeout"))
                Timeout = (int) args["Timeout"];

           _ping = new Ping();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void StopMonitor()
        {
            SessionFinished();
        }

        public override void SessionStarting() { }

        public override void SessionFinished() { }

        public override void IterationStarting(uint iterationCount, bool isReproduction) { }

        public override bool IterationFinished()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool DetectedFault()
        {
            PingReply reply = null;
            try
            {
                if( Data.Length > 0 )
                    reply = _ping.Send( Host, Timeout, ASCIIEncoding.ASCII.GetBytes( Data ));
                else
                    reply = _ping.Send( Host, Timeout );

                if( reply.Status != IPStatus.Success && !FaultOnSuccess )
                    return false;
                else
                    return true;
            }
            catch(Exception)
            {
                return false; 
            }
        }

        /// <summary>
        /// return any data from the accepted socket  
        /// </summary>
        /// <returns></returns>
        public override Fault GetMonitorData()
        {
            Fault fault = new Fault();
            fault.detectionSource = "PingMonitor";
            fault.folderName = "PingMonitor"; 
            fault.type = FaultType.Fault;
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
