using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;

namespace Peach.Core.Fixups
{
    [FixupAttribute("IcmpChecksumFixup", "Standard ICMP checksum.")]
    [FixupAttribute("checksums.IcmpChecksumFixup", "Standard ICMP checksum.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    [Serializable]
    public class IcmpChecksumFixup : Fixup
    {
        public IcmpChecksumFixup(Dictionary<string, Variant> args) : base(args)
        {
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, IcmpChecksumFixup requires a 'ref' argument!");
        }

        protected override Variant fixupImpl(DataElement obj)
        {
            string objRef = (string)args["ref"];
            DataElement from = obj.find(objRef);
            byte[] data = from.Value.Value;
            uint chcksm = 0;
            int idx = 0;

            // add a byte if not divisible by 2
            if (data.Length % 2 != 0)
                data = ArrayExtensions.Combine(data, new byte[] { 0x00 });

            // calculate checksum
            while (idx < data.Length)
            {
                chcksm += Convert.ToUInt32(BitConverter.ToUInt16(data, idx));
                idx += 2;
            }

            chcksm = (chcksm >> 16) + (chcksm & 0xFFFF);
            chcksm += (chcksm >> 16);

            return new Variant((ushort)(~chcksm));
        }
    }
}

// end
