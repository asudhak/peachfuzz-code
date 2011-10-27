using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

namespace Peach.Core.WebProxy
{
    public class HttpHeader
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return Name + ": " + HttpUtility.UrlEncode(Value) + "\r\n";
        }

        public static HttpHeader Parse(string data)
        {
            Match m = Regex.Match(data, @"([^\s]+): ([^\r\n]+)(\r\n|$)");

            if (m == null)
                return null;
            if (m.Groups.Count < 3)
                return null;

            var header = new HttpHeader();
            header.Name = m.Groups[1].Value;
            header.Value = m.Groups[2].Value;

            return header;
        }
    }
}
