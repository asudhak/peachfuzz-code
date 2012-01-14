using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Peach.Core.Dom;

namespace Peach.Core.Fixups
{
    [FixupAttribute("SHA512Fixup", "Standard SHA512 checksum.")]
    [FixupAttribute("checksums.SHA512Fixup", "Standard SHA512 checksum.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    [Serializable]
    public class SHA512Fixup : Fixup
    {
        public SHA512Fixup(Dictionary<string, Variant> args) : base(args)
        {
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, SHA512Fixup requires a 'ref' argument!");
        }

        protected override Variant fixupImpl(DataElement obj)
        {
            string objRef = (string)args["ref"];
            DataElement from = obj.find(objRef);
            byte[] data = from.Value.Value;

            SHA512 sha512Tool = new SHA512CryptoServiceProvider();

            return new Variant(sha512Tool.ComputeHash(data));
        }
    }
}

// end
