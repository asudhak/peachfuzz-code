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

using NLog;

namespace Peach.Core.ComContainer
{
	public class ComContainer : MarshalByRefObject, IComContainer
	{
		dynamic comObject = null;

		#region IComContainer Members

		public bool Intialize(string control)
		{
			Type type = null;
			type = Type.GetTypeFromProgID(control);
			if (type == null)
			{
				try
				{
					type = Type.GetTypeFromCLSID(Guid.Parse(control));
				}
				catch
				{
				}
			}

			if (type == null)
				throw new Exception("Error, ComContainer was unable to create object from id '" + control + "'");

			comObject = Activator.CreateInstance(type);

			if (comObject == null)
				throw new Exception("Error, ComContainer was unable to create object from id '" + control + "'");

			return true;
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

			string cmd = "ComObject." + property;
			return Peach.Core.Scripting.EvalExpression(cmd, state);
		}

		public void SetProperty(string property, object value)
		{
			if (comObject == null)
				throw new Exception("Error, please call Initalize first!");

			Dictionary<string, object> state = new Dictionary<string, object>();
			state["ComObject"] = comObject;
			state["ComArg"] = value;

			string cmd = "ComObject." + property + " = ComArg";
			Scripting.EvalExpression(cmd, state);
		}

		#endregion
	}
}
