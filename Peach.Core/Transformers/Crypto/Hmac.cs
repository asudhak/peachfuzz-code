using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using System.Security.Cryptography;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Crypto
{
    [TransformerAttribute("Hmac", "HMAC as described in RFC 2104.")]
    [TransformerAttribute("crypto.Hmac", "HMAC as described in RFC 2104.")]
    [Serializable]
    public class Hmac : Transformer
    {
        public Hmac(Dictionary<string, Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            HMAC hmacTool = HMAC.Create();
            return new BitStream(hmacTool.ComputeHash(data.Value));
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
