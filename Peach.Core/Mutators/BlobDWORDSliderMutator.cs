
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
using NLog;
using Peach.Core.IO;

namespace Peach.Core.Mutators
{
    [Mutator("Slides a DWORD through the blob")]
    [Hint("BlobDWORDSliderMutator", "Ability to disable this mutator.")]
    public class BlobDWORDSliderMutator : Mutator
    {
        static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        // constants
        //
        static byte[] inject = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };

        // members
        //
        uint position;
        int length;

        // CTOR
        //
        public BlobDWORDSliderMutator(DataElement obj)
        {
            position = 0;
            length = (int)obj.Value.LengthBytes;
            if (length <= 1)
                length = 0;
            name = "BlobDWORDSliderMutator";
        }

        // MUTATION
        //
        public override uint mutation
        {
            get { return position; }
            set { position = value; }
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

        // SEQUENTIAL_MUTATION
        //
        public override void sequentialMutation(DataElement obj)
        {
            performMutation(obj, (int)position);
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            if (obj.Value.LengthBytes <= 1)
            {
                logger.Error("Error, length is " + length + ", unable to perform mutation.");
                return;
            }

            performMutation(obj, context.Random.Next((int)obj.Value.LengthBytes));
        }

        // PERFORM_MUTATION
        //
        private void performMutation(DataElement obj, int pos)
        {
            var stream = obj.Value;

            System.Diagnostics.Debug.Assert(pos < stream.LengthBytes);

            int len = Math.Min((int)stream.LengthBytes - pos, inject.Length);
            long cur = stream.TellBits();

            stream.SeekBytes(pos, System.IO.SeekOrigin.Begin);

            for (int i = 0; i < len; ++i)
                stream.WriteByte(inject[i]);

            stream.SeekBits(cur, System.IO.SeekOrigin.Begin);

            obj.MutatedValue = new Variant(obj.Value);
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
        }
    }
}

// end
