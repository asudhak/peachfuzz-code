
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
using Peach.Core.Agent;
using System.Runtime.Serialization;
using System.Xml;

namespace Peach.Core.Dom
{
	[Serializable]
	public class Dom : INamed
	{
		public string fileName = "";
		public string version = "";
		public string author = "";
		public string description = "";

		public RunContext context = null;
		public OrderedDictionary<string, Dom> ns = new OrderedDictionary<string, Dom>();
		public OrderedDictionary<string, DataModel> dataModels = new OrderedDictionary<string, DataModel>();
		public OrderedDictionary<string, StateModel> stateModels = new OrderedDictionary<string, StateModel>();
		public OrderedDictionary<string, Agent> agents = new OrderedDictionary<string, Agent>();
		public OrderedDictionary<string, Test> tests = new OrderedDictionary<string, Test>();
		public OrderedDictionary<string, Data> datas = new OrderedDictionary<string, Data>();

		public Dom()
		{
			name = "";

			dataModels.AddEvent += new AddEventHandler<string, DataModel>(dataModels_AddEvent);
			stateModels.AddEvent += new AddEventHandler<string, StateModel>(stateModels_AddEvent);
			agents.AddEvent += new AddEventHandler<string, Agent>(agents_AddEvent);
			tests.AddEvent += new AddEventHandler<string, Test>(tests_AddEvent);
		}

		void agents_AddEvent(OrderedDictionary<string, Agent> sender, string key, Agent value)
		{
		}

		#region OrderedDictionary AddEvent Handlers

		void tests_AddEvent(OrderedDictionary<string, Test> sender, string key, Test value)
		{
			value.parent = this;
		}

		void stateModels_AddEvent(OrderedDictionary<string, StateModel> sender, string key, StateModel value)
		{
			value.parent = this;
		}

		void dataModels_AddEvent(OrderedDictionary<string, DataModel> sender, string key, DataModel value)
		{
			value.dom = this;
		}

		#endregion

		/// <summary>
		/// Execute all analyzers on all data models in DOM.
		/// </summary>
		public void evaulateDataModelAnalyzers()
		{
			foreach (DataModel model in dataModels.Values)
				model.evaulateAnalyzers();

			foreach (Test test in tests.Values)
			{
				foreach (State state in test.stateModel.states.Values)
				{
					foreach (Action action in state.actions)
					{
						if (action.dataModel != null)
							action.dataModel.evaulateAnalyzers();

						foreach (ActionParameter ap in action.parameters)
						{
							if (ap.dataModel != null)
								ap.dataModel.evaulateAnalyzers();
						}
					}
				}
			}
		}

		#region INamed Members

		public virtual string name
		{
			get; set;
		}

		#endregion

		public XmlDocument pitSerialize()
		{

			XmlDocument doc = new XmlDocument();

			XmlNode node = doc.CreateNode(XmlNodeType.Element, "Peach", "");

			foreach (DataModel dataModel in dataModels.Values)
			{
				node.AppendChild(dataModel.pitSerialize(doc, node));
			}

			foreach (StateModel stateModel in stateModels.Values)
			{
				node.AppendChild(stateModel.pitSerialize(doc, node));
			}

			foreach (Agent agent in agents.Values)
			{
				node.AppendChild(agent.pitSerialize(doc, node));
			}

			foreach (Test test in tests.Values)
			{
				node.AppendChild(test.pitSerialize(doc, node));
			}

			doc.AppendChild(node);

			return doc;
		}
	}
}


// END
