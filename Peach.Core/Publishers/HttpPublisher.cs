
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
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Peach.Core.Dom;
using Peach.Core.IO;

using NLog;

namespace Peach.Core.Publishers
{
	[Publisher("Http", true)]
	[Parameter("Method", typeof(string), "Method type")]
	[Parameter("Url", typeof(string), "Url")]
	[Parameter("BaseUrl", typeof(string), "Optional BaseUrl for authentication", "")]
	[Parameter("Username", typeof(string), "Optional username for authentication", "")]
	[Parameter("Password", typeof(string), "Optional password for authentication", "")]
	[Parameter("Domain", typeof(string), "Optional domain for authentication", "")]
	[Parameter("Cookies", typeof(bool), "Track cookies (defaults to true)", "true")]
	[Parameter("CookiesAcrossIterations", typeof(bool), "Track cookies across iterations (defaults to false)", "false")]
	public class HttpPublisher : StreamPublisher
	{
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected override NLog.Logger Logger { get { return logger; } }

		public string Url { get; set; }
		public string Method { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Domain { get; set; }
		public string BaseUrl { get; set; }
		public bool Cookies { get; set; }
		public bool CookiesAcrossIterations { get; set; }

		protected CookieContainer CookieJar = new CookieContainer();
		protected HttpWebResponse Response { get; set; }
		protected string Query { get; set; }
		protected Dictionary<string, string> Headers = new Dictionary<string, string>();
		protected CredentialCache credentials = null;

		public HttpPublisher(Dictionary<string, Variant> args)
			: base(args)
		{
			if (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password))
			{
				Uri baseUrl = new Uri(Url);

				if (!string.IsNullOrWhiteSpace(BaseUrl))
					baseUrl = new Uri(BaseUrl);

				credentials = new CredentialCache();
				credentials.Add(baseUrl, "Basic", new NetworkCredential(Username, Password));

				if (!string.IsNullOrWhiteSpace(Domain))
				{
					credentials.Add(baseUrl, "NTLM", new NetworkCredential(Username, Password, Domain));
					credentials.Add(baseUrl, "Digest", new NetworkCredential(Username, Password, Domain));
				}
			}
		}

		protected override Variant OnCall(string method, List<ActionParameter> args)
		{
			switch (method)
			{
				case "Query":
					Query = UTF8Encoding.UTF8.GetString(args[0].dataModel.Value.Value);
					break;
				case "Header":
					Headers[UTF8Encoding.UTF8.GetString(args[0].dataModel.Value.Value)] = UTF8Encoding.UTF8.GetString(args[1].dataModel.Value.Value);
					break;
			}

			return null;
		}

		/// <summary>
		/// Send data
		/// </summary>
		/// <param name="data">Data to send/write</param>
		protected override void OnOutput(Stream data)
		{
			Response = null;

			// Send request with data as body.
			var request = (HttpWebRequest)HttpWebRequest.Create(Url);
			request.Method = Method;

			if(Cookies)
				request.CookieContainer = CookieJar;

			if(credentials != null)
				request.Credentials = credentials;

			using (var sout = request.GetRequestStream())
			{
				data.Position = 0;
				data.CopyTo(sout);
			}

			Response = (HttpWebResponse) request.GetResponse();
			stream = Response.GetResponseStream();
		}

		protected override void OnClose()
		{
			if (Cookies && !CookiesAcrossIterations)
				CookieJar = new CookieContainer();

			if (Response != null && stream != null)
				stream.Dispose();

			Response = null;
			stream = null;
			Query = null;
			Headers.Clear();
		}

	}
}

// END
