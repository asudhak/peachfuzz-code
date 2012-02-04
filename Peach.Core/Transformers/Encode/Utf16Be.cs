using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Encode
{
    //TODO: Validate claims same for C#.
    [TransformerAttribute("Utf16Be", "Encode on output a string as UTF-16BE. Supports surrogate pair encoding of values larger then 0xFFFF.")]
    [TransformerAttribute("encode.Utf16Be", "Encode on output a string as UTF-16BE. Supports surrogate pair encoding of values larger then 0xFFFF.")]
    [Serializable]
    public class Utf16Be : Transformer
    {
        public Utf16Be(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
        {
            return new BitStream(System.Text.Encoding.BigEndianUnicode.GetBytes(System.Text.ASCIIEncoding.ASCII.GetString(data.Value)));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(System.Text.Encoding.BigEndianUnicode.GetString(data.Value)));
        }
    }
}

// end
