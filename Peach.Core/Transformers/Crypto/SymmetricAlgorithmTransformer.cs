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
    [Serializable]
    public abstract class SymmetricAlgorithmTransformer : Transformer
    {
        public HexString Key { get; protected set; }
        public HexString IV { get; protected set; }

        protected abstract SymmetricAlgorithm GetEncryptionAlgorithm();

        public SymmetricAlgorithmTransformer(Dictionary<string, Variant> args)
            : base(args)
        {
            ParameterParser.Parse(this, args);
            GetEncryptionAlgorithm();           //Used for parameter validation
        }

        protected override BitwiseStream internalEncode(BitwiseStream data)
        {
            ICryptoTransform ict = GetEncryptionAlgorithm().CreateEncryptor();
            return CryptoStream(data, ict, CryptoStreamMode.Write);
        }

        protected override BitStream internalDecode(BitStream data)
        {
            ICryptoTransform ict = GetEncryptionAlgorithm().CreateDecryptor();
            return CryptoStream(data, ict, CryptoStreamMode.Read);
        }

    }
}
