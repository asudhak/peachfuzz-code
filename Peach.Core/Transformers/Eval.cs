using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.IO;
using Peach.Core.Dom;

namespace Peach.Core.Transformers
{
    [TransformerAttribute("Eval", "Utility transformer to evaluate a statement.")]
    public class Eval : Transformer
    {
        public Eval(Dictionary<string,Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            throw new NotImplementedException();
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
