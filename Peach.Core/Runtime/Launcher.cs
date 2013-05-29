using System;
using System.IO;
using System.Reflection;

namespace Peach.Core.Runtime
{
	public class Launcher
	{
		public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var vars = Environment.GetEnvironmentVariables();
			if (!vars.Contains("DEVPATH"))
				return null;

			foreach (var path in ((string)vars["DEVPATH"]).Split(Path.PathSeparator))
			{
				string name = args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll";
				string candidate = Path.Combine(path, name);
				if (File.Exists(candidate))
					return Assembly.LoadFrom(candidate);
			}

			return null;
		}

// PUT THIS INTO YOUR PROGRAM!
//        static int Main(string[] args)
//        {
//#if !MONO
//            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
//#endif

//            return Program.Run(args);
//        }
	}
}
