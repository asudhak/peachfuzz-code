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
    [Description("Aes128 transform (hex & binary).")]
    [Transformer("Aes128", true)]
    [Transformer("crypto.Aes128")]
    [Parameter("Key", typeof(HexString), "Secret Key")]
    [Parameter("IV", typeof(HexString), "Initialization Vector")]
    [Serializable]
    public class Aes128 : SymmetricAlgorithmTransformer
    {
        public Aes128(Dictionary<string, Variant> args)
            : base(args)
        {
        }

        protected override SymmetricAlgorithm GetEncryptionAlgorithm()
        {
            Rijndael aes = Rijndael.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.Zeros;
            aes.Key = Key.Value;
            aes.IV = IV.Value;
            return aes;
        }
    }
}