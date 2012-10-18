
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
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("PopupWatcher")]
	[Parameter("WindowNames", typeof(string), "Window names separated by a ';'.  Defaults to all.", false)]
	[Parameter("Fault", typeof(bool), "Should we fault when a window is found?", false)]
	public class PopupWatcher : Monitor
	{
		delegate bool EnumDelegate(IntPtr hWnd, int lParam);

		[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
		static extern int GetWindowText(IntPtr hWnd, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int nLen);
		[DllImport("user32.dll", SetLastError = true)]
		static extern bool EnumWindows(EnumDelegate lpEnumFunc, IntPtr lParam);
		[DllImport("user32.dll", SetLastError = true)]
		static extern bool EnumChildWindows(IntPtr hWndParent, EnumDelegate lpEnumFunc, int lParam);
		[DllImport("user32.Dll")]
		static extern int PostMessage(IntPtr hWnd, UInt32 msg, int wParam, int lParam);
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
		static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

		private const UInt32 WM_CLOSE = 0x0010;

		List<string> _windowNames = new List<string>();
		bool _fault = false;
		Thread _worker = null;
		bool _workerStop = false;

		public PopupWatcher(string name, Dictionary<string, Variant> args)
			: base(name, args)
		{
			_windowNames.AddRange(((string)args["WindowNames"]).Split(';'));

			if (args.ContainsKey("Fault"))
				_fault = ((string)args["Fault"]).ToLower() == "true";
		}

		bool EnumHandler(IntPtr hWnd, int lParam)
		{
			try
			{
				StringBuilder strbTitle = new StringBuilder(255);
				int nLength = GetWindowText(hWnd, strbTitle, strbTitle.Capacity);
				string strTitle = strbTitle.ToString();

				foreach (string windowName in _windowNames)
				{
					if (windowName.IndexOf("Fuzz Bang") > -1)
						continue;

					if (strTitle.IndexOf(windowName) > -1)
					{
						PostMessage(hWnd, WM_CLOSE, 0, 0);
						SendMessage(hWnd, WM_CLOSE, 0, 0);

						return false;
					}
				}

				//if (lParam == 0)
				//{
				//    // Recursively check child windows
				//    EnumChildWindows(hWnd, new EnumDelegate(EnumHandler), 1);
				//}

				return true;
			}
			catch
			{
				return false;
			}
		}

		public void Work()
		{
			EnumDelegate filter = new EnumDelegate(EnumHandler);

			while (_workerStop == false)
			{
				// Find top level windows
				EnumWindows(filter, IntPtr.Zero);

				// Lets not hog the cpu
				Thread.Sleep(200);
			}
		}

		public override void StopMonitor()
		{
			if (_worker != null)
			{
				_worker.Abort();
				_worker.Join();
				_worker = null;
				_workerStop = false;
			}
		}

		public override void SessionStarting()
		{
			if (_worker != null)
			{
				_worker.Abort();
				_worker.Join();
				_worker = null;
				_workerStop = false;
			}

			_worker = new Thread(new ThreadStart(Work));
			_worker.Start();
		}

		public override void SessionFinished()
		{
			_workerStop = true;
			_worker.Join();
			_worker = null;
			_workerStop = false;
		}

		public override bool DetectedFault()
		{
			return false;
		}


		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
		}

		public override bool IterationFinished()
		{
			return false;
		}

		public override Fault GetMonitorData()
		{
            return null;
		}

		public override bool MustStop()
		{
			return false;
		}

		public override Variant Message(string name, Variant data)
		{
			return null;
		}
	}
}

