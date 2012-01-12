
/* Copyright (c) 2009 Michael Eddington
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
	/// <summary>
	/// Object generator allows us to seed the ClassGenerator with an
	/// object instance that will be used to fuzz methods/parameters.
	/// </summary>
	public class ObjectGenerator: ClassGenerator
	{
		protected static object _target;

		protected ObjectGenerator(IContext context, IGroup group, Type type) : base(context, group, type)
		{
		}

		public object ObjectInstance
		{
			get { return _target; }
			set
			{
				if (_target != null && value.GetType() != _target.GetType())
					throw new ApplicationException("Invalid parameter to ObjectInstance.  Type change not allowed.");

				Reset();
				_target = value;
			}
		}

		#region ITypeGenerator Members

		public static bool SupportedType(Type type)
		{
			// TODO: Make this correct!

			return false;

			//if (type.IsSubclassOf(typeof(MethodInfo)) ||
			//    type.IsSubclassOf(typeof(PropertyInfo)) ||
			//    type.IsSubclassOf(typeof(EventInfo)) ||
			//    type == typeof(Object))
			//    return false;

			//if (type.IsClass && !type.IsAbstract)
			//    return true;

			//return false;
		}

		public static ITypeGenerator CreateInstance(IContext context, IGroup group, Type type, object[] obj)
		{
			return new ObjectGenerator(context, group, type);
		}

		#endregion
	}
}
