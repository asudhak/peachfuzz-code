
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
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using Peach.Core.Dom;
using Peach.Core.IO;
using Peach.Core.Cracker;
using System.Linq;

namespace Peach.Core.Analyzers
{
	[Analyzer("StringToken", true)]
	[Analyzer("StringTokenAnalyzer")]
	[Analyzer("stringtoken.StringTokenAnalyzer")]
	[Parameter("Tokens", typeof(string), "List of character tokens", StringTokenAnalyzer.TOKENS)]
	[Serializable]
	public class StringTokenAnalyzer : Analyzer
	{

		/// <summary>
		/// Default token set.  Order is important!
		/// </summary>
		public const string TOKENS = "\r\n\"'[]{}<>` \t.,~!@#$%^?&*_=+-|\\:;/";

		protected string tokens = TOKENS;
		protected Dictionary<string, Variant> args = null;
		protected StringType encodingType = StringType.ascii;
		protected Encoding encoding = null;
		protected Dictionary<DataElement, Position> positions = null;

		static StringTokenAnalyzer()
		{
			supportParser = false;
			supportDataElement = true;
			supportCommandLine = false;
			supportTopLevel = false;
		}

		public StringTokenAnalyzer()
		{
		}

		public StringTokenAnalyzer(Dictionary<string, Variant> args)
		{
			this.args = args;
		}

		public override void asDataElement(DataElement parent, Dictionary<DataElement, Position> positions)
		{
			if (args != null && args.ContainsKey("Tokens"))
				tokens = (string)args["Tokens"];

			if (!(parent is Dom.String))
				throw new PeachException("Error, StringToken analyzer only operates on String elements!");

			var str = parent as Dom.String;
			encodingType = str.stringType;
			encoding = Encoding.GetEncoding(encodingType.ToString());

			// Are our tokens present in this string?
			var val = (string)str.InternalValue;
			if (!val.Any(c => tokens.IndexOf(c) > -1))
				return;

			try
			{
				this.positions = positions;

				Dom.Block block = new Block(str.name);
				block.parent[str.name] = block;
				block.Add(str);

				// Mark the position of the block
				if (positions != null)
				{
					var end = str.Value.LengthBits;
					positions[block] = new Position(0, end);
					positions[str] = new Position(0, end);
				}

				// Start splitting string.
				foreach (char token in tokens)
				{
					long offset = 0;
					splitOnToken(block, token, ref offset);
				}
			}
			finally
			{
				this.positions = null;
			}
		}

		/// <summary>
		/// Split on token recursively
		/// </summary>
		/// <param name="el"></param>
		/// <param name="token"></param>
		/// <param name="offset"></param>
		protected void splitOnToken(DataElement el, char token, ref long offset)
		{
			if (el is Dom.String)
			{
				var strEl = (Dom.String)el;
				var str = (string) el.DefaultValue;
				var tokenIndex = str.IndexOf(token);

				if (tokenIndex == -1)
				{
					if (positions != null)
						offset = positions[el].end;
					return;
				}

				var preString = new Dom.String() { stringType = strEl.stringType };
				var tokenString = new Dom.String() { stringType = strEl.stringType };
				var postString = new Dom.String() { stringType = strEl.stringType };

				preString.stringType = encodingType;
				tokenString.stringType = encodingType;
				postString.stringType = encodingType;

				preString.DefaultValue = new Variant(str.Substring(0, tokenIndex));
				tokenString.DefaultValue = new Variant(token.ToString());
				postString.DefaultValue = new Variant(str.Substring(tokenIndex + 1));

				var block = new Dom.Block(el.name);
				block.Add(preString);
				block.Add(tokenString);
				block.Add(postString);
				el.parent[el.name] = block;

				if (positions != null)
				{
					var lenPre = 8 * encoding.GetByteCount((string)preString.DefaultValue);
					var lenToken = 8 * encoding.GetByteCount((string)tokenString.DefaultValue);
					var lenPost = 8 * encoding.GetByteCount((string)postString.DefaultValue);

					var prePos = new Position() { begin = offset, end = offset + lenPre };
					var tokenPos = new Position() { begin = prePos.end, end = prePos.end + lenToken };
					var postPos = new Position() { begin = tokenPos.end, end = tokenPos.end + lenPost };
					var blockPos = new Position() { begin = prePos.begin, end = postPos.end };

					positions.Remove(el);
					positions[block] = blockPos;
					positions[preString] = prePos;
					positions[tokenString] = tokenPos;
					positions[postString] = postPos;

					offset = postPos.begin;
				}

				splitOnToken(postString, token, ref offset);
			}
			else if (el is Dom.Block)
			{
				List<DataElement> children = new List<DataElement>();

				foreach (DataElement child in ((Block)el))
					children.Add(child);

				foreach (DataElement child in children)
					splitOnToken(child, token, ref offset);
			}
		}
	}
}

// end
