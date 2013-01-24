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
    [Parameter("Key", typeof(string), "Secret Key")]
    [Parameter("IV", typeof(string), "Initialization Vector")]
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
            aes.Key = System.Text.Encoding.ASCII.GetBytes(Key);
            aes.IV = System.Text.Encoding.ASCII.GetBytes(IV);
            return aes;
        }
    }
}