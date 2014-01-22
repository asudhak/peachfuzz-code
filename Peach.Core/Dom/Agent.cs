
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
using System.ComponentModel;

namespace Peach.Core.Dom
{
	/// <summary>
	/// Configure a local or remote agent. Agents can perform various tasks during
	/// a fuzzing run. This element must include at least one Monitor child.
	/// </summary>
	[Serializable]
	// TODO: Old XSD defines <PythonPath> and <Import> children
	public class Agent : INamed
	{
		public Agent()
		{
			platform = Platform.OS.All;
			monitors = new NamedCollection<Monitor>();
		}

		/// <summary>
		/// Name of agent. May not contain spaces or periods (.).
		/// </summary>
		[XmlAttribute]
		public string name { get; set; }

		/// <summary>
		/// Specify location of agent. Value is "&lt;channel%gt;://&lt;hostname&gt;" where
		/// &lt;channel%gt; specifies the remoting channel (tcp or local) and
		/// &lt;hostname%gt; specifies the hostname/ipaddress of the agent.
		/// If this attribute is not set a local agent will be used.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(null)]
		public string location { get; set; }

		/// <summary>
		/// Password to the remote agent if needed.
		/// </summary>
		[XmlAttribute]
		[DefaultValue(null)]
		public string password { get; set; }

		/// <summary>
		/// Limit Agent to specific platform.
		/// </summary>
		public Platform.OS platform { get; set; }

		/// <summary>
		/// List of monitors Agent should spin up.
		/// </summary>
		[PluginElement("class", typeof(Peach.Core.Agent.Monitor), Named = true)]
		[DefaultValue(null)]
		public NamedCollection<Monitor> monitors { get; set; }
	}
}

// END
