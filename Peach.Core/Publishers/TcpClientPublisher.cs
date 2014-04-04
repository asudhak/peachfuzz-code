
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
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Net.Sockets;

using Peach.Core.Dom;

using NLog;
using System.Diagnostics;

namespace Peach.Core.Publishers
{
	[Publisher("Tcp", true)]
	[Publisher("TcpClient")]
	[Publisher("tcp.Tcp")]
	[Parameter("Host", typeof(string), "Hostname or IP address of remote host")]
	[Parameter("Port", typeof(ushort), "Local port to listen on")]
	[Parameter("Timeout", typeof(int), "How many milliseconds to wait when receiving data (default 3000)", "3000")]
	[Parameter("SendTimeout", typeof(int), "How many milliseconds to wait when sending data (default infinite)", "0")]
	[Parameter("ConnectTimeout", typeof(int), "Max milliseconds to wait for connection (default 10000)", "10000")]
	public class TcpClientPublisher : TcpPublisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public string Host { get; set; }
		public int ConnectTimeout { get; set; }

		public TcpClientPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override void OnOpen()
		{
			base.OnOpen();

			var timeout = ConnectTimeout;
			var sw = new Stopwatch();

			for (int i = 1; _tcp == null; i *= 2)
			{
				try
				{
					// Must build a new client object after every failed attempt to connect.
					// For some reason, just calling BeginConnect again does not work on mono.
					_tcp = new TcpClient();

					sw.Restart();

					var ar = _tcp.BeginConnect(Host, Port, null, null);
					if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeout)))
						throw new TimeoutException();
					_tcp.EndConnect(ar);
				}
				catch (Exception ex)
				{
					sw.Stop();

					if (_tcp != null)
					{
						_tcp.Close();
						_tcp = null;
					}

					timeout -= (int)sw.ElapsedMilliseconds;

					if (timeout > 0)
					{
						int waitTime = Math.Min(timeout, i);
						timeout -= waitTime;

						Logger.Warn("open: Warn, Unable to connect to remote host {0} on port {1}.  Trying again in {2}ms...", Host, Port, waitTime);
						Thread.Sleep(waitTime);
					}
					else
					{
						Logger.Error("open: Error, Unable to connect to remote host {0} on port {1}.", Host, Port);
						throw new SoftException(ex);
					}
				}
			}

			StartClient();
		}
	}
}
