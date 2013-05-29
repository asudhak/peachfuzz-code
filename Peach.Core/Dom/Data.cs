
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
using System.Text.RegularExpressions;
using Peach.Core.Agent;
using System.Runtime.Serialization;

namespace Peach.Core.Dom
{
	/// <summary>
	/// Data specification for a DataModel
	/// </summary>
	[Serializable]
	public class Data : INamed
	{
		static int nameNum = 0;
		string _name = "Unknown Data " + (++nameNum);

		public OrderedDictionary<string, Variant> fields = new OrderedDictionary<string, Variant>();

		public Data()
		{
			DataType = Core.Dom.DataType.Fields;
			FileName = null;
		}

		public DataType DataType { get; set; }
		public List<string> Files = new List<string>();
		public string FileName { get; set; }

		#region INamed Members

		public string name
		{
			get { return _name; }
			set { _name = value; }
		}

		#endregion

		public void ApplyFields(DataElementContainer model)
		{
			// Examples of valid field names:
			//
			//  1. foo
			//  2. foo.bar
			//  3. foo[N].bar[N].foo
			//

			foreach (string field in fields.Keys)
			{
				Variant value = fields[field];
				DataElement elem = model;
				DataElementContainer container = model;
				var names = field.Split('.');

				for(int i = 0; i<names.Length; i++)
				{
					string name = names[i];
					Match m = Regex.Match(name, @"(.*)\[(-?\d+)\]$");

					if (m.Success)
					{
						name = m.Groups[1].Value;
						int index = int.Parse(m.Groups[2].Value);

						if (!container.ContainsKey(name))
							throw new PeachException("Error, unable to resolve field \"" + field + "\" against \"" + model.fullName + "\".");

						var array = container[name] as Array;
						if (array == null)
							throw new PeachException("Error, cannot use array index syntax on field name unless target element is an array. Field: " + field);

						// Are we disabling this array?
						if (index == -1)
						{
							if (array.minOccurs > 0)
								throw new PeachException("Error, cannot set array to zero elements when minOccurs > 0. Field: " + field + " Element: " + array.fullName);

							// Remove all children
							array.Clear();
							return;
						}

						if (array.maxOccurs != -1 && index > array.maxOccurs)
							throw new PeachException("Error, index larger that maxOccurs.  Field: " + field + " Element: " + array.fullName);

						if (!array.hasExpanded && array.origionalElement == null)
						{
							array.origionalElement = array[0];
							array.RemoveAt(0);
						}

						// Add elements upto our index
						for (int x = array.Count; x <= index; x++)
						{
							string itemName = array.origionalElement.name + "_" + x;
							var item = array.origionalElement.Clone(itemName);
							array.Add(item);
						}

						array.hasExpanded = true;
						elem = array[index];
						container = elem as DataElementContainer;
					}
					else if (container is Choice)
					{
						elem = null;
						var choice = container as Choice;
						if(!choice.choiceElements.TryGetValue(name, out elem))
							throw new PeachException("Error, unable to resolve field \"" + field + "\" against \"" + model.fullName + "\".");

						container = elem as DataElementContainer;

						choice.SelectedElement = elem;
					}
					else
					{
						if(!container.ContainsKey(name))
							throw new PeachException("Error, unable to resolve field \"" + field + "\" against \"" + model.fullName + "\".");

						elem = container[name];
						container = elem as DataElementContainer;
					}
				}

				if (!(elem is DataElementContainer))
					elem.DefaultValue = value;
			}
		}
	}

	/// <summary>
	/// Type of Data
	/// </summary>
	public enum DataType
	{
		Fields,
		File,
		Files
	}
}


// END
