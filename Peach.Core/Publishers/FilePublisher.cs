
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
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Peach.Core.Dom;
using Peach.Core.IO;

using NLog;

namespace Peach.Core.Publishers
{
	[Publisher("File", true)]
	[Publisher("FileStream")]
	[Publisher("file.FileWriter")]
	[Publisher("file.FileReader")]
	[ParameterAttribute("FileName", typeof(string), "Name of file to open for reading/writing", true)]
	[ParameterAttribute("Overwrite", typeof(bool), "Replace existing file? [true/false, default true]", false)]
	[ParameterAttribute("Append", typeof(bool), "Append to end of file [true/false, default flase]", false)]
	public class FilePublisher : Publisher
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected string fileName;
		protected bool overwrite = true;
		protected bool append = false;
		protected FileStream stream = null;

		public FilePublisher(Dictionary<string, Variant> args) : base(args)
		{
			if (!args.ContainsKey("FileName"))
				throw new PeachException("Error, File publisher missing parameter 'FileName' which is required.");

			fileName = (string) args["FileName"];

			if (args.ContainsKey("Overwrite"))
			{
				string value = ((string)args["Overwrite"]).ToLower();

				if (value == "true")
					overwrite = true;
				else if (value == "false")
					overwrite = false;
				else
					throw new PeachException("Error, Unexpected value for parameter 'Overwrite' to File publisher.  Expected 'True' or 'False'.");
			}

			if (args.ContainsKey("Append"))
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

		public override void open(Core.Dom.Action action)
		{
			close(action);

			OnOpen(action);
			IsOpen = true;
			logger.Debug("open()");

			for (int i = 0; i < 10; i++)
			{
				try
				{
					if (overwrite)
						stream = System.IO.File.Open(fileName, FileMode.Create);
					else if (append)
						stream = System.IO.File.Open(fileName, FileMode.Append | FileMode.OpenOrCreate);
					else
						stream = System.IO.File.Open(fileName, FileMode.OpenOrCreate);

					break;
				}
				catch (Exception ex)
				{
					if (i < 9)
						Thread.Sleep(200);
					else
					{
						logger.Error(ex.Message);
						throw ex;
					}
				}
			}
		}

		public override void close(Core.Dom.Action action)
		{
			if (stream != null)
			{
				IsOpen = false;
				OnClose(action);
				logger.Debug("close()");

				try
				{
					stream.Close();
					stream.Dispose();
				}
				catch(Exception ex)
				{
					logger.Error(ex.Message);
				}

				stream = null;
			}
		}

		public override void output(Core.Dom.Action action, Variant data)
		{
			if (stream == null)
				open(action);

			OnOutput(action, data);
			logger.Debug("output()");

			try
			{
				if (data.GetVariantType() == Variant.VariantType.BitStream)
				{
					((BitStream)data).Stream.Position = 0;
					((BitStream)data).Stream.CopyTo(stream);
				}
				else
				{
					byte[] buff = (byte[])data;
					stream.Write(buff, 0, buff.Length);
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.Message);
				throw ex;
			}
		}

		#region Stream

		public override bool CanRead
		{
			get { return stream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return stream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return stream.CanWrite; }
		}

		public override void Flush()
		{
			stream.Flush();
		}

		public override long Length
		{
			get { return stream.Length; }
		}

		public override long Position
		{
			get
			{
				return stream.Position;
			}
			set
			{
				stream.Position = value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			OnInput(currentAction, count);

			return stream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			OnOutput(currentAction, new Variant(buffer));

			stream.Write(buffer, offset, count);
		}

		#endregion
	}
}

// END
