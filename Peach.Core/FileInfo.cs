using System;

namespace Peach.Core
{
	public interface IFileInfo
	{
		Platform.Architecture GetArch(string fileName);
	}

	public class FileInfo : StaticPlatformFactory<IFileInfo>
	{
	}
}