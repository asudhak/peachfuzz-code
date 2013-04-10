using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Linq;
using System.IO;

namespace Peach.Core
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class PlatformImplAttribute : Attribute
	{
		public Platform.OS OS { get; private set; }

		public PlatformImplAttribute(Platform.OS OS)
		{
			this.OS = OS;
		}
	}

	public class PlatformFactory<T> where T : class
	{
		private static Type impl = FindImpl();

		private static Type FindImpl()
		{
			Platform.OS os = Platform.GetOS();
			Type type = typeof(T);
			var cls = ClassLoader.FindTypeByAttribute<PlatformImplAttribute>((t, a) => a.OS == os && (t.BaseType == type || t.GetInterfaces().Contains(type)));
			if (cls == null)
				throw new TypeLoadException("Could not find an instance of '" + type.FullName + "' for the " + os + " platform.");
			return cls;
		}

		public static T CreateInstance(params object[] args)
		{
			object obj = Activator.CreateInstance(impl, args);
			T ret = obj as T;
			return ret;
		}
	}

	public class StaticPlatformFactory<T> where T : class
	{
		public static T Instance { get { return instance; } }

		private static T instance = LoadInstance();

		private static T LoadInstance()
		{
			Platform.OS os = Platform.GetOS();
			Type type = typeof(T);
			var cls = ClassLoader.FindTypeByAttribute<PlatformImplAttribute>((t, a) => a.OS == os && t.GetInterfaces().Contains(type));
			if (cls == null)
				throw new TypeLoadException("Could not find an instance of '" + type.FullName + "' for the " + os + " platform.");
			object obj = Activator.CreateInstance(cls);
			T ret = obj as T;
			return ret;
		}
	}

	/// <summary>
	/// Helper class to determine the OS/Platform we are on.  The built in 
	/// method returns incorrect results.
	/// </summary>
	public static class Platform
	{
		public enum OS { None = 0, Windows = 1, OSX = 2, Linux = 4, Unix = 6, All = 7 };
		public enum Architecture { x64, x86 };

		public static Architecture GetArch()
		{
			return _arch;
		}

		public static OS GetOS()
		{
			return _os;
		}

		public static void LoadAssembly()
		{
			//if (!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem)
			//	throw new PeachException("Error: Cannot use the 32bit version of Peach 3 on a 64bit operating system.");

			//if (Environment.Is64BitProcess && !Environment.Is64BitOperatingSystem)
			//	throw new PeachException("Error: Cannot use the 64bit version of Peach 3 on a 32bit operating system.");

			string osAssembly = null;

			switch (Platform.GetOS())
			{
				case Platform.OS.OSX:
					osAssembly = "Peach.Core.OS.OSX.dll";
					break;
				case Platform.OS.Linux:
					osAssembly = "Peach.Core.OS.Linux.dll";
					break;
				case Platform.OS.Windows:
					osAssembly = "Peach.Core.OS.Windows.dll";
					break;
			}

			try
			{
				ClassLoader.LoadAssembly(osAssembly);
			}
			catch (Exception ex)
			{
				throw new PeachException(string.Format("Error, could not load platform assembly '{0}'.  {1}", osAssembly, ex.Message), ex);
			}
		}

		static Architecture _arch = _GetArch();

		static Architecture _GetArch()
		{
			if (IntPtr.Size == 64)
				return Architecture.x64;

			return Architecture.x86;
		}

		static OS _os = _GetOS();

		static OS _GetOS()
		{
			if (System.IO.Path.DirectorySeparatorChar == '\\')
				return Platform.OS.Windows;
			if (IsRunningOnMac())
				return OS.OSX;
			if (System.Environment.OSVersion.Platform == PlatformID.Unix)
				return OS.Linux;
			return OS.None;
		}

		[DllImport("libc")]
		static extern int uname(IntPtr buf);

		//From Managed.Windows.Forms/XplatUI
		static bool IsRunningOnMac()
		{
			IntPtr buf = IntPtr.Zero;
			try
			{
				buf = Marshal.AllocHGlobal(8192);
				// This is a hacktastic way of getting sysname from uname ()
				if (uname(buf) == 0)
				{
					string os = Marshal.PtrToStringAnsi(buf);
					if (os == "Darwin") return true;
				}
			}
			catch
			{
			}
			finally
			{
				if (buf != IntPtr.Zero) Marshal.FreeHGlobal(buf);
			}
			return false;
		}
	}
}
