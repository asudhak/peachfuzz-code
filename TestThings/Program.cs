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
				int bit = (b >> 7 - i) & 1;
				ret += bit == 0 ? "0" : "1";
			}

			return ret;
		}

		static void Main(string[] args)
		{
			
		}
	}
}
