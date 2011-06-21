using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Peach.Core.Debuggers.DebugEngine;
using System.IO;
using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;

namespace TestThings
{
	class Program
	{
		protected static string Byte2String(byte b)
		{
			string ret = "";

			for (int i = 0; i < 8; i++)
			{
				int bit = b >> 7 - i;
				ret += bit == 0 ? "0" : "1";
			}

			return ret;
		}

		static void Main(string[] args)
		{
			BitStream bs = new BitStream();

			bs.WriteBit(0);
			bs.WriteBit(1);
			bs.WriteBit(1);
			bs.WriteBit(0);
			bs.WriteBit(0);
			bs.WriteBit(1);

			bs.SeekBits(0, SeekOrigin.Begin);

			foreach (byte b in bs.buff)
			//foreach (byte b in new byte[] {0xff, 0x0, 0x1})
					Console.WriteLine(Byte2String(b));

			Console.ReadKey();
		}
	}
}
