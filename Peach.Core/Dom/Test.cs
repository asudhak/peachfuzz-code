
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Peach.Core.Agent;
using System.Runtime.Serialization;

namespace Peach.Core.Dom
{
	[Serializable]
	public class Test : INamed
	{
		public string _name = null;
		public object parent = null;
		public Run run = null;
		public StateModel stateModel = null;
		public MutationStrategy strategy = null;
		public OrderedDictionary<string, Logger> loggers = new OrderedDictionary<string, Logger>();
		public OrderedDictionary<string, Publisher> publishers = new OrderedDictionary<string, Publisher>();
		public OrderedDictionary<string, Agent> agents = new OrderedDictionary<string, Agent>();
		public List<string> includedMutators = null;
		public List<string> exludedMutators = null;

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
	}
}

// END
