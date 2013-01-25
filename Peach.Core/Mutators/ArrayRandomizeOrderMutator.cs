
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
    [Mutator("Randomize the order of the array")]
    [Hint("ArrayRandomizeOrderMutator-N", "Gets N by checking node for hint, or returns default (50).")]
    public class ArrayRandomizeOrderMutator : Mutator
    {
        // members
        //
        uint currentCount;
        int n;

        // CTOR
        //
        public ArrayRandomizeOrderMutator(DataElement obj)
            : base(obj)
        {
            name = "ArrayRandomizeOrderMutator";
            currentCount = 0;
            n = getN(obj, 50);
        }

        // GET N
        //
        public int getN(DataElement obj, int n)
        {
            // check for hint
            if (obj.Hints.ContainsKey(name + "-N"))
            {
                Hint h = null;
                if (obj.Hints.TryGetValue(name + "-N", out h))
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
            get { return currentCount; }
            set { currentCount = value; }
        }

        // COUNT
        //
        public override int count
        {
            get { return n; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if (obj is Dom.Array && obj.isMutable)
                return true;

            return false;
        }

        // SEQUENTIAL_MUTATION
        //
        public override void sequentialMutation(DataElement obj)
        {
            // Only called via the Sequential mutation strategy, which should always have a consistent seed

            performMutation(obj);
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            performMutation(obj);
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
        }

        // PERFORM_MUTATION
        //
        private void performMutation(DataElement obj)
        {
            Dom.Array objAsArray = (Dom.Array)obj;
            List<DataElement> items = new List<DataElement>();

            for (int i = 0; i < objAsArray.Count; ++i)
                items.Add(objAsArray[i]);

            var shuffledItems = context.Random.Shuffle(items.ToArray());
            objAsArray.Clear();

            for (int i = 0; i < shuffledItems.Length; ++i)
                objAsArray.Add(shuffledItems[i]);
        }
    }
}

// end
