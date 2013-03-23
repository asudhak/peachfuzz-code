
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
using System.Xml.Serialization;
using System.Xml;
using System.Xml.XPath;

namespace Peach.Core.Dom
{
	/// <summary>
	/// A dom element to hold Agent configuration information
	/// </summary>
	[Serializable]
	public class Agent
	{
		/// <summary>
		/// Name for agent
		/// </summary>
		public string name;

		/// <summary>
		/// URL of agent
		/// </summary>
		public string url;

		/// <summary>
		/// Optional password for agent
		/// </summary>
		public string password;

		/// <summary>
		/// Limit Agent to specific platform.  Platform of unknown is 
		/// any OS.
		/// </summary>
		public Platform.OS platform = Platform.OS.All;

		/// <summary>
		/// List of monitors Agent should spin up.
		/// </summary>
		public List<Monitor> monitors = new List<Monitor>();
	}
}

// END
