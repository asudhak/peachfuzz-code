
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
using Peach.Core.Dom;
using System.Runtime.Serialization;

namespace Peach.Core
{
	[Serializable]
	public abstract class Fixup
	{
		protected Dictionary<string, Variant> args;
		protected bool isRecursing = false;
		protected DataElement parent = null;
		protected Dictionary<string, DataElement> elements = null;

		// Needed for re-subscribing the Invalidated event on deserialize
		private List<DataElement> elementList = new List<DataElement>();
		private string[] refs;

		public Fixup(DataElement parent, Dictionary<string, Variant> args, params string[] refs)
		{
			this.parent = parent;
			this.args = args;
			this.refs = refs;

			if (!refs.SequenceEqual(refs.Intersect(args.Keys)))
			{
				string msg = string.Format("Error, {0} requires a '{1}' argument!",
					this.GetType().Name,
					string.Join("' AND '", refs));

				throw new PeachException(msg);
			}
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
				return doFixupImpl(obj);
			}
			finally
			{
				isRecursing = false;
			}
		}

		private Variant doFixupImpl(DataElement obj)
		{
			System.Diagnostics.Debug.Assert(parent != null);

			if (elements == null)
			{
				elements = new Dictionary<string,DataElement>();

				foreach (var refName in refs)
				{
					string elemName = (string)args[refName];

					var elem = obj.find(elemName);
					if (elem == null)
						throw new PeachException(string.Format("{0} could not find ref element '{1}'", this.GetType().Name, elemName));

					elem.Invalidated += new InvalidatedEventHandler(OnInvalidated);
					elements.Add(refName, elem);
					elementList.Add(elem);
				}
			}

			return fixupImpl(obj);
		}

		private void OnInvalidated(object sender, EventArgs e)
		{
			parent.Invalidate();
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			DataElement.CloneContext ctx = context.Context as DataElement.CloneContext;
			if (ctx == null)
				return;

			// DataElement.Invalidated is not serialized, so re-subscribe to the event
			// Can't use the Dictionary, must use a list
			// See: http://stackoverflow.com/questions/457134/strange-behaviour-of-net-binary-serialization-on-dictionarykey-value
			foreach (var item in elementList)
				item.Invalidated += new InvalidatedEventHandler(OnInvalidated);
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
