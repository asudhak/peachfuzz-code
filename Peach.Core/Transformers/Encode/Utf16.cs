using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Encode
{
    //TODO: Validate claims same for C#.
    [TransformerAttribute("Utf16", "Encode on output a string as UTF-16. String is prefixed with a BOM. Supports surrogate pair	encoding of values larger then 0xFFFF.")]
    [TransformerAttribute("encode.Utf16", "Encode on output a string as UTF-16. String is prefixed with a BOM. Supports surrogate pair	encoding of values larger then 0xFFFF.")]
    public class Utf16 : Transformer
    {
        public Utf16(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override BitStream internalEncode(BitStream data)
        {
            return new BitStream(System.Text.UnicodeEncoding.Unicode.GetBytes(System.Text.ASCIIEncoding.ASCII.GetString(data.Value)));
        }

        protected override BitStream internalDecode(BitStream data)
        {
            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(System.Text.UnicodeEncoding.Unicode.GetString(data.Value)));
        }
    }
}

// end
