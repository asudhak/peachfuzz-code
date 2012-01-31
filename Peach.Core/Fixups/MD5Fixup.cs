using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Peach.Core.Dom;

namespace Peach.Core.Fixups
{
    [FixupAttribute("MD5Fixup", "Standard MD5 checksum.")]
    [FixupAttribute("checksums.MD5Fixup", "Standard MD5 checksum.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    [Serializable]
    public class MD5Fixup : Fixup
    {
        public MD5Fixup(Dictionary<string, Variant> args) : base(args)
        {
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, MD5Fixup requires a 'ref' argument!");
        }

        protected override Variant fixupImpl(DataElement obj)
        {
            string objRef = (string)args["ref"];
            DataElement from = obj.find(objRef);
            byte[] data = from.Value.Value;

            MD5 md5Tool = MD5.Create();

            return new Variant(md5Tool.ComputeHash(data));
        }
    }
}

// end
