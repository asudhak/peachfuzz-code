
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
using System.Linq;
using IronPython;
using IronPython.Hosting;
using IronRuby;
using IronRuby.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Math;
using System.Reflection;
using System.IO;

namespace Peach.Core
{
	public enum ScriptingEngines
	{
		Python,
		Ruby
	}

	/// <summary>
	/// Scripting class provides easy to use
	/// methods for using Python/Ruby with Peach.
	/// </summary>
	public class Scripting
	{
		static public ScriptingEngines DefaultScriptingEngine = ScriptingEngines.Python;
		static public List<string> Imports = new List<string>();
		static public List<string> Paths = new List<string>();
		static public Dictionary<string, object> GlobalScope = new Dictionary<string, object>();
		static public string StdLib = ClassLoader.FindFile("IronPython.StdLib.zip");

		private static class Engine
		{
			static public ScriptEngine Instance { get; private set; }
			static public Dictionary<string, ScriptScope> Modules { get; private set; }

			static Engine()
			{
				// Construct the correct engine type
				if (DefaultScriptingEngine == ScriptingEngines.Python)
					Instance = IronPython.Hosting.Python.CreateEngine();
				else
					Instance = IronRuby.Ruby.CreateEngine();

				// Add any specified paths to our engine.
				ICollection<string> enginePaths = Instance.GetSearchPaths();
				foreach (string path in Paths)
					enginePaths.Add(path);
				enginePaths.Add(StdLib);
				Instance.SetSearchPaths(enginePaths);

				// Import any modules
				Modules = new Dictionary<string,ScriptScope>();
				foreach (string import in Imports)
					if (!Modules.ContainsKey(import))
						Modules.Add(import, Instance.ImportModule(import));
			}
		}

		public static void Exec(string code, Dictionary<string, object> localScope)
		{
			var missing = Imports.Except(Engine.Modules.Keys).ToList();
			foreach (string import in missing)
				Engine.Modules.Add(import, Engine.Instance.ImportModule(import));

			ScriptScope scope = Engine.Instance.CreateScope();

			foreach (var kv in Engine.Modules)
				scope.SetVariable(kv.Key, kv.Value);

			foreach (var kv in GlobalScope)
				scope.SetVariable(kv.Key, kv.Value);

			foreach (var kv in localScope)
				scope.SetVariable(kv.Key, kv.Value);

			try
			{
				scope.Engine.Execute(code, scope);
			}
			catch (Exception ex)
			{
				throw new PeachException("Error executing expression [" + code + "]: " + ex.ToString(), ex);
			}
			finally
			{
				// Clean up any internal state created by the scope
				var names = scope.GetVariableNames().ToList();
				foreach (var name in names)
					scope.RemoveVariable(name);
			}
		}

		public static object EvalExpression(string code, Dictionary<string, object> localScope)
		{
			var missing = Imports.Except(Engine.Modules.Keys).ToList();
			foreach (string import in missing)
				Engine.Modules.Add(import, Engine.Instance.ImportModule(import));

			ScriptScope scope = Engine.Instance.CreateScope();

			foreach (var kv in Engine.Modules)
				scope.SetVariable(kv.Key, kv.Value);

			foreach (var kv in GlobalScope)
				scope.SetVariable(kv.Key, kv.Value);

			foreach (var kv in localScope)
				scope.SetVariable(kv.Key, kv.Value);

			try
			{
				ScriptSource source = scope.Engine.CreateScriptSourceFromString(code, SourceCodeKind.Expression);
				object obj = source.Execute(scope);

				if (obj != null && obj.GetType() == typeof(BigInteger))
				{
					BigInteger bint = (BigInteger)obj;

					int i32;
					uint ui32;
					long i64;
					ulong ui64;

					if (bint.AsInt32(out i32))
						return i32;

					if (bint.AsInt64(out i64))
						return i64;

					if (bint.AsUInt32(out ui32))
						return ui32;

					if (bint.AsUInt64(out ui64))
						return ui64;
				}

				return obj;
			}
			catch (Exception ex)
			{
				throw new PeachException("Error executing expression ["+code+"]: " + ex.ToString(), ex);
			}
			finally
			{
				var names = scope.GetVariableNames().ToList();
				foreach (var name in names)
					scope.RemoveVariable(name);
			}
		}
	}
}
