
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
	/// Array of data elements.  Can be
	/// zero or more elements.
	/// </summary>
	[DataElement("Array")]
	[PitParsable("Array")]
	[DataElementChildSupported(DataElementTypes.Any)]
	[DataElementRelationSupported(DataElementRelations.Any)]
	[Parameter("minOccurs", typeof(int), "Minimum number of occurrences 0-N", false)]
	[Parameter("maxOccurs", typeof(int), "Maximum number of occurrences (-1 for unlimited)", false)]
	[Serializable]
	public class Array : Block
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		public int minOccurs = 1;
		public int maxOccurs = 1;
		public int occurs = 1;

		public bool hasExpanded = false;
		public int? overrideCount = null;

		public DataElement origionalElement = null;

		public override string name
		{
			get { return _name; }
			set
			{
				_name = value;

				if (this.Count > 0)
					this[0].name = value;
			}
		}

		public override void Crack(DataCracker context, BitStream data)
		{
			Array element = this;

			logger.Debug("Crack: {0} data.TellBits: {1}", element.fullName, data.TellBits());
			logger.Debug("Crack: {0} type: {1}", element.fullName, element[0].GetType());

			element.origionalElement = element[0];
			element.Clear();

			if (element.relations.hasOfCountRelation || (minOccurs == 1 && maxOccurs == 1))
			{
				long count = element.relations.hasOfCountRelation ? element.relations.getOfCountRelation().GetValue() : occurs;

				logger.Debug("Crack: {0} found count relation/occurs. Count = {1}", element.fullName, count);

				if (count < 0)
					throw new CrackingFailure("Unable to crack Array '" + element.fullName + "'. Count relation negative: " + count, element, data);
				if (((count > maxOccurs && maxOccurs != -1) || count < minOccurs) && count != occurs)
					throw new CrackingFailure("Unable to crack Array '" + element.fullName + "'. Count outside of bounds of minOccurs='" +
						minOccurs + "' and maxOccurs='" + maxOccurs + "'. (Count = " + count + ")", element, data);

				for (int i = 0; i < count; i++)
				{
					logger.Debug("Crack: ======================");
					logger.Debug("Crack: {0} Trying #{1}", element, i.ToString());


					DataElement clone = ObjectCopier.Clone<DataElement>(element.origionalElement);
					clone.name = clone.name + "_" + i.ToString();
					clone.parent = element;
					element.Add(clone);
					Peach.Core.Cracker.DataCracker.ClearRelationsRecursively(clone);

					try
					{
						context.handleNode(clone, data);
					}
					catch
					{
						logger.Debug("Crack: {0} Failed on #{1}", element.fullName, i.ToString());
						throw;
					}
				}

				logger.Debug("Crack: {0} Done!", element.fullName);
			}

			else if (maxOccurs > 1 || maxOccurs == -1)
			{
				int cnt = 0;
				for (cnt = 0; maxOccurs == -1 || cnt < maxOccurs; cnt++)
				{
					logger.Debug("Crack: ======================");
					logger.Debug("Crack: {0} Trying #{1}", element.fullName, cnt.ToString());

					long pos = data.TellBits();
					DataElement clone = ObjectCopier.Clone<DataElement>(element.origionalElement);
					clone.name = clone.name + "_" + cnt.ToString();
					clone.parent = element;
					element.Add(clone);
					Peach.Core.Cracker.DataCracker.ClearRelationsRecursively(clone);

					try
					{
						context.handleNode(clone, data);
					}
					catch
					{
						logger.Debug("Crack: {0} Failed on #{1}", element.fullName, cnt.ToString());
						element.Remove(clone);
						data.SeekBits(pos, System.IO.SeekOrigin.Begin);
						break;
					}

					if (cnt == 0 && minOccurs == 0 && !context.lookAhead(this, data))
					{
						// Broke our look ahead, must be only zero elements in this array.
						logger.Debug("Crack: {0}, minOccurs = 0, our look ahead failed, must be zero elements in this array.",
							element.fullName, cnt.ToString());

						element.Remove(clone);
						data.SeekBits(pos, System.IO.SeekOrigin.Begin);

						break;
					}

					if (data.TellBits() == data.LengthBits)
					{
						logger.Debug("Crack: {0} Found EOF, all done!", element.fullName);
						// Include this successful crack in the count
						cnt++;
						break;
					}
				}

				if (cnt < minOccurs)
				{
					throw new CrackingFailure(
						string.Format("Crack: {0} Failed on #{1}. Not enough data to meet minOccurs value of {2}", element.fullName, cnt.ToString(), minOccurs),
						element, data);
				}
			}
		}

		public new static DataElement PitParser(PitParser context, XmlNode node, DataElementContainer parent)
		{
			var array = new Array();

			// name
			if (context.hasXmlAttribute(node, "name"))
				array.name = context.getXmlAttribute(node, "name");

			if (context.hasXmlAttribute(node, "minOccurs"))
			{
				array.minOccurs = int.Parse(context.getXmlAttribute(node, "minOccurs"));
				array.maxOccurs = -1;
			}

			if (context.hasXmlAttribute(node, "maxOccurs"))
				array.maxOccurs = int.Parse(context.getXmlAttribute(node, "maxOccurs"));

			if (context.hasXmlAttribute(node, "occurs"))
				array.occurs = int.Parse(context.getXmlAttribute(node, "occurs"));

			return array;
		}
	}
}

// end
