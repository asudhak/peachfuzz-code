using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Peach.Core.Agent;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Text.RegularExpressions;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("Ssh", true)]
	[Parameter("Host", typeof(string), "Host to ssh to.")]
	[Parameter("Username", typeof(string), "Username for ssh")]
	[Parameter("Password", typeof(string), "Password for ssh account", "")]
	[Parameter("KeyPath", typeof(string), "Path to ssh key", "")]
	[Parameter("Command", typeof(string), "Command to check for fault")]
	[Parameter("CheckValue", typeof(string), "Regex to match on response", "")]
	[Parameter("FaultOnMatch", typeof(bool), "Fault if regex matches", "true")]
	public class SSHMonitor : Peach.Core.Agent.Monitor
	{
		public string Host { get; protected set; }
		public string Username { get; protected set; }
		public string Password { get; protected set; }
		public string KeyPath { get; protected set; }
		public string Command { get; protected set; }
		public string CheckValue { get; protected set; }
		public bool FaultOnMatch { get; protected set; }

		private Fault _fault = null;
		private SshClient _sshClient = null;
		private Regex _regex = null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="agent"></param>
		/// <param name="name"></param>
		/// <param name="args"></param>
		public SSHMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);

			if (Password == null && KeyPath == null)
				throw new PeachException("Either Password or KeyPath is required.");

			try
			{
				_regex = new Regex(CheckValue ?? "", RegexOptions.Multiline);
			}
			catch (ArgumentException ex)
			{
				throw new PeachException("'CheckValue' is not a valid regular expression.  " + ex.Message, ex);
			}
		}

		void OnAuthPrompt(object sender, AuthenticationPromptEventArgs e)
		{
			foreach (var prompt in e.Prompts)
			{
				prompt.Response = Password;
			}
		}

		void OpenClient(ConnectionInfo ci)
		{
			try
			{
				_sshClient = new SshClient(ci);
				_sshClient.Connect();
			}
			catch (Exception ex)
			{
				_sshClient.Dispose();
				_sshClient = null;

				throw new PeachException("Could not start the ssh monitor.  " + ex.Message, ex);
			}
		}

		public override void SessionStarting()
		{
			if (KeyPath != null)
			{
				var ci = new PrivateKeyConnectionInfo(Host, Username, new PrivateKeyFile(KeyPath));
				OpenClient(ci);
			}
			else
			{
				var auth_passwd = new PasswordAuthenticationMethod(Username, Password);
				var auth_kb = new KeyboardInteractiveAuthenticationMethod(Username);
				auth_kb.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(OnAuthPrompt);
				var ci = new ConnectionInfo(Host, Username, auth_kb, auth_passwd);
				OpenClient(ci);
			}
		}

		public override void SessionFinished()
		{
			if (_sshClient != null)
			{
				_sshClient.Disconnect();
				_sshClient = null;
			}
		}

		public override bool DetectedFault()
		{
			_fault = new Fault();
			_fault.type = FaultType.Fault;
			_fault.detectionSource = "SshMonitor";
			_fault.folderName = "SshMonitor";

			try
			{
				using (SshCommand cmd = _sshClient.RunCommand(Command))
				{
					_fault.title = "Response";
					_fault.description = cmd.Execute();

					bool match = _regex.IsMatch(_fault.description);

					if (match)
						_fault.type = FaultOnMatch ? FaultType.Fault : FaultType.Data;
					else
						_fault.type = FaultOnMatch ? FaultType.Data : FaultType.Fault;
				}
			}
			catch (Exception ex)
			{
				_fault.title = "Exception";
				_fault.description = ex.Message;
			}

			return _fault.type == FaultType.Fault;

		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			_fault = null;
		}

		public override bool IterationFinished()
		{
			return false;
		}

		public override void StopMonitor()
		{
			SessionFinished();
		}

		public override Fault GetMonitorData()
		{
			return _fault;
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
