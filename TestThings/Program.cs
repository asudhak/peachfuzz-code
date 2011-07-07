using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Peach.Core.Debuggers.DebugEngine;
using System.IO;
using Peach.Core;
using Peach.Core.IO;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.Debuggers.WindowsSystem;

namespace TestThings
{
	class Program
	{
		public static void Main(string[] argv)
		{
			new Program();
		}
		public Program()
		{
			byte[] buff;
			using (Stream sin = File.OpenRead(@"c:\4-Key.png"))
			{
				buff = new byte[sin.Length];
				sin.Read(buff, 0, buff.Length);
			}

			PitParser parser = new PitParser();
			Dom dom = parser.asParser(new Dictionary<string, string>(), File.OpenRead(@"c:\peach3.0\peach\template.xml"));

			BitStream data = new BitStream(buff);
			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels["Png"], data);
		}
	}
}

// end
