using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Peach.Core.WebProxy
{
    public class HttpResponse : HttpMessage
    {
		public string Version { get; set; }
		public string Status { get; set; }
		public string Reason { get; set; }

		public override string ToString()
		{
			return string.Format("HTTP/{0} {1} {2}\r\n{3}\r\n{4}",
				Version,
				Status,
				Reason,
				Headers.ToString(),
				Body);
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

				string data = ASCIIEncoding.ASCII.GetString(buff);

				match = Regex.Match(data, @"^HTTP/([^\s]+) ([^\s]+) ([^\r\n]+)\r\n(.*\r\n\r\n)(.*)$", RegexOptions.Singleline);
				if (match == null || match.Groups.Count < 6)
					return null;

				HttpResponse res = new HttpResponse();
				res.Version = match.Groups[1].Value;
				res.Status = match.Groups[2].Value;
				res.Reason = match.Groups[3].Value;

				res.ParseRequestHeader(match.Groups[4].Value);

				newPos += match.Groups[4].Index + match.Groups[4].Length;

				if (!res.Headers.ContainsKey("content-length"))
				{
					res.Body = match.Groups[5].Value;
				}
				else
				{
					int len = int.Parse(res.Headers["content-length"].Value);
					if(match.Groups[5].Length < len)
						return null;

					res.Body = match.Groups[5].Value.Substring(0, len);
				}

				pos = newPos + res.Body.Length;

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
