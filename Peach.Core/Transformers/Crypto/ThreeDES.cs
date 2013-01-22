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
    [Description("ThreeDES transform (hex & binary).")]
    [Transformer("ThreeDES", true)]
    [Transformer("crypto.ThreeDES")]
    [Parameter("Key", typeof(string), "Secret Key")]
    [Parameter("IV", typeof(string), "Initialization Vector")]
    [Serializable]
    public class ThreeDES : SymmetricAlgorithmTransformer
    {
        public ThreeDES(Dictionary<string, Variant> args)
            : base(args)
        {
        }

        protected override SymmetricAlgorithm GetEncryptionAlgorithm()
        {
            TripleDES tdes = TripleDES.Create();
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.Zeros;
            tdes.Key = System.Text.Encoding.ASCII.GetBytes(Key);
            tdes.IV = System.Text.Encoding.ASCII.GetBytes(IV);
            return tdes;
        }
    }
}