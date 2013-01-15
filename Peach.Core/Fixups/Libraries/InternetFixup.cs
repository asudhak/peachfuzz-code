using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core.Fixups.Libraries
{
	class InternetFixup
	{


		private uint _checksum;

		public InternetFixup()
		{
			_checksum = 0;
		}

		private static ushort ChecksumConvertToUInt16(byte[] value, int startIndex)
		{

			if (BitConverter.IsLittleEndian)
				return System.BitConverter.ToUInt16(value.Reverse().ToArray(), value.Length - sizeof(UInt16) - startIndex);
	        return System.BitConverter.ToUInt16(value, startIndex);
		}

		public bool ChecksumAddPayload(byte[] data)
		{
			byte[] payload;
			if (data.Length % 2 == 1)
			{
				payload = new byte[data.Length + 1];
				data.CopyTo(payload, 0);
				payload[data.Length] = 0;
			}
			else
			{
				payload = data;
			}
			for (int i = 0; i < payload.Length; i = i + 2)
			{
				_checksum += ChecksumConvertToUInt16(payload, i);
			}
			return true;
		}

		public ushort ChecksumFinal()
		{
			ushort final = (ushort)~(_checksum + (ushort)(_checksum >> 16));
			if (BitConverter.IsLittleEndian)
			{
				byte[] endianArray = BitConverter.GetBytes(final);
				Array.Reverse(endianArray);
				final = BitConverter.ToUInt16(endianArray, 0);
			}
			return final;
		}
	}
}