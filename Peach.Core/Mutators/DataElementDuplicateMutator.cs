
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
    [Mutator("Duplicate a node's value starting from 1x - 50x")]
    public class DataElementDuplicateMutator : Mutator
    {
        // members
        //
        uint currentCount;
        int minCount;
        int maxCount;

        // CTOR
        //
        public DataElementDuplicateMutator(DataElement obj)
        {
            minCount = 1;
            maxCount = 50;
            currentCount = (uint)minCount;
            name = "DataElementDuplicateMutator";
        }

        // MUTATION
        //
        public override uint mutation
        {
            get { return currentCount - (uint)minCount; }
            set { currentCount = value + (uint)minCount; }
        }

        // COUNT
        //
        public override int count
        {
            get { return maxCount - minCount; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if (obj.isMutable)
                return true;

            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;

            for (int i = 0; i < currentCount; ++i)
            {
                var newElem = ObjectCopier.Clone<DataElement>(obj);
                newElem.name += "_" + i;
                obj.parent.Insert(obj.parent.IndexOf(obj), newElem);
            }
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;

            // TODO: Since 'this.mutation = X' is only called by the
            // sequencial mutation strategy, the random strategy
            // will wither duplicate the element once or not
            // duplicate at all.  Is this right? Should this really be:
            //int newCount = context.Random.Next(minCount, maxCount + 1);
            int newCount = context.Random.Next((int)currentCount + 1);

            for (uint i = 0; i < newCount; ++i)
            {
                var newElem = ObjectCopier.Clone<DataElement>(obj);
                newElem.name += "_" + i;
                obj.parent.Insert(obj.parent.IndexOf(obj), newElem);
            }
        }
    }
}

// end
