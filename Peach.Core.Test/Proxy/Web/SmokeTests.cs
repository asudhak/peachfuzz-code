
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
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core.Proxy.Web;

namespace Peach.Core.Test.Proxy.Web
{
	[TestFixture]
	public class SmokeTests
	{
		[Test]
		public void SetCookie()
		{
			HttpCookie cookie;

			cookie = HttpCookie.ParseSetCookie("Key=Value; Domain=docs.foo.com; Path=/accounts; Expires=Wed, 13-Jan-2021 22:23:01 GMT; Secure; HttpOnly");
			Assert.IsNotNull(cookie);
			Assert.AreEqual("Key", cookie.Name);
			Assert.AreEqual("Value", cookie.Value);
			Assert.AreEqual("docs.foo.com", cookie.Domain);
			Assert.AreEqual("/accounts", cookie.Path);
			Assert.AreEqual(DateTime.Parse("Wed, 13-Jan-2021 22:23:01 GMT"), cookie.Expires);
			Assert.IsTrue(cookie.IsSecure);
			Assert.IsTrue(cookie.IsHttpOnly);

			cookie = HttpCookie.ParseSetCookie("Key=Value; Domain=docs.foo.com; Path=/accounts; Expires=Wed, 13-Jan-2021 22:23:01 GMT; Secure");
			Assert.IsNotNull(cookie);
			Assert.AreEqual("Key", cookie.Name);
			Assert.AreEqual("Value", cookie.Value);
			Assert.AreEqual("docs.foo.com", cookie.Domain);
			Assert.AreEqual("/accounts", cookie.Path);
			Assert.AreEqual(DateTime.Parse("Wed, 13-Jan-2021 22:23:01 GMT"), cookie.Expires);
			Assert.IsTrue(cookie.IsSecure);
			Assert.IsFalse(cookie.IsHttpOnly);

			cookie = HttpCookie.ParseSetCookie("Key=Value; Domain=docs.foo.com; Path=/accounts; Expires=Wed, 13-Jan-2021 22:23:01 GMT");
			Assert.IsNotNull(cookie);
			Assert.AreEqual("Key", cookie.Name);
			Assert.AreEqual("Value", cookie.Value);
			Assert.AreEqual("docs.foo.com", cookie.Domain);
			Assert.AreEqual("/accounts", cookie.Path);
			Assert.AreEqual(DateTime.Parse("Wed, 13-Jan-2021 22:23:01 GMT"), cookie.Expires);
			Assert.IsFalse(cookie.IsSecure);
			Assert.IsFalse(cookie.IsHttpOnly);

			cookie = HttpCookie.ParseSetCookie("Key=Value; Domain=docs.foo.com; Path=/accounts");
			Assert.IsNotNull(cookie);
			Assert.AreEqual("Key", cookie.Name);
			Assert.AreEqual("Value", cookie.Value);
			Assert.AreEqual("docs.foo.com", cookie.Domain);
			Assert.AreEqual("/accounts", cookie.Path);
			Assert.IsNull(cookie.Expires);
			Assert.IsFalse(cookie.IsSecure);
			Assert.IsFalse(cookie.IsHttpOnly);

			cookie = HttpCookie.ParseSetCookie("Key=Value; Domain=docs.foo.com");
			Assert.IsNotNull(cookie);
			Assert.AreEqual("Key", cookie.Name);
			Assert.AreEqual("Value", cookie.Value);
			Assert.AreEqual("docs.foo.com", cookie.Domain);
			Assert.IsNullOrEmpty(cookie.Path);
			Assert.IsNull(cookie.Expires);
			Assert.IsFalse(cookie.IsSecure);
			Assert.IsFalse(cookie.IsHttpOnly);

			cookie = HttpCookie.ParseSetCookie("Key=Value");
			Assert.IsNotNull(cookie);
			Assert.AreEqual("Key", cookie.Name);
			Assert.AreEqual("Value", cookie.Value);
			Assert.IsNullOrEmpty(cookie.Domain);
			Assert.IsNullOrEmpty(cookie.Path);
			Assert.IsNull(cookie.Expires);
			Assert.IsFalse(cookie.IsSecure);
			Assert.IsFalse(cookie.IsHttpOnly);

		}

		[Test]
		public void Cookie()
		{
			HttpCookie[] cookies;

			cookies = HttpCookie.Parse("Key=Value");
			Assert.IsNotNull(cookies);
			Assert.AreEqual(1, cookies.Length);
			Assert.AreEqual("Key", cookies[0].Name);
			Assert.AreEqual("Value", cookies[0].Value);

			cookies = HttpCookie.Parse("Key=Value;Foo=Bar");
			Assert.IsNotNull(cookies);
			Assert.AreEqual(2, cookies.Length);
			Assert.AreEqual("Key", cookies[0].Name);
			Assert.AreEqual("Value", cookies[0].Value);
			Assert.AreEqual("Foo", cookies[1].Name);
			Assert.AreEqual("Bar", cookies[1].Value);

			cookies = HttpCookie.Parse(" Key=Value; Foo=Bar");
			Assert.IsNotNull(cookies);
			Assert.AreEqual(2, cookies.Length);
			Assert.AreEqual("Key", cookies[0].Name);
			Assert.AreEqual("Value", cookies[0].Value);
			Assert.AreEqual("Foo", cookies[1].Name);
			Assert.AreEqual("Bar", cookies[1].Value);
		}

		[Test]
		public void Header()
		{
			var header = HttpHeader.Parse("Foo: Bar\r\n");
			Assert.NotNull(header);
			Assert.AreEqual("Foo", header.Name);
			Assert.AreEqual("Bar", header.Value);

			header = HttpHeader.Parse("Foo: Bar");
			Assert.NotNull(header);
			Assert.AreEqual("Foo", header.Name);
			Assert.AreEqual("Bar", header.Value);
		}

		[Test]
		public void Request()
		{
			HttpRequest req;

			req = HttpRequest.Parse(
				new MemoryStream(
					ASCIIEncoding.ASCII.GetBytes("GET /foo.bar HTTP/1.0\r\nContent-Length: 10\r\nHost: google.com\r\n\r\n1234567890")));
			Assert.NotNull(req);
			Assert.AreEqual("GET", req.Method);
			Assert.AreEqual("/foo.bar", req.Uri);
			Assert.AreEqual("1.0", req.Version);
			Assert.AreEqual(2, req.Headers.Count);
			Assert.IsTrue(req.Headers.ContainsKey("content-length"));
			Assert.IsTrue(req.Headers.ContainsKey("host"));
			Assert.AreEqual(Encoding.ASCII.GetBytes("1234567890"), req.Body);
		}

		[Test]
		public void Response()
		{
			HttpResponse res;

			res = HttpResponse.Parse(
				new MemoryStream(
					ASCIIEncoding.ASCII.GetBytes("HTTP/1.0 200 OK\r\nContent-Length: 10\r\nHost: google.com\r\n\r\n1234567890")));
			Assert.NotNull(res);
			Assert.AreEqual("1.0", res.Version);
			Assert.AreEqual("200", res.Status);
			Assert.AreEqual("OK", res.Reason);
			Assert.AreEqual(2, res.Headers.Count);
			Assert.IsTrue(res.Headers.ContainsKey("content-length"));
			Assert.IsTrue(res.Headers.ContainsKey("host"));
			Assert.AreEqual(Encoding.ASCII.GetBytes("1234567890"), res.Body);
		}
	}
}
