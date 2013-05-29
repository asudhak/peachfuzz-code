
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
using System.Runtime.Serialization;
using System.Xml.XPath;

using Peach.Core.Agent;
using System.Xml;
using System.Reflection;

using System.Linq;
namespace Peach.Core.Dom
{
	[Serializable]
	public class Test : INamed
	{
		public string _name = null;
		public object parent = null;
		public int controlIterationEvery = 0;

		/// <summary>
		/// Do not fault when actions miss-match
		/// </summary>
		public bool nonDeterministicActions = false;

		[NonSerialized]
		public List<Logger> loggers = new List<Logger>();

		public StateModel stateModel = null;

		[NonSerialized]
		public MutationStrategy strategy = null;

		//[NonSerialized]
		//public OrderedDictionary<string, Logger> loggers = new OrderedDictionary<string, Logger>();

		[NonSerialized]
		public OrderedDictionary<string, Publisher> publishers = new OrderedDictionary<string, Publisher>();

		[NonSerialized]
		public OrderedDictionary<string, Agent> agents = new OrderedDictionary<string, Agent>();

		/// <summary>
		/// List of mutators to include in run
		/// </summary>
		/// <remarks>
		/// If exclude is empty, and this collection contains values, then remove all mutators and only
		/// include these.
		/// </remarks>
		public List<string> includedMutators = new List<string>();

		/// <summary>
		/// List of mutators to exclude from run
		/// </summary>
		/// <remarks>
		/// If include is empty then use all mutators excluding those in this list.
		/// </remarks>
		public List<string> excludedMutators = new List<string>();

		/// <summary>
		/// Collection of xpaths to mark state model/data models as mutable true/false
		/// at runtime.  This collection is set using Include and Exclude elements in a
		/// Test definition.
		/// </summary>
		public List<Tuple<bool, string>> mutables = new List<Tuple<bool, string>>();

		public Test()
		{
			publishers.AddEvent += new AddEventHandler<string, Publisher>(publishers_AddEvent);

			replayEnabled = true;
			waitTime = 0;
			faultWaitTime = 2;
		}

		#region OrderedDictionary AddEvent Handlers

		void publishers_AddEvent(OrderedDictionary<string, Publisher> sender, string key, Publisher value)
		{
			value.Test = this;
		}

		#endregion

		public string name
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// Should iterations be replayed when a fault occurs.
		/// </summary>
		public bool replayEnabled { get; set; }

		/// <summary>
		/// Time to wait in seconds between each test case. Value can be fractional
		/// (0.25). Defaults to zero (0).
		/// </summary>
		public decimal waitTime { get; set; }

		/// <summary>
		/// Time to wait in seconds between each test case when reproducing faults. Value can be fractional
		/// (0.25). Defaults to two (2) seconds.
		/// </summary>
		/// <remarks>
		/// This value should be large enough to make sure a fault is detected at the correct
		/// iteration.  We only wait this time when verifying a fault was detected.
		/// </remarks>
		public decimal faultWaitTime { get; set; }

		public void markMutableElements()
		{
			Dom dom;

			if (parent is Dom)
				dom = parent as Dom;
			else if (parent is Test)
				dom = (parent as Test).parent as Dom;
			else
				throw new PeachException("Parent is crazy type!");

			var nav = new XPath.PeachXPathNavigator(dom);
			XPathNodeIterator nodeIter = null;

			foreach (Tuple<bool, string> item in mutables)
			{
				nodeIter = nav.Select(item.Item2);

				while (nodeIter.MoveNext())
				{
					var dataElement = ((XPath.PeachXPathNavigator)nodeIter.Current).currentNode as DataElement;

					if (dataElement != null)
						markMutable(dataElement, item.Item1);
				}
			}
		}

		private static void markMutable(DataElement elem, bool isMutable)
		{
			elem.isMutable = isMutable;

			var cont = elem as DataElementContainer;
			if (cont == null)
				return;

			foreach (var child in cont)
			{
				markMutable(child, isMutable);
			}
		}
	}
}
// END
