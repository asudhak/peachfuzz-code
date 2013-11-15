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
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

using Peach.Core.Cracker;
using Peach.Core.IO;

namespace Peach.Core.Dom
{
	/// <summary>
	/// Interface for Data
	/// </summary>
	public interface Data : INamed
	{
		/// <summary>
		/// Applies the Data to the specified data model
		/// </summary>
		/// <param name="model"></param>
		void Apply(DataModel model);
	}

	/// <summary>
	/// Data that comes from a file
	/// </summary>
	[Serializable]
	public class DataFile : Data
	{
		public DataFile(DataSet dataSet, string fileName)
		{
			name = "{0}/{1}".Fmt(dataSet.name, Path.GetFileName(fileName));
			FileName = fileName;
		}

		public void Apply(DataModel model)
		{
			try
			{
				DataCracker cracker = new DataCracker();
				cracker.CrackData(model, new BitStream(File.OpenRead(FileName)));
			}
			catch (Cracker.CrackingFailure ex)
			{
				throw new PeachException("Error, failed to crack \"" + FileName +
					"\" into \"" + model.fullName + "\": " + ex.Message, ex);
			}
		}

		public string name
		{
			get;
			private set;
		}

		public string FileName
		{
			get;
			private set;
		}
	}

	/// <summary>
	/// Data that comes from fields
	/// </summary>
	[Serializable]
	public class DataField : Data
	{
		[Serializable]
		public class Field
		{
			public string Name { get; set; }
			public Variant Value { get; set; }
		}

		[Serializable]
		public class FieldCollection : KeyedCollection<string, Field>
		{
			protected override string GetKeyForItem(Field item)
			{
				return item.Name;
			}
		}

		public DataField(DataSet dataSet)
		{
			name = dataSet.name;
			Fields = new FieldCollection();
		}

		public string name
		{
			get;
			private set;
		}

		public FieldCollection Fields
		{
			get;
			private set;
		}

		public void Apply(DataModel model)
		{
			// Examples of valid field names:
			//
			//  1. foo
			//  2. foo.bar
			//  3. foo[N].bar[N].foo
			//

			foreach (var kv in Fields)
			{
				ApplyField(model, kv.Name, kv.Value);
			}

			model.evaulateAnalyzers();
		}

		static void ApplyField(DataElementContainer model, string field, Variant value)
		{
			DataElement elem = model;
			DataElementContainer container = model;
			var names = field.Split('.');

			for (int i = 0; i < names.Length; i++)
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
					if (!choice.choiceElements.TryGetValue(name, out elem))
						throw new PeachException("Error, unable to resolve field \"" + field + "\" against \"" + model.fullName + "\".");

					container = elem as DataElementContainer;

					choice.SelectedElement = elem;
				}
				else
				{
					if (!container.ContainsKey(name))
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

