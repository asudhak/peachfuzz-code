
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.IO;
using System.Reflection;

namespace Peach
{
	/// <summary>
	/// Command line interface for Peach 3.  Mostly backwards compatable with
	/// Peach 2.3.
	/// </summary>
	public class Program 
	{
		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
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

		static int Main(string[] args)
		{
#if !MONO
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
#endif

			// Keep all references to Peach.Core in a seperate class
			// so we can hook up AssemblyResolve before they are resolved
			return Invoke.Run(args);
		}
	}

	internal class Invoke
	{
		public static int Run(string[] args)
		{
			Peach.Core.AssertWriter.Register();

			return new Peach.Core.Runtime.Program(args).exitCode;
		}
	}
}

// end
