using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;

namespace Peach.Core.Fixups
{
    [FixupAttribute("SequenceIncrementFixup", "Standard sequencial increment fixup.")]
    [FixupAttribute("sequence.SequenceIncrementFixup", "Standard sequencial increment fixup.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    [Serializable]
    public class SequenceIncrementFixup : Fixup
    {
        int num = 1;

        public SequenceIncrementFixup(Dictionary<string, Variant> args) : base(args)
        {
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, SequenceIncrementFixup requires a 'ref' argument!");
        }

        protected override Variant fixupImpl(DataElement obj)
        {
            ++num;
            return new Variant(num);
        }
    }
}

// end
