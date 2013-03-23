
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
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Peach.Core.Dom;

namespace Peach.Core.Agent.Monitors
{
	/// <summary>
	/// Monitor will use OS X's built in CrashReporter (similar to watson)
	/// to detect and report crashes.
	/// </summary>
	[Monitor("CrashReporter", true)]
	[Monitor("osx.CrashReporter")]
	[Parameter("ProcessName", typeof(string), "Process name to watch for (defaults to all)", "")]
	public class CrashReporter : Monitor, IDisposable
	{
		private Regex _regex = new Regex("Saved crash report for (.*)\\[\\d+\\] version .*? to (.*)");
		private string _processName = null;
		private string _lastTime = "0";
		private IntPtr _asl = IntPtr.Zero;
		private string[] _crashLogs = null;

		public CrashReporter(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			if (args.ContainsKey("ProcessName"))
				this._processName = (string)args["ProcessName"];

			this._asl = asl_new(asl_type.ASL_TYPE_QUERY);
			if (this._asl == IntPtr.Zero)
				throw new PeachException("Couldn't open ASL handle.");
		}

		~CrashReporter()
		{
			Dispose();
		}
		
		public void Dispose()
		{
			if (this._asl != IntPtr.Zero)
			{
				asl_free(this._asl);
				this._asl = IntPtr.Zero;
				GC.SuppressFinalize(this);
			}
		}

		private string[] GetCrashLogs()
		{
			List<string> ret = new List<string>();
			int err;
			
			err = asl_set_query(this._asl, ASL_KEY_SENDER, "ReportCrash", asl_query_op.ASL_QUERY_OP_EQUAL);
			if (err != 0)
				throw new Exception();
			
			err = asl_set_query(this._asl, ASL_KEY_TIME, this._lastTime, asl_query_op.ASL_QUERY_OP_GREATER);
			if (err != 0)
				throw new Exception();
			
			IntPtr response = asl_search(IntPtr.Zero, this._asl);
			if (response != IntPtr.Zero)
			{
				IntPtr msg;
				
				while (IntPtr.Zero != (msg = aslresponse_next(response)))
				{
					IntPtr time = asl_get(msg, "Time");
					IntPtr message = asl_get(msg, "Message");
					
					// TODO: Log
					if (time == IntPtr.Zero || message == IntPtr.Zero)
						continue;
					
					//Saved crash report for CrashingProgram\[22774\] version ??? (???) to /path/to/crash.crash
					
					this._lastTime = Marshal.PtrToStringAnsi(time);
					string value = Marshal.PtrToStringAnsi(message);
					
					Match match = this._regex.Match(value);
					if (match.Success)
					{
						if (this._processName == null || match.Groups[1].Value == this._processName)
							ret.Add(match.Groups[2].Value);
					}
				}
				
				aslresponse_free(response);
			}
			
			return ret.ToArray();
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			this._crashLogs = null;
		}

		public override bool DetectedFault()
		{
			// Method will get called multiple times
			// we only want to pause the first time.
			if (this._crashLogs == null)
			{
				// Wait for CrashReporter to report!
				Thread.Sleep(500);
				this._crashLogs = GetCrashLogs();
			}

			return this._crashLogs.Length > 0;
		}

		public override Fault GetMonitorData()
		{
			if (!DetectedFault())
				return null;

			Fault fault = new Fault();
			fault.detectionSource = "CrashReporter";
			fault.folderName = "CrashReporter";
			fault.type = FaultType.Fault;
			fault.description = _processName + " crash report.";

			foreach (string file in _crashLogs)
			{
				string key = Path.GetFileName(file);
				fault.collectedData[key] = File.ReadAllBytes(file);
			}

			return fault;
		}

		public override bool MustStop()
		{
			return false;
		}

		public override void StopMonitor()
		{
		}

		public override void SessionStarting()
		{
			// Skip past any old messages in the log
			GetCrashLogs();
		}

		public override void SessionFinished()
		{
		}

		public override bool IterationFinished()
		{
			return true;
		}

		public override Variant Message(string name, Variant data)
		{
			return null;
		}

#region ASL P/Invokes
		private enum asl_type : uint
		{
			ASL_TYPE_MSG = 0,
			ASL_TYPE_QUERY = 1
		};
		
