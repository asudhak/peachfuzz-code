
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
using Peach.Core;
using System.Xml.Serialization;

namespace Peach.Core.Dom
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class DataElementAttribute : Attribute
	{
		public string elementName;

		public DataElementAttribute(string elementName)
		{
			this.elementName = elementName;
		}
	}

  public enum DataElementTypes
	{
		Any,
		Containers,
		NonContainers,
		NonDataElements,
		Parameter,
		Relation,
		Transformer,
		Fixup,
		Hint
	}

	public enum DataElementRelations
	{
    [System.ComponentModel.Browsable(false)]
    [XmlIgnore]
		Any,
    [XmlEnum("size")]
		Size,
    [XmlEnum("count")]
		Count,
    [XmlEnum("offset")]
		Offset
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class DataElementChildSupportedAttribute : Attribute
	{
		public DataElementChildSupportedAttribute(string elementName)
		{
		}
		public DataElementChildSupportedAttribute(DataElementTypes type)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class DataElementRelationSupportedAttribute : Attribute
	{
		public DataElementRelationSupportedAttribute(DataElementRelations type)
		{
		}
	}
}
