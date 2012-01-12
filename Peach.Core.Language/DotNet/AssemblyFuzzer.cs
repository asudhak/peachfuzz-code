
/* Copyright (c) 2007-2009 Michael Eddington
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights 
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in	
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * Authors:
 *   Michael Eddington (mike@phed.org)
 * 
 * $Id$
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;

using Peach.Core.Language.DotNet.Generators;

namespace Peach.Core.Language.DotNet
{
	/// <summary>
	/// This assembly fuzzer will use reflection to 
	/// locate all types and try to dynamically create
	/// and invoke them.
	/// 
	/// Methods and properties with type parameters will get
	/// fuzzed using data from PeachData.
	/// </summary>
	public class AssemblyFuzzer : IContext
	{
		List<Assembly> _assemblies = new List<Assembly>();
		List<Type> _typeGenerators = new List<Type>();

		public AssemblyFuzzer()
		{
			RegisterTypeGenerator(typeof(ArrayGenerator));
			RegisterTypeGenerator(typeof(GuidGenerator));
			RegisterTypeGenerator(typeof(BoolGenerator));
			RegisterTypeGenerator(typeof(NumberGenerator));
			RegisterTypeGenerator(typeof(CtorGenerator));
			RegisterTypeGenerator(typeof(MethodGenerator));
			RegisterTypeGenerator(typeof(PropertyGenerator));

			RegisterTypeGenerator(typeof(StringGenerator));

			// Always be last
			RegisterTypeGenerator(typeof(ClassGenerator));
		}

		public void RegisterTypeGenerator(Type typeGenerator)
		{
			if (typeGenerator.GetInterface("ITypeGenerator") != typeof(ITypeGenerator))
				throw new Exception("Attempted to register type generator that did not implement ITypeGenerator");

			_typeGenerators.Add(typeGenerator);
		}

		public void AddAssembly(Assembly asm)
		{
			_assemblies.Add(asm);
		}

		public void Run()
		{
			foreach (Assembly a in _assemblies)
			{
				foreach (Type type in a.GetTypes())
				{
					if (type.IsAbstract)
						continue;

					int count = 1;
					ITypeGenerator gen = GetTypeGenerator(null, type, null);
					try
					{
						while (true)
						{
							gen.GetValue();
							gen.Next();
							count++;
						}
					}
					catch (GeneratorCompleted)
					{
						Debug.WriteLine(String.Format("Performed {0} tests on type {1}.", count, type.ToString()));
					}
				}
			}
		}

		#region IContext Members

		public ITypeGenerator GetTypeGenerator(IGroup group, Type type, object [] obj)
		{
			Boolean ret;
			object [] parms = { type };
			object [] ctorArgs = { this, group, type, obj };

			foreach(Type typeGenerator in _typeGenerators)
			{
				try
				{
					ret = (Boolean)typeGenerator.InvokeMember("SupportedType",
						BindingFlags.Default | BindingFlags.InvokeMethod, null, null, parms);
				}
				catch (Exception e)
				{
					Debug.WriteLine("GetTypeGenerator(): SupportedType call excepted.");
					Debug.WriteLine(e.ToString());
					continue;
				}

				if (ret)
				{
					try
					{
						return (ITypeGenerator)typeGenerator.InvokeMember("CreateInstance",
							BindingFlags.Default | BindingFlags.InvokeMethod, null, null, ctorArgs);
					}
					catch (Exception e)
					{
						Debug.WriteLine("GetTypeGenerator(): CreateInstance call excepted.");
						Debug.WriteLine(e.ToString());
						continue;
					}
				}
			}

			return NullGenerator.CreateInstance(this, null, null, null);
		}

		#endregion
	}

	public class GeneratorCompleted : Exception
	{
		public GeneratorCompleted() : base() { }
		public GeneratorCompleted(string msg) : base(msg) { }
	}

	public class GroupCompleted : Exception
	{
	}

	public interface IContext
	{
		ITypeGenerator GetTypeGenerator(IGroup group, Type type, object [] objs);
	}

	public interface IGroup
	{
		void Next();
		void Reset();
	}

	public interface IGenerator
	{
		object GetValue();
		void Next();
		void Reset();
	}

	public interface ITypeGenerator : IGenerator
	{
		/// <summary>
		/// Check if this type fuzzer supports the specified type.
		/// </summary>
		/// <param name="type">Type to check</param>
		/// <returns>Returns true if type if supported, else false.</returns>
		//public static bool SupportedType(Type type);

		/// <summary>
		/// Create an instance of this Generator.
		/// </summary>
		/// <param name="context">The current context</param>
		/// <param name="group">Group this Generator is assigned to</param>
		/// <param name="type">Type to Generate</param>
		/// <param name="obj">Optional parameter</param>
		/// <returns>Returns an instance of the Generator</returns>
		//public static ITypeGenerator CreateInstance(IContext context, IGroup group, Type type, object obj);
	}
}

// end
