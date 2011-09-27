
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
using Peach.Core.IO;

namespace Peach.Core.Mutators
{
    [Mutator("Change the length of arrays to count - N to count + N")]
    [Hint("ArrayVarianceMutator-N", "Gets N by checking node for hint, or returns default (50).")]
	public class ArrayVarianceMutator : Mutator
	{
        // members
        //
        int n;
        int arrayCount;
        int minCount;
        int maxCount;
        int currentCount;

        // CTOR
        //
        public ArrayVarianceMutator(DataElement obj)
        {
            n = getN(obj, 50);
            arrayCount = ((Dom.Array)obj).Count;
            minCount = arrayCount - n;
            maxCount = arrayCount + n;
            name = "ArrayVarianceMutator";

            if (minCount < 0)
                minCount = 0;

            currentCount = minCount;
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
                    catch
                    {
                        throw new PeachException("Expected numerical value for Hint named " + name + "-N");
                    }
                }
            }

            return n;
        }

        // NEXT
        //
        public override void next()
        {
            currentCount++;
            if (currentCount > maxCount)
                throw new MutatorCompleted();
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
            if (obj is Dom.Array && obj.isMutable)
                return true;

            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
            performMutation(obj, currentCount);
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            int rand = context.random.Next(minCount, maxCount);
            performMutation(obj, rand);
        }

        // PERFORM_MUTATION
        //
        public void performMutation(DataElement obj, int num)
        {
            Dom.Array array = (Dom.Array)(obj);
            var arrayElements = array.Value.Value;
            int newN = num; 

            if (newN < arrayCount)
            {
                // foreach item in the list of [arrayCount - 1 -> n - 1] moving by -1

                int startIdx = arrayCount - 1;
                int stopIdx = newN - 1;
                int span = Math.Abs(stopIdx - startIdx);
                int i = startIdx;

                for (int x = 0; x < span; ++x)
                {
                    // remove some items
                    var elem = array[i];

                    if (elem == null)
                        break;

                    elem.parent.Remove(elem);
                    i--;
                }
            }
            else if (newN > arrayCount)
            {
                // add some items

                int headIdx = array.parent.IndexOf(array);
                var elem = array[arrayCount - 1];

                //if (elem == null)
                //    throw new OutOfMemoryException();

                try
                {
                    //elem.Value = elem.Value. * (newN - arrayCount);

                    int startIdx = arrayCount - 1;
                    int stopIdx = newN - 1;
                    int span = Math.Abs(stopIdx - startIdx);
                    int i = startIdx;

                    for (int x = 0; x < span; ++x)
                    {
                        var copy = array;
                        array.parent.Insert(headIdx + i, copy);
                        i++;
                    }
                }
                catch
                {
                    // throw new OutOfMemoryException();
                }
            }

            obj.MutatedValue = new Variant(array.parent.GenerateValue());
        }
	}
}
