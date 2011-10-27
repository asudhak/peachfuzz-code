using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core.WebProxy;

namespace Peach.Core.WebProxy.Test
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
	}
}
