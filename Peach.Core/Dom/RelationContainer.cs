
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
using System.Diagnostics;
using System.Linq;

namespace Peach.Core.Dom
{
	[Serializable]
	public class RelationContainer : List<Binding>
	{
		private DataElement parent;

		public RelationContainer(DataElement parent)
		{
			this.parent = parent;
		}

		public IEnumerable<T> Of<T>() where T : Binding
		{
			VerifyBinding();

			foreach (var item in this)
			{
				var ret = item as T;
				if (ret != null && ret.Of == parent)
					yield return ret;
			}
		}

		public IEnumerable<T> From<T>() where T : Binding
		{
			VerifyBinding();

			foreach (var item in this)
			{
				var ret = item as T;
				if (ret != null && ret.From == parent)
					yield return ret;
			}
		}

		public bool HasOf<T>() where T : Binding
		{
			return Of<T>().Any();
		}

		public bool HasFrom<T>() where T : Binding
		{
			return From<T>().Any();
		}

		[Conditional("DEBUG")]
		private void VerifyBinding()
		{
		}

#if DISABLED
		private static string FmtMessage(Relation r, DataElement obj, string who)
		{
			return string.Format("Relation Of=\"{0}\" From=\"{1}\" not {2}element \"{3}\"",
					r.Of.fullName, r.From.fullName, who, obj.fullName);
		}

		private bool ContainsNamedRelation(Relation r)
		{
			string fullFrom = r.From.fullName;
			string fullOf = r.Of.fullName;

			foreach (var item in _relations)
			{
				if (fullOf == item.Of.fullName && fullFrom == item.From.fullName)
					return true;
			}

			return false;
		}

		protected bool IsFromRelation(Relation r)
		{
#if DEBUG
			if (!_relations.Contains(r))
				throw new ArgumentException(FmtMessage(r, this, "referenced by "));

			if (r.From != null && r.From.parent == null)
				throw new PeachException(FmtMessage(r, r.From, "valid parent in from="));

			if (r.Of == null)
				return r.From == this;

			// r.Of.parent can be null if r.Of is the data model

			if (!r.From.ContainsNamedRelation(r))
				throw new PeachException(FmtMessage(r, r.From, "referenced in from="));

			if (!r.Of.ContainsNamedRelation(r))
				throw new PeachException(FmtMessage(r, r.Of, "contained in of="));

			if (!r.From.relations.Contains(r))
				throw new PeachException(FmtMessage(r, r.From, "contained in from="));

			if (!r.Of.relations.Contains(r))
				throw new PeachException(FmtMessage(r, r.Of, "referenced in of="));

			bool notFromStr = r.From.fullName != this.fullName;
			bool notOfStr = r.Of.fullName != this.fullName;

			if (notOfStr == notFromStr)
				throw new PeachException(FmtMessage(r, this, "named from or of="));

			bool notFrom = r.From != this;
			bool notOf = r.Of != this;

			if (notOf == notFrom)
				throw new PeachException(FmtMessage(r, this, "from or of="));
#endif
			return r.From == this;
		}

		public void VerifyRelations()
		{
#if DEBUG
			foreach (var r in _relations)
				IsFromRelation(r);

			DataElementContainer cont = this as DataElementContainer;
			if (cont == null)
				return;

			foreach (var c in cont)
				c.VerifyRelations();
#endif
		}
#endif
	}
}

// end
