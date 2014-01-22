
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
using Peach.Core.Analyzers;

namespace Peach.Core.Dom
{
	/// <summary>
	/// DataModel is just a top level Block.
	/// </summary>
	[Serializable]
	[DataElement("DataModel")]
	[DataElementParentSupported(null)]
	[PitParsable("DataModel", topLevel = true)]
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

		public DataModel()
		{
		}

		public DataModel(string name)
			: base(name)
		{
		}

		public static new DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			string name = node.getAttr("name", null);
			string refName = node.getAttr("ref", null);

			DataModel dataModel = null;

			if (refName != null)
			{
				var refObj = context.getReference(refName, parent) as DataModel;
				if (refObj == null)
					throw new PeachException("Error, DataModel {0}could not resolve ref '{1}'. XML:\n{2}".Fmt(
						name == null ? "" : "'" + name + "' ", refName, node.OuterXml));

				if (string.IsNullOrEmpty(name))
					name = refName;

				dataModel = refObj.Clone(name) as DataModel;
				dataModel.isReference = true;
				dataModel.referenceName = refName;
			}
			else
			{
				if (string.IsNullOrEmpty(name))
					throw new PeachException("Error, DataModel missing required 'name' attribute.");

				dataModel = new DataModel(name);
			}

			context.handleCommonDataElementAttributes(node, dataModel);
			context.handleCommonDataElementChildren(node, dataModel);
			context.handleDataElementContainer(node, dataModel);

			return dataModel;
		}
	}
}

// end
