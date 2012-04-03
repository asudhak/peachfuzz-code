using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;
using EasyHook;

namespace PeachHooker.Network
{
	public abstract class Interface : MarshalByRefObject
	{
		public abstract void IsInstalled(Int32 clientPid);

		public abstract byte[] OnRecv(byte[] data);

		public abstract byte[] OnSend(byte[] data);

		public abstract void ReportException(Exception ex);

		public abstract void Ping();
	}
}

// end
