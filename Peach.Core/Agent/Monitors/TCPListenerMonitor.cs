using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Peach.Core.Agent.Monitors
{
    [Monitor("TcpListener", true)]
    [Parameter("Host", typeof(string), "Interface to listen on defaults to 0.0.0.0", "0.0.0.0")]
    [Parameter("Port", typeof(int), "Port to listen on for connection default 8080", "8080")]
    [Parameter("Delay", typeof(int), "Length of time to wait before checking if connections was accepted default 1000 ms", "1000")]
    public class TCPListenerMonitor : Peach.Core.Agent.Monitor
    {
        protected string Host = "0.0.0.0";
        protected int    Port    = 8080;
        protected string Pattern = "";
        protected int    Backlog = 100;
        protected int    Delay   = 1000; 

        private Socket _Listener = null; 

        public class StateObject
        {
            public bool Accepted = false; 
            public Socket WorkSocket = null;
            public const int BufferSize = 1024; 
            public byte[] Buffer = new byte[BufferSize];
            public StringBuilder StringBuffer = new StringBuilder();
        }

        private static StateObject _State = new StateObject();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        public TCPListenerMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
            : base(agent, name, args)
        {
            if (args.ContainsKey("Host"))
                Host = (string) args["Host"];

            if (args.ContainsKey("Port"))
                Port = (int) args["Port"];

            if (args.ContainsKey("Pattern"))
                Pattern = (string) args["Pattern"];

            if (args.ContainsKey("Backlog"))
                Backlog = (int) args["Backlog"];

            if (args.ContainsKey("Delay"))
                Delay = (int) args["Delay"]; 

            try
            {
                IPHostEntry hostInfo        = Dns.GetHostEntry( Host );
                IPAddress   address         = hostInfo.AddressList[0]; 
                IPEndPoint  localEndPoint   = new IPEndPoint( address, Port );

                _Listener = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
                _Listener.Bind( localEndPoint );
                _Listener.Listen( Backlog );
                _Listener.BeginAccept( new AsyncCallback( AcceptCallback ), _Listener ); 
            }
            catch ( Exception e )
            {
                throw new PeachException( e.Message ); 
            }
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
            System.Threading.Thread.Sleep(Delay);

            if( _State.Accepted )  
            {
                _State.Accepted = false;
                _Listener.BeginAccept( new AsyncCallback( AcceptCallback ), _Listener ); 
                return true; 
            }
                return false;
        }

        /// <summary>
        /// return any data from the accepted socket  
        /// </summary>
        /// <returns></returns>
        public override Fault GetMonitorData()
        {
            Fault fault = new Fault();
            fault.detectionSource = "TCPListenerMonitor";
            fault.folderName = "TCPListenerMonitor"; 
            fault.type = FaultType.Fault;
            fault.collectedData["Response"] = _State.Buffer;
            return fault; 
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            Socket listener = (Socket) ar.AsyncState;
            Socket handler = listener.EndAccept(ar); 

            lock(_State)
            {
                _State.Accepted = true; 
                _State.WorkSocket = handler;
                handler.Close();
                //handler.BeginReceive( _State.Buffer, 0, StateObject.BufferSize, 0,
                //                     new AsyncCallback(ReadCallBack), _State );
            }
        }

        public static void ReadCallBack(IAsyncResult ar)
        {
            StateObject state = (StateObject) ar.AsyncState;
            Socket handler = state.WorkSocket;

            int bytesRead = handler.EndReceive(ar); 

            if( bytesRead > 0 )
            {
                _State.StringBuffer.Append(Encoding.ASCII.GetString(_State.Buffer, 0, bytesRead));
            }

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
