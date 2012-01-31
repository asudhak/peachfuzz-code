using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;

namespace Peach.Core.Fixups
{
    [FixupAttribute("SequenceRandomFixup", "Standard sequencial random fixup.")]
    [FixupAttribute("sequence.SequenceRandomFixup", "Standard sequencial Random fixup.")]
    [Serializable]
    public class SequenceRandomFixup : Fixup
    {
        System.Random rand = new System.Random();

        public SequenceRandomFixup(Dictionary<string, Variant> args) : base(args)
        {
        }

        protected override Variant fixupImpl(DataElement obj)
        {
            return new Variant(rand.Next());
        }
    }
}

// end
