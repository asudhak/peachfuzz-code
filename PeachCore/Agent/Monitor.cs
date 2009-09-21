using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeachCore.Agent
{
	/// <summary>
	/// Monitors are hosted by agent processes and are
	/// able to report detected faults and gather information
	/// that is usefull when a fualt is detected.
	/// </summary>
	internal class Monitor
	{
		internal Monitor(Dictionary<string, string> args)
		{
		}

		/// <summary>
		/// Called before start of test.
		/// </summary>
		public abstract void OnTestStarting();

		/// <summary>
		/// Called when test is completed.
		/// </summary>
		public abstract void OnTestFinished();

		/// <summary>
		/// Allows monitor to indicate a fault was detected.
		/// </summary>
		/// <returns>True if fault was detected, else False</returns>
		public abstract bool DetectedFault();

		/// <summary>
		/// Called when a fault was detected.
		/// </summary>
		public abstract void OnFault();

		/// <summary>
		/// Called to get any data that was collected.
		/// </summary>
		public abstract void GetData();

		/// <summary>
		/// Called to shutdown current monitor.
		/// </summary>
		public abstract void OnShutdown();

		/// <summary>
		/// Allows monitor to stop test run by returning false.
		/// </summary>
		/// <returns>False to stop run, else True</returns>
		public abstract bool StopRun();

		/// <summary>
		/// Called when a call action is being performed.  Call
		/// actions are used to launch programs, this gives the
		/// monitor a chance to determin if it should be running
		/// the program under a debugger instead.
		/// 
		/// Note: This is a bit of a hack to get this working
		/// </summary>
		/// <param name="method"></param>
		/// <param name="args"></param>
		public abstract void PublisherCall(string method, object args);
	}
}
