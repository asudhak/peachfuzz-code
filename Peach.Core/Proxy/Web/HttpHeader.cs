
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
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

namespace Peach.Core.Proxy.Web
{
    public class HttpHeader
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
			//return Name + ": " + HttpUtility.UrlEncode(Value) + "\r\n";
			return Name + ": " + Value + "\r\n";
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
