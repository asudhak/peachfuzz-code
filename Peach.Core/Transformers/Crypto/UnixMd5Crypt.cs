using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Crypto
{
    [TransformerAttribute("UnixMd5Crypt", "UNIX style MD5 crypt.")]
    [TransformerAttribute("crypto.UnixMd5Crypt", "UNIX style MD5 crypt.")]
    public class UnixMd5Crypt : Transformer
    {
        public UnixMd5Crypt(Dictionary<string,Variant> args) : base(args)
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

// end
