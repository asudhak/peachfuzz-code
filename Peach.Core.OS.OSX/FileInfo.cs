using System;
using System.IO;

namespace Peach.Core
{
	[PlatformImpl(Platform.OS.OSX)]
	public class FileInfoImpl : IFileInfo
	{
		public Platform.Architecture GetArch(string fileName)
		{
			// As of right now all native peach code is universal, so we don't need
			// to conditionally switch how we function based on the architecture of
			// the target file.  At some point we should actually query the fileName
			// and return the appropriate architecture.
			return Platform.Architecture.x86;
		}
	}
}