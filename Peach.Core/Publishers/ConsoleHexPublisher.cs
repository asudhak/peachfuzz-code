using System;
using System.Collections.Generic;
using Peach.Core.Dom;
using System.Text;

namespace Peach.Core.Publishers
{
	[Publisher("ConsoleHex", true)]
	[Publisher("StdoutHex")]
	[Publisher("stdout.StdoutHex")]
	[Parameter("length", typeof(int), "How many columns per row", false)]
	public class ConsoleHexPublisher : ConsolePublisher
	{
		protected int bytesPerLine = 16;

		public ConsoleHexPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
			if (args.ContainsKey("length"))
				bytesPerLine = (int)args["length"];
		}

		public override void output(Core.Dom.Action action, Variant data)
		{
			open(action);

			OnOutput(action, data);

			string str = Utilities.HexDump((byte[])data, bytesPerLine);
			byte[] buff = Encoding.ASCII.GetBytes(str);

			stream.Write(buff, 0, buff.Length);
		}
	}
}
