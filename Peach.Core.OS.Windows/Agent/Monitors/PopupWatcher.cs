
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
	[Monitor("PopupWatcher", true)]
	[Parameter("WindowNames", typeof(string[]), "Window names separated by a ','")]
	[Parameter("Fault", typeof(bool), "Trigger fault when a window is found", "false")]
	public class PopupWatcher : Monitor
	{
		public string[] WindowNames { get; private set; }
		public bool Fault { get; private set; }

		#region P/Invokes

		delegate bool EnumDelegate(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern int GetWindowTextLength(IntPtr hWnd);
		[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
		static extern int GetWindowText(IntPtr hWnd, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int nLen);
		[DllImport("user32.dll", SetLastError = true)]
		static extern bool EnumWindows(EnumDelegate lpEnumFunc, IntPtr lParam);
		[DllImport("user32.dll", SetLastError = true)]
		static extern bool EnumChildWindows(IntPtr hWndParent, EnumDelegate lpEnumFunc, IntPtr lParam);
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern int PostMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		private const uint WM_CLOSE = 0x0010;

		#endregion

		Thread _worker = null;
		ManualResetEvent _event = null;

		SortedSet<string> _closedWindows = new SortedSet<string>();
		object _lock = new object();
		bool _continue = true;
		Fault _fault = null;

		public PopupWatcher(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);
		}

		bool EnumHandler(IntPtr hWnd, IntPtr lParam)
		{
			int nLength = GetWindowTextLength(hWnd);
			if (nLength == 0)
				return _continue;

			StringBuilder strbTitle = new StringBuilder(nLength + 1);
			nLength = GetWindowText(hWnd, strbTitle, strbTitle.Capacity);
			if (nLength == 0)
				return _continue;

			string strTitle = strbTitle.ToString();

			foreach (string windowName in WindowNames)
			{
				if (strTitle.IndexOf(windowName) > -1)
				{
					PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
					SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

					lock (_lock)
					{
						_closedWindows.Add(strTitle);
					}

					_continue = false;
					return _continue;
				}
			}

			// Recursively check child windows
			EnumChildWindows(hWnd, EnumHandler, IntPtr.Zero);

			return _continue;
		}


		public void Work()
		{
			do
			{
				// Reset continue for subsequent enum call
				_continue = true;

				// Find top level windows
				EnumWindows(EnumHandler, IntPtr.Zero);
			}
			while (!_event.WaitOne(200));
		}

		public override void StopMonitor()
		{
			if (_worker != null)
			{
				_event.Set();

				_worker.Join();
				_worker = null;

				_event.Close();
				_event = null;
			}
		}

		public override void SessionStarting()
		{
			StopMonitor();

			_event = new ManualResetEvent(false);

			_worker = new Thread(new ThreadStart(Work));
			_worker.Start();
		}

		public override void SessionFinished()
		{
			StopMonitor();
		}

		public override bool DetectedFault()
		{
			return _fault != null && _fault.type == FaultType.Fault;
		}


		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
		}

		public override bool IterationFinished()
		{
			_fault = null;

			lock (_lock)
			{
				if (_closedWindows.Count > 0)
				{
					_fault = new Fault();
					_fault.detectionSource = "PopupWatcher";
					_fault.type = Fault ? FaultType.Fault : FaultType.Data;
					_fault.title = string.Format("Closed {0} popup window{1}.", _closedWindows.Count, _closedWindows.Count > 1 ? "s" : "");
					_fault.description = "Window Titles:" + Environment.NewLine + string.Join(Environment.NewLine, _closedWindows);
				}

				_closedWindows.Clear();
			}

			return false;
		}

		public override Fault GetMonitorData()
		{
			return _fault;
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

