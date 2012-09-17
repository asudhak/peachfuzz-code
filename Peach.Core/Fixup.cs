
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
using Peach.Core.Dom;

namespace Peach.Core
{
	[Serializable]
	public abstract class Fixup
	{
		protected Dictionary<string, Variant> args;
		protected bool isRecursing = false;
		protected DataElement parent = null;

		public Fixup(DataElement parent, Dictionary<string, Variant> args)
		{
			this.parent = parent;
			this.args = args;
		}

		public Dictionary<string, Variant> arguments
		{
			get { return args; }
			set { args = value; }
		}

		/// <summary>
		/// Perform fixup operation
		/// </summary>
		/// <param name="obj">Parent data element</param>
		/// <returns></returns>
		public Variant fixup(DataElement obj)
		{
			if (isRecursing)
				return obj.DefaultValue;

			try
			{
				isRecursing = true;
				return fixupImpl(obj);
			}
			finally
			{
				isRecursing = false;
			}
		}

		protected abstract Variant fixupImpl(DataElement obj);
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class FixupAttribute : PluginAttribute
	{
		public string description;
		public bool isDefault;

		public FixupAttribute(string name, string description, bool isDefault = false)
			: base(name)
		{
			this.description = description;
			this.isDefault = isDefault;
		}
	}
}

// end
