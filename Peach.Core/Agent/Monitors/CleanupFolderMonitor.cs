using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using NLog;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("CleanupFolder", true)]
	[Parameter("Folder", typeof(string), "The folder to cleanup.")]
	public class CleanupFolderMonitor : Peach.Core.Agent.Monitor
	{
		public string Folder { get; private set; }

		public CleanupFolderMonitor(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);
		}

		public override void StopMonitor()
		{
		}

		public override void SessionStarting()
		{
			folderListing = GetListing();
		}

		public override void SessionFinished()
		{
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			var toDel = GetListing().Except(folderListing);

			foreach (var item in toDel)
			{
				try
				{
					var attr = File.GetAttributes(item);
					if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
						Directory.Delete(item, true);
					else
						File.Delete(item);
				}
				catch (Exception ex)
				{
					logger.Debug("Could not delete '{0}'. {1}", item, ex.Message);
				}
			}
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
			return null;
		}

		public override bool MustStop()
		{
			return false;
		}

		public override Variant Message(string name, Variant data)
		{
			return null;
		}

		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		List<string> folderListing;

		List<string> GetListing()
		{
			try
			{
				return Directory.EnumerateFileSystemEntries(Folder, "*", SearchOption.TopDirectoryOnly).ToList();
			}
			catch (Exception ex)
			{
				logger.Debug("Could not list contents of folder '{0}'. {1}", Folder, ex.Message);
				return new List<string>();
			}
		}
	}
}
