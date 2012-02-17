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
    [Parameter("eval", typeof(string), "Formatter for data.", true)]
    [Serializable]
    public class Eval : Transformer
    {
        Dictionary<string, Variant> m_args;

        public Eval(Dictionary<string,Variant> args) : base(args)
		{
            m_args = args;
		}

		protected override BitStream internalEncode(BitStream data)
		{
            string format;
            if (m_args.ContainsKey("eval"))
                format = (string)(m_args["eval"]);

            return new BitStream(data.Value);
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
