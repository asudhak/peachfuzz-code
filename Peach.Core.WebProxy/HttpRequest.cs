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
            Match m = Regex.Match(data, @"[^\r\n]+\r\n(.*)");
            if (m == null)
                throw new ArgumentException("Unable to parse data into HTTP Request Line");

        }

        public void ParseRequestLine(string data)
        {
            Match m = Regex.Match(data, @"[^\s]+ [^\s]+ [^\s]+\r\n");
            if (m == null)
                throw new ArgumentException("Unable to parse data into HTTP Request Line");

            Method = m.Groups[1].Value;
            Uri = m.Groups[2].Value;
            Version = m.Groups[3].Value;
        }
    }
}