		[Flags]
		private enum asl_query_op : uint
		{
			ASL_QUERY_OP_CASEFOLD      = 0x0010,
			ASL_QUERY_OP_PREFIX        = 0x0020,
			ASL_QUERY_OP_SUFFIX        = 0x0040,
			ASL_QUERY_OP_SUBSTRING     = 0x0060,
			ASL_QUERY_OP_NUMERIC       = 0x0080,
			ASL_QUERY_OP_REGEX         = 0x0100,
			
			ASL_QUERY_OP_EQUAL         = 0x0001,
			ASL_QUERY_OP_GREATER       = 0x0002,
			ASL_QUERY_OP_GREATER_EQUAL = 0x0003,
			ASL_QUERY_OP_LESS          = 0x0004,
			ASL_QUERY_OP_LESS_EQUAL    = 0x0005,
			ASL_QUERY_OP_NOT_EQUAL     = 0x0006,
			ASL_QUERY_OP_TRUE          = 0x0007,
		};
		
		private static string ASL_KEY_TIME        { get { return "Time"; } }          /* Timestamp.  Set automatically */
		private static string ASL_KEY_TIME_NSEC   { get { return "TimeNanoSec"; } }   /* Nanosecond time. */
		private static string ASL_KEY_HOST        { get { return "Host"; } }          /* Sender's address (set by the server). */
		private static string ASL_KEY_SENDER      { get { return "Sender"; } }        /* Sender's identification string.  Default is process name. */
		private static string ASL_KEY_FACILITY    { get { return "Facility"; } }      /* Sender's facility.  Default is "user". */
		private static string ASL_KEY_PID         { get { return "PID"; } }           /* Sending process ID encoded as a string.  Set automatically. */
		private static string ASL_KEY_UID         { get { return "UID"; } }           /* UID that sent the log message (set by the server). */
		private static string ASL_KEY_GID         { get { return "GID"; } }           /* GID that sent the log message (set by the server). */
		private static string ASL_KEY_LEVEL       { get { return "Level"; } }         /* Log level number encoded as a string.  See levels above. */
		private static string ASL_KEY_MSG         { get { return "Message"; } }       /* Message text. */
		private static string ASL_KEY_READ_UID    { get { return "ReadUID"; } }       /* User read access (-1 is any group). */
		private static string ASL_KEY_READ_GID    { get { return "ReadGID"; } }       /* Group read access (-1 is any group). */
		private static string ASL_KEY_EXPIRE_TIME { get { return "ASLExpireTime"; } } /* Expiration time for messages with long TTL. */
		private static string ASL_KEY_MSG_ID      { get { return "ASLMessageID"; } }  /* 64-bit message ID number (set by the server). */
		private static string ASL_KEY_SESSION     { get { return "Session"; } }       /* Session (set by the launchd). */
		private static string ASL_KEY_REF_PID     { get { return "RefPID"; } }        /* Reference PID for messages proxied by launchd */
		private static string ASL_KEY_REF_PROC    { get { return "RefProc"; } }       /* Reference process for messages proxied by launchd */
		
		[DllImport("libc")]
		// int asl_send(aslclient asl, aslmsg msg);
		private static extern IntPtr asl_new(asl_type type);
		
		[DllImport("libc")]
		// void asl_free(aslmsg msg);
		private static extern void asl_free(IntPtr msg);
		
		[DllImport("libc")]
		// int asl_set_query(aslmsg msg, const char *key, const char *value, uint32_t op);
		private static extern int asl_set_query(IntPtr msg, [MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPStr)] string value, asl_query_op op);
		
		[DllImport("libc")]
		// aslresponse asl_search(aslclient asl, aslmsg msg);
		private static extern IntPtr asl_search(IntPtr asl, IntPtr msg);
		
		[DllImport("libc")]
		// void aslresponse_free(aslresponse r);
		private static extern void aslresponse_free(IntPtr r);
		
		[DllImport("libc")]
		// aslmsg aslresponse_next(aslresponse r);
		private static extern IntPtr aslresponse_next(IntPtr r);
		
		[DllImport("libc")]
		//const char *asl_key(aslmsg msg, uint32_t n);
		private static extern IntPtr asl_key(IntPtr msg, uint n);
		
		[DllImport("libc")]
		//const char *asl_get(aslmsg msg, const char *key);
		private static extern IntPtr asl_get(IntPtr msg, [MarshalAs(UnmanagedType.LPTStr)] string key);
#endregion
	}
}

// end
