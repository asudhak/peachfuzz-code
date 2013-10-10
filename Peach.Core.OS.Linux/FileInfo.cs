using System;
using System.IO;

namespace Peach.Core
{
	[PlatformImpl(Platform.OS.Linux)]
	public class FileInfoImpl : IFileInfo
	{
		enum MachineType : ushort
		{
			EM_386 = 0x03,
			IMAGE_FILE_MACHINE_I386 = 0x14c,
			IMAGE_FILE_MACHINE_IA64 = 0x200,
		}


		public Platform.Architecture GetArch(string fileName)
		{
			return GetMachineType(fileName);
		}

		public static Platform.Architecture GetMachineType(string fileName)
		{
			// First 16 bytes are elf identification
			using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				var br = new BinaryReader(fs);
				fs.Seek(0, SeekOrigin.Begin);
				var ei_magic = br.ReadUInt32();
				if (ei_magic != 0x464c457f) // "\x7fELF", little-endian
					throw new PeachException(fileName + " does not contain a valid ELF header.");
				fs.Seek(18, SeekOrigin.Begin);
				var ei_machine = br.ReadUInt16();
				switch (ei_machine)
				{
					case 0x03:
						return Platform.Architecture.x86;
					case 0x3e:
						return Platform.Architecture.x64;
					default:
						throw new PeachException("{0} has unrecognized machine type 0x{1:X}.".Fmt(fileName, ei_machine));
				}
			}
		}
	}
}