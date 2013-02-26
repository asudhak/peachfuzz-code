
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)
//   Ross Salpino (rsal42@gmail.com)
//   Mikhail Davidov (sirus@haxsys.net)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Peach.Core.Dom;

namespace Peach.Core.Fixups
{
    [Description("Standard Hmac checksum.")]
    [Fixup("HMAC", true)]
    [Parameter("ref", typeof(DataElement), "Reference to data element")]
    [Parameter("Key", typeof(HexString), "Key used in the hash algorithm")]
    [Parameter("Hash", typeof(Algorithms), "Hash algorithm to use", "HMACSHA1")]
    [Parameter("Length", typeof(int), "Length in bytes to return (Value of 0 means don't truncate)", "0")]
    [Serializable]
    public class HMACFixup : Fixup
    {
        static void Parse(string str, out DataElement val)
        {
            val = null;
        }

        public HexString Key { get; protected set; }
        public Algorithms Hash { get; protected set; }
        public int Length { get; protected set; }
        protected DataElement _ref { get; set; }

        public enum Algorithms { HMACSHA1, HMACMD5, HMACRIPEMD160, HMACSHA256, HMACSHA384, HMACSHA512, MACTripleDES  };

        public HMACFixup(DataElement parent, Dictionary<string, Variant> args)
            : base(parent, args, "ref")
		{
            ParameterParser.Parse(this, args);
            HMAC hashSizeTest = HMAC.Create(Hash.ToString());
            if (Length > (hashSizeTest.HashSize / 8))
                throw new PeachException("The truncate length is greater than the hash size for the specified algorithm.");
            if (Length < 0)
                throw new PeachException("The truncate length must be greater than or equal to 0.");
		}

		protected override Variant fixupImpl()
		{
			var from = elements["ref"];
			byte[] data = from.Value.Value;
			HMAC hashTool = HMAC.Create(Hash.ToString());
            hashTool.Key = Key.Value;
            byte[] hash = hashTool.ComputeHash(data);

            byte[] truncatedHash;
            if (Length == 0)
            {
                truncatedHash = hash;
            }
            else
            {
                truncatedHash = new byte[Length];
                System.Array.Copy(hash, truncatedHash, Length);
            }

			return new Variant(truncatedHash);
		}
    }
}

// end
