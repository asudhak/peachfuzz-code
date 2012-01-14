using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using System.Runtime.InteropServices;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers.Crypto
{
    [TransformerAttribute("Crypt", "UNIX style crypt.")]
    [TransformerAttribute("crypto.Crypt", "UNIX style crypt.")]
    public class Crypt : Transformer
    {
        [DllImport("libcrypt.so", EntryPoint = "crypt", ExactSpelling = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr UnixCrypt([MarshalAs(UnmanagedType.LPStr)]string key, [MarshalAs(UnmanagedType.LPStr)]string salt);
        //public static extern IntPtr UnixCrypt(string key, string salt);

        public Crypt(Dictionary<string,Variant> args) : base(args)
		{
		}

		protected override BitStream internalEncode(BitStream data)
		{
            string dataAsString = Convert.ToBase64String(data.Value);
            string salt = dataAsString.Substring(0, 2);
            var result = UnixCrypt(dataAsString, salt);
            string strResult = Marshal.PtrToStringAnsi(result);
            return new BitStream(System.Text.ASCIIEncoding.ASCII.GetBytes(strResult));
		}

		protected override BitStream internalDecode(BitStream data)
		{
            throw new NotImplementedException();
		}
    }
}

// end
