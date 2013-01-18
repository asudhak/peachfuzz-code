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
    [Description("AES128 transform (hex & binary).")]
    [Transformer("AES128", true)]
    [Transformer("crypto.AES128")]
    [Parameter("Key", typeof(string), "Secret Key")]
    [Parameter("IV", typeof(string), "Initialization Vector")]
    [Serializable]
    public class AES128 : Transformer
    {
        public string Key { get; private set; }
        public string IV { get; private set; }

        byte[] key;
        byte[] iv;
        public AES128(Dictionary<string, Variant> args)
            : base(args)
        {
            ParameterParser.Parse(this, args);
            key = System.Text.Encoding.ASCII.GetBytes(Key);
            iv = System.Text.Encoding.ASCII.GetBytes(IV);
        }

        protected override BitStream internalEncode(BitStream data)
        {
            Rijndael aes = Rijndael.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.Zeros;
            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform ict = aes.CreateEncryptor();
            byte[] enc = ict.TransformFinalBlock(data.Value, 0, data.Value.Length);
            
            return new BitStream(enc);
        }

        protected override BitStream internalDecode(BitStream data)
        {
            Rijndael aes = Rijndael.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.Zeros;
            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform ict = aes.CreateDecryptor();
            byte[] dec = ict.TransformFinalBlock(data.Value, 0, data.Value.Length);

            return new BitStream(dec);
        }
    }
}