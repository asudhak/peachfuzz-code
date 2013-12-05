using System;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Peach.Core
{
	public class SubProcess
	{
		public static int Run(string fileName, string arguments)
		{
			return new SubProcess(fileName, arguments).Run();
		}

		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		ProcessStartInfo psi;

		private SubProcess(string fileName, string arguments)
		{
			psi = new ProcessStartInfo()
			{
				FileName = fileName,
				Arguments = arguments,
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			};
		}

		private int Run()
		{
			using (var p = Process.Start(psi))
			{
				string name = "{0} (0x{1:X})".Fmt(Path.GetFileName(psi.FileName), p.Id);

				var t1 = new Thread(new ThreadStart(delegate
				{
					try
					{
						using (var sr = p.StandardError)
						{
							while (!sr.EndOfStream)
							{
								var line = sr.ReadLine();

								if (!string.IsNullOrEmpty(line) && logger.IsTraceEnabled)
									logger.Trace("{0}: {1}", name, line);
							}
						}
					}
					catch
					{
					}
				}));

				Thread t2 = new Thread(new ThreadStart(delegate
				{
					try
					{
						using (var sr = p.StandardOutput)
						{
							while (!sr.EndOfStream)
							{
								var line = sr.ReadLine();

								if (!string.IsNullOrEmpty(line) && logger.IsTraceEnabled)
									logger.Trace("{0}: {1}", name, line);
							}
						}
					}
					catch
					{
					}
				}));

				t1.Start();
				t2.Start();

				try
				{
					t1.Join();
				}
				catch (ThreadInterruptedException)
				{
				}

				try
				{
					t2.Join();
				}
				catch (ThreadInterruptedException)
				{
				}

				p.WaitForExit();

				return p.ExitCode;
			}
		}
	}
}
