
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

namespace Peach.Core.Agent.XmlRpc
{
	public class HttpListenerWrapper : MarshalByRefObject
	{
		protected HttpListener _listener;
		protected string _virtualDir;
		protected string _physicalDir;
		protected IAsyncResult AsyncResult;

		public void Configure(string[] prefixes, string vdir, string pdir)
		{
			_virtualDir = vdir;
			_physicalDir = pdir;
			_listener = new HttpListener();

			foreach (string prefix in prefixes)
				_listener.Prefixes.Add(prefix);
		}
		
		public void Start()
		{
			_listener.Start();
		}
		
		public void Stop()
		{
			// Wait to see if we can complete the request.
			if (AsyncResult != null)
				AsyncResult.AsyncWaitHandle.WaitOne(300);

			_listener.Stop();
			_listener.Abort();
			_listener.Close();
		}

		public void ProcessRequest()
		{
			if (AsyncResult == null || AsyncResult.AsyncWaitHandle.WaitOne(300))
			{
				AsyncResult = _listener.BeginGetContext(
					new AsyncCallback(ListenerCallback), this);
			}
		}

		public static void ListenerCallback(IAsyncResult result)
		{
			HttpListenerWrapper self = (HttpListenerWrapper)result.AsyncState;
			HttpListener listener = self._listener;

			// Call EndGetContext to complete the asynchronous operation.
			try
			{
				HttpListenerContext context = listener.EndGetContext(result);
				HttpListenerWorkerRequest workerRequest =
					new HttpListenerWorkerRequest(context, self._virtualDir, self._physicalDir);
				HttpRuntime.ProcessRequest(workerRequest);
			}
			catch (HttpListenerException)
			{
				// ignore
				return;
			}
			catch (ObjectDisposedException e)
			{
				// ignore
				return;
			}
		}
	}
}
