using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.Fixups.Libraries;

namespace Peach.Core.Fixups
{
    [FixupAttribute("IcmpChecksumFixup", "Standard ICMP checksum.")]
    [FixupAttribute("checksums.IcmpChecksumFixup", "Standard ICMP checksum.")]
    [ParameterAttribute("ref", typeof(DataElement), "Reference to data element", true)]
    [Serializable]
    public class IcmpChecksumFixup : Fixup
    {
        public IcmpChecksumFixup(Dictionary<string, Variant> args)
            : base(args)
        {
            if (!args.ContainsKey("ref"))
                throw new PeachException("Error, IcmpChecksumFixup requires a 'ref' argument!");
        }

        protected override Variant fixupImpl(DataElement obj)
        {
            string objRef = (string)args["ref"];
            DataElement from = obj.find(objRef);
            byte[] data = from.Value.Value;
            byte[] final;

            // add a byte if not divisible by 2
            if (data.Length % 2 != 0)
                final = ArrayExtensions.Combine(data, new byte[] { (byte)('\0') });
            else
                final = data;

            // build a list of 16-bit words
            List<short> words = new List<short>();
            for (int i = 0; i < final.Length; i += 2)
                words.Add((short)final[i]);

            // perform ones-compliment arithmetic
            int sum = 0;
            for (int i = 0; i < words.Count; ++i)
                sum += (words[i] & 0xFFFF);

            int hi = sum >> 16;
            int lo = sum & 0xFFFF;
            sum = hi + lo;
            sum += sum >> 16;

            return new Variant((int)((~sum) & 0xFFFF));
        }
    }
}
