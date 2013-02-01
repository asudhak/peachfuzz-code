
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Runtime.InteropServices;

using Peach.Core;
using Peach.Core.Publishers.Com;

using NLog;

namespace Peach.Core.ComContainer
{
	public class ComContainer : MarshalByRefObject, IComContainer
	{
		dynamic comObject = null;

		#region IComContainer Members

		public void Intialize(string control)
		{
			Type type = Type.GetTypeFromProgID(control);

			if (type == null)
				type = Type.GetTypeFromCLSID(Guid.Parse(control));

			if (type == null)
				throw new Exception("ComContainer was unable to create type from id '" + control + "'.");

			comObject = Activator.CreateInstance(type);

			if (comObject == null)
				throw new Exception("Error, ComContainer was unable to create object from id '" + control + "'.");
		}

		void finalize()
		{
			if (comObject != null)
			{
				Marshal.ReleaseComObject(comObject);
				comObject = null;
			}
		}

		public object CallMethod(string method, object[] args)
		{
			if(comObject == null)
				throw new Exception("Error, please call Initalize first!");

			Dictionary<string, object> state = new Dictionary<string, object>();
			state["ComObject"] = comObject;

			string cmd = "ComObject." + method + "(";

			int count = 0;
			foreach (object arg in args)
			{
				state["ComArgs_" + count] = arg;
				cmd += "ComArgs_" + count + ",";
				count++;
			}

			if (count > 0)
				// Remove that last comma :)
				cmd = cmd.Substring(0, cmd.Length - 1);

			cmd += ")";

			return Scripting.EvalExpression(cmd, state);
		}

		public object GetProperty(string property)
		{
			if (comObject == null)
				throw new Exception("Error, please call Initalize first!");

			Dictionary<string, object> state = new Dictionary<string, object>();
			state["ComObject"] = comObject;

			string cmd = "getattr(ComObject, '" + property + "')";
			return Peach.Core.Scripting.EvalExpression(cmd, state);
		}

		public void SetProperty(string property, object value)
		{
			if (comObject == null)
				throw new Exception("Error, please call Initalize first!");

			Dictionary<string, object> state = new Dictionary<string, object>();
			state["ComObject"] = comObject;
			state["ComArg"] = value;

			string cmd = "setattr(ComObject, '" + property + "', ComArg)";
			Scripting.EvalExpression(cmd, state);
		}

		public void Shutdown()
		{
			Program.Shutdown = true;
		}

		#endregion
	}
}
