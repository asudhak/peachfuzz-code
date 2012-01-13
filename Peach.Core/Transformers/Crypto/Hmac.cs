using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Crypto
{
    [TransformerAttribute("Hmac", "HMAC as described in RFC 2104.")]
    public class Hmac : Transformer
    {
        public Hmac(Dictionary<string, Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            throw new NotImplementedException();
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}
