
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

using NLog;

namespace Peach.Core.Mutators
{
    [Mutator("Change the length of arrays to numerical edge cases")]
    [Hint("ArrayNumericalEdgeCasesMutator-N", "Gets N by checking node for hint, or returns default (50).")]
    public class ArrayNumericalEdgeCasesMutator : Mutator
    {
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		// members
        //
        long[] values;
        int currentCount;
        int arrayCount;
        int n;

        // CTOR
        //
        public ArrayNumericalEdgeCasesMutator(DataElement obj)
        {
            name = "ArrayNumericalEdgeCasesMutator";
            currentCount = 0;
            arrayCount = ((Dom.Array)obj).Count;
            n = getN(obj, 50);
            values = NumberGenerator.GenerateBadPositiveNumbers(16, n);

			// this will weed out invalid values that would cause the length to be less than 0
			List<long> newVals = new List<long>(values);
			newVals.RemoveAll(RemoveInvalid);
			values = newVals.ToArray();
		}

		private bool RemoveInvalid(long n)
		{
			return n < 0;
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
            if (currentCount >= count)
                throw new MutatorCompleted();
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
            if (obj is Dom.Array && obj.isMutable)
                return true;

            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
            performMutation(obj, (int)values[currentCount]);
			obj.mutationFlags = DataElement.MUTATE_DEFAULT;
		}

        // RANDOM_MUTATION
        //
        public override void randomMutation(DataElement obj)
        {
            var rand = new Random(context.random.Seed + context.IterationCount + obj.fullName.GetHashCode());
            performMutation(obj, (int)rand.Choice(values));
			obj.mutationFlags = DataElement.MUTATE_DEFAULT;
		}

        // PERFORM_MUTATION
        //
        public void performMutation(DataElement obj, int num)
        {
			logger.Debug("performMutation(num=" + num + ")");

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
                try
                {
					// We are not actually going to make this array thousands of items long.  Instead
					// we are going to override the count and copy the last element many times simulating a 
					// very long array.

					objAsArray.overrideCount = num;

					var elemValue = objAsArray[objAsArray.Count - 1].Value.Value;

					var newValue = new BitStream(elemValue);
					for (int i = objAsArray.Count; i < num; i++)
						newValue.WriteBytes(elemValue);

					objAsArray[objAsArray.Count - 1].MutatedValue = new Variant(newValue);
					objAsArray[objAsArray.Count - 1].mutationFlags = DataElement.MUTATE_DEFAULT | DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
                }
                catch
                {
                    throw new OutOfMemoryException();
                }
            }
        }
    }
}

// end
