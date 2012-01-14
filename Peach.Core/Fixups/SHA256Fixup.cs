using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Peach.Core.Dom;

namespace Peach.Core.Fixups
{
    [FixupAttribute("SHA256Fixup", "Standard SHA256 checksum.")]
    [FixupAttribute("checksums.SHA256Fixup", "Standard SHA256 checksum.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    [Serializable]
    public class SHA256Fixup : Fixup
    {
        public SHA256Fixup(Dictionary<string, Variant> args) : base(args)
        {
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, SHA256Fixup requires a 'ref' argument!");
        }

        protected override Variant fixupImpl(DataElement obj)
        {
            string objRef = (string)args["ref"];
            DataElement from = obj.find(objRef);
            byte[] data = from.Value.Value;

            SHA256 sha256Tool = new SHA256CryptoServiceProvider();

            return new Variant(sha256Tool.ComputeHash(data));
        }
    }
}

// end
