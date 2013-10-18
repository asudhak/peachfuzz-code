using System;
using Peach.Core.Debuggers.WindowsSystem;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Peach.Core.OS.Windows
{
	public class Privilege : IDisposable
	{
		public static string SeDebugPrivilege = "SeDebugPrivilege";

		private IntPtr hToken;
		private string name;

		public Privilege(string name)
		{
			this.name = name;

			IntPtr hThread = UnsafeMethods.GetCurrentThread();

			if (!UnsafeMethods.OpenThreadToken(hThread, UnsafeMethods.TOKEN_ADJUST_PRIVILEGES | UnsafeMethods.TOKEN_QUERY, false, out hToken))
			{
				int error = Marshal.GetLastWin32Error();

				if (error != UnsafeMethods.ERROR_NO_TOKEN)
					throw new Win32Exception(error);

				if (!UnsafeMethods.ImpersonateSelf(UnsafeMethods.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation))
				{
					error = Marshal.GetLastWin32Error();
					throw new Win32Exception(error);
				}

				if (!UnsafeMethods.OpenThreadToken(hThread, UnsafeMethods.TOKEN_ADJUST_PRIVILEGES | UnsafeMethods.TOKEN_QUERY, false, out hToken))
				{
					error = Marshal.GetLastWin32Error();
					throw new Win32Exception(error);
				}
			}

			if (!SetPrivilege(true))
			{
				int error = Marshal.GetLastWin32Error();
				UnsafeMethods.CloseHandle(hToken);
				hToken = IntPtr.Zero;
				throw new Win32Exception(error);
			}
		}

		public void Dispose()
		{
			if (IntPtr.Zero != hToken)
			{
				SetPrivilege(false);
				UnsafeMethods.CloseHandle(hToken);
				hToken = IntPtr.Zero;
			}
		}

		private bool SetPrivilege(bool bEnablePrivilege)
		{
			UnsafeMethods.TOKEN_PRIVILEGES tp;
			UnsafeMethods.LUID luid;

			if (!UnsafeMethods.LookupPrivilegeValue(null, name, out luid))
				return false;

			tp.PrivilegeCount = 1;
			tp.Luid = luid;
			tp.Attributes = bEnablePrivilege ? UnsafeMethods.SE_PRIVILEGE_ENABLED : 0;

			// Enable the privilege or disable all privileges.

			if (!UnsafeMethods.AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
				return false;

			int err = Marshal.GetLastWin32Error();

			if (err == UnsafeMethods.ERROR_NOT_ALL_ASSIGNED)
				return false;

			return true;
		}
	}
}
