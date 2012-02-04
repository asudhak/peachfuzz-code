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
    [Serializable]
    public class UnixMd5Crypt : Transformer
    {
        public UnixMd5Crypt(Dictionary<string,Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            string dataAsString = Encoding.ASCII.GetString(data.Value);
            string salt = dataAsString.Substring(0, 2);
            string result = UnixMd5CryptTool.crypt(dataAsString, salt, "$1$");
            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(result));
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
