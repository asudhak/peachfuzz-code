using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

using Peach.Core;

namespace Peach.Core.Agent.Monitors
{
	/// <summary>
	/// IP Power 9258 is a network enabled power strip.  The power ports can be
	/// switched on/off via HTTP.  By default this monitor will turn off then on
	/// a port of your choice when a fault is detected.  Optionally you can have
	/// the on/off occur before every iteration.
	/// </summary>
	/// <remarks>
	/// http://www.opengear.com/product-ip-power-9258.html
	/// </remarks>
	[Monitor("IpPower9258", true)]
	[Parameter("Host", typeof(string), "Host or IP address (can include http interface port e.g. :8080)")]
	[Parameter("User", typeof(string), "Username")]
	[Parameter("Password", typeof(string), "Password")]
	[Parameter("Port", typeof(int), "Port number to reset")]
	[Parameter("ResetEveryIteration", typeof(bool), "Reset power on every iteration (default is false)", "false")]
	[Parameter("PowerOnOffPause", typeof(int), "Pause in milliseconds between power off/power on (default is 1/2 second)", "500")]
	public class IpPower9258Monitor : Monitor
	{
		string _host = null;
		string _user = null;
		string _pass = null;
		string _port = null;
		int _powerPause = 500;
		bool _everyIteration = false;

		public IpPower9258Monitor(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			if (args.ContainsKey("Host"))
				_host = (string)args["Host"];
			if (args.ContainsKey("User"))
				_user = (string)args["User"];
			if (args.ContainsKey("Password"))
				_pass = (string)args["Password"];
			if (args.ContainsKey("Port"))
				_port = (string)args["Port"];
			if (args.ContainsKey("ResetEveryIteration"))
				_everyIteration = (string)args["ResetEveryIteration"] == "true";
			if (args.ContainsKey("PowerOnOffPause"))
				_powerPause = (int)args["PowerOnOffPause"];
		}

		void resetPower(bool turnOff = true)
		{
			string challenge;

			using (WebClientEx client = new WebClientEx())
			{
				client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/4.0 (compatible; MSIE 5.5; Windows NT)";

				using (Stream sin = client.OpenRead("http://" + _host + "/"))
				using (StreamReader srin = new StreamReader(sin))
				{
					string data = srin.ReadToEnd();
					var m = Regex.Match(data, "NAME=\"Challenge\" VALUE=\"(.*)\"> <input");
					challenge = m.Groups[1].Value;
				}

				var md5 = MD5.Create();
				var hash = md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(_user + _pass + challenge));

				// step 2, convert byte array to hex string
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < hash.Length; i++)
				{
					sb.Append(hash[i].ToString("x2"));
				}

				// Make final web requests

				client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

				string postData = "Username=" + _user + "&Response=" + sb.ToString() + "&Challenge=&Password=";
				client.UploadString("http://"+_host+"/tgi/login.tgi", postData);

				if (turnOff)
				{
					// Off

					postData = "P6" + _port + "=Off&ButtonName=Apply";
					client.UploadString("http://" + _host + "/tgi/iocontrol.tgi", postData);

					// Pause
					System.Threading.Thread.Sleep(_powerPause);
				}

				// On

				postData = "P6" + _port + "=On&ButtonName=Apply";
				client.UploadString("http://" + _host + "/tgi/iocontrol.tgi", postData);
			}
		}

		public override void StopMonitor()
		{
		}

		public override void SessionStarting()
		{
			// Make sure port is on :)
			resetPower(false);
		}

		public override void SessionFinished()
		{
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			if (_everyIteration)
			{
				resetPower();
			}
		}

		public override bool IterationFinished()
		{
			return false;
		}

		public override bool DetectedFault()
		{
			return false;
		}

		public override Fault GetMonitorData()
		{
			// This indicates a fault was detected and we
			// should reset a port.

			if(!_everyIteration)
				resetPower();

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
