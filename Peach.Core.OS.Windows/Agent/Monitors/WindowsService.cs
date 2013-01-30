
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
using System.Linq;
using System.Text;
using System.ServiceProcess;
using Peach.Core.Agent;
using NLog;

namespace Peach.Core.OS.Windows.Agent.Monitors
{
	[Monitor("WindowsService", true)]
	[Parameter("Service", typeof(string), "The name that identifies the service to the system. This can also be the display name for the service.")]
	[Parameter("MachineName", typeof(string), "The computer on which the service resides. (optional, defaults to local machine)", "")]
	[Parameter("FaultOnEarlyExit", typeof(bool), "Fault if service exists early. (defaults to false)", "false")]
	[Parameter("Restart", typeof(bool), "Restart service on every iteration. (defaults to false)", "false")]
	[Parameter("StartTimout", typeof(int), "Time in minutes to wait for service start. (defaults to 1 minute)", "1")]
	public class WindowsService : Monitor
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		string _service = null;
		string _machineName = null;
		int _startTimeout = 1;
		bool _faultOnEarlyExit = false;
		bool _restart = false;
		ServiceController _serviceController = null;

		public WindowsService(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			_service = (string)args["Service"];

			if (args.ContainsKey("MachineName"))
				_machineName = (string)args["MachineName"];

			if (args.ContainsKey("StartTimeout"))
				_startTimeout = int.Parse((string)args["StartTimeout"]);

			if (args.ContainsKey("FaultOnEarlyExit") && ((string)args["FaultOnEarlyExit"]).ToLower() == "true")
				_faultOnEarlyExit = true;

			if (args.ContainsKey("Restart") && ((string)args["Restart"]).ToLower() == "true")
				_restart = true;

			if (_machineName == null)
			{
				_serviceController = new ServiceController(_service);
				if (_serviceController == null)
					throw new PeachException("WindowsService monitor was unable to connect to [" + _service + "].");
			}
			else
			{
				_serviceController = new ServiceController(_service, _machineName);
				if (_serviceController == null)
					throw new PeachException("WindowsService monitor was unable to connect to [" + _service + "] on computer ["+ _machineName + "].");
			}
		}

		protected void StartService()
		{
			try
			{
				if (_machineName == null)
					logger.Debug("StartService(" + _service + ")");
				else
					logger.Debug("StartService(" + _service + ", " + _machineName + ")");

				switch (_serviceController.Status)
				{
					case ServiceControllerStatus.ContinuePending:
						break;
					case ServiceControllerStatus.Paused:
						_serviceController.Continue();
						break;
					case ServiceControllerStatus.PausePending:
						_serviceController.WaitForStatus(ServiceControllerStatus.Paused, new TimeSpan(0, _startTimeout, 0));
						_serviceController.Continue();
						break;
					case ServiceControllerStatus.Running:
						break;
					case ServiceControllerStatus.StartPending:
						break;
					case ServiceControllerStatus.Stopped:
						_serviceController.Start();
						break;
					case ServiceControllerStatus.StopPending:
						_serviceController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, _startTimeout, 0));
						_serviceController.Start();
						break;
				}

				_serviceController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, _startTimeout, 0));
			}
			catch (System.ServiceProcess.TimeoutException ex)
			{
				if (_machineName == null)
				{
					logger.Debug("StartService: Timeout starting service [" + _service + "].");
					throw new PeachException("Error, WindowsService monitor was unable to start service [" + _service + "].", ex);
				}
				else
				{
					logger.Debug("StartService: Timeout starting service [" + _service + "] on computer ["+ _machineName +"].");
					throw new PeachException("Error, WindowsService monitor was unable to start service [" + _service + "] on computer [" + _machineName + "].", ex);
				}
			}
		}

		protected void StopService()
		{
			try
			{
				if(_machineName == null)
					logger.Debug("StopService(" + _service + ")");
				else
					logger.Debug("StopService(" + _service + ", "+_machineName+")");

				switch (_serviceController.Status)
				{
					case ServiceControllerStatus.ContinuePending:
						_serviceController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, _startTimeout, 0));
						_serviceController.Stop();
						break;
					case ServiceControllerStatus.Paused:
						_serviceController.Stop();
						break;
					case ServiceControllerStatus.PausePending:
						_serviceController.WaitForStatus(ServiceControllerStatus.Paused, new TimeSpan(0, _startTimeout, 0));
						_serviceController.Stop();
						break;
					case ServiceControllerStatus.Running:
						_serviceController.Stop();
						break;
					case ServiceControllerStatus.StartPending:
						_serviceController.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, _startTimeout, 0));
						_serviceController.Stop();
						break;
					case ServiceControllerStatus.Stopped:
						break;
					case ServiceControllerStatus.StopPending:
						break;
				}

				_serviceController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, _startTimeout, 0));
			}
			catch (System.ServiceProcess.TimeoutException ex)
			{
				if (_machineName == null)
				{
					logger.Debug("StartService: Timeout stopping service [" + _service + "].");
					throw new PeachException("Error, WindowsService monitor was unable to stop service [" + _service + "].", ex);
				}
				else
				{
					logger.Debug("StartService: Timeout stopping service [" + _service + "] on computer [" + _machineName + "].");
					throw new PeachException("Error, WindowsService monitor was unable to stop service [" + _service + "] on computer [" + _machineName + "].", ex);
				}
			}
		}

		public override void StopMonitor()
		{
		}

		public override void SessionStarting()
		{
		}

		public override void SessionFinished()
		{
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			if (_restart)
				StopService();

			StartService();
		}

		public override bool IterationFinished()
		{
			return false;
		}

		public override bool DetectedFault()
		{
			if (_faultOnEarlyExit && _serviceController.Status != ServiceControllerStatus.Running)
				return true;

			return false;
		}

		public override Fault GetMonitorData()
		{
			if (!(_faultOnEarlyExit && _serviceController.Status != ServiceControllerStatus.Running))
				return null;

			Fault fault = new Fault();
			fault.type = FaultType.Fault;
			fault.folderName = "WindowsService";
			fault.detectionSource = "WindowsService";
			if(_machineName == null)
				fault.description = "The windows service [" + _service + "] stopped early.";
			else
				fault.description = "The windows service [" + _service + "] on computer ["+ _machineName+"] stopped early.";
			fault.collectedData["WindowsService.txt"] = ASCIIEncoding.ASCII.GetBytes(fault.description);

			return fault;
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
