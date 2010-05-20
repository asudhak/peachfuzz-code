
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
using System.Collections;
using System.Text;
using System.Diagnostics;
using Peach;
using Peach.DotNetFuzzer;

namespace Peach.DotNetFuzzer.Generators
{
	public class SystemTypesGenerator : Generator, ITypeGenerator
	{
		#region IGenerator Members

		public override object GetRawValue()
		{
			return null;
		}

		public override void Next()
		{
			throw new GeneratorCompleted();
		}

		public override void Reset()
		{
		}

		#endregion
	}

	public class StringGenerator : SystemTypesGenerator
	{
		int _position = 0;

		StringGenerator(IContext context, IGroup group, Type type)
		{

		}

		#region ITypeGenerator Members

		public static bool SupportedType(Type type)
		{
			if (type == typeof(string))
			{
				Debug.WriteLine("StringGenerator.SupportedType(): true");
				return true;
			}

			return false;
		}

		public static ITypeGenerator CreateInstance(IContext context, IGroup group, Type type, object [] obj)
		{
			return (ITypeGenerator) new StringGenerator(context, group, type);
		}

		#endregion

		public override object GetRawValue()
		{
			return PeachData.badStrings[_position];
		}

		public override void Next()
		{
			_position++;
			if (_position >= PeachData.badStrings.Length)
			{
				_position--;
				throw new GeneratorCompleted();
			}
		}

		public override void Reset()
		{
			_position = 0;
		}
	}

	public class NullGenerator : SystemTypesGenerator
	{
		#region ITypeGenerator Members

		public static bool SupportedType(Type type)
		{
			if (type == null)
				return true;

			return false;
		}

		public static ITypeGenerator CreateInstance(IContext context, IGroup group, Type type, object[] obj)
		{
			return (ITypeGenerator)new NullGenerator();
		}

		#endregion

		#region IGenerator Members

		public override object GetRawValue()
		{
			return null;
		}

		#endregion
	}

	public class NumberGenerator : SystemTypesGenerator
	{
		int _position = 0;
		Type _type;

		NumberGenerator(IContext context, IGroup group, Type type)
		{
			_type = type;
		}

		#region ITypeGenerator Members

		public static bool SupportedType(Type type)
		{
			if (type == typeof(int) ||
				type == typeof(long) ||
				type == typeof(float) ||
				type == typeof(uint) || 
				type == typeof(uint) || 
				type == typeof(ulong) ||
				type == typeof(UInt16) ||
				type == typeof(UInt32) ||
				type == typeof(UInt64) ||
				type == typeof(Int16) ||
				type == typeof(Int32) ||
				type == typeof(Int64) ||
				type.Name == "UInt16&" ||
				type.Name == "UInt32&" ||
				type.Name == "UInt64&" ||
				type.Name == "Int16&" ||
				type.Name == "Int32&" ||
				type.Name == "Int64&"
				)
				return true;

			return false;
		}

		public static ITypeGenerator CreateInstance(IContext context, IGroup group, Type type, object[] obj)
		{
			return (ITypeGenerator)new NumberGenerator(context, group, type);
		}

		#endregion

		#region IGenerator Members

		public override object GetRawValue()
		{
			return PeachData.badNumbers[_position];
		}

		public override void Next()
		{
			_position++;
			if (_position >= PeachData.badNumbers.Length)
			{
				_position--;
				throw new GeneratorCompleted("NumberGenerator");
			}
		}

		public override void Reset()
		{
			_position = 0;
		}

		#endregion
	}

	public class BoolGenerator : SystemTypesGenerator
	{
		enum State { True, False, Completed };
		State _state = State.True;

		#region ITypeGenerator Members

		public static bool SupportedType(Type type)
		{
			if (type == typeof(bool))
				return true;

			return false;
		}

		public static ITypeGenerator CreateInstance(IContext context, IGroup group, Type type, object[] obj)
		{
			return (ITypeGenerator)new BoolGenerator();
		}

		#endregion

		public override object GetRawValue()
		{
			if( _state == State.False )
				return false;

			return true;
		}

		public override void Reset()
		{
			_state = State.True;
		}

		public override void Next()
		{
			switch (_state)
			{
				case State.True:
					_state = State.False;
					break;
				case State.False:
				default:
					_state = State.Completed;
					throw new GeneratorCompleted("BoolGenerator");
			}
		}

	}

	public class GuidGenerator : SystemTypesGenerator
	{
		Guid _guid = Guid.Empty;

		#region ITypeGenerator Members

		public static bool SupportedType(Type type)
		{
			if (type == typeof(Guid) ||
				type.Name == "Guid&")
				return true;

			return false;
		}

		public static ITypeGenerator CreateInstance(IContext context, IGroup group, Type type, object[] obj)
		{
			return (ITypeGenerator)new GuidGenerator();
		}

		#endregion

		public override object GetRawValue()
		{
			if (_guid == Guid.Empty)
				_guid = Guid.NewGuid();

			return _guid;
		}

		public override void Next()
		{
			_guid = Guid.NewGuid();
			throw new GeneratorCompleted("GuidGenerator");
		}
	}

	public class ArrayGenerator : SystemTypesGenerator
	{
		IGenerator _elementGenerator;
		Type _type;
		object _array;

		public ArrayGenerator(IContext context, IGroup group, Type type, object[] obj)
			: base()
		{
			_type = type;
			_elementGenerator = context.GetTypeGenerator(null, type.GetElementType(), null);
			//_elementGenerator = context.GetTypeGenerator(null,
			//	Type.GetType(type.FullName.Substring(0, type.FullName.LastIndexOf('['))), null);

			_array = Activator.CreateInstance(_type, new object[] { 0 });
		}

		#region ITypeGenerator Members

		public static bool SupportedType(Type type)
		{
			if (type.IsArray)
				return true;
			
			return false;
		}

		public static ITypeGenerator CreateInstance(IContext context, IGroup group, Type type, object[] obj)
		{
			return (ITypeGenerator)new ArrayGenerator(context, group, type, obj);
		}

		#endregion

		public override object GetRawValue()
		{
			return _array;
		}

		public override void Next()
		{
			throw new GeneratorCompleted("GuidGenerator");
		}

	}
}

// end
