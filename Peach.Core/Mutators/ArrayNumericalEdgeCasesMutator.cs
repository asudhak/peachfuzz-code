
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
using Peach.Core.Dom;

namespace Peach.Core.Mutators
{
    //[Mutator("ArrayNumericalEdgeCasesMutator")]
    public class ArrayNumericalEdgeCasesMutator : ArrayVarianceMutator
    {
        // members
        //
        //int[] counts = new int[] { };
        //int currentCount;
        //int countsIndex;

        // CTOR
        //
        public ArrayNumericalEdgeCasesMutator(DataElement obj) : base(obj)
        {
            //if self._counts == None:
            //    ArrayNumericalEdgeCasesMutator._counts = []
            //    gen = BadPositiveNumbersSmaller()
            //    try:
            //        while True:
            //            self._counts.append(int(gen.getValue()))
            //            gen.next()
            //    except:
            //        pass

            //currentCount = 0;
            //countsIndex = 0;

            //minCount = 0;
            //maxCount = 0;

            //int countsIndex = 0;
            //currentCount = counts[countsIndex];

            name = "ArrayNumericalEdgeCasesMutator";
        }

        // NEXT
        //
        public override void next()
        {
            //countsIndex++;
            //if (countsIndex >= counts.Length)
            throw new MutatorCompleted();
        }

        // COUNT
        //
        public override int count
        {
            get { return 0; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if (obj is Dom.Array && obj.isMutable)
                return true;

            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
        }

        // RANDOM_MUTATION
        //
        public override void randomMutation(DataElement obj)
        {
            //base.performMutation(obj, 0);
        }
    }
}
