
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
using Peach.Core.Dom;

namespace Peach.Core.Publishers
{
	[PublisherAttribute("Com")]
	[PublisherAttribute("com.Com")]
	[ParameterAttribute("clsid", typeof(string), "COM CLSID of object", true)]
	public class ComPublisher : Publisher
	{
		dynamic _object = null;
		string _clsid = null;

		public ComPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
			_clsid = (string)args["clsid"];
		}

		public override void open(Core.Dom.Action action)
		{
			if (_object == null)
			{
				OnOpen(action);


				Type type = null;
				type = Type.GetTypeFromProgID(_clsid);
				if (type == null)
				{
					try
					{
						type = Type.GetTypeFromCLSID(Guid.Parse(_clsid));
					}
					catch
					{
					}
				}

				if (type == null)
					throw new PeachException("Error, ComPublisher was unable to create object from id '" + _clsid + "'");

				_object = Activator.CreateInstance(type);
				
				if (_object == null)
					throw new PeachException("Error, ComPublisher was unable to create object from id '" + _clsid + "'");
			}
		}

		public override void close(Core.Dom.Action action)
		{
			OnClose(action);

			if (_object != null)
			{
				Marshal.ReleaseComObject(_object);
				_object = null;
			}
		}

		public override Variant call(Core.Dom.Action action, string method, List<ActionParameter> args)
		{
			if (_object == null)
				open(action);

			OnCall(action, method, args);

			Dictionary<string, object> state = new Dictionary<string, object>();
			state["ComObject"] = _object;

			string cmd = "ComObject." + method + "(";

			int count = 0;
			foreach(ActionParameter arg in args)
			{
				state["ComArgs_" + count] = (string)((DataElementContainer)arg.dataModel)[0].InternalValue;
				cmd += "ComArgs_" + count + ",";
			}

			// Remove that last comma :)
			cmd = cmd.Substring(0, cmd.Length - 1) + ")";

			object value = Scripting.EvalExpression(cmd, state);
			return new Variant(value.ToString());
		}

		public override void setProperty(Core.Dom.Action action, string property, Variant value)
		{
			if (_object == null)
				open(action);

			OnSetProperty(action, property, value);

			Dictionary<string, object> state = new Dictionary<string, object>();
			state["ComObject"] = _object;
			state["ComArg"] = (string)value;

			string cmd = "ComObject." + property + " = ComArg";
			Scripting.EvalExpression(cmd, state);
		}

		public override Variant getProperty(Core.Dom.Action action, string property)
		{
			if (_object == null)
				open(action);

			OnGetProperty(action, property);

			Dictionary<string, object> state = new Dictionary<string, object>();
			state["ComObject"] = _object;

			string cmd = "ComObject." + property;
			return new Variant(Scripting.EvalExpression(cmd, state).ToString());
		}
	}
}

// END
