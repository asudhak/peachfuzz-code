
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
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;
using NLog;

namespace Peach.Core.Mutators
{
    [Mutator("Flip a % of total bits in a blob. Default is 20%.")]
    [Hint("BlobBitFlipperMutator-N", "Gets N by checking node for hint, or returns default (20).")]
    public class BlobBitFlipperMutator : Mutator
    {
        // members
        //
        int n;
        int countMax;
        uint current;
        long length;

        // CTOR
        //
        public BlobBitFlipperMutator(DataElement obj)
        {
            current = 0;
            n = getN(obj, 20);
            length = obj.Value.LengthBits;
            name = "BlobBitFlipperMutator";

            if (n != 0)
                countMax = (int)((length) * (n / 100.0));
            else
                countMax = (int)((length) * 0.2);
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
                    catch (Exception ex)
                    {
                        throw new PeachException("Expected numerical value for Hint named " + h.Name, ex);
                    }
                }
            }

            return n;
        }

        // MUTATION
        //
        public override uint mutation
        {
            get { return current; }
            set { current = value; }
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

        // SEQUENTIAL_MUTATION
        //
        public override void sequentialMutation(DataElement obj)
        {
            // Only called via the Sequential mutation strategy, which should always have a consistent seed

            randomMutation(obj);
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            byte[] data = obj.Value.Value;

            if (data.Length == 0)
                return;

            BitStream bs = new BitStream(data);

            // pick a random bit
            int bit = context.Random.Next((int)bs.LengthBits);

            // seek, read, rewind
            bs.SeekBits(bit, SeekOrigin.Begin);
            var value = bs.ReadBit();
            bs.SeekBits(bit, SeekOrigin.Begin);

            // flip
            if (value == 0)
                bs.WriteBit(1);
            else
                bs.WriteBit(0);

            obj.MutatedValue = new Variant(bs.Value);
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
        }
    }
}

// end
