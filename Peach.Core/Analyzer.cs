
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
using System.Reflection;
using System.IO;
using Peach.Core.Dom;

namespace Peach.Core
{
	[Serializable]
	public abstract class Analyzer
	{
		public static bool supportParser = false;
		public static bool supportDataElement = false;
		public static bool supportCommandLine = false;
		public static bool supportTopLevel = false;

		public static Analyzer defaultParser = null;

		static Analyzer()
		{
			foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
			{
				if (!type.IsAbstract && type.IsClass &&
					type.IsPublic && type.IsSubclassOf(typeof(Analyzer)))
				{
					// Found an Analyzer!

					type.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
				}
			}
		}

		/// <summary>
		/// Replaces the parser for fuzzer definition.
		/// </summary>
		/// <param name="args">Command line arguments</param>
		/// <param name="fileName">File to parse</param>
		public virtual Dom.Dom asParser(Dictionary<string, object> args, string fileName)
		{
			try
			{
				using(Stream fin = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					Dom.Dom ret = asParser(args, fin);
					ret.fileName = fileName;
					return ret;
				}
			}
			catch (FileNotFoundException fileNotFoundException)
			{
				throw new PeachException("Error, " + fileNotFoundException.Message, fileNotFoundException);
			}
			catch (PathTooLongException pathTooLongException)
			{
				throw new PeachException("Error, " + pathTooLongException.Message, pathTooLongException);
			}
			catch (DirectoryNotFoundException directoryNotFoundException)
			{
				throw new PeachException("Error, " + directoryNotFoundException.Message, directoryNotFoundException);
			}
			catch (UnauthorizedAccessException unauthorizedAccessException)
			{
				throw new PeachException("Error, " + unauthorizedAccessException.Message, unauthorizedAccessException);
			}
			catch (NotSupportedException notSupportedException)
			{
				throw new PeachException("Error, " + notSupportedException.Message, notSupportedException);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public virtual Dom.Dom asParser(Dictionary<string, object> args, Stream data)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Called to perform validation pass.
		/// </summary>
		/// <param name="args">Arguments</param>
		/// <param name="fileName">Filename to test</param>
		/// <returns>Throws PeachException on error.</returns>
		public virtual void asParserValidation(Dictionary<string, object> args, string fileName)
		{
			try
			{
				asParserValidation(args, File.OpenRead(fileName));
			}
			catch (PeachException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new PeachException("Error, " + ex.Message, ex);
			}
		}

		public virtual void asParserValidation(Dictionary<string, object> args, Stream data)
		{
			throw new NotImplementedException();
		}

		public virtual void asDataElement(DataElement parent, object dataBuffer)
		{
			throw new NotImplementedException();
		}

		public virtual void asCommandLine(Dictionary<string, string> args)
		{
			throw new NotImplementedException();
		}

		public virtual void asTopLevel(Dom.Dom dom, Dictionary<string, string> args)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Used to indicate a class is a valid Publisher and 
	/// provide it's invoking name used in the Pit XML file.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class AnalyzerAttribute : PluginAttribute
	{
		public AnalyzerAttribute(string name, bool isDefault = false)
			: base(typeof(Analyzer), name, isDefault)
		{
		}
	}

}

// end
