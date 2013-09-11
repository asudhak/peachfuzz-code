
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
using Peach.Core.Dom;
using Peach.Core.IO;
using System.Security.Cryptography;
using System.IO;

namespace Peach.Core
{
	/// <summary>
	/// Transformers perform static transforms of data.
	/// </summary>
	[Serializable]
	public abstract class Transformer
	{
		public Transformer anotherTransformer;

		public Transformer(Dictionary<string, Variant> args)
		{
		}

		/// <summary>
		/// Encode data, will properly call any chained transformers.
		/// </summary>
		/// <param name="data">Data to encode</param>
		/// <returns>Returns encoded value or null if encoding is not supported.</returns>
		public virtual BitwiseStream encode(BitwiseStream data)
		{
			data.Seek(0, System.IO.SeekOrigin.Begin);

			data = internalEncode(data);

			if (anotherTransformer != null)
				return anotherTransformer.encode(data);

			return data;
		}

		/// <summary>
		/// Decode data, will properly call any chained transformers.
		/// </summary>
		/// <param name="data">Data to decode</param>
		/// <returns>Returns decoded value or null if decoding is not supported.</returns>
		public virtual BitStream decode(BitStream data)
		{
			if (anotherTransformer != null)
				data = anotherTransformer.decode(data);

			data.Seek(0, System.IO.SeekOrigin.Begin);

			return internalDecode(data);
		}

		/// <summary>
		/// Implement to perform actual encoding of 
		/// data.
		/// </summary>
		/// <param name="data">Data to encode</param>
		/// <returns>Returns encoded data</returns>
		protected abstract BitwiseStream internalEncode(BitwiseStream data);

		/// <summary>
		/// Implement to perform actual decoding of
		/// data.
		/// </summary>
		/// <param name="data">Data to decode</param>
		/// <returns>Returns decoded data</returns>
		protected abstract BitStream internalDecode(BitStream data);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="transform"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		protected static BitStream CryptoStream(BitwiseStream stream, ICryptoTransform transform, CryptoStreamMode mode)
		{
			BitStream ret = new BitStream();

			if (mode == CryptoStreamMode.Write)
			{
				var cs = new CryptoStream(ret, transform, mode);
				stream.CopyTo(cs);
				cs.FlushFinalBlock();
			}
			else
			{
				var cs = new CryptoStream(stream, transform, mode);
				cs.CopyTo(ret);
			}

			if (stream.Position != stream.Length)
				throw new PeachException("Didn't transform all bytes.");

			ret.Seek(0, SeekOrigin.Begin);
			return ret;
		}
	}

	/// <summary>
	/// Use this attribute to identify Transformers
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class TransformerAttribute : PluginAttribute
	{
		public TransformerAttribute(string name, bool isDefault = false)
			: base(typeof(Transformer), name, isDefault)
		{
		}
	}
}

// end
