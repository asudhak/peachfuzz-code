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
    [TransformerAttribute("Sha1", "SHA-1 transform (hex & binary).")]
    [TransformerAttribute("crypto.Sha1", "SHA-1 transform (hex & binary).")]
    public class Sha1 : Transformer
    {
        public Sha1(Dictionary<string, Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            SHA1 sha1Tool = SHA1.Create();
            return new BitStream(sha1Tool.ComputeHash(data.Value));
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
