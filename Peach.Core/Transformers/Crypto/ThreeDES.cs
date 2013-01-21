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
    public class ThreeDES : Transformer
    {
        public string Key { get; private set; }
        public string IV { get; private set; }

        byte[] key;
        byte[] iv;
        public ThreeDES(Dictionary<string, Variant> args)
            : base(args)
        {
            ParameterParser.Parse(this, args);
            if (IV.Length != 8)
                throw new PeachException("The intialization vector must be 8 bytes long");
            key = System.Text.Encoding.ASCII.GetBytes(Key);
            iv = System.Text.Encoding.ASCII.GetBytes(IV);
        }

        protected override BitStream internalEncode(BitStream data)
        {
            TripleDES tdes = TripleDES.Create();

            try
            {
                
                tdes.Mode = CipherMode.CBC;
                tdes.Padding = PaddingMode.Zeros;
                tdes.Key = key;
                tdes.IV = iv;
            }
            catch (CryptographicException ex)
            {
                throw new PeachException("The specified secret key is a known weak key and cannot be used.", ex);
            }

            ICryptoTransform ict = tdes.CreateEncryptor();
            byte[] enc = ict.TransformFinalBlock(data.Value, 0, data.Value.Length);
            
            return new BitStream(enc);
        }

        protected override BitStream internalDecode(BitStream data)
        {
            TripleDES tdes = TripleDES.Create();
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.Zeros;
            tdes.Key = key;
            tdes.IV = iv;

            ICryptoTransform ict = tdes.CreateDecryptor();
            byte[] dec = ict.TransformFinalBlock(data.Value, 0, data.Value.Length);

            return new BitStream(dec);
        }
    }
}