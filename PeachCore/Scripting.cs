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
