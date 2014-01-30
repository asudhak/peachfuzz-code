using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Text;

namespace Peach.Core
{
	public class SubProcess
	{
		public bool Timeout { get; private set; }
		public int ExitCode { get; private set; }
		public StringBuilder StdErr { get; private set; }
		public StringBuilder StdOut { get; private set; }

		public static SubProcess Run(string fileName, string arguments, int timeout = -1)
		{
			var ret = new SubProcess(fileName, arguments, timeout);
			ret.Run();
			return ret;
		}

		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		ProcessStartInfo psi;
		int waitForExit = 0;

		private SubProcess(string fileName, string arguments, int timeout)
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

			waitForExit = timeout;
		}

		private void Run()
		{
			var endEvt = new AutoResetEvent(false);
			var errEvt = new AutoResetEvent(false);
			var outEvt = new AutoResetEvent(false);

			try
			{
				StdOut = new StringBuilder();
				StdErr = new StringBuilder();

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

									StdErr.AppendLine(line ?? "");

									if (!string.IsNullOrEmpty(line) && logger.IsTraceEnabled)
										logger.Trace("{0}: {1}", name, line);
								}
							}
						}
						catch
						{
						}
						finally
						{
							errEvt.Set();
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

									StdOut.AppendLine(line ?? "");

									if (!string.IsNullOrEmpty(line) && logger.IsTraceEnabled)
										logger.Trace("{0}: {1}", name, line);
								}
							}
						}
						catch
						{
						}
						finally
						{
							outEvt.Set();
						}
					}));

					Thread t3 = new Thread(new ThreadStart(delegate
					{
						try
						{
							p.WaitForExit();
						}
						finally
						{
							endEvt.Set();
						}
					}));

					t1.Start();
					t2.Start();
					t3.Start();

					if (!WaitHandle.WaitAll(new[] { errEvt, outEvt, endEvt }, waitForExit))
					{
						Timeout = true;

						try
						{
							p.Kill();
						}
						catch
						{
						}

						try
						{
							t1.Abort();
						}
						catch
						{
						}

						try
						{
							t2.Abort();
						}
						catch
						{
						}

						try
						{
							t3.Abort();
						}
						catch
						{
						}
					}

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

					try
					{
						t3.Join();
					}
					catch (ThreadInterruptedException)
					{
					}

					p.WaitForExit();

					ExitCode = p.ExitCode;
				}
			}
			finally
			{
				outEvt.Dispose();
				errEvt.Dispose();
				endEvt.Dispose();
			}
		}
	}
}
