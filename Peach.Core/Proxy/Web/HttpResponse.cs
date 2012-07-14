
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
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Peach.Core.IO;

namespace Peach.Core.Proxy.Web
{
    public class HttpResponse : HttpMessage
    {
		public string Version { get; set; }
		public string Status { get; set; }
		public string Reason { get; set; }

		public virtual byte[] ToByteArray()
		{
			BitStream msg = new BitStream();
			msg.WriteBytes(ASCIIEncoding.ASCII.GetBytes(
				string.Format("HTTP/{0} {1} {2}\r\n{3}\r\n",
					Version,
					Status,
					Reason,
					Headers.ToString())));
			msg.WriteBytes(Body);

			return msg.Value;
		}

		public override string ToString()
		{
			return string.Format("HTTP/{0} {1} {2}\r\n{3}\r\n{4}",
				Version,
				Status,
				Reason,
				Headers.ToString(),
				ASCIIEncoding.ASCII.GetString(Body));
		}

		public static HttpResponse Parse(MemoryStream stream)
		{
			long pos = stream.Position;

			try
			{
				long newPos = pos;
				Match match;
				byte[] buff = new byte[stream.Length - stream.Position];
				stream.Read(buff, 0, (int) (stream.Length - stream.Position));
				BitStream dataBuffer = new BitStream(buff);

				string data = ReadLine(dataBuffer);
				if (data == null)
					return null;

				match = Regex.Match(data, @"^[\xff]*HTTP/([^\s]+) ([^\s]+) ([^\r\n]+)\r\n", RegexOptions.Singleline);
				if (match == null || match.Groups.Count < 4)
					return null;

				HttpResponse res = new HttpResponse();
				res.Version = match.Groups[1].Value;
				res.Status = match.Groups[2].Value;
				res.Reason = match.Groups[3].Value;

				string headerData = "";
				while (true)
				{
					string line = ReadLine(dataBuffer);
					if (line == null)
						return null;

					headerData += line;
					if (line == "\r\n")
					{
						break;
					}
				}

				res.ParseRequestHeader(headerData);

				newPos += dataBuffer.TellBytes();
				match = null;

				if (res.Headers.ContainsKey("content-length"))
				{
					int len = int.Parse(res.Headers["content-length"].Value);
					if ((dataBuffer.LengthBytes - dataBuffer.TellBytes()) < len)
						return null;

					if (len == 0)
					{
						res.Body = new byte[0];
					}
					else
					{
						res.Body = dataBuffer.ReadBytes(len);
					}
					pos = newPos + res.Body.Length;
				}
				else if(res.Headers.ContainsKey("transfer-encoding") && res.Headers["transfer-encoding"].Value == "chunked")
				{
					int startingPosistion = (int)dataBuffer.TellBytes();
					res.Chunks = new List<byte[]>();
					res.Body = null;
					int length = 0;

					while (true)
					{
						string body = ReadLine(dataBuffer);
						if (body == null)
							return null;

						Match matchLine = Regex.Match(body, @"(.*)\r\n");
						if (matchLine.Groups.Count < 2)
							return null;

						try
						{
							length = Convert.ToInt32(matchLine.Groups[1].Value, 16);
						}
						catch
						{
							return null;
						}
						
						// chunks end on a 0
						if (length == 0)
							break;

						// not enough data
						if (length > (dataBuffer.LengthBytes - dataBuffer.TellBytes()))
							return null;

						byte[] chunk = dataBuffer.ReadBytes(length);
						dataBuffer.ReadBytes(2); // Skip \r\n at end of chunk

						res.Chunks.Add(chunk);
					}

					pos = newPos + (dataBuffer.TellBytes() - startingPosistion);
				}
				else
				{
					if ((dataBuffer.LengthBytes - dataBuffer.TellBytes()) == 0)
					{
						res.Body = new byte[0];
						pos = newPos;
					}
					else
					{
						res.Body = dataBuffer.ReadBytes(dataBuffer.LengthBytes - dataBuffer.TellBytes());
						pos = newPos + res.Body.Length;
					}
				}

				return res;
			}
			finally
			{
				stream.Position = pos;
			}
		}

		public void ParseRequestHeader(string data)
		{
			MatchCollection matches = Regex.Matches(data, @"([^\r\n]+)\r\n");
			if (matches == null)
				throw new ArgumentException("Unable to parse data into HTTP Request Line");

			Headers = new HttpHeaderCollection();
			foreach (Match match in matches)
			{
				if (match.Groups.Count < 2)
					break;

				var header = HttpHeader.Parse(match.Groups[1].Value);
				if (header == null)
					break;

				string headerName = header.Name.ToLower();
				int cnt = 0;
				while (Headers.ContainsKey(headerName))
					headerName += ++cnt;

				Headers.Add(headerName, header);
			}
		}
	}
}
