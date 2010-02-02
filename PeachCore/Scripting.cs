
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Math;

namespace PeachCore
{
	/// <summary>
	/// Scripting class provides easy to use
	/// methods for using Python/Ruby with Peach.
	/// </summary>
	public class Scripting
	{
		/// <summary>
		/// Returns the correct scripting engine.
		/// </summary>
		/// <returns>Scipting engine</returns>
		public static ScriptEngine GetEngine()
		{
			return Python.CreateEngine();
		}

		public static object EvalExpression(string code, Dictionary<string, object> localScope)
		{
			ScriptEngine engine = GetEngine();
			ScriptScope scope = engine.CreateScope();

			foreach (string key in localScope.Keys)
				scope.SetVariable(key, localScope[key]);

			try
			{
				ScriptSource source = engine.CreateScriptSourceFromString(code, SourceCodeKind.Expression);
				object obj = source.Execute(scope);

				if (obj.GetType() == typeof(BigInteger))
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
				throw new PeachException("Error executing expression ["+code+"]: " + ex.ToString());
			}
		}
	}
}
