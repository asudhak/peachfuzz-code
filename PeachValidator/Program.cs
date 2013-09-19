using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PeachValidator
{

#if MONO
	static class Extensions
	{
		public static void BeginInit(this SplitContainer cont)
		{
		}

		public static void EndInit(this SplitContainer cont)
		{
		}
	}
#endif

	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			string pit = (args.Length > 0) ? args[0] : null;
			string sample = (args.Length > 1) ? args[1] : null;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm() { pitFileName = pit, sampleFileName = sample });
		}
	}
}
