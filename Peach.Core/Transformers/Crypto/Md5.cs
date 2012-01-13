using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Crypto
{
    [TransformerAttribute("Md5", "MD5 transform (hex & binary).")]
    public class Md5 : Transformer
    {
        public Md5(Dictionary<string, Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            throw new NotImplementedException();
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
