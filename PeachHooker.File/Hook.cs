
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

using EasyHook;

namespace PeachHooker.File
{
	public class Hook : EasyHook.IEntryPoint
	{
		public static Hook Context = null;
		Interface hookInterface;
		LocalHook RecvHook;
		LocalHook SendHook;
		Stack<HookQueue> Queue = new Stack<HookQueue>();

		// ReadFile
		// ReadFileEx


			//FILE *fopen( 
			//const char *filename,
			//const char *mode 
			//);
			//FILE *_wfopen( 
			//const wchar_t *filename,
			//const wchar_t *mode 
			//);
		// fopen
		// _wfopen

		// fread
		// _read

		//		int _open(
		//   const char *filename,
		//   int oflag [,
		//   int pmode] 
		//);
		//int _wopen(
		//   const wchar_t *filename,
		//   int oflag [,
		//   int pmode] 
		//);
		//errno_t fopen_s( 
		//FILE** pFile,
		//const char *filename,
		//const char *mode 
		//);
		//errno_t _wfopen_s(
		//FILE** pFile,
		//const wchar_t *filename,
		//const wchar_t *mode 
		//);

		//msvcr100.dll
		//msvcr100d.dll

		public Hook(RemoteHooking.IContext inContext, string inChannelName)
		{
			log("Hook()");
			hookInterface = RemoteHooking.IpcConnectClient<Interface>(inChannelName);
			hookInterface.Ping();
			Context = this;
		}

		public static void log(string foo)
		{
			System.IO.File.AppendAllText(@"c:\log.txt", foo + "\n");
		}

		public void Run(RemoteHooking.IContext inContext, string inChannelName)
		{
			log("Run()");

			try
			{
				RecvHook = LocalHook.Create(LocalHook.GetProcAddress("WS2_32.dll", "recv"),
					new DRecv(recv_Hooked), this);

				RecvHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });

				SendHook = LocalHook.Create(LocalHook.GetProcAddress("WS2_32.dll", "send"),
					new DSend(send_Hooked), this);
				
				SendHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
			}
			catch(Exception ex)
			{
				log(ex.ToString());
				hookInterface.ReportException(ex);
				return;
			}

			log("IsInstalled");
			hookInterface.IsInstalled(RemoteHooking.GetCurrentProcessId());

			log("Waking up process");
			RemoteHooking.WakeUpProcess();

			try
			{
				while (true)
				{
					Thread.Sleep(500);

					if (Queue.Count > 0)
					{
						HookQueue[] items;

						lock (Queue)
						{
							items = Queue.ToArray();
							Queue.Clear();
						}

						foreach (HookQueue item in items)
						{
							if (item.hookedCall == HookedCall.recv)
							{
								log("OnRecv");
								var data = hookInterface.OnRecv((byte[])item.parameters[1]);
								item.result = new object[] { data };
								item.resultReady.Set();
							}
							else
							{
								log("OnSend");
								var data = hookInterface.OnSend((byte[])item.parameters[1]);
								item.result = new object[] { data };
								item.resultReady.Set();
							}
						}
					}
					else
					{
						log("Ping");
						hookInterface.Ping();
					}
				}
			}
			catch (Exception ex)
			{
				log("ReportException: "+ ex.ToString());
				hookInterface.ReportException(ex);
				return;
			}
		}

		//int recv(
		//  __in   SOCKET s,
		//  __out  char *buf,
		//  __in   int len,
		//  __in   int flags
		//);
		//Ws2_32.dll
		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		delegate int DRecv(uint s, IntPtr buff, int len, int flags);

		[UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
		delegate int DSend(uint s, IntPtr buff, int len, int flags);

		[DllImport("WS2_32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		static extern int recv(uint s, IntPtr buff, int len, int flags);

		[DllImport("WS2_32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		static extern int send(uint s, IntPtr buff, int len, int flags);

		static int recv_Hooked(uint s, IntPtr dataPointer, int len, int flags)
		{
			log("recv_Hooked");

			log("recv_Hooked: calling recv: " + len);
			int ret = recv(s, dataPointer, len, flags);

			if (ret == 0 || ret > len)
			{
				log("recv_Hooked: returned 0 bytes");
				return 0;
			}

			try
			{
				Hook This = Hook.Context;

				log("recv_Hooked: marshaling dataPointer into buff");
				byte[] buff = new byte[ret];
				Marshal.Copy(dataPointer, buff, 0, ret);

				HookQueue entry = new HookQueue()
				{
					parameters = new object[] { s, buff, ret, flags },
					hookedCall = HookedCall.recv
				};

				lock (This.Queue)
				{
					This.Queue.Push(entry);
				}

				log("recv_Hooked: entry.resultReady.WaitOne()");
				entry.resultReady.WaitOne();

				if (entry.result != null && entry.result.Length == 1)
				{
					log("recv_Hooked: reciving returned data");

					Marshal.Copy((byte[])entry.result[0], 0, dataPointer, ret);
					return ret;
				}

				log("recv_Hooked: receiving origional data");
				return ret;
			}
			catch (Exception ex)
			{
				log("recv_Hooked: " + ex.ToString());
				return ret;
			}
		}

		static int send_Hooked(uint s, IntPtr dataPointer, int len, int flags)
		{
			try
			{
				log("send_Hooked");
				log("send_Hooked: marshaling dataPointer into buff");
				byte[] buff = new byte[len];
				Marshal.Copy(dataPointer, buff, 0, len);

				Hook This = Hook.Context;
				HookQueue entry = new HookQueue()
				{
					parameters = new object[] { s, buff, len, flags },
					hookedCall = HookedCall.send
				};

				lock (This.Queue)
				{
					This.Queue.Push(entry);
				}

				log("send_Hooked: entry.resultReady.WaitOne()");
				entry.resultReady.WaitOne();

				if (entry.result != null && entry.result.Length == 1)
				{
					log("send_Hooked: sending returned data");
					Marshal.Copy((byte[])entry.result[0], 0, dataPointer, len);
					return send(s, dataPointer, len, flags);
				}

				log("send_Hooked: sending origional");
				return send(s, dataPointer, len, flags);
			}
			catch (Exception ex)
			{
				log("send_Hooked: send_Hooked: " + ex.ToString());
				return send(s, dataPointer, len, flags);
			}
		}
	}

	enum HookedCall
	{
		unknown,
		recv,
		send
	}

	class HookQueue
	{
		public ManualResetEvent resultReady = new ManualResetEvent(false);
		public object[] parameters = null;
		public HookedCall hookedCall = HookedCall.unknown;
		public object[] result = null;
	}
}

// end
