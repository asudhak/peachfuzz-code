
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
using System.Linq;
using System.Text;
using PeachCore.Dom;

namespace PeachCore.Publishers
{
	[PublisherAttribute("FileStream")]
	[ParameterAttribute("FileName", typeof(string), "Name of file to open for reading/writing")]
	[ParameterAttribute("Overwrite", typeof(bool), "Replace existing file? [true/false, default false]")]
	[ParameterAttribute("Append", typeof(bool), "Append to end of file [true/false, default flase]")]
	public class File : Publisher
	{
		public string fileName;
		public bool overwrite = false;
		public bool append = false;
		protected FileStream stream = null;

		public File(Dictionary<string, Variant> args)
		{
			if (!args.Contains("FileName"))
				throw new PeachException("Error, File publisher missing parameter 'FileName' which is required.");

			fileName = args["FileName"];

			if (args.Contains("Overwrite"))
			{
				string value = ((string)args["Overwrite"]).ToLower();

				if (value == "true")
					overwrite = true;
				else if (value == "false")
					overwrite = false;
				else
					throw new PeachException("Error, Unexpected value for parameter 'Overwrite' to File publisher.  Expected 'True' or 'False'.");
			}

			if (args.Contains("Append"))
			{
				string value = ((string)args["Append"]).ToLower();

				if (value == "true")
				{
					if (overwrite)
						throw new PeachException("Error, File publisher does not support Overwrite and Append being enabled at once.");

					append = true;
				}
				else if (value == "false")
					append = false;
				else
					throw new PeachException("Error, Unexpected value for parameter 'Append' to File publisher.  Expected 'True' or 'False'.");
			}
		}

		public override void open(Action action)
		{
			close(action);

			OnOpen(action);

			if (overwrite)
				stream = System.IO.File.Open(fileName, FileMode.CreateNew);
			else if (append)
				stream = System.IO.File.Open(fileName, FileMode.Append | FileMode.OpenOrCreate);
			else
				stream = System.IO.File.Open(fileName, FileMode.OpenOrCreate);
		}

		public override void close(Action action)
		{
			OnClose(action);

			if (stream != null)
			{
				stream.Close();
				stream = null;
			}
		}

		public override Variant input(Action action)
		{
			// TODO: Improve speed for large reads
			OnInput(action);

			List<byte> listBuffer = new List<byte>();
			byte [] buffer = new byte[1024];
			int readBytes = 0;

			do
			{
				readBytes = stream.Read(buffer, 0, buffer.Length);
				if (readBytes == buffer.Length)
					listBuffer.AddRange(buffer);
				else
				{
					for (int i = 0; i < readBytes; i++)
						listBuffer.Add(buffer[i]);
				}
			}
			while (readBytes > 0);

			Variant ret = new Variant(listBuffer.ToArray());
		}

		public override Variant input(Action action, int size)
		{
			OnInput(action, size);

			byte[] buffer = new byte[size];
			int readBytes = stream.Read(buffer, 0, size);

			if (readBytes < size)
			{
				byte[] retBuffer = new byte[readBytes];
				for (int i = 0; i < readBytes; i++)
					retBuffer[i] = buffer[i];

				Variant ret = new Variant(retBuffer);
			}

			return new Variant(buffer);
		}

		public override void output(Action action, Variant data)
		{
			OnOutput(action, data);

			byte [] buff = data;
			stream.Write(buff, 0, buff.Length);
		}
	}
}

// END
