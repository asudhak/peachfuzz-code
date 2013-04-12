
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
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

using System.Linq;

namespace Peach.Core.Dom
{
	/// <summary>
	/// DataModel is just a top level Block.
	/// </summary>
	[Serializable]
	[DataElement("DataModel")]
	[PitParsable("DataModel")]
	[Parameter("name", typeof(string), "Model name", "")]
	[Parameter("ref", typeof(string), "Model to reference", "")]
	public class DataModel : Block
	{
		/// <summary>
		/// Dom parent of data model if any
		/// </summary>
		/// <remarks>
		/// A data model can be the child of two (okay three) different types,
		///   1. Dom (dom.datamodel collection)
		///   2. Action (Action.dataModel)
		///   3. ActionParam (Action.parameters[0].dataModel)
		///   
		/// This variable is one of those parent holders.
		/// </remarks>
		[NonSerialized]
		public Dom dom = null;

		/// <summary>
		/// Action parent of data model if any
		/// </summary>
		/// <remarks>
		/// A data model can be the child of two (okay three) different types,
		///   1. Dom (dom.datamodel collection)
		///   2. Action (Action.dataModel)
		///   3. ActionParam (Action.parameters[0].dataModel)
		///   
		/// This variable is one of those parent holders.
		/// </remarks>
		[NonSerialized]
		public Action action = null;

		[NonSerialized]
		private CloneCache cache = null;

		[NonSerialized]
		private bool cracking = false;

		public DataModel()
		{
			this.Invalidated += new InvalidatedEventHandler(DataModel_Invalidated);
		}

		public DataModel(string name)
			: base(name)
		{
			this.Invalidated += new InvalidatedEventHandler(DataModel_Invalidated);
		}

		[OnDeserialized]
		void OnDeserialized(StreamingContext context)
		{
			this.Invalidated += new InvalidatedEventHandler(DataModel_Invalidated);
		}

		void  DataModel_Invalidated(object sender, EventArgs e)
		{
			cache = null;
		}

		public override DataElement Clone()
		{
			if (cracking)
				return new CloneCache(this, this.name).Get();

			if (cache == null)
				cache = new CloneCache(this, this.name);

			var ret = cache.Get() as DataModel;
			ret.cache = this.cache;

			return ret;
		}

		public override void Crack(Cracker.DataCracker context, IO.BitStream data, long? size)
		{
			try
			{
				cache = null;
				cracking = true;
				base.Crack(context, data, size);
			}
			finally
			{
				cracking = false;
			}
		}
	}
}

// end
