using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Peach.Core.WebProxy
{
    public class HttpCookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Domain { get; set; }
        public string Path { get; set; }
        public int MaxAge { get; set; }
        public bool IsSecure { get; set; }
        public bool IsHttpOnly { get; set; }
        public DateTime Expires { get; set; }
        public string RawCookie { get; set; }

		public HttpCookie()
		{
		}

		public HttpCookie(string name, string value)
		{
			Name = name;
			Value = value;
		}

        public override string ToString()
        {
            return null;
        }

        /// <summary>
        /// Parse a cookie string into a cookie class
        /// </summary>
        /// <param name="data">String containing single cookie</param>
        /// <returns></returns>
        public static HttpCookie[] Parse(string data)
        {
			var cookies = new List<HttpCookie>();
			var rxCookie = new Regex(@"\s*([^\s=;]+)\s*=\s*([^;]+)\s*(;|$)");

			var matches = rxCookie.Matches(data);
			if (matches == null || matches.Count < 1 || matches[0].Groups.Count < 3)
				return null;

			foreach(Match match in matches)
			{
				var cookie = new HttpCookie();
				cookie.Name = match.Groups[1].Value;
				cookie.Value = match.Groups[2].Value;

				cookies.Add(cookie);
			}

            return cookies.ToArray();
        }

		public static HttpCookie ParseSetCookie(string data)
		{
			var match = Regex.Match(data, @"([^=]+)=([^;]+)");
			if (match == null || match.Groups.Count == 1)
				return null;

			var cookie = new HttpCookie();
			cookie.Name = match.Groups[1].Value;
			cookie.Value = match.Groups[2].Value;

			match = Regex.Match(data, @";\s*Expires=([^;]+)");
			if (match != null && match.Groups.Count > 1)
				cookie.Expires = DateTime.Parse(match.Groups[1].Value);

			match = Regex.Match(data, @";\s*Domain=([^;]+)");
			if (match != null && match.Groups.Count > 1)
				cookie.Domain = match.Groups[1].Value;

			match = Regex.Match(data, @";\s*Path=([^;]+)");
			if (match != null && match.Groups.Count > 1)
				cookie.Path = match.Groups[1].Value;

			match = Regex.Match(data, @";\s*Secure\s*(;|$)");
			if (match != null && match.Groups.Count > 1)
				cookie.IsSecure = true;

			match = Regex.Match(data, @";\s*HttpOnly\s*(;|$)");
			if (match != null && match.Groups.Count > 1)
				cookie.IsHttpOnly = true;

			return cookie;
		}
    }
}

// end
