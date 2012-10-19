
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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Ipc;

using Peach.Core.Dom;
using Peach.Core.Publishers.Com;
using NLog;

namespace Peach.Core.Publishers
{
	[Publisher("Com")]
	[Publisher("com.Com")]
	[Parameter("clsid", typeof(string), "COM CLSID of object", true)]
	public class ComPublisher : Publisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public string clsid { get; set; }

		private IComContainer _proxy = null;

		public ComPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected void startTcpRemoting()
		{
			try
			{
				ChannelServices.RegisterChannel(new IpcChannel(), false);
			}
			catch
			{
			}

			_proxy = (IComContainer)Activator.GetObject(typeof(IComContainer),
				"ipc://Peach_Com_Container/PeachComContainer");

			if (!_proxy.Intialize(clsid))
				throw new PeachException("Error, ComPublisher was unable to create object from id '" + clsid + "'");
		}

		protected void stopTcpRemoting()
		{
			// TODO - How do we stop this madnes?
			_proxy = null;
		}

		protected override void OnStart()
		{
			System.Diagnostics.Debug.Assert(_proxy == null);
			startTcpRemoting();
		}

		protected override void OnStop()
		{
			System.Diagnostics.Debug.Assert(_proxy != null);
			stopTcpRemoting();
		}

		protected override Variant OnCall(string method, List<ActionParameter> args)
		{
			try
			{
				List<object> parameters = new List<object>();

				foreach(ActionParameter arg in args)
					parameters.Add((string)((DataElementContainer)arg.dataModel)[0].InternalValue);

				object value = _proxy.CallMethod(method, parameters.ToArray());

				if (value != null)
					return new Variant(value.ToString());
			}
			catch(Exception ex)
			{
				logger.Error("Ignoring exception: " + ex.Message);
			}

			return null;
		}

		protected override void OnSetProperty(string property, Variant value)
		{
			try
			{
				_proxy.SetProperty(property, (string)value);
			}
			catch (Exception ex)
			{
				logger.Error("Ignoring exception: " + ex.Message);
			}
		}

		protected override Variant OnGetProperty(string property)
		{
			try
			{
				object value = _proxy.GetProperty(property);

				if (value != null)
					return new Variant(value.ToString());
			}
			catch (Exception ex)
			{
				logger.Error("Ignoring exception: " + ex.Message);
			}

			return null;
		}
	}
}

// END
