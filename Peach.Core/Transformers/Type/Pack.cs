using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Type
{
    [TransformerAttribute("Pack", "Single pack transform.")]
    [TransformerAttribute("type.Pack", "Single pack transform.")]
    public class Pack : Transformer
    {
        public Pack(Dictionary<string,Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
			MemoryStream sin = new MemoryStream(data.Value);
			MemoryStream sout = new MemoryStream();

            sin.CopyTo(sout);

            return new BitStream(sout.ToArray());
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
