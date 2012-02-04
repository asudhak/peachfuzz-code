using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Crypto
{
    [TransformerAttribute("Crypt", "UNIX style crypt.")]
    [TransformerAttribute("crypto.Crypt", "UNIX style crypt.")]
    [Serializable]
    public class Crypt : Transformer
    {
        public Crypt(Dictionary<string,Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            string dataAsString = Encoding.ASCII.GetString(data.Value);
            string salt = dataAsString.Substring(0, 2);
            string result = UnixCryptTool.Crypt(salt, dataAsString);
            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(result));
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
