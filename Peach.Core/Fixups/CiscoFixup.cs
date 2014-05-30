using System;
using System.Collections.Generic;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Fixups.Libraries
{
	/// <summary>
	/// Computes the checksum in Host order for an array of bytes and
	/// compensates for Cisco off-by-one.
	/// </summary>
	public class CiscoCDPChecksum : InternetChecksum
	{
		public CiscoCDPChecksum()
		{
		}

		public override void Update(byte[] buf, int offset, int count)
		{
			int i = offset;
			for (; i < count - 1; i += 2)
				sum += (uint)((buf[i] << 8) + buf[i + 1]);

			if (i != count)
			{
				if ((buf[count - 1] & 0x80) != 0)
					sum += (uint)((buf[count - 1] - 1) | 0xff00);
				else
					sum += (uint)(buf[count - 1]);
			}
		}
	}


	[Fixup("CiscoCdpChecksum", true)]
	[Fixup("CiscoFixup")]
	[Parameter("ref", typeof(DataElement), "Reference to data element")]
	[Serializable]
	public class CiscoFixup : Fixup
	{
		public CiscoFixup(DataElement parent, Dictionary<string, Variant> args)
			: base(parent, args, "ref")
		{
		}

		protected override Variant fixupImpl()
		{
			var elem = elements["ref"];
			var data = elem.Value;

			System.Diagnostics.Debug.Assert((BitwiseStream.BlockCopySize % 2) == 0);
			var buf = new byte[BitwiseStream.BlockCopySize];
			var sum = new CiscoCDPChecksum();
			data.Seek(0, System.IO.SeekOrigin.Begin);

			int nread;
			while ((nread = data.Read(buf, 0, buf.Length)) != 0)
				sum.Update(buf, 0, nread);

			return new Variant(sum.Final());
		}

	}
}