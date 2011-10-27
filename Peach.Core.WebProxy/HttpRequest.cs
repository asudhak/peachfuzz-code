
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

        public static HttpRequest Parse(string data)
        {
            Match m = Regex.Match(data, @"(.*\r\n\r\n)(.*)");
            if (m == null)
                return null;

            var request = new HttpRequest();
            request.ParseRequestHeader(m.Groups[1].Value);

            if (!request.Headers.ContainsKey("content-length"))
            {
                request.Body = m.Groups[2].Value;
                return request;
            }

            int contentLength = int.Parse(request.Headers["content-length"].Value);
            request.Body = m.Groups[2].Value.Substring(0, contentLength);

            return request;
        }

        public void ParseRequestHeader(string data)
        {
            Match m = Regex.Match(data, @"([^\r\n]+)\r\n(.*)");
            if (m == null)
                throw new ArgumentException("Unable to parse data into HTTP Request Line");

        }

        public void ParseRequestLine(string data)
        {
            Match m = Regex.Match(data, @"([^\s]+) ([^\s]+) ([^\s]+)\r\n");
            if (m == null)
                throw new ArgumentException("Unable to parse data into HTTP Request Line");

            Method = m.Groups[1].Value;
            Uri = m.Groups[2].Value;
            Version = m.Groups[3].Value;
        }
    }
}

// end
