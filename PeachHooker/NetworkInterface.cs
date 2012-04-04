using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PeachHooker.Network;

namespace PeachHooker
{
	public class NetworkInterface : Interface
	{
		static Random _random = new Random();
		static int _fuzzFactor = 10;

		public override void IsInstalled(int clientPid)
		{
			Console.WriteLine("IsIntalled");
		}

		public override byte[] OnRecv(byte[] data)
		{
			if (Program.Context.recv)
			{
				Console.WriteLine("Fuzzing received data");
				return fuzzData(data);
			}

			return data;
		}

		public override byte[] OnSend(byte[] data)
		{
			if (Program.Context.send)
			{
				Console.WriteLine("Fuzzing sent data");
				return fuzzData(data);
			}

			return data;
		}

		public override void ReportException(Exception ex)
		{
			Console.WriteLine("ReportException: " + ex.ToString());
		}

		public override void Ping()
		{
			Console.WriteLine("Ping");
		}

		public byte[] fuzzData(byte[] data)
		{
			// Flip a coin, should we fuzz?
			if(_random.Next(2) == 0)
				return data;

			int numwrites = (int)Math.Ceiling( ((decimal)data.Length) / ((decimal)_fuzzFactor) ) + 1;
			for(int count = 0; count < numwrites; count++)
				data[_random.Next(data.Length)] = (byte)_random.Next(256);

			return data;
		}
	}
}

// end
