
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

namespace PeachHooker.Network
{
	public class Hook : EasyHook.IEntryPoint
	{
		Interface hookInterface;
		LocalHook RecvHook;
		LocalHook SendHook;
		Stack<HookQueue> Queue = new Stack<HookQueue>();

		public Hook(RemoteHooking.IContext inContext, string inChannelName)
		{
			log("Hook()");
			hookInterface = RemoteHooking.IpcConnectClient<Interface>(inChannelName);
			hookInterface.Ping();
		}

		public static void log(string foo)
		{
			File.AppendAllText(@"c:\log.txt", foo + "\n");
		}

		public void Run(RemoteHooking.IContext inContext, string inChannelName)
		{
			log("Run()");

			try
			{
				RecvHook = LocalHook.Create(LocalHook.GetProcAddress("WS2_32.dll", "recv"),
					new DRecv(recv_Hooked), this);

				if (LocalHook.GetProcAddress("WS2_32.dll", "recv") == IntPtr.Zero)
					log("Run(): recv address is zero.");
				if (LocalHook.GetProcAddress("WS2_32.dll", "send") == IntPtr.Zero)
					log("Run(): send address is zero.");

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
		[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true)]
		delegate int DRecv(uint s, ref byte[] buff, int len, int flags);

		[UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true)]
		delegate int DSend(uint s, byte[] buff, int len, int flags);

		[DllImport("WS2_32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		static extern int recv(uint s, ref byte[] buff, int len, int flags);

		[DllImport("WS2_32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		static extern int send(uint s, byte[] buff, int len, int flags);

		static int recv_Hooked(uint s, ref byte[] buff, int len, int flags)
		{
			log("recv_Hooked");

			log("calling recv: " + len);
			int ret = recv(s, ref buff, len, flags);
			log("recv returned: " + ret);

			if (ret == 0)
				return 0;

			try
			{

				Hook This = (Hook)HookRuntimeInfo.Callback;
				HookQueue entry = new HookQueue()
				{
					parameters = new object[] { s, buff, ret, flags },
					hookedCall = HookedCall.recv
				};

				lock (This.Queue)
				{
					This.Queue.Push(entry);
				}

				log("entry.resultReady.WaitOne()");
				entry.resultReady.WaitOne();

				if (entry.result != null && entry.result.Length == 1)
				{
					log("reciving returned data");

					for (int i = 0; i < buff.Length; i++)
						buff[i] = ((byte[])entry.result[0])[i];

					return ret;
				}

				log("receiving origional data");
				return ret;
			}
			catch (Exception ex)
			{
				// TODO - Log exception
				log("recv_Hooked: " + ex.ToString());
				return ret;
			}
		}

		static int send_Hooked(uint s, byte[] buff, int len, int flags)
		{
			log("send_Hooked");
			try
			{
				Hook This = (Hook)HookRuntimeInfo.Callback;
				HookQueue entry = new HookQueue()
				{
					parameters = new object[] { s, buff, len, flags },
					hookedCall = HookedCall.send
				};

				lock (This.Queue)
				{
					This.Queue.Push(entry);
				}

				log("entry.resultReady.WaitOne()");
				entry.resultReady.WaitOne();

				if (entry.result != null && entry.result.Length == 1)
				{
					log("sending returned data");
					return send(s, (byte[])entry.result[0], ((byte[])entry.result[0]).Length, flags);
				}

				log("sending origional");
				return send(s, buff, len, flags);
			}
			catch (Exception ex)
			{
				// TODO - Log exception
				log("send_Hooked: " + ex.ToString());
				return send(s, buff, len, flags);
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
