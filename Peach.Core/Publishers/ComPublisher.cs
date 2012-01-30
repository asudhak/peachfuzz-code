
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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using Peach.Core.Dom;
using Peach.Core.Publishers.Com;

namespace Peach.Core.Publishers
{
	[PublisherAttribute("Com")]
	[PublisherAttribute("com.Com")]
	[ParameterAttribute("clsid", typeof(string), "COM CLSID of object", true)]
	public class ComPublisher : Publisher
	{
		string _clsid = null;
		string _host = "localhost";
		int _port = 9001;
		IComContainer _proxy = null;
		TcpChannel _chan = null;
		bool _initialized = false;

		public ComPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
			_clsid = (string)args["clsid"];

			if (args.ContainsKey("host"))
				_host = (string) args["host"];
			if (args.ContainsKey("port"))
				_port = (int)args["port"];
		}

		protected void startTcpRemoting()
		{
			if (_proxy != null)
				return;

			TcpChannel chan = new TcpChannel();
			ChannelServices.RegisterChannel(chan, false); // Disable security for speed
			_proxy = (IComContainer)Activator.GetObject(typeof(IComContainer),
				string.Format("tcp://{0}:{1}/PeachComContainer", _host, _port));

			_initialized = false;
		}

		protected void stopTcpRemoting()
		{
			// TODO - How do we stop this madnes?
			_proxy = null;
			_initialized = false;
		}

		public override void open(Core.Dom.Action action)
		{
			if (_initialized)
				return;

			OnOpen(action);

			if (_proxy == null)
				startTcpRemoting();

			if (!_proxy.Intialize(_clsid))
				throw new PeachException("Error, ComPublisher was unable to create object from id '" + _clsid + "'");

			_initialized = true;
		}

		public override void close(Core.Dom.Action action)
		{
			OnClose(action);

			stopTcpRemoting();
		}

		public override Variant call(Core.Dom.Action action, string method, List<ActionParameter> args)
		{
			open(action);

			OnCall(action, method, args);

			List<object> parameters = new List<object>();

			foreach(ActionParameter arg in args)
				parameters.Add((string)((DataElementContainer)arg.dataModel)[0].InternalValue);

			try
			{
				object value = _proxy.CallMethod(method, parameters.ToArray());

				if (value != null)
					return new Variant(value.ToString());
			}
			catch
			{
			}

			return null;
		}

		public override void setProperty(Core.Dom.Action action, string property, Variant value)
		{
			open(action);

			OnSetProperty(action, property, value);

			try
			{
				_proxy.SetProperty(property, (string)value);
			}
			catch
			{
			}
		}

		public override Variant getProperty(Core.Dom.Action action, string property)
		{
			open(action);

			OnGetProperty(action, property);

			try
			{
				object value = _proxy.GetProperty(property);

				if (value != null)
					return new Variant(value.ToString());
			}
			catch
			{
			}

			return null;
		}
	}
}

// END
