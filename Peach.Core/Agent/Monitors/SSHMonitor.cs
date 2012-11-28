using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Peach.Core.Agent;
using Renci.SshNet;

namespace Peach.Core.Agent.Monitors
{
    [Monitor("SSHMonitor")]
    [Parameter("Host", typeof (string), "Host to ssh to.")]
    [Parameter("Username", typeof (string), "Username for ssh", "")]
	[Parameter("Password", typeof(string), "Password for ssh account", "")]
	[Parameter("KeyPath", typeof(string), "Path to ssh key", "")]
	[Parameter("Command", typeof(string), "Command to check for fault", "")]
	[Parameter("CheckValue", typeof(string), "Value to look for in response", "")]
    [Parameter("Fetch", typeof(bool), "Download the remote file that is output of the ssh command", "false")]
    [Parameter("Remove", typeof(bool), "Remove the remote file after download", "false")]
    [Parameter("GDBAnalyze", typeof(bool), "Analyze the core file in GDB (not implemented)", "false")]
    [Parameter("GDBPath", typeof(string), "Path to GDB (not implemented)", "gdb")]
    public class SSHMonitor : Peach.Core.Agent.Monitor
    {
        protected string Host       = "";
        protected string Username   = "";
        protected string Password   = "";
        protected string KeyPath    = ""; 
        protected string Command    = "";
        protected string CheckValue = "";
        protected bool   Fetch      = false;
        protected bool   Remove     = false;
        protected bool   GDBAnalyze = false;
        protected string GDBPath    = "gdb"; 

        private String _FaultResponse = "";

        private SshClient _sshClient = null; 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        public SSHMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
            : base(agent, name, args)
        {
            if (args.ContainsKey("Host"))
                Host = (string) args["Host"];

            if (args.ContainsKey("Password"))
                Password = (string) args["Password"];

            if (args.ContainsKey("Username"))
                Username = (string) args["Username"];

            if (args.ContainsKey("Command"))
                Command = (string) args["Command"];

            if (args.ContainsKey("CheckValue"))
                CheckValue = (string) args["CheckValue"];

            if (args.ContainsKey("Fetch"))
                Fetch = ((string) args["Fetch"]).ToLower() == "true"; 

            if(args.ContainsKey("Fetch"))
                Remove = ((string) args["Remove"]).ToLower() == "true"; 

            _sshClient = new SshClient(Host, Username, Password);

            _sshClient.Connect();
        }

        public override void StopMonitor()
        {
            SessionFinished();
        }

        public override void SessionStarting() { }

        public override void SessionFinished()
        {
            _sshClient.Disconnect();
        }


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
            try
            {
                    SshCommand sshCommand = _sshClient.RunCommand(Command);
                    _FaultResponse = sshCommand.Execute();
                    _FaultResponse = _FaultResponse.Replace("\n", "");
            }
            catch (Exception e)
            {
                throw new PeachException(e.Message);
                return false; 
            }

            //TODO change to regex
            if (_FaultResponse.Contains(CheckValue))
                return true;
            else
                return false; 

        }

        public override Fault GetMonitorData()
        {
            Fault fault = new Fault();
            fault.detectionSource = "SSHMonitor";
            fault.folderName = "SSHMonitor"; 
            fault.type = FaultType.Fault;

            if(Fetch)
            {
                try
                {
                    using (SftpClient sftpClient = new SftpClient(Host, Username, Password))
                    {

                        sftpClient.Connect();
                        MemoryStream memoryStream = new MemoryStream(10000);
                        sftpClient.DownloadFile(_FaultResponse, memoryStream);
                        fault.collectedData["corefile"] = memoryStream.ToArray();
                        sftpClient.Disconnect();
                    }
                }
                catch (Exception e)
                {
                    throw new PeachException(e.Message);
                }
            }

            if(GDBAnalyze)
            {
                string gdbOutput = "";
                try
                {
                    using (SshClient ssh = new SshClient(Host, Username, Password))
                    {

                        ssh.Connect();
                        //SshCommand sshCommand = ssh.RunCommand();
                        //string gdbOutput = sshCommand.Execute(); 
                        ssh.Disconnect();
                    }
                }
                catch(Exception e)
                {
                 throw new PeachException(e.Message);                    
                }

                fault.collectedData["gdbAnalyze"] = Encoding.ASCII.GetBytes(gdbOutput);  
            }

            if(Remove)
            {
                try
                {
                    using (SftpClient sftpClient = new SftpClient(Host, Username, Password))
                    {
                        sftpClient.Connect();
                        sftpClient.DeleteFile(_FaultResponse);
                        sftpClient.Disconnect();
                    }
                }
                catch (Exception e)
                {
                    throw new PeachException(e.Message);
                }
           }

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
