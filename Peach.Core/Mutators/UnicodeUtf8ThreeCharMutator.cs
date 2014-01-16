
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

namespace Peach.Core.Mutators
{
    [Mutator("UnicodeUtf8ThreeCharMutator")]
    [Description("Generates bad long UTF-8 three byte strings")]
    public partial class UnicodeUtf8ThreeCharMutator : Mutator
    {
        // members
        //
        uint pos = 0;

        // CTOR
        //
        public UnicodeUtf8ThreeCharMutator(DataElement obj)
        {
            pos = 0;
            name = "UnicodeUtf8ThreeCharMutator";
        }

        // MUTATION
        //
        public override uint mutation
        {
            get { return pos; }
            set { pos = value; }
        }

        // COUNT
        //
        public override int count
        {
            get { return values.Length; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if (obj is Dom.String && obj.isMutable)
                return true;

            return false;
        }

        // SEQUENTIAL_MUTATION
        //
        public override void sequentialMutation(DataElement obj)
        {
            var item = values[pos];
            var bs = new BitStream();
            bs.Write(item, 0, item.Length);
            obj.MutatedValue = new Variant(bs);

            obj.mutationFlags = MutateOverride.Default;
            obj.mutationFlags |= MutateOverride.TypeTransform;
        }

        // RANDOM_MUTATION
        //
        public override void randomMutation(DataElement obj)
        {
            var item = context.Random.Choice(values);
            var bs = new BitStream();
            bs.Write(item, 0, item.Length);
            obj.MutatedValue = new Variant(bs);

            obj.mutationFlags = MutateOverride.Default;
            obj.mutationFlags |= MutateOverride.TypeTransform;
        }
    }
}

// end
