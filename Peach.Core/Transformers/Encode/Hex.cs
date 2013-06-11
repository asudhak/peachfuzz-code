
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
//   Mikhail Davidov (sirus@haxsys.net)

// $Id$

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;
using System.Security.Cryptography;

namespace Peach.Core.Transformers.Encode
{
	[Description("Encode on output as a hex string.")]
	[Transformer("Hex", true)]
	[Transformer("encode.Hex")]
	[Serializable]
	public class Hex : Transformer
	{
		#region Hex Encoder Transform

		class Encoder : ICryptoTransform
		{
			public bool CanReuseTransform
			{
				get { return true; }
			}

			public bool CanTransformMultipleBlocks
			{
				get { return false; }
			}

			public int InputBlockSize
			{
				get { return 1; }
			}

			public int OutputBlockSize
			{
				get { return 2; }
			}

			public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
			{
				if ((outputBuffer.Length % 2) != 0)
					throw new ArgumentOutOfRangeException("outputBuffer");
				if ((outputOffset % 2) != 0)
					throw new ArgumentOutOfRangeException("outputOffset");

				int offset = outputOffset;
				int end = inputOffset + inputCount;
				for (int i = inputOffset; i < end; ++i)
				{
					outputBuffer[offset++] = GetChar(inputBuffer[i] >> 4);
					outputBuffer[offset++] = GetChar(inputBuffer[i] & 0x0f);
				}
				return offset - outputOffset;
			}

			public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
			{
				if (inputCount != 0)
					throw new ArgumentOutOfRangeException("outputOffset");

				return new byte[0];
			}

			private byte GetChar(int nibble)
			{
				if (nibble > 0xf)
					throw new ArgumentOutOfRangeException("nibble");

				if (nibble < 0x0a)
					return (byte)(nibble + 0x30);
				else
					return (byte)(nibble - 0x0a + 0x61);
			}

			public void Dispose()
			{
			}
		}

		#endregion

		#region Hex Decoder Transform

		class Decoder : ICryptoTransform
		{
			public bool CanReuseTransform
			{
				get { return true; }
			}

			public bool CanTransformMultipleBlocks
			{
				get { return false; }
			}

			public int InputBlockSize
			{
				get { return 2; }
			}

			public int OutputBlockSize
			{
				get { return 1; }
			}

			public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
			{
				if ((inputCount % 2) != 0)
					throw new ArgumentOutOfRangeException("inputCount");

				int offset = outputOffset;
				int end = inputOffset + inputCount;
				for (int i = inputOffset; i < end; ++i)
				{
					outputBuffer[offset] = (byte)(GetNibble(inputBuffer[i++]) << 4);
					outputBuffer[offset++] |= GetNibble(inputBuffer[i]);
				}
				return offset - outputOffset;
			}

			public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
			{
				if (inputCount != 0)
					throw new SoftException("Hex decode failed, invalid length.");

				return new byte[0];
			}

			private byte GetNibble(byte c)
			{
				if (c < '0')
					throw new SoftException("Hex decode failed, invalid bytes.");
				if (c <= '9')
					return (byte)(c - '0');
				if (c < 'A')
					throw new SoftException("Hex decode failed, invalid bytes.");
				if (c <= 'F')
					return (byte)(c - 'A' + 0xA);
				if (c < 'a')
					throw new SoftException("Hex decode failed, invalid bytes.");
				if (c <= 'f')
					return (byte)(c - 'a' + 0xA);

				throw new SoftException("Hex decode failed, invalid bytes.");
			}

			public void Dispose()
			{
			}
		}

		#endregion

		public Hex(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override BitwiseStream internalEncode(BitwiseStream data)
		{
			return CryptoStream(data, new Encoder(), CryptoStreamMode.Write);
		}

		protected override BitStream internalDecode(BitStream data)
		{
			return CryptoStream(data, new Decoder(), CryptoStreamMode.Read);
		}
	}
}

// end
