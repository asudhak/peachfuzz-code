
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
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Peach.Core.Language.DotNet.Generators
{
	public class ClassGenerator : ITypeGenerator
	{
		static string[] _avoidTheseTypes = new string[] {
			"TypeId",
			};

		//static Stack<string> _stack = new Stack<string>();
		protected static Dictionary<string, ITypeGenerator> _stack = new Dictionary<string, ITypeGenerator>();

		protected string _name;
		protected IContext _context;
		protected IGroup _group;
		protected Type _type;

		protected ConstructorInfo _defaultCtor;

		protected List<ConstructorInfo> _ctors = new List<ConstructorInfo>();
		protected List<MethodInfo> _methods = new List<MethodInfo>();
		protected List<EventInfo> _events = new List<EventInfo>();
		protected List<PropertyInfo> _properties = new List<PropertyInfo>();

		protected int _position = 0;
		protected List<MemberInfo> _stuffToFuzz = new List<MemberInfo>();

		protected Dictionary<MemberInfo, IGenerator> _typeGenerators = new Dictionary<MemberInfo, IGenerator>();

		protected ClassGenerator(IContext context, IGroup group, Type type)
		{
			try
			{
				_name = type.ToString();

				_stack.Add(_name, this);

				_context = context;
				_group = group;
				_type = type;

				_ctors.AddRange(type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
				_methods.AddRange(type.GetMethods(BindingFlags.Instance | BindingFlags.Public | 
					BindingFlags.NonPublic | BindingFlags.Static));
				_events.AddRange(type.GetEvents());
				_properties.AddRange(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | 
					BindingFlags.NonPublic | BindingFlags.Static));

				_defaultCtor = FindDefaultConstructor();
				if(_defaultCtor == null)
					Debugger.Break();

				IGenerator defaultCtorGenerator = _context.GetTypeGenerator(null, _defaultCtor.GetType(), new object[] { _defaultCtor });

				foreach (MemberInfo memberInfo in _ctors)
					_typeGenerators.Add(memberInfo, _context.GetTypeGenerator(_group, memberInfo.GetType(), new object[] { memberInfo, defaultCtorGenerator }));

				foreach (MemberInfo memberInfo in _methods)
					_typeGenerators.Add(memberInfo, _context.GetTypeGenerator(_group, memberInfo.GetType(), new object[] { memberInfo, defaultCtorGenerator }));

				foreach (MemberInfo memberInfo in _events)
					_typeGenerators.Add(memberInfo, _context.GetTypeGenerator(_group, memberInfo.GetType(), new object[] { memberInfo, defaultCtorGenerator }));

				foreach (MemberInfo memberInfo in _properties)
					_typeGenerators.Add(memberInfo, _context.GetTypeGenerator(_group, memberInfo.GetType(), new object[] { memberInfo, defaultCtorGenerator }));

				// Setup our order of fuzzing.
				//  1. .ctors
				//  2. methods w/default .ctor
				//  3. properties w/default .ctor
				//  4. static methods

				foreach (MethodBase method in _ctors)
				{
					if (method.IsStatic)
						continue;
					if (method.IsAbstract)
						continue;
					if (ShouldWeAvoid(method.Name))
						continue;

					_stuffToFuzz.Add((MemberInfo)method);
				}

				foreach (MethodBase method in _methods)
				{
					if (method.IsStatic)
						continue;
					if (method.IsAbstract)
						continue;
					if (ShouldWeAvoid(method.Name))
						continue;

					_stuffToFuzz.Add((MemberInfo)method);
				}

				foreach (MemberInfo method in _properties)
				{
					if (ShouldWeAvoid(method.Name))
						continue;
					_stuffToFuzz.Add((MemberInfo)method);
				}

				foreach (MethodBase method in _methods)
				{
					if (!method.IsStatic)
						continue;
					if (method.IsAbstract)
						continue;
					if (ShouldWeAvoid(method.Name))
						continue;

					_stuffToFuzz.Add((MemberInfo)method);
				}
			}
			catch
			{
				string s = "a";
			}

			_stack.Remove(_name);
		}

		protected virtual bool ShouldWeAvoid(string name)
		{
			foreach (string typeName in _avoidTheseTypes)
				if (name == typeName)
					return true;

			return false;
		}

		/// <summary>
		/// Locate the best constructor to use as the default case.  This method can
		/// be overriden to control the default contructor chosen.
		/// </summary>
		/// <returns>Returns default contrictor information</returns>
		protected ConstructorInfo FindDefaultConstructor()
		{
			// First look for a default contructor
			foreach (ConstructorInfo ctor in _ctors)
				if (ctor.GetParameters().Length == 0)
					return ctor;

			// Now lets find all the ones with all basic types as params
			ConstructorInfo ret = null;
			int retParamCount = 10000;

			foreach (ConstructorInfo ctor in _ctors)
			{
				if (AllBasicTypes(ctor.GetParameters()))
				{
					if (ctor.GetParameters().Length < retParamCount)
					{
						ret = ctor;
						retParamCount = ctor.GetParameters().Length;
					}
				}
			}

			if (ret != null)
				return ret;

			// Now lets just find the least number of args
			retParamCount = 10000;
			foreach (ConstructorInfo ctor in _ctors)
			{
				if (ctor.GetParameters().Length < retParamCount)
				{
					ret = ctor;
					retParamCount = ctor.GetParameters().Length;
				}
			}

			return ret;
		}

		protected bool AllBasicTypes(ParameterInfo[] parms)
		{
			foreach (ParameterInfo p in parms)
				if (!IsBasicType(p.ParameterType))
					return false;

			return true;
		}

		protected bool IsBasicType(Type type)
		{
			if(type == typeof(string))
				return true;
			if(type == typeof(int))
				return true;
			if(type == typeof(long))
				return true;
			if(type == typeof(float))
				return true;
			if(type == typeof(bool))
				return true;

			return false;
		}

		#region ITypeGenerator Members

		public static bool SupportedType(Type type)
		{
			if (type.IsSubclassOf(typeof(MethodInfo)) ||
				type.IsSubclassOf(typeof(PropertyInfo)) ||
				type.IsSubclassOf(typeof(EventInfo)) ||
				type == typeof(Object))
				return false;
			
			if (type.IsClass && !type.IsAbstract)
				return true;

			return false;
		}

		public static ITypeGenerator CreateInstance(IContext context, IGroup group, Type type, object [] obj)
		{
			if (_stack.ContainsKey(type.ToString()))
			{
				return _stack[type.ToString()];
			}

			return new ClassGenerator(context, group, type);
		}

		#endregion

		#region IGenerator Members

		public virtual object GetValue()
		{
			if (_position >= _stuffToFuzz.Count)
				return null;

			MemberInfo info = _stuffToFuzz[_position];
			if (_typeGenerators[info] == null)
				return null;

			return _typeGenerators[info].GetValue();
		}

		public virtual void Next()
		{
			if (_position >= _stuffToFuzz.Count)
				throw new GeneratorCompleted();

			try
			{
				MemberInfo info = _stuffToFuzz[_position];
				if (_typeGenerators[info] == null)
					throw new GeneratorCompleted();
				_typeGenerators[info].Next();
			}
			catch (GeneratorCompleted)
			{
				_position++;
				if (_position >= _stuffToFuzz.Count)
				{
					_position--;
					throw new GeneratorCompleted("ClassGenerator for " + _name + " has completed");
				}
				else
				{
					MemberInfo info = _stuffToFuzz[_position];
					Console.WriteLine("ClassGenerator(): Next type: " + info);
				}
			}
		}

		public virtual void Reset()
		{
			_position = 0;

			foreach (IGenerator generator in _typeGenerators.Values)
				generator.Reset();
		}

		#endregion
	}
}
