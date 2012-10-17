
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

namespace Peach.Core.Publishers
{
	[Publisher("Tcp", true)]
	[Publisher("TcpClient")]
	[Publisher("tcp.Tcp")]
	[Parameter("Host", typeof(string), "Hostname or IP address of remote host", true)]
	[Parameter("Port", typeof(ushort), "Local port to listen on", true)]
	[Parameter("Timeout", typeof(int), "How many milliseconds to wait for data/connection (default 3000)", "3000")]
	public class TcpClientPublisher : TcpPublisher
	{
		public string Host { get; set; }

		private int _errorsMax = 10;
		private int _errors = 0;

		public TcpClientPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override void OnOpen()
		{
			base.OnOpen();

			try
			{
				_client = new TcpClient();
				var ar = _client.BeginConnect(Host, Port, null, null);
				if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(Timeout)))
					throw new TimeoutException();
				_client.EndConnect(ar);
			}
			catch (Exception ex)
			{
				if (ex is TimeoutException)
				{
					logger.Debug("Could not connect to {0}:{1} within {2}ms, timing out.",
						Host, Port, Timeout);
				}
				else
				{
					logger.Error("Could not connect to {0}:{1}. {2}",
						Host, Port, ex.Message);
				}

				if (_client != null)
				{
					_client.Close();
					_client = null;
				}

				if (++_errors == _errorsMax)
					throw new PeachException("Failed to connect after " + _errors + " attempts.");

				throw new SoftException();
			}

			StartClient();
		}
	}
}
