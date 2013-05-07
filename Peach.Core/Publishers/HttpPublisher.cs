
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
	public class HttpPublisher : BufferedStreamPublisher
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

		protected override void OnInput()
		{
			if (Response == null)
				CreateClient(null, 0, 0);

			base.OnInput();
		}

		/// <summary>
		/// Send data
		/// </summary>
		/// <param name="buffer">Data to send/write</param>
		/// <param name="offset">The byte offset in buffer at which to begin writing from.</param>
		/// <param name="count">The maximum number of bytes to write.</param>
		protected override void OnOutput(byte[] buffer, int offset, int count)
		{
			lock (_clientLock)
			{
				if (_client != null)
					CloseClient();
			}

			CreateClient(buffer, offset, count);
		}

		private void CreateClient(byte[] buffer, int offset, int count)
		{
			if (Response != null)
			{
				Response.Close();
				Response = null;
			}

			// Send request with data as body.
			Uri url = new Uri(Url);
			if (!string.IsNullOrWhiteSpace(Query))
				url = new Uri(Url + "?" + Query);

			var request = (HttpWebRequest)HttpWebRequest.Create(url);
			request.Method = Method;

			if (Cookies)
				request.CookieContainer = CookieJar;

			if (credentials != null)
				request.Credentials = credentials;

			foreach (var header in Headers.Keys)
				request.Headers[header] = Headers[header];

			if (buffer != null)
			{
				try
				{
					using (var sout = request.GetRequestStream())
					{
						sout.Write(buffer, offset, count);
					}
				}
				catch (ProtocolViolationException ex)
				{
					throw new SoftException(ex);
				}
			}
			else
			{
				request.ContentLength = 0;
			}

			try
			{
				Response = (HttpWebResponse)request.GetResponse();
			}
			catch (WebException ex)
			{
				throw new SoftException(ex);
			}

			_client = Response.GetResponseStream();
			_clientName = url.ToString();

			StartClient();
		}

		protected override void OnClose()
		{
			base.OnClose();

			if (Cookies && !CookiesAcrossIterations)
				CookieJar = new CookieContainer();

			if (Response != null)
			{
				Response.Close();
				Response = null;
			}

			Query = null;
			Headers.Clear();
		}

	}
}

// END
