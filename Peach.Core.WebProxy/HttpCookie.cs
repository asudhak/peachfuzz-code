using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public override string ToString()
        {
            return null;
        }

        /// <summary>
        /// Parse a cookie string into a cookie class
        /// </summary>
        /// <param name="data">String containing single cookie</param>
        /// <returns></returns>
        public static HttpCookie Parse(string data)
        {
            return null;
        }
    }
}
