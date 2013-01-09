
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
	public class DataModel : Block, IPitSerializable
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

		public DataModel()
		{
		}

		public DataModel(string name)
			: base(name)
		{
		}

		public override DataElement Clone()
		{
			if (cache == null)
				cache = new CloneCache(this, this.name);

			var ret = cache.Get() as DataModel;
			ret.cache = this.cache;

			return ret;
		}

		public override void Crack(Cracker.DataCracker context, IO.BitStream data)
		{
			cache = null;
			base.Crack(context, data);
		}

    public System.Xml.XmlNode pitSerialize(System.Xml.XmlDocument doc, System.Xml.XmlNode parent)
    {
      XmlNode node = doc.CreateNode(XmlNodeType.Element, "DataModel", null);

      node.AppendAttribute("name", this.name);
      //minOccurs, maxOccurs, occurs, ref, constraint, mutable, pointer, pointerDepth
      
      
      foreach (DataElement dataElement in this._childrenList)
      {
        Type elementType = dataElement.GetType();
        List<object> attribs = new List<object>(elementType.GetCustomAttributes(false));

        DataElementAttribute dataElementAttrib = (from o in attribs where o is DataElementAttribute select o).First() as DataElementAttribute;

        XmlNode eDataElement = doc.CreateNode(XmlNodeType.Element, dataElementAttrib.elementName, null);

        List<object> parameterAttributes = (from o in attribs where o is ParameterAttribute select o).ToList();

        foreach (object attrib in parameterAttributes)
        {
          ParameterAttribute parameterAttribute = (ParameterAttribute)attrib;
          try
          {
            object propertyValue = dataElement.GetParameter(parameterAttribute.name);
            eDataElement.AppendAttribute(parameterAttribute.name, propertyValue.ToString());
          }
          catch (Exception ex)
          {
            throw ex;
          }
        }
        node.AppendChild(eDataElement);
        
      }

      foreach (Relation relation in this.relations)
      {
        node.AppendChild(relation.pitSerialize(doc, node));
      }

      if (this.transformer != null)
      {
        Transformer currentTransformer = this.transformer;
        XmlNode eTransformer = doc.CreateElement("Transformer", null);
        while (currentTransformer != null)
        {
          List<object> attribs = new List<object>(currentTransformer.GetType().GetCustomAttributes(false));

          TransformerAttribute transformerAttrib = (from o in attribs where (o is TransformerAttribute) && ((TransformerAttribute)o).IsDefault select o).First() as TransformerAttribute;
          eTransformer.AppendAttribute("class", transformerAttrib.Name);

          if (currentTransformer.anotherTransformer != null)
          {
            currentTransformer = currentTransformer.anotherTransformer;
          }
          else
          {
            break;
          }
        }
        node.AppendChild(eTransformer);
      }

      foreach (Hint hint in this.Hints.Values)
      {
        XmlNode eHint = doc.CreateElement("Hint");
        eHint.AppendAttribute("name", hint.Name);
        eHint.AppendAttribute("value", hint.Value);
        node.AppendChild(eHint);
      }

      if (placement != null)
      {
        XmlNode ePlacement = doc.CreateElement("Placement");
        ePlacement.AppendAttribute("after", this.placement.after);
        ePlacement.AppendAttribute("before", this.placement.before);
        node.AppendChild(ePlacement);
      }

      return node;
    }
  }
}

// end
