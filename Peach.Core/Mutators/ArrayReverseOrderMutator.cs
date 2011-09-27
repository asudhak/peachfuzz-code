
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
    //[Mutator("Reverse the order of the array")]
    public class ArrayReverseOrderMutator : ArrayVarianceMutator
	{
        // CTOR
        //
        public ArrayReverseOrderMutator(DataElement obj) : base(obj)
        {
            name = "ArrayReverseOrderMutator";
        }

        // NEXT
        //
        public override void next()
        {
            throw new MutatorCompleted();
        }

        // COUNT
        //
        public override int count
        {
            get { return 1; }
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
            // pointer to head
            // save head index
            // empty array 'items'
            // parent (???)
            // loop through the array and add each element to 'items'
            // loop through 'items' and delete parents (???)
            // loop through 'items' inversely and add to parent (???)

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

            int x = 0;
            for (int i = items.Count - 1; i > 0; --i)
            {
                parent.Insert(headIdx + x, items[i]);
                x++;
            }

            obj.MutatedValue = new Variant(parent.GenerateValue());
        }
	}
}
