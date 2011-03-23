using System;
using System.Collections.Generic;
using System.Text;

namespace PeachCore.Transformers.Encode
{
    [TransformerAttribute("Utf8", "Encode on output a string as UTF-8.")]
    class Utf8 : Transformer
    {
        public Utf8(Dictionary<string,string> args) : base(args)
		{
		}


        protected override BitStream internalEncode(BitStream data)
        {
            return new BitStream(System.Text.UTF8Encoding.UTF8.GetBytes(System.Text.ASCIIEncoding.ASCII.GetString(data.Value)));
        }

        protected override BitStream internalDecode(BitStream data)
        {

            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(System.Text.UTF8Encoding.UTF8.GetString(data.Value)));
        }
    }
    //TODO: Validate claims same for C#.
    [TransformerAttribute("Utf16", "Encode on output a string as UTF-16. String is prefixed with a BOM. Supports surrogate pair	encoding of values larger then 0xFFFF.")] 
    class Utf16 : Transformer
    {
        public Utf16(Dictionary<string,string> args) : base(args)
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

    //TODO: Validate claims same for C#.
    [TransformerAttribute("Utf16Le", "Encode on output a string as UTF-16LE.  Supports surrogate pair encoding of values larger then 0xFFFF." )]
    class Utf16Le : Transformer 
    {
        public Utf16Le(Dictionary<string,string> args) : base(args)
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

    //TODO: Validate claims same for C#.
    [TransformerAttribute("Utf16Be", "Encode on output a string as UTF-16BE. Supports surrogate pair encoding of values larger then 0xFFFF.")]
    class Utf16Be : Transformer
    {
        public Utf16Be(Dictionary<string, string> args)
            : base(args)
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
