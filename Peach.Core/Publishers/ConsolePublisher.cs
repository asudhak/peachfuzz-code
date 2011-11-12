
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
using System.IO;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;

namespace Peach.Core.Publishers
{
	[PublisherAttribute("Console")]
	[PublisherAttribute("Stdout")]
	[PublisherAttribute("stdout.Stdout")]
	[NoParametersAttribute()]
	public class ConsolePublisher : Publisher
	{
		Stream _sout = null;

		public ConsolePublisher(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		public override void open(Core.Dom.Action action)
		{
			if (_sout == null)
			{
				OnOpen(action);
				_sout = System.Console.OpenStandardOutput();
			}
		}

		public override void close(Core.Dom.Action action)
		{
			OnClose(action);

			if (_sout != null)
			{
				_sout.Close();
				_sout = null;
			}
		}

		public override void output(Core.Dom.Action action, Variant data)
		{
			open(action);

			OnOutput(action, data);
			byte[] buff = (byte[])data;

			for (int cnt = 0; cnt < buff.Length; cnt += 1024)
			{
				_sout.Write(buff, cnt, ((buff.Length - cnt) > 1024) ? 1024 : (buff.Length - cnt));
			}
		}
	}
}

// END
