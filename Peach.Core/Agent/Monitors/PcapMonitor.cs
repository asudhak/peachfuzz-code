
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Peach.Core.Dom;

namespace Peach.Core.Agent.Monitors
{
	[Monitor("PcapMonitor")]
	[Monitor("network.PcapMonitor")]
	[Parameter("Device", typeof(string), "TODO", true)]
	[Parameter("Filter", typeof(string), "TODO", true)]
	public class PcapMonitor : Monitor
    {
		protected string _device = null;
		protected string _filter = null;

		public PcapMonitor(string name, Dictionary<string, Variant> args)
			: base(name, args)
		{
			if (args.ContainsKey("Device"))
				_device = (string)args["Device"];
			if (args.ContainsKey("Filter"))
				_filter = (string)args["Filter"];
		}

		public override void StopMonitor()
		{
			throw new NotImplementedException();
		}

		public override void SessionStarting()
		{
			throw new NotImplementedException();
		}

		public override void SessionFinished()
		{
			throw new NotImplementedException();
		}

		public override void IterationStarting(int iterationCount, bool isReproduction)
		{
			throw new NotImplementedException();
		}

		public override bool IterationFinished()
		{
			throw new NotImplementedException();
		}

		public override bool DetectedFault()
		{
			throw new NotImplementedException();
		}

		public override System.Collections.Hashtable GetMonitorData()
		{
			throw new NotImplementedException();
		}

		public override bool MustStop()
		{
			throw new NotImplementedException();
		}

		public override Variant Message(string name, Variant data)
		{
			throw new NotImplementedException();
		}
	}
}

// end
