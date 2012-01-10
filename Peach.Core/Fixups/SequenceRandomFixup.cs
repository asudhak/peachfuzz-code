using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.Fixups.Libraries;

namespace Peach.Core.Fixups
{
    [FixupAttribute("SequenceRandomFixup", "Standard sequencial random fixup.")]
    [FixupAttribute("sequence.SequenceRandomFixup", "Standard sequencial Random fixup.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    [Serializable]
    public class SequenceRandomFixup : Fixup
    {
        public SequenceRandomFixup(Dictionary<string, Variant> args)
            : base(args)
        {
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, SequenceRandomFixup requires a 'ref' argument!");
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
