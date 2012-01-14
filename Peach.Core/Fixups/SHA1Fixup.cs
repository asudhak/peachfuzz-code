using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Peach.Core.Dom;

namespace Peach.Core.Fixups
{
    [FixupAttribute("SHA1Fixup", "Standard SHA1 checksum.")]
    [FixupAttribute("checksums.SHA1Fixup", "Standard SHA1 checksum.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    [Serializable]
    public class SHA1Fixup : Fixup
    {
        public SHA1Fixup(Dictionary<string, Variant> args) : base(args)
        {
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, SHA1Fixup requires a 'ref' argument!");
        }

        protected override Variant fixupImpl(DataElement obj)
        {
            string objRef = (string)args["ref"];
            DataElement from = obj.find(objRef);
            byte[] data = from.Value.Value;

            SHA1 sha1Tool = new SHA1CryptoServiceProvider();

            return new Variant(sha1Tool.ComputeHash(data));
        }
    }
}

// end
