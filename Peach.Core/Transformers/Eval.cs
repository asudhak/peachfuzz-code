using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.IO;
using Peach.Core.Dom;

namespace Peach.Core.Transformers
{
    [TransformerAttribute("Eval", "Utility transformer to evaluate a statement.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    public class Eval : Transformer
    {
        Dictionary<string, Variant> m_args;

        public Eval(Dictionary<string,Variant> args) : base(args)
		{
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, EvalTransformer requires a 'ref' argument!");
            else
                m_args = args;
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
