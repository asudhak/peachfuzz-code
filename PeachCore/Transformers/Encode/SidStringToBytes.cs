using System;
using System.Collections.Generic;
using System.Text;

namespace PeachCore.Transformers.Encode
{
    [TransformerAttribute("SidStringToBytes", "Encode on output from a string reprisentation of a SID to bytes. (Format: S-1-5-21-2127521184-1604012920-1887927527-1712781)")]
    class SidStringToBytes : Transformer
    {
        public SidStringToBytes(Dictionary<string, string> args)
            : base(args)
		{
		}

        protected override BitStream internalEncode(BitStream data)
        {
            var sids = System.Text.ASCIIEncoding.ASCII.GetString(data.Value);
            
            //Hopefully this is in mono...
            var sid = new System.Security.Principal.SecurityIdentifier(sids);
            byte[] bsid = new byte[sid.BinaryLength];
            sid.GetBinaryForm(bsid, 0);
            return new BitStream();


        }

        protected override BitStream internalDecode(BitStream data)
        {
            throw new NotImplementedException();
        }
    }
}
