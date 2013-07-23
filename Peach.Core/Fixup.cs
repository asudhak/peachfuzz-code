
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

		private Dictionary<string, string> refs = new Dictionary<string,string>();

		/// <summary>
		/// Returns mapping of ref key to ref value, eg: ("ref1", "DataModel.Emenent_0")
		/// </summary>
		public IEnumerable<Tuple<string, string>> references
		{
			get
			{
				foreach (var item in refs)
				{
					yield return new Tuple<string, string>(item.Key, item.Value);
				}
			}
		}

		public IEnumerable<DataElement> dependents
		{
			get
			{
				if (elements != null)
				{
					foreach (var kv in elements)
					{
						yield return kv.Value;
					}
				}
			}
		}

		public Fixup(DataElement parent, Dictionary<string, Variant> args, params string[] refs)
		{
			this.parent = parent;
			this.args = args;

			if (!refs.SequenceEqual(refs.Intersect(args.Keys)))
			{
				string msg = string.Format("Error, {0} requires a '{1}' argument!",
					this.GetType().Name,
					string.Join("' AND '", refs));

				throw new PeachException(msg);
			}

			foreach (var item in refs)
				this.refs.Add(item, (string)args[item]);
		}

		public void updateRef(string refKey, string refValue)
		{
			refs[refKey] = refValue;

			if (elements != null)
			{
				DataElement elem;
				if (elements.TryGetValue(refKey, out elem))
					elem.Invalidated -= OnInvalidated;

				elem = parent.find(refValue);
				if (elem == null)
					throw new PeachException(string.Format("{0} could not find ref element '{1}'", this.GetType().Name, refValue));

				elem.Invalidated += new InvalidatedEventHandler(OnInvalidated);
				elements[refKey] = elem;
			}
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
				elements = new Dictionary<string, DataElement>();

				foreach (var kv in refs)
				{
					var elem = obj.find(kv.Value);
					if (elem == null)
						throw new PeachException(string.Format("{0} could not find ref element '{1}'", this.GetType().Name, kv.Value));

					elem.Invalidated += new InvalidatedEventHandler(OnInvalidated);
					elements.Add(kv.Key, elem);
				}
			}

			return fixupImpl();
		}

		private void OnInvalidated(object sender, EventArgs e)
		{
			parent.Invalidate();
		}

		[OnCloned]
		private void OnCloned(Fixup original, object context)
		{
			if (elements != null)
			{
				foreach (var kv in elements)
				{
					// DataElement.Invalidated is not serialized, so register for a re-subscribe to the event
					kv.Value.Invalidated += new InvalidatedEventHandler(OnInvalidated);
				}
			}

			DataElement.CloneContext ctx = context as DataElement.CloneContext;
			if (ctx != null)
			{
				var toUpdate = new Dictionary<string, string>();

				// Find all ref='xxx' values where the name should be changed to ref='yyy'
				foreach (var kv in original.refs)
				{
					DataElement elem;
					if (original.elements == null || !original.elements.TryGetValue(kv.Key, out elem))
						elem = null;
					else if (elem != ctx.root.getRoot() && !elem.isChildOf(ctx.root.getRoot()))
						continue; // ref'd element was removed by a mutator

					string name = ctx.UpdateRefName(original.parent, elem, kv.Value);
					if (name != kv.Value)
						toUpdate.Add(kv.Key, name);
				}

				foreach (var kv in toUpdate)
				{
					updateRef(kv.Key, kv.Value);
				}
			}
		}

		protected abstract Variant fixupImpl();
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class FixupAttribute : PluginAttribute
	{
		public FixupAttribute(string name, bool isDefault = false)
			: base(typeof(Fixup), name, isDefault)
		{
		}
	}
}

// end
