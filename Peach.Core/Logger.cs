
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
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Peach.Core
{
	/// <summary>
	/// Base class for different logging methods.
	/// </summary>
	public abstract class Logger : Watcher
	{
		/// <summary>
		/// Make the actual log path to use based on the run name,
		/// run time and path parameter.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		protected static string GetLogPath(RunContext context, string path)
		{
			var sb = new StringBuilder();

			sb.Append(Path.Combine(path, Path.GetFileName(context.config.pitFile)));

			if (context.config.runName != "Default")
			{
				sb.Append("_");
				sb.Append(context.config.runName);
			}

			sb.Append("_");
			sb.Append(context.config.runDateTime.ToString("yyyyMMddHHmmss"));

			return sb.ToString();
		}
	}

	/// <summary>
	/// Used to indicate a class is a valid Publisher and 
	/// provide it's invoking name used in the Pit XML file.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class LoggerAttribute : PluginAttribute
	{
		public LoggerAttribute(string name, bool isDefault = false)
			: base(typeof(Logger), name, isDefault)
		{
		}
	}

}
