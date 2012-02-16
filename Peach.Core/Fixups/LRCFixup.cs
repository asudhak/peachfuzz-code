using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;

namespace Peach.Core.Fixups
{
    [FixupAttribute("LRCFixup", "XOR bytes of data.")]
    [FixupAttribute("checksums.LRCFixup", "XOR bytes of data.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    [Serializable]
    public class LRCFixup : Fixup
    {
        public LRCFixup(Dictionary<string, Variant> args) : base(args)
        {
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, LRCFixup requires a 'ref' argument!");
        }

        protected override Variant fixupImpl(DataElement obj)
        {
            string objRef = (string)args["ref"];
            DataElement from = obj.find(objRef);
            byte[] data = from.Value.Value;

            long lrc = 0;
            foreach (byte b in data)
                lrc ^= b;

            return new Variant(lrc);
        }
    }
}

// end
