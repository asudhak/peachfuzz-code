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
    [TransformerAttribute("Md5", "MD5 transform (hex & binary).")]
    [TransformerAttribute("crypto.Md5", "MD5 transform (hex & binary).")]
    [Serializable]
    public class Md5 : Transformer
    {
        public Md5(Dictionary<string, Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            MD5 md5Tool = MD5.Create();
            return new BitStream(md5Tool.ComputeHash(data.Value));
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
