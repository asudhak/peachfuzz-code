
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

namespace Peach.Core.Mutators
{
    [Mutator("Slides a DWORD through the blob")]
    [Hint("BlobDWORDSliderMutator", "Ability to disable this mutator.")]
	public class BlobDWORDSliderMutator : Mutator
	{
        // members
        //
        int position;
        int length;
        UInt32 DWORD;

        // CTOR
        //
        public BlobDWORDSliderMutator(DataElement obj)
        {
            position = 0;
            DWORD = 0xFFFFFFFF;
            length = (int)obj.Value.LengthBytes;
            name = "BlobDWORDSliderMutator";
        }

        // NEXT
        //
        public override void next()
        {
            position++;
            if (position >= length)
                throw new MutatorCompleted();
        }

        // COUNT
        //
        public override int count
        {
            get { return length; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if (obj is Dom.Blob && obj.isMutable)
            {
                Hint h = null;
                if (obj.Hints.TryGetValue("BlobDWORDSliderMutator", out h))
                {
                    if (h.Value == "off")
                        return false;
                }
                return true;
            }
            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
            performMutation(obj, position);
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            var rand = new Random(context.random.Seed + context.IterationCount + obj.fullName.GetHashCode());
            performMutation(obj, rand.Next(length - 1));
        }

        // PERFORM_MUTATION
        //
        private void performMutation(DataElement obj, int pos)
        {
            byte[] data = obj.Value.Value;
            byte[] inject;
            int currLen = data.Length;

            if (pos >= currLen)
                return;

            int remaining = currLen - pos;

            if (remaining == 1)
            {
                inject = new byte[] { 0xFF };
            }
            else if (remaining == 2)
            {
                inject = new byte[] { 0xFF, 0xFF };
            }
            else if (remaining == 3)
            {
                inject = new byte[] { 0xFF, 0xFF, 0xFF };
            }
            else
            {
                inject = BitConverter.GetBytes(DWORD);
            }

            var pt1 = ArrayExtensions.Slice(data, 0, pos);
            var pt2 = ArrayExtensions.Slice(data, pos + inject.Length, data.Length);

            obj.MutatedValue = new Variant(ArrayExtensions.Combine(pt1, inject, pt2));
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
        }
	}
}

// end
