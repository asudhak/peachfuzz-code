
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
    [Mutator("Change the length of arrays to count - N to count + N")]
    [Hint("ArrayVarianceMutator-N", "Gets N by checking node for hint, or returns default (50).")]
	public class ArrayVarianceMutator : Mutator
	{
        // members
        //
        int n;
        int minCount;
        int maxCount;
        int currentCount;
        int arrayCount;

        // CTOR
        //
        public ArrayVarianceMutator(DataElement obj)
        {
            name = "ArrayVarianceMutator";
            n = getN(obj, 50);
            arrayCount = ((Dom.Array)obj).Count;
            minCount = arrayCount - n;
            maxCount = arrayCount + n;

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
                        throw new PeachException("Expected numerical value for Hint named " + h.Name);
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
            performMutation(obj, context.random.Next(minCount, maxCount));
        }

        // PERFORM_MUTATION
        //
        public void performMutation(DataElement obj, int num)
        {
            Dom.Array objAsArray = (Dom.Array)obj;

            //if (num == 0)
              //  return;
            if (num < objAsArray.Count)
            {
                // remove some items
                foreach (int i in ArrayExtensions.Range(objAsArray.Count - 1, num - 1, -1))
                {
                    if (objAsArray[i] == null)
                        break;

                    objAsArray.RemoveAt(i);
                }
            }
            else if (num > objAsArray.Count)
            {
                // add some items
                try
                {
                    var newElem = ObjectCopier.Clone<DataElement>(objAsArray[objAsArray.Count - 1]);
                    foreach (int i in ArrayExtensions.Range(objAsArray.Count, num, 1))
                    {
                        newElem.name = "child" + (i + 1);
                        objAsArray.Add(newElem);
                    }
                }
                catch
                {
                    throw new OutOfMemoryException();
                }
            }
        }
	}
}
