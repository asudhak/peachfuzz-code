using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Peach.Core.Fixups.Libraries
{
	class InternetFixup
	{


		private uint _checksum;
		private bool _IPv6;

		public InternetFixup()
		{
			_checksum = 0;
			_IPv6 = false;
		}

		private static ushort ChecksumConvertToUInt16(byte[] value, int startIndex)
		{

			if (BitConverter.IsLittleEndian)
				return System.BitConverter.ToUInt16(value.Reverse().ToArray(), value.Length - sizeof(UInt16) - startIndex);
	        return System.BitConverter.ToUInt16(value, startIndex);
		}

		public bool isIPv6()
		{
			return _IPv6;
		}

		public bool ChecksumAddAddress(string address)
		{
			byte[] addressBytes;
			IPAddress addressObject;
			if (IPAddress.TryParse(address, out addressObject))
			{
				if (addressObject.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
					_IPv6 = true;
				addressBytes = addressObject.GetAddressBytes();
			}
			else
			{
				throw new PeachException(address + " is not a valid address and could not be parsed by the fixup.");
			}

			for (int i = 0; i < addressBytes.Length; i = i + 2)
			{
				_checksum += ChecksumConvertToUInt16(addressBytes, i);
			}
			return true;
		}

		public bool ChecksumAddPseudoHeader(byte[] data)
		{
			if (data.Length % 2 == 1)
			{
				throw new PeachException("All pseudo header values in InternetFixup must have an even number of bytes");
			}
			else
			{
				return ChecksumAddPayload(data);
			}
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
			return (ushort)~(_checksum + (ushort)(_checksum >> 16));
		}
	}
}