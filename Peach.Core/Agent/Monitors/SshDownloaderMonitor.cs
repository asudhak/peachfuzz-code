using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peach.Core.Agent;
using Renci.SshNet;
using NLog;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("SshDownloader", true)]
	[Parameter("Host", typeof(string), "Host to ssh to.")]
	[Parameter("Username", typeof(string), "Username for ssh", "")]
	[Parameter("Password", typeof(string), "Password for ssh account", "")]
	[Parameter("KeyPath", typeof(string), "Path to ssh key", "")]
	[Parameter("File", typeof(string), "File to download", "")]
	[Parameter("Folder", typeof(string), "Folder to download", "")]
	[Parameter("Remove", typeof(bool), "Remove the remote file after download", "true")]
	public class SshDownloaderMonitor : Monitor
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		string Host = null;
		string Password = null;
		string Username = null;
		string KeyPath = null;
		string File = null;
		string Folder = null;
		bool Remove = true;

		ConnectionInfo connectionInfo = null;

		public SshDownloaderMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
            : base(agent, name, args)
        {
            if (args.ContainsKey("Host"))
                Host = (string) args["Host"];

            if (args.ContainsKey("Password"))
                Password = (string) args["Password"];

            if (args.ContainsKey("Username"))
                Username = (string) args["Username"];

			if (args.ContainsKey("KeyPath"))
				KeyPath = (string)args["KeyPath"];

			if (args.ContainsKey("File"))
				File = (string)args["File"];

			if (args.ContainsKey("Folder"))
				Folder = (string)args["Folder"];

			if (args.ContainsKey("Remove"))
                Remove = ((string) args["Remove"]).ToLower() == "true";
        }

		void info_AuthenticationPrompt(object sender, Renci.SshNet.Common.AuthenticationPromptEventArgs e)
		{
			foreach (var prompt in e.Prompts)
			{
				prompt.Response = Password;
			}
		}

		public override void StopMonitor()
		{
		}

		public override void SessionStarting()
		{
			logger.Debug(">> SessionStarting()");

			// Verify we can connect to host
			if (KeyPath != null)
			{
				logger.Debug("Connecting with key file");
				connectionInfo = new PrivateKeyConnectionInfo(Host, Username, new PrivateKeyFile(KeyPath));
				using (var sshClient = new SshClient(connectionInfo))
					sshClient.Connect();
			}
			else
			{
				logger.Debug("Connecting with username/password");

				try
				{
					connectionInfo = new PasswordConnectionInfo(Host, Username, Password);
					using (var sshClient = new SshClient(connectionInfo))
						sshClient.Connect();
				}
				catch (Renci.SshNet.Common.SshAuthenticationException)
				{
					connectionInfo = new KeyboardInteractiveConnectionInfo(Host, Username);
					((KeyboardInteractiveConnectionInfo)connectionInfo).AuthenticationPrompt += new EventHandler<Renci.SshNet.Common.AuthenticationPromptEventArgs>(info_AuthenticationPrompt);

					using (var sshClient = new SshClient(connectionInfo))
						sshClient.Connect();
				}
			}

			logger.Debug("<< SessionStarting()");
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
			try
			{
				logger.Debug(">> GetMonitorData");

				var fault = new Fault();
				fault.type = FaultType.Data;
				fault.title = "SSH Downloader: " + File != null ? File : Folder;
				fault.detectionSource = "SshDownloader";
				fault.description = "";

				using (var client = new SftpClient(connectionInfo))
				{
					client.Connect();

					if (!string.IsNullOrWhiteSpace(File))
					{
						using (var sout = new MemoryStream())
						{
							logger.Debug("Trying to download \"" + File + "\".");

							client.DownloadFile(File, sout);
							if (Remove)
								client.DeleteFile(File);

							sout.Position = 0;
							fault.collectedData[Path.GetFileName(File)] = sout.ToArray();
						}

						fault.description = File;

						return fault;
					}

					if (string.IsNullOrWhiteSpace(Folder))
						return null;

					logger.Debug("Downloading all files from \"" + Folder + "\".");
					foreach (var file in client.ListDirectory(Folder))
					{
						if (file.FullName.EndsWith("/.") || file.FullName.EndsWith("/..") || file.FullName.EndsWith(@"\.") || file.FullName.EndsWith(@"\.."))
						{
							logger.Debug("Skipping \"" + file.FullName + "\".");
							continue;
						}

						logger.Debug("Downloading \"" + file.FullName + "\".");

						using (var sout = new MemoryStream((int)file.Length))
						{
							try
							{
								client.DownloadFile(file.FullName, sout);
								if (Remove)
									client.DeleteFile(file.FullName);

								sout.Position = 0;
								fault.collectedData[Path.GetFileName(file.FullName)] = sout.ToArray();

								fault.description += file.FullName + "\n";
							}
							catch(Exception ex)
							{
								logger.Warn("Warning, could not d/l file [" + file.FullName + "]: " + ex.Message);
								fault.description += "Warning, could not d/l file [" + file.FullName + "]\n";
							}
						}
					}

					logger.Debug("<< GetMonitorData");
					return fault;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.Message);
				throw;
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

// end
