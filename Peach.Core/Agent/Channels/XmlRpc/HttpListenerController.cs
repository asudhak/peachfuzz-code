
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
using System.Text;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Threading;
using System.Diagnostics;

namespace Peach.Core.Agent.Channels.XmlRpc
{
	public class HttpListenerController
	{
		private Thread _pump;
		private bool _listening = false;
		private string _virtualDir;
		private string _physicalDir;
		private string[] _prefixes;
		private HttpListenerWrapper _listener;

		public HttpListenerController(string[] prefixes, string vdir, string pdir)
		{
			_prefixes = prefixes;
			_virtualDir = vdir;
			_physicalDir = pdir;
		}

		public void Start()
		{
			_listening = true;
			_pump = new Thread(new ThreadStart(Pump));
			_pump.Start();
		}

		public void Stop()
		{
			_listening = false;
			
			if(!_pump.Join(1000))
				_pump.Abort();

			_pump.Join();
		}

		private void Pump()
		{
			try
			{
				_listener = (HttpListenerWrapper)ApplicationHost.CreateApplicationHost(
					typeof(HttpListenerWrapper), _virtualDir, _physicalDir);
				_listener.Configure(_prefixes, _virtualDir, _physicalDir);
				_listener.Start();

				while (_listening)
				{
					try
					{
						_listener.ProcessRequest();
					}
					catch (Exception e)
					{
					}
				}

				_listener.Stop();
			}
			catch (Exception ex)
			{
				EventLog myLog = new EventLog();
				myLog.Source = "HttpListenerController";
				if (null != ex.InnerException)
					myLog.WriteEntry(ex.InnerException.ToString(), EventLogEntryType.Error);
				else
					myLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
			}
		}
	}
}
