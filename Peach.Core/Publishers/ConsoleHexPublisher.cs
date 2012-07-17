using System;
using System.Collections.Generic;
using Peach.Core.Dom;
using System.Text;

namespace Peach.Core.Publishers
{
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

		// Slightly tweaked from:
		// http://www.codeproject.com/Articles/36747/Quick-and-Dirty-HexDump-of-a-Byte-Array
		public void HexDump(byte[] bytes, int bytesPerLine)
		{
			System.Diagnostics.Debug.Assert(bytes != null);
			int bytesLength = bytes.Length;

			char[] HexChars = "0123456789ABCDEF".ToCharArray();

			int firstHexColumn =
				  8                   // 8 characters for the address
				+ 3;                  // 3 spaces

			int firstCharColumn = firstHexColumn
				+ bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
				+ (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
				+ 2;                  // 2 spaces 

			int lineLength = firstCharColumn
				+ bytesPerLine           // - characters to show the ascii value
				+ Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

			char[] line = (new System.String(' ', lineLength - 2) + Environment.NewLine).ToCharArray();
			int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;

			for (int i = 0; i < bytesLength; i += bytesPerLine)
			{
				line[0] = HexChars[(i >> 28) & 0xF];
				line[1] = HexChars[(i >> 24) & 0xF];
				line[2] = HexChars[(i >> 20) & 0xF];
				line[3] = HexChars[(i >> 16) & 0xF];
				line[4] = HexChars[(i >> 12) & 0xF];
				line[5] = HexChars[(i >> 8) & 0xF];
				line[6] = HexChars[(i >> 4) & 0xF];
				line[7] = HexChars[(i >> 0) & 0xF];

				int hexColumn = firstHexColumn;
				int charColumn = firstCharColumn;

				for (int j = 0; j < bytesPerLine; j++)
				{
					if (j > 0 && (j & 7) == 0) hexColumn++;
					if (i + j >= bytesLength)
					{
						line[hexColumn] = ' ';
						line[hexColumn + 1] = ' ';
						line[charColumn] = ' ';
					}
					else
					{
						byte b = bytes[i + j];
						line[hexColumn] = HexChars[(b >> 4) & 0xF];
						line[hexColumn + 1] = HexChars[b & 0xF];
						line[charColumn] = (b < 32 ? '·' : (char)b);
					}
					hexColumn += 3;
					charColumn++;
				}
				string s = new string(line);
				byte[] buf = Encoding.ASCII.GetBytes(s);
				stream.Write(buf, 0, buf.Length);
			}
		}
		
		public override void output(Core.Dom.Action action, Variant data)
		{
			open(action);

			OnOutput(action, data);
			byte[] buff = (byte[])data;

			HexDump(buff, bytesPerLine);
		}
	}
}
