
/* Copyright (c) 2007 Michael Eddington
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

namespace Peach.Core.Language.DotNet.Generators
{
	public class PropertyGenerator : Generators.Generator, ITypeGenerator
	{
		IContext _context;
		Type _type;
		PropertyInfo _info;
		IGenerator _ctorGenerator;

		Dictionary<MethodInfo, IGenerator> _methodGenerators = new Dictionary<MethodInfo, IGenerator>(2);

		int _position = 0;
		List<MethodInfo> _methods = new List<MethodInfo>(2);

		public PropertyGenerator(IContext context, IGroup group, Type type, object[] objs)
		{
			_context = context;
			Group = group;
			_type = type;
			_info = (PropertyInfo)objs[0];
			_ctorGenerator = (IGenerator)objs[1];

			MethodInfo methodInfo;

			if (_info.CanRead)
			{
				methodInfo = _info.GetGetMethod(true);
				_methods.Add(methodInfo);
				_methodGenerators.Add(methodInfo, _context.GetTypeGenerator(null, methodInfo.GetType(), 
					new object[] { methodInfo }));
			}

			if (_info.CanWrite)
			{
				methodInfo = _info.GetSetMethod(true);
				_methods.Add(methodInfo);
				_methodGenerators.Add(methodInfo, _context.GetTypeGenerator(null, methodInfo.GetType(),
					new object[] { methodInfo }));
			}
		}

		#region ITypeGenerator Members

		public static bool SupportedType(Type type)
		{
			if (type == typeof(PropertyInfo))
				return true;

			return false;
		}

		public static ITypeGenerator CreateInstance(IContext context, IGroup group, Type type, object[] objs)
		{
			return new PropertyGenerator(context, group, type, objs);
		}

		#endregion

		#region IGenerator Members

		public override object GetRawValue()
		{
			MethodInfo info = _methods[_position];
			if (_methodGenerators[info] == null)
				return null;

			try
			{
				return _methodGenerators[info].GetValue();
			}
			catch
			{
				return null;
			}
		}

		public override void Next()
		{
			try
			{
				MethodInfo info = _methods[_position];
				if (_methodGenerators[info] == null)
					throw new GeneratorCompleted();
				_methodGenerators[info].Next();
			}
			catch (GeneratorCompleted)
			{
				_position++;
				if (_position >= _methods.Count)
				{
					_position--;
					throw new GeneratorCompleted("PropertyGenerator has completed");
				}
			}
		}

		public override void Reset()
		{
			_position = 0;

			foreach (IGenerator generator in _methodGenerators.Values)
				generator.Reset();
		}

		#endregion
	}
}
