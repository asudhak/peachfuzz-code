using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Fixups.Libraries
{
	/// <summary>
	/// Computes the checksum in Host order for an array of bytes
	/// </summary>
	public class InternetChecksum
	{
		protected uint sum = 0;

		public InternetChecksum()
		{
		}

		public virtual void Update(uint value)
		{
			sum += value;
		}

		public virtual void Update(byte[] buf, int offset, int count)
		{
			int i = offset;
			for (; i < count - 1; i += 2)
				sum += (uint)((buf[i] << 8) + buf[i + 1]);

			if (i != count)
				sum += (uint)(buf[count - 1] << 8);
		}

		public virtual ushort Final()
		{
			sum = (sum >> 16) + (sum & 0xffff);
			sum += (sum >> 16);
			return (ushort)~sum;
		}
	}

	/// <summary>
	/// Base class for internet checksum fixups
	/// </summary>
	[Serializable]
	public abstract class InternetFixup : Fixup
	{
		// Needed for ParameterParser to work
		static void Parse(string str, out DataElement val)
		{
			val = null;
		}

		// Needed for ParameterParser to work
		protected IPAddress src { get; set; }
		protected IPAddress dst { get; set; }
		protected DataElement _ref { get; set; }

		protected byte[] srcAddress;
		protected byte[] dstAddress;

		protected virtual bool AddLength { get { return false; } }
		protected virtual ushort Protocol { get { return 0; } }

		public InternetFixup(DataElement parent, Dictionary<string, Variant> args, params string[] refs)
			: base(parent, args, refs)
		{
			ParameterParser.Parse(this, args);

			srcAddress = src != null ? src.GetAddressBytes() : new byte[0];
			dstAddress = dst != null ? dst.GetAddressBytes() : new byte[0];
		}

		protected override Variant fixupImpl()
		{
			var elem = elements["ref"];
			var data = elem.Value;

			InternetChecksum sum = new InternetChecksum();

			sum.Update(srcAddress, 0, srcAddress.Length);
			sum.Update(dstAddress, 0, dstAddress.Length);
			sum.Update(Protocol);

			if (AddLength)
				sum.Update((uint)data.Length);

			System.Diagnostics.Debug.Assert((BitwiseStream.BlockCopySize % 2) == 0);
			var buf = new byte[BitwiseStream.BlockCopySize];
			data.Seek(0, System.IO.SeekOrigin.Begin);

			int nread;
			while ((nread = data.Read(buf, 0, buf.Length)) != 0)
				sum.Update(buf, 0, nread);

			return new Variant(sum.Final());
		}

	}

}