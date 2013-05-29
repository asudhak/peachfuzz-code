
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

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Peach.Core.Transformers.Crypto
{
    public static class UnixMd5CryptTool
    {
        //** Password hash magic */
        //private static String magic = "$1$";
   
        /** Characters for base64 encoding */
        private static String itoa64 = "./0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
   
        /// <summary>
        /// A function to concatenate bytes[]
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns>New adition array</returns>
        private static byte[] Concat(byte[] array1, byte[] array2)
        {
            byte[] concat = new byte[array1.Length + array2.Length];
            System.Buffer.BlockCopy(array1, 0, concat, 0, array1.Length);
            System.Buffer.BlockCopy(array2, 0, concat, array1.Length, array2.Length);
            return concat;
        }

        /// <summary>
        /// Another function to concatenate bytes[]
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <param name="max"></param>
        /// <returns>New adition array</returns>
        private static byte[] PartialConcat(byte[] array1, byte[] array2,int max)
        {
            byte[] concat = new byte[array1.Length + max];
            System.Buffer.BlockCopy(array1, 0, concat, 0, array1.Length);
            System.Buffer.BlockCopy(array2, 0, concat, array1.Length, max);
            return concat;
        }
   
        /// <summary>
        /// Base64-Encode integer value
        /// </summary>
        /// <param name="value"> The value to encode</param>
        /// <param name="length"> Desired length of the result</param>
        /// <returns>@return Base64 encoded value</returns>
        private static String to64(int value, int length)
        {
            StringBuilder result;
   
            result = new StringBuilder();
            while (--length >= 0)
            {
                result.Append(itoa64.Substring(value & 0x3f, 1));
                value >>= 6;
            }
            return (result.ToString());
        }

        /// <summary>
        /// Unix-like Crypt-MD5 function
        /// </summary>
        /// <param name="password">The user password</param>
        /// <param name="salt">The salt or the pepper of the password</param>
        /// <param name="magic">Extra characters to add</param>
        /// <returns>a human readable string</returns>
        public static String crypt(String password, String salt, String magic)
        {
            int saltEnd;
            int len;
            int value;
            int i;
            byte[] final;
            byte[] passwordBytes;
            byte[] saltBytes;
            byte[] ctx;
  
            StringBuilder result;
            HashAlgorithm x_hash_alg = HashAlgorithm.Create("MD5");
  
            // Skip magic if it exists
            if (salt.StartsWith(magic))
                salt = salt.Substring(magic.Length);
  
            // Remove password hash if present
            if ((saltEnd = salt.LastIndexOf('$')) != -1)
                salt = salt.Substring(0, saltEnd);
  
            // Shorten salt to 8 characters if it is longer
            if (salt.Length > 8)
                salt = salt.Substring(0, 8);
 
            ctx = Encoding.ASCII.GetBytes((password + magic + salt));
            final = x_hash_alg.ComputeHash(Encoding.ASCII.GetBytes((password + salt + password)));
  
  
            // Add as many characters of ctx1 to ctx
            for (len = password.Length; len > 0; len -= 16)
            {
                if (len > 16)
                {
                    ctx = Concat(ctx, final);
                }
                else
                {
                    ctx = PartialConcat(ctx, final, len);
                }
  
                //System.Buffer.BlockCopy(final, 0, hash16, ctx.Length, len);
                //System.Buffer.BlockCopy(ctx, 0, hash16, 0, ctx.Length);
  
            }
            //ctx = hashM;
  
            // Then something really weird...
            passwordBytes = Encoding.ASCII.GetBytes(password);
  
            for (i = password.Length; i > 0; i >>= 1)
            {
                if ((i & 1) == 1)
                {
                    ctx = Concat(ctx, new byte[] { 0 });
                }
                else
                {
                    ctx = Concat(ctx, new byte[] { passwordBytes[0] });
                }
            }
  
            final = x_hash_alg.ComputeHash(ctx);
  
            byte[] ctx1;
  
            // Do additional mutations
            saltBytes = Encoding.ASCII.GetBytes(salt);  //.getBytes();
            for (i = 0; i < 1000; i++)
            {
                ctx1 = new byte[] { };

                if ((i & 1) == 1)
                {
                    ctx1 = Concat(ctx1, passwordBytes);
                }
                else
                {
                    ctx1 = Concat(ctx1, final);
                }
                if (i % 3 != 0)
                {
                    ctx1 = Concat(ctx1, saltBytes);
                }
                if (i % 7 != 0)
                {
                    ctx1 = Concat(ctx1, passwordBytes);
                }
                if ((i & 1) != 0)
                {
                    ctx1 = Concat(ctx1, final);
                }
                else
                {
                    ctx1 = Concat(ctx1, passwordBytes);
                }

                final = x_hash_alg.ComputeHash(ctx1);  
            }

            result = new StringBuilder();

            // Add the password hash to the result string
            value = ((final[0] & 0xff) << 16) | ((final[6] & 0xff) << 8)
                    | (final[12] & 0xff);
            result.Append(to64(value, 4));
            value = ((final[1] & 0xff) << 16) | ((final[7] & 0xff) << 8)
                    | (final[13] & 0xff);
            result.Append(to64(value, 4));
            value = ((final[2] & 0xff) << 16) | ((final[8] & 0xff) << 8)
                    | (final[14] & 0xff);
            result.Append(to64(value, 4));
            value = ((final[3] & 0xff) << 16) | ((final[9] & 0xff) << 8)
                    | (final[15] & 0xff);
            result.Append(to64(value, 4));
            value = ((final[4] & 0xff) << 16) | ((final[10] & 0xff) << 8)
                    | (final[5] & 0xff);
            result.Append(to64(value, 4));
            value = final[11] & 0xff;
            result.Append(to64(value, 2));

            // Return result string
            return magic + salt + "$" + result.ToString();
        } 
    }
}

// end
