
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

using Peach.Core.Analyzers;
using Peach.Core.IO;
using Peach.Core.Cracker;

using NLog;

namespace Peach.Core.Dom
{
	/// <summary>
	/// Choice allows the selection of a single
	/// data element based on the current data set.
	/// 
	/// The other options in the choice are available
	/// for mutation by the mutators.
	/// </summary>
	[DataElement("Choice")]
	[PitParsable("Choice")]
	[DataElementChildSupported(DataElementTypes.Any)]
	[Parameter("name", typeof(string), "", "")]
	[Serializable]
	public class Choice : DataElementContainer
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		public OrderedDictionary<string, DataElement> choiceElements = new OrderedDictionary<string, DataElement>();
		DataElement _selectedElement = null;

		public Choice()
		{
		}

		public Choice(string name)
			: base(name)
		{
		}

		public override void Crack(DataCracker context, BitStream data)
		{
			Choice element = this;

			logger.Trace("Crack: {0} data.TellBits: {1}", element.fullName, data.TellBits());

			BitStream sizedData = data;
			SizeRelation sizeRelation = null;

			// Do we have relations or a length?
			if (element.relations.hasOfSizeRelation)
			{
				sizeRelation = element.relations.getOfSizeRelation();

				if (!element.isParentOf(sizeRelation.From))
				{
					int size = (int)sizeRelation.GetValue();
					context._sizedBlockStack.Add(element);
					context._sizedBlockMap[element] = size;

					sizedData = new BitStream(data.ReadBytes(size));
					sizeRelation = null;
				}
			}
			else if (element.hasLength)
			{
				long size = element.lengthAsBits;
				context._sizedBlockStack.Add(element);
				context._sizedBlockMap[element] = size;

				sizedData = new BitStream(data.ReadBytes(size));
			}

			long startPosition = sizedData.TellBits();
			bool foundElement = false;

			foreach (DataElement child in element.choiceElements.Values)
			{
				try
				{
					logger.Debug("handleChoice: Trying next child: " + child.fullName);

					child.parent = element;
					sizedData.SeekBits(startPosition, System.IO.SeekOrigin.Begin);
					context.handleNode(child, sizedData);
					element.SelectedElement = child;
					foundElement = true;

					logger.Debug("handleChoice: Keeping child!");
					break;
				}
				catch (CrackingFailure)
				{
					logger.Debug("handleChoice: Child failed to crack: " + child.fullName);
					foundElement = false;
				}
				catch (Exception ex)
				{
					logger.Debug("handleChoice: Child threw exception: " + child.fullName + ": " + ex.Message);
				}
			}

			if (!foundElement)
				throw new CrackingFailure("Unable to crack '" + element.fullName + "'.", element, data);
		}

		public void SelectDefault()
		{
			this.Clear();
			this.Add(choiceElements[0]);
			_selectedElement = this[0];
		}

		public static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			if (node.Name != "Choice")
				return null;

			Choice choice = DataElement.Generate<Choice>(node);

			context.handleCommonDataElementAttributes(node, choice);
			context.handleCommonDataElementChildren(node, choice);
			context.handleDataElementContainer(node, choice);

			// Move children to choiceElements collection
			foreach (DataElement elem in choice)
			{
				choice.choiceElements.Add(elem.name, elem);
				elem.parent = choice;
			}

			choice.Clear(false);

			return choice;
		}

		public DataElement SelectedElement
		{
			get
			{
				//if (_selectedElement == null && choiceElements.Count > 0)
				//{
				//    this.Clear();
				//    this.Add(choiceElements[0]);
				//    _selectedElement = this[0];
				//}

				return _selectedElement;
			}
			set
			{
				if (!choiceElements.Values.Contains(value))
					throw new KeyNotFoundException("value was not found");

				this.Clear();
				this.Add(value);
				_selectedElement = value;
				Invalidate();
			}
		}

		public override IEnumerable<DataElement> EnumerateAllElements(List<DataElement> knownParents)
		{
			// First our children
			foreach (DataElement child in this)
				yield return child;

			// Next our children's children
			foreach (DataElement child in this)
			{
				if (!knownParents.Contains(child))
				{
					foreach (DataElement subChild in child.EnumerateAllElements(knownParents))
						yield return subChild;
				}
			}

			if (_selectedElement == null)
			{
				foreach (DataElement child in choiceElements.Values)
					yield return child;

				// Next our children's children
				foreach (DataElement child in choiceElements.Values)
				{
					if (!knownParents.Contains(child))
					{
						foreach (DataElement subChild in child.EnumerateAllElements(knownParents))
							yield return subChild;
					}
				}
			}
		}

		public override Variant GenerateInternalValue()
		{
			Variant value;

			// 1. Default value

			if (_selectedElement == null)
				SelectDefault();

			if (_mutatedValue == null)
				value = new Variant(SelectedElement.Value);

			else
				value = MutatedValue;

			// 2. Relations

			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_RELATIONS) != 0)
			{
				return MutatedValue;
			}

			foreach (Relation r in _relations)
			{
				if (IsFromRelation(r))
				{
					// CalculateFromValue can return null sometimes
					// when mutations mess up the relation.
					// In that case use the exsiting value for this element.

					var relationValue = r.CalculateFromValue();
					if (relationValue != null)
						value = relationValue;
				}
			}

			// 3. Fixup

			if (_mutatedValue != null && (mutationFlags & MUTATE_OVERRIDE_FIXUP) != 0)
			{
				return MutatedValue;
			}

			if (_fixup != null)
				value = _fixup.fixup(this);

			return value;
		}

    public override object GetParameter(string parameterName)
    {
      switch (parameterName)
      {
        case "name":
          return this.name;
        default:
          throw new PeachException(System.String.Format("Parameter '{0}' does not exist in Peach.Core.Dom.Choice", parameterName));
      }
    }
	}
}

// end
