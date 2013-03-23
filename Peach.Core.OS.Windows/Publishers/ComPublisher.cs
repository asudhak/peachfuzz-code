
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
using Peach.Core.IO;

namespace Peach.Core.Publishers
{
	[Publisher("Com", true)]
	[Publisher("com.Com")]
	[Parameter("clsid", typeof(string), "COM CLSID of object")]
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
			if (_proxy != null)
				return;

			try
			{
				ChannelServices.RegisterChannel(new IpcChannel(), false);
			}
			catch
			{
			}

			_proxy = (IComContainer)Activator.GetObject(typeof(IComContainer),
				"ipc://Peach_Com_Container/PeachComContainer");

			try
			{
				_proxy.Intialize(clsid);
			}
			catch (Exception ex)
			{
				throw new PeachException("Error, ComPublisher was unable to create object.  " + ex.Message, ex);
			}
		}

		protected void stopTcpRemoting()
		{
			_proxy = null;
		}

		protected static object GetObj(Variant v)
		{
			switch (v.GetVariantType())
			{
				case Variant.VariantType.BitStream:
					return ((BitStream)v).Value;
				case Variant.VariantType.Boolean:
					return (bool)v;
				case Variant.VariantType.ByteString:
					return (byte[])v;
				case Variant.VariantType.Int:
					return (int)v;
				case Variant.VariantType.Long:
					return (long)v;
				case Variant.VariantType.String:
					return (string)v;
				case Variant.VariantType.ULong:
					return (ulong)v;
				case Variant.VariantType.Unknown:
				default:
					throw new NotImplementedException();
			}
		}

		protected override Variant OnCall(string method, List<ActionParameter> args)
		{
			try
			{
				startTcpRemoting();

				List<object> parameters = new List<object>();

				foreach(ActionParameter arg in args)
					parameters.Add(GetObj(arg.dataModel[0].InternalValue));

				object value = _proxy.CallMethod(method, parameters.ToArray());

				if (value != null)
					return new Variant(value.ToString());
			}
			catch(Exception ex)
			{
				stopTcpRemoting();
				throw new SoftException(ex);
			}

			return null;
		}

		protected override void OnSetProperty(string property, Variant value)
		{
			try
			{
				startTcpRemoting();

				_proxy.SetProperty(property, GetObj(value));
			}
			catch (Exception ex)
			{
				stopTcpRemoting();
				throw new SoftException(ex);
			}
		}

		protected override Variant OnGetProperty(string property)
		{
			try
			{
				startTcpRemoting();

				object value = _proxy.GetProperty(property);

				if (value != null)
					return new Variant(value.ToString());
			}
			catch (Exception ex)
			{
				stopTcpRemoting();
				throw new SoftException(ex);
			}

			return null;
		}
	}
}

// END
