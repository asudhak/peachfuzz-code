
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
using System.Runtime.Serialization;
using System.Xml.Serialization;

using System.Linq;
using System.ComponentModel;

namespace Peach.Core.Dom
{
	/// <summary>
	/// Mark state model/data models as mutable at runtime.
	/// </summary>
	public class IncludeMutable : MarkMutable
	{
		public override bool mutable { get { return true; } }
	}

	/// <summary>
	/// Mark state model/data models as non-mutable at runtime.
	/// </summary>
	public class ExcludeMutable : MarkMutable
	{
		public override bool mutable { get { return false; } }
	}

	/// <summary>
	/// Mark state model/data models as mutable true/false at runtime.
	/// </summary>
	public abstract class MarkMutable
	{
		[XmlIgnore]
		public abstract bool mutable { get; }

		/// <summary>
		/// Name of element to mark as mutable/non-mutable.
		/// </summary>
		[XmlAttribute("ref")]
		[DefaultValue(null)]
		public string refName { get; set; }

		/// <summary>
		/// Xpath to elements to mark as mutable/non-mutable.
		/// </summary>
		[XmlAttribute("xpath")]
		[DefaultValue(null)]
		public string xpath { get; set; }
	}

	public class MutatorFilter
	{
		public enum Mode
		{
			[XmlEnum("include")]
			Include,

			[XmlEnum("exclude")]
			Exclude,
		}

		[XmlAttribute]
		public Mode mode { get; set; }

		[PluginElement("class", typeof(Peach.Core.Mutator))]
		public List<Peach.Core.Mutator> Mutators { get; set; }
	}

	public class AgentRef
	{
		[XmlAttribute("ref")]
		public string refName { get; set; }

		[XmlAttribute("platform")]
		[DefaultValue(Platform.OS.All)]
		public Platform.OS platform { get; set; }
	}

	public class StateModelRef
	{
		[XmlAttribute("ref")]
		public string refName { get; set; }
	}

	/// <summary>
	/// Define a test to run. Currently a test is defined as a combination of a
	/// Template and optionally a Data set. In the future this will expand to include a state model,
	/// defaults for generation, etc.
	/// </summary>
	public class Test : INamed
	{
		#region Attributes

		/// <summary>
		/// Name of test case.
		/// </summary>
		[XmlAttribute]
		public string name { get; set; }

		/// <summary>
		/// Description of test case.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(null)]
		public string description { get; set; }

		/// <summary>
		/// Time to wait in seconds between each test case. Value can be fractional
		/// (0.25). Defaults to zero (0).
		/// </summary>
		[XmlAttribute]
		[DefaultValue(0.0)]
		public decimal waitTime { get; set; }

		/// <summary>
		/// Time to wait in seconds between each iteration when in fault reproduction mode.
		/// This occurs when a fault has been detected and is being verified. Value can
		/// be fractional (0.25). Defaults to two (2) seconds.
		/// </summary>
		/// <remarks>
		/// This value should be large enough to make sure a fault is detected at the correct
		/// iteration.  We only wait this time when verifying a fault was detected.
		/// </remarks>
		[XmlAttribute]
		[DefaultValue(2.0)]
		public decimal faultWaitTime { get; set; }

		/// <summary>
		/// Should iterations be replayed when a fault occurs.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(true)]
		public bool replayEnabled { get; set; }

		/// <summary>
		/// How often we should perform a control iteration.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(0)]
		public int controlIteration { get; set; }

		/// <summary>
		/// Are action run counts non-deterministic.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(false)]
		public bool nonDeterministicActions { get; set; }

		#endregion

		[OnCloning]
		private bool OnCloning(object context)
		{
			throw new NotSupportedException();
		}

		#region Elements

		[PluginElement("class", typeof(Logger))]
		[DefaultValue(null)]
		public List<Logger> loggers { get; set; }

		[XmlElement("Include", typeof(IncludeMutable))]
		[XmlElement("Exclude", typeof(ExcludeMutable))]
		[DefaultValue(null)]
		public List<MarkMutable> mutables { get; set; }

		[PluginElement("Strategy", "class", typeof(MutationStrategy))]
		[DefaultValue(null)]
		public MutationStrategy strategy { get; set; }

		#endregion

		#region Schema Elements

		/// <summary>
		/// Currently unused.  Exists for schema generation.
		/// </summary>
		[XmlElement("Mutator")]
		[DefaultValue(null)]
		public MutatorFilter mutators { get; set; }

		/// <summary>
		/// Currently unused.  Exists for schema generation.
		/// </summary>
		[XmlElement("Agent")]
		[DefaultValue(null)]
		public List<AgentRef> agentRef { get; set; }

		/// <summary>
		/// Currently unused.  Exists for schema generation.
		/// </summary>
		[XmlElement("StateModel")]
		public StateModelRef stateModelRef { get; set; }

		/// <summary>
		/// Currently unused.  Exists for schema generation.
		/// </summary>
		[PluginElement("class", typeof(Publisher))]
		public List<Publisher> pubs { get; set; }

		#endregion

		public Dom parent { get; set; }

		public StateModel stateModel = null;

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

		public Test()
		{
			publishers.AddEvent += new AddEventHandler<string, Publisher>(publishers_AddEvent);

			replayEnabled = true;
			waitTime = 0;
			faultWaitTime = 2;

			loggers = new List<Logger>();
			mutables = new List<MarkMutable>();
			agentRef = new List<AgentRef>();
			pubs = new List<Publisher>();
		}

		#region OrderedDictionary AddEvent Handlers

		void publishers_AddEvent(OrderedDictionary<string, Publisher> sender, string key, Publisher value)
		{
			value.Test = this;
		}

		#endregion


		public void markMutableElements()
		{
			var nav = new XPath.PeachXPathNavigator(parent);

			foreach (var item in mutables)
			{
				var nodeIter = nav.Select(item.xpath);

				while (nodeIter.MoveNext())
				{
					var dataElement = ((XPath.PeachXPathNavigator)nodeIter.Current).currentNode as DataElement;

					if (dataElement != null)
					{
						dataElement.isMutable = item.mutable;
						foreach (var child in dataElement.EnumerateAllElements())
							child.isMutable = item.mutable;
					}
				}
			}
		}
	}
}
// END
