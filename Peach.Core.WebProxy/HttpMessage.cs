using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peach.Core.WebProxy
{
    public abstract class HttpMessage
    {
        public string StartLine { get; set; }
        public string Body { get; set; }
        public HttpHeaderCollection Headers { get; set; }

    }
}
