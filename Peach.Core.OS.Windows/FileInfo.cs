using System;
using System.IO;

namespace Peach.Core
{
	[PlatformImpl(Platform.OS.Windows)]
	public class FileInfoImpl : IFileInfo
	{
		enum MachineType : ushort
		{
			IMAGE_FILE_MACHINE_AMD64 = 0x8664,
			IMAGE_FILE_MACHINE_I386 = 0x14c,
			IMAGE_FILE_MACHINE_IA64 = 0x200,
		}

		public Platform.Architecture GetArch(string fileName)
		{
			return GetMachineType(fileName);
		}

		public static Platform.Architecture GetMachineType(string fileName)
		{
			//see http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
			//offset to PE header is always at 0x3C
			//PE header starts with "PE\0\0" =  0x50 0x45 0x00 0x00
			//followed by 2-byte machine type field (see document above for enum)
			using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				var br = new BinaryReader(fs);
				fs.Seek(0x3c, SeekOrigin.Begin);
				var peOffset = br.ReadInt32();
				fs.Seek(peOffset, SeekOrigin.Begin);
				var peHead = br.ReadUInt32();
				if (peHead != 0x00004550) // "PE\0\0", little-endian
					throw new PeachException(fileName + " does not contain a valid PE header.");
				var machineType = br.ReadUInt16();

				switch (machineType)
				{
					case (ushort)MachineType.IMAGE_FILE_MACHINE_AMD64:
						return Platform.Architecture.x64;
					case (ushort)MachineType.IMAGE_FILE_MACHINE_I386:
						return Platform.Architecture.x86;
					default:
						throw new PeachException("{0} has unrecognized machine type 0x{1:X}.".Fmt(fileName, machineType));
				}
			}
		}
	}
}