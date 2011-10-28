
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
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Peach.Core.WebProxy
{
    public class HttpRequest : HttpMessage
    {
        public string RequestLine { get; set; }
        public string Method { get; set; }
        public string Uri { get; set; }
        public string Version { get; set; }

		static Regex rxSingleLine = new Regex(@"([^\r\n]+)\r\n");

		public override string ToString()
		{
			return string.Format("{0} {1} HTTP/{2}\r\n{3}\r\n{4}",
				Method,
				Uri,
				Version,
				Headers.ToString(),
				Body);
		}

        public static HttpRequest Parse(MemoryStream stream)
        {
			long pos = stream.Position;

			try
			{
				long newPos = pos;
				Match m;
				var request = new HttpRequest();

				byte[] buff = new byte[stream.Length - stream.Position];
				stream.Read(buff, 0, (int)(stream.Length - stream.Position));

				string data = ASCIIEncoding.ASCII.GetString(buff);

				m = rxSingleLine.Match(data);
				if (m == null)
					return null;

				request.RequestLine = m.Groups[1].Value;
				request.ParseRequestLine();
				data = rxSingleLine.Replace(data, "", 1);

				newPos += m.Groups[1].Index + m.Groups[1].Length;

				m = Regex.Match(data, @"^(.*\r\n\r\n)(.*)$", RegexOptions.Singleline);
				if (m == null)
					return null;

				request.ParseRequestHeader(m.Groups[1].Value);
				newPos += m.Groups[1].Index + m.Groups[1].Length;

				if (!request.Headers.ContainsKey("content-length"))
				{
					request.Body = m.Groups[2].Value;
				}
				else
				{
					int contentLength = int.Parse(request.Headers["content-length"].Value);
					request.Body = m.Groups[2].Value.Substring(0, contentLength);
				}

				pos = newPos + request.Body.Length;

				return request;
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

				Headers.Add(header.Name.ToLower(), header);
			}
        }

        public void ParseRequestLine()
        {
			Match m = Regex.Match(RequestLine, @"([^\s]+) ([^\s]+) HTTP/([^\s]+)(\r\n|$)");
            if (m == null || m.Groups.Count < 4)
                throw new ArgumentException("Unable to parse data into HTTP Request Line");

            Method = m.Groups[1].Value;
            Uri = m.Groups[2].Value;
            Version = m.Groups[3].Value;
        }
    }
}

// end
