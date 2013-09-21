
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
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;

using NLog;

namespace Peach.Core.Transformers.Encode
{
    [Description("Encode on output as HTML (encoding < > & and \").")]
    [Transformer("HtmlEncode", true)]
    [Transformer("encode.HtmlEncode")]
    [Serializable]
    public class HtmlEncode : Transformer
    {
		private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		protected NLog.Logger Logger { get { return logger; } }

		public HtmlEncode(Dictionary<string, Variant> args)
			: base(args)
        {
        }

		protected string HtmlEncodeBytes(byte[] buf)
		{
			var ret = new StringBuilder(buf.Length);

			foreach (byte b in buf)
			{
				ret.Append("&#");
				ret.Append((int)b);
				ret.Append(";");
			}

			return ret.ToString();
		}

        protected override BitwiseStream internalEncode(BitwiseStream data)
        {
			try
			{
				var s = new BitReader(data).ReadString();
				var ds = System.Web.HttpUtility.HtmlAttributeEncode(s);
				var ret = new BitStream();
				var writer = new BitWriter(ret);
				writer.WriteString(ds);
				ret.Seek(0, System.IO.SeekOrigin.Begin);
				return ret;
			}
			catch (System.Text.DecoderFallbackException ex)
			{
				logger.Warn("Caught DecoderFallbackException, throwing soft exception.");
				throw new SoftException(ex);
			}
        }

        protected override BitStream internalDecode(BitStream data)
        {
            var s = new BitReader(data).ReadString();
            var ds = System.Web.HttpUtility.HtmlDecode(s);
            var ret = new BitStream();
            var writer = new BitWriter(ret);
            writer.WriteString(ds);
            ret.Seek(0, System.IO.SeekOrigin.Begin);
            return ret;
        }
    }
}

// end
