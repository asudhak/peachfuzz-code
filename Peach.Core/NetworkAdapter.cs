using System;
using Peach.Core;

namespace Peach.Core
{
	/// <summary>
	/// Helper class to control properties of a network adapter.
	/// </summary>
	public abstract class NetworkAdapter : PlatformFactory<NetworkAdapter>, IDisposable
	{
		public NetworkAdapter(string name) { Name = name; }

		public abstract void Dispose();

		public abstract uint? MTU { get; set; }

		public string Name { get; private set; }
	}
}
