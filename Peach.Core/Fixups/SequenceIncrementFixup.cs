using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.Fixups.Libraries;

namespace Peach.Core.Fixups
{
    [FixupAttribute("SequenceIncrementFixup", "Standard sequencial increment fixup.")]
    [FixupAttribute("sequence.SequenceIncrementFixup", "Standard sequencial increment fixup.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    [Serializable]
    public class SequenceIncrementFixup : Fixup
    {
        public SequenceIncrementFixup(Dictionary<string, Variant> args)
            : base(args)
        {
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, SequenceIncrementFixup requires a 'ref' argument!");
        }

        protected override Variant fixupImpl(DataElement obj)
        {
            string objRef = (string)args["ref"];
            DataElement from = obj.find(objRef);
            byte[] data = from.Value.Value;

            return new Variant("");
        }
    }
}
