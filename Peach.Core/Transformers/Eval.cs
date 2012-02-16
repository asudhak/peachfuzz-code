using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers
{
    [TransformerAttribute("Eval", "Evaluate a statement.")]
    [TransformerAttribute("misc.Eval", "Evaluate a statement.")]
    [Serializable]
    public class Eval : Transformer
    {
        public Eval(Dictionary<string,Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            return new BitStream();
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
