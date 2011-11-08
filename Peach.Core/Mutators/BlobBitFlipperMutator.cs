
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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Mutators
{
    [Mutator("Flip a % of total bits in a blob. Default is 20%.")]
    [Hint("BlobBitFlipperMutator-N", "Gets N by checking node for hint, or returns default (20).")]
	public class BlobBitFlipperMutator : Mutator
	{
        // members
        //
        int n;
        int length;
        int countMax;
        int current;

        // CTOR
        //
        public BlobBitFlipperMutator(DataElement obj)
        {
            current = 0;
            n = getN(obj, 20);
            length = obj.Value.Value.Length;
            name = "BlobBitFlipperMutator";

            if (n != 0)
                countMax = (int)((length * 8) * (n / 100.0));
            else
                countMax = (int)((length * 8) * 0.2);
        }

        // GET N
        //
        public int getN(DataElement obj, int n)
        {
            // check for hint
            if (obj.Hints.ContainsKey("BlobBitFlipperMutator-N"))
            {
                Hint h = null;
                if (obj.Hints.TryGetValue("BlobBitFlipperMutator-N", out h))
                {
                    try
                    {
                        n = Int32.Parse(h.Value);
                    }
                    catch
                    {
                        throw new PeachException("Expected numerical value for Hint named " + h.Name);
                    }
                }
            }

            return n;
        }

        // NEXT
        //
        public override void next()
        {
            current++;
            if (current >= count)
                throw new MutatorCompleted();
        }

        // COUNT
        //
        public override int count
        {
            get { return countMax; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if (obj is Dom.Blob && obj.isMutable)
                return true;

            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
            //int c;
            //foreach (int i in context.random.Range(0, context.random.Next(10), 1))
            //{
            //    if (length - 1 <= 0)
            //        c = 0;
            //    else
            //        c = context.random.Next(length - 1);

            //    data = performMutation(obj, data, c);
            //}

            byte[] data = obj.Value.Value;
            BitStream bs = new BitStream(data);

            // pick a random bit
            int bit = context.random.Next(bs.LengthBits);

            // seek
            bs.SeekBits(bit, SeekOrigin.Begin);

            // flip
            if (bs.ReadBit() == 0)
                bs.WriteBit(1);
            else
                bs.WriteBit(0);

            var x = bs.Value;

            obj.MutatedValue = new Variant(bs.Value);
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            byte[] data = obj.Value.Value;
            BitStream bs = new BitStream(data);

            // pick a random bit
            int bit = context.random.Next(bs.LengthBits);

            // seek
            bs.SeekBits(bit, SeekOrigin.Begin);

            // flip
            if (bs.ReadBit() == 0)
                bs.WriteBit(1);
            else
                bs.WriteBit(0);

            obj.MutatedValue = new Variant(bs.Value);
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
        }

        // PERFORM_MUTATION
        //
        private byte[] performMutation(DataElement obj, byte[] data, int pos)
        {
            //int currLength = data.Length;

            //if (currLength == 0)
            //    return data;

            //int[] bytes = { 1, 2, 4, 8 };
            //int size = context.random.Choice(bytes);

            //if (pos + size >= length)
            //    pos = length - size;
            //if (pos < 0)
            //    pos = 0;
            //if (size > length)
            //    size = length;

            //foreach (int i in context.random.Range(pos, pos + size, 1))
            //{
            //    byte b = data[i];
            //    b ^= (byte)(context.random.Next(255));

            //    // reassemble data
            //    var pt1 = context.random.Slice(data, 0, i);
            //    byte[] pt2 = { b };
            //    var pt3 = context.random.Slice(data, i + 1, data.Length);

            //    data = context.random.Combine(pt1, pt2, pt3);
            //}

            //return data;

            return null;
        }
	}
}
