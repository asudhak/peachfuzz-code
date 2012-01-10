using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Peach.Core.Dom;
using Peach.Core.Fixups.Libraries;

namespace Peach.Core.Fixups
{
    [FixupAttribute("SHA384Fixup", "Standard SHA384 checksum.")]
    [FixupAttribute("checksums.SHA384Fixup", "Standard SHA384 checksum.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    [Serializable]
    public class SHA384Fixup : Fixup
    {
        public SHA384Fixup(Dictionary<string, Variant> args)
            : base(args)
        {
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, SHA384Fixup requires a 'ref' argument!");
        }

        protected override Variant fixupImpl(DataElement obj)
        {
            string objRef = (string)args["ref"];
            DataElement from = obj.find(objRef);
            byte[] data = from.Value.Value;

            SHA384 sha384Tool = new SHA384CryptoServiceProvider();

            return new Variant(sha384Tool.ComputeHash(data));
        }
    }
}
