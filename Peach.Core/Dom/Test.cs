
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
	public class Test : INamed, IPitSerializable
	{
		public string _name = null;
		public object parent = null;
		public Logger logger = null;
		public StateModel stateModel = null;
		public MutationStrategy strategy = null;
		public OrderedDictionary<string, Logger> loggers = new OrderedDictionary<string, Logger>();
		public OrderedDictionary<string, Publisher> publishers = new OrderedDictionary<string, Publisher>();
		public OrderedDictionary<string, Agent> agents = new OrderedDictionary<string, Agent>();

		/// <summary>
		/// List of mutators to include in run
		/// </summary>
		/// <remarks>
		/// If exclude is null, and this collection contains values, then remove all mutators and only
		/// include these.
		/// </remarks>
		public List<string> includedMutators = null;

		/// <summary>
		/// List of mutators to exclude from run
		/// </summary>
		/// <remarks>
		/// If include is null then use all mutators excluding those in this list.
		/// </remarks>
		public List<string> exludedMutators = null;

		/// <summary>
		/// Collection of xpaths to mark state model/data models as mutable true/false
		/// at runtime.  This collection is set using Include and Exclude elements in a
		/// Test definition.
		/// </summary>
		public List<Tuple<bool, string>> mutables = new List<Tuple<bool, string>>();

		public Test()
		{
			loggers.AddEvent += new AddEventHandler<string, Logger>(loggers_AddEvent);
			publishers.AddEvent += new AddEventHandler<string, Publisher>(publishers_AddEvent);
			//agents.AddEvent += new AddEventHandler<string, Agent>(agents_AddEvent);
		}

		#region OrderedDictionary AddEvent Handlers

		//void agents_AddEvent(OrderedDictionary<string, Agent> sender, string key, Agent value)
		//{
		//    value.parent = this;
		//}

		void publishers_AddEvent(OrderedDictionary<string, Publisher> sender, string key, Publisher value)
		{
			value.parent = this;
		}

		void loggers_AddEvent(OrderedDictionary<string, Logger> sender, string key, Logger value)
		{
			value.parent = this;
		}

		#endregion

		public string name
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// Time to wait in seconds between each test case. Value can be fractional
		/// (0.25). Defaults to zero (0).
		/// </summary>
		public decimal waitTime { get; set; }

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
					{
						dataElement.isMutable = item.Item1;
					}
				}
			}
		}

    public System.Xml.XmlNode pitSerialize(System.Xml.XmlDocument doc, System.Xml.XmlNode parent)
    {
      XmlNode node = doc.CreateNode(XmlNodeType.Element, "Test", null);

      node.AppendAttribute("name", this.name);

      #region Include
      if (this.includedMutators != null)
      {
        foreach (string s in includedMutators)
        {
          XmlNode eInclude = doc.CreateElement("Include");
          eInclude.AppendAttribute("name", s);
          node.AppendChild(eInclude);
        }
      }
      #endregion

      #region Exclude
      if (this.exludedMutators != null)
      {
        foreach (string s in this.exludedMutators)
        {
          XmlNode eExclude = doc.CreateElement("Exclude");
          eExclude.AppendAttribute("name", s);
          node.AppendChild(eExclude);
        }
      }
      #endregion

      #region Strategy
      if (this.strategy != null)
      {
        Type t = this.strategy.GetType();
        object[] attribs = t.GetCustomAttributes(true);
        foreach (object attrib in attribs)
        {
          if (attrib is MutationStrategyAttribute)
          {
            XmlNode eStrategy = doc.CreateElement("Strategy");
            eStrategy.AppendAttribute("class", ((MutationStrategyAttribute)attrib).name);
            node.AppendChild(eStrategy);
            break;
          }
        }
      }
      #endregion

      #region StateModel
      if (this.stateModel != null)
      {
        XmlNode eStateModel = doc.CreateElement("StateModel");
        eStateModel.AppendAttribute("ref", this.stateModel.name);
        node.AppendChild(eStateModel);
      }
      #endregion

      #region Agents
      if (this.agents != null)
      {
        foreach (Agent agent in this.agents.Values)
        {
          XmlNode eAgent = doc.CreateElement("Agent");
          eAgent.AppendAttribute("ref", agent.name);
          node.AppendChild(eAgent);
        }
      }
      #endregion

      #region Publisher
      if (this.publishers != null)
      {
        foreach (Publisher publisher in this.publishers.Values)
        {
          bool skip = false;
          Type publisherType = publisher.GetType();
          object[] attribs = publisherType.GetCustomAttributes(true);
          XmlNode ePublisher = doc.CreateElement("Publisher");
          string className = System.String.Empty;

          foreach (object attrib in attribs)
          {
            if (attrib is NotPitParsableAttribute)
            {
              skip = true;
              break;
            }
          }

          if (skip == false)
          {
            foreach (object attrib in attribs)
            {
              if (attrib is PublisherAttribute)
              {
                if (((PublisherAttribute)attrib).isDefault)
                {
                  className = ((PublisherAttribute)attrib).invokeName;
                  ePublisher.AppendAttribute("class", className);
                  break;
                }
              }
            }

            if (System.String.IsNullOrEmpty(className) == false)
            {
              foreach (object attrib in attribs)
              {
                if (attrib is ParameterAttribute)
                {

                    XmlNode eParam = doc.CreateElement("Param", null);
                    string paramName = ((ParameterAttribute)attrib).name;
                    eParam.AppendAttribute("name", paramName);
                    eParam.AppendAttribute("valueType", ((ParameterAttribute)attrib).type.ToString());
                    PropertyInfo pi = publisherType.GetProperty(paramName);
                    if (pi != null)
                    {
                      object propertyValue = pi.GetValue(publisher, null);
                      eParam.AppendAttribute("value", propertyValue.ToString());
                      ePublisher.AppendChild(eParam);
                    }
                    else
                    {
                      throw new PeachException(System.String.Format("Can not find property '{0}' in class '{1}'", paramName, publisherType.ToString()));
                    }
                }
              }
              node.AppendChild(ePublisher);
            }
          }
        }
      }
      #endregion

      if (this.logger != null)
      {
        Type loggerType = this.logger.GetType();
        List<object> attribs = new List<object>(loggerType.GetCustomAttributes(false));
        LoggerAttribute loggerAttrib = (from o in attribs where (o is LoggerAttribute) && (((LoggerAttribute)o).isDefault = true) select o).First() as LoggerAttribute;
        List<ParameterAttribute> paramAttribs = (from o in attribs where (o is ParameterAttribute) select o as ParameterAttribute).ToList();
        XmlNode eLogger = doc.CreateElement("Logger");
        eLogger.AppendAttribute("class", loggerAttrib.invokeName);

        foreach (ParameterAttribute paramAttrib in paramAttribs)
        {
          XmlNode eParam = doc.CreateElement("Param");
          eParam.AppendAttribute("name", paramAttrib.name);
          PropertyInfo pi = loggerType.GetProperty(paramAttrib.name);
          if (pi != null)
          {
            object paramValue = pi.GetValue(this.logger, null);
            eParam.AppendAttribute("value", paramValue.ToString());
            eLogger.AppendChild(eParam);
          }
          else
          {
            throw new PeachException(System.String.Format("Can not find property '{0}' in class '{1}'", paramAttrib.name, loggerType.ToString()));
          }
        }

        node.AppendChild(eLogger);
      }

      return node;
    }
  }
}

// END
