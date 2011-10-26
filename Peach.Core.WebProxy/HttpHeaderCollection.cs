using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core.WebProxy
{
	public class HttpHeaderCollection : Dictionary<string, HttpHeader>
    {
		//public I Headers { get; set; }

        public override string ToString()
        {
            StringBuilder headers = new StringBuilder();

            foreach(HttpHeader header in Values)
            {
                headers.Append(header.ToString());
            }

            headers.Append("\r\n");

            return headers.ToString();
        }
    }
}
