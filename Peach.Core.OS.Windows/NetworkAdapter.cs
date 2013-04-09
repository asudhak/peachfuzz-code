using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;

namespace Peach.Core
{
	[PlatformImpl(Platform.OS.Windows)]
	public class NetworkAdapterImpl : NetworkAdapter
	{
		uint? index;
		ManagementObject obj;

		public NetworkAdapterImpl(string name)
			: base(name)
		{
			string desc = null;

			foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (iface.Name == name)
				{
					desc = iface.Id;
					break;
				}
			}

			if (desc == null)
				throw new ArgumentException("Could not find GUID for adapter named '" + name + "'.");

			string wmiQuery1 = "SELECT Index, InterfaceIndex FROM Win32_NetworkAdapter WHERE GUID LIKE '{0}'".Fmt(desc);
			using (var searcher = new ManagementObjectSearcher(wmiQuery1))
			{
				foreach (ManagementObject item in searcher.Get())
				{
					index = (uint)item["InterfaceIndex"];
					item.Dispose();
				}
			}

			// Loopback interfaces do not have an underlying physical adapter
			if (!index.HasValue)
				return;

			string wmiQuery2 = "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE InterfaceIndex LIKE '{0}'".Fmt(index);
			using (var searcher = new ManagementObjectSearcher(wmiQuery2))
			{
				foreach (ManagementObject item in searcher.Get())
				{
					if (obj == null)
						obj = item;
					else
						item.Dispose();
				}
			}

			if (obj == null)
				throw new ArgumentException("Could not find adapter configuration for '" + name + "'.");
		}

		public override void Dispose()
		{
			if (obj != null)
			{
				obj.Dispose();
				obj = null;
			}
		}

		public override uint? MTU
		{
			get
			{
				if (index.HasValue && obj == null)
					throw new ObjectDisposedException("NetworkAdapterImpl");

				if (obj == null)
					return null;

				object mtu = obj["MTU"];

				if (mtu == null)
					return null;

				return (uint)mtu;
			}
			set
			{
				if (index.HasValue && obj == null)
					throw new ObjectDisposedException("NetworkAdapterImpl");

				if (obj == null)
				{
					if (value.HasValue)
						throw new NotSupportedException("MTU changes are not supported on interface '" + Name + "'.");

					return;
				}

				if (value.HasValue)
					obj.SetPropertyValue("MTU", (uint)value);
				else
					obj.SetPropertyValue("MTU", null);
			}
		}
	}
}
