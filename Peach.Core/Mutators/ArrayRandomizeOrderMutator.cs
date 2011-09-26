
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
    //[Mutator("Randomize the order of the array")]
	class ArrayRandomizeOrderMutator : ArrayVarianceMutator
	{
        // members
        //
        int currentCount;
        int n;

        // CTOR
        //
        public ArrayRandomizeOrderMutator(DataElement obj) : base(obj)
        {
            currentCount = 0;
            n = ((Dom.Array)obj).Count;
        }

        // NEXT
        //
        public override void next()
        {
            currentCount++;
            if (currentCount > n)
                throw new MutatorCompleted();
        }

        // COUNT
        //
        public override int count
        {
            get { return n; }
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
            performMutation(obj);
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            performMutation(obj);
        }

        // PERFORM_MUTATION
        //
        public void performMutation(DataElement obj)
        {
            Dom.Array arrayHead = (Dom.Array)(obj);
            int headIdx = arrayHead.parent.IndexOf(arrayHead);
            Dom.Array items = new Dom.Array();
            var parent = arrayHead.parent;

            for (int i = 0; i < arrayHead.Count; ++i)
            {
                var item = arrayHead[i];
                items.Add(item);
            }



            foreach (var item in items)
            {
                parent.Remove(parent[item.name]);
            }

            for (int i = 0; i < items.Count; ++i)
            {
                parent.Insert(headIdx + i, items[i]);
            }
        }
	}
}
