
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
    [Mutator("Change the length of sizes to count - N to count + N")]
    [Hint("SizedVaranceMutator-N", "Gets N by checking node for hint, or returns default (50).")]
	public class SizedVaranceMutator : Mutator
	{
        // members
        //
        int n;
        int[] values;
        int currentCount;

        // CTOR
        //
        public SizedVaranceMutator(DataElement obj)
        {
            currentCount = 0;
            n = getN(obj, 50);
            name = "SizedVaranceMutator";
            PopulateValues();
        }

        // POPULATE_VALUES
        //
        private void PopulateValues()
        {
            // generate values from [-n, n]
            List<int> temp = new List<int>();

            for (int i = -n; i <= n; ++i)
                temp.Add(i);

            values = temp.ToArray();
        }

        // GET N
        //
        public int getN(DataElement obj, int n)
        {
            // check for hint
            if (obj.Hints.ContainsKey("SizedVaranceMutator-N"))
            {
                Hint h = null;
                if (obj.Hints.TryGetValue("SizedVaranceMutator-N", out h))
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
            // verify data element has size relation
            if (obj.isMutable && obj.relations.hasSizeRelation)
                return true;

            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
            performMutation(obj, values[currentCount]);
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            performMutation(obj, context.random.Choice(values));
        }

        // PERFORM_MUTATION
        //
        private void performMutation(DataElement obj, int curr)
        {
            var sizeRelation = obj.GetSizeRelation();
            var objOf = sizeRelation.Of;
            var size = ((Number)obj).Size;
            var realSize = objOf.Value.LengthBytes;
            var diff = size - realSize;
            n = size + curr;

            if (n - diff < 0)
            {
                objOf.MutatedValue = new Variant("");
                return;
            }

            // can we make the value?
            if (n <= 0)
            {
                objOf.MutatedValue = new Variant("");
            }
            else if (n < size)
            {
                // shorten the size
                byte[] data = objOf.Value.Value;
                List<byte> newData = new List<byte>();

                for (int i = 0; i < n - diff; ++i)
                    newData.Add(data[i]);

                objOf.MutatedValue = new Variant(newData.ToArray());
                objOf.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
            }
            else if (size == 0)
            {
                // fill in with A's
                List<byte> newData = new List<byte>();

                for (int i = 0; i < n - diff; ++i)
                    newData.Add((byte)('A'));

                objOf.MutatedValue = new Variant(newData.ToArray());
                objOf.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
            }
            else
            {
                try
                {
                    // wrap the data to fill size
                    byte[] data = objOf.Value.Value;
                    List<byte> newData = new List<byte>();
                    int cnt = 0;

                    while (cnt < n - diff)
                    {
                        for (int i = 0; i < data.Length; ++i)
                        {
                            newData.Add(data[i]);
                            cnt++;

                            if (cnt >= n - diff)
                                break;
                        }
                    }

                    objOf.MutatedValue = new Variant(newData.ToArray());
                    objOf.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
                }
                catch
                {
                    // catch divide by zero exception
                    objOf.MutatedValue = new Variant("");
                }
            }

            //objOf.Invalidate();
            //var wtf = obj.GenerateValue();
            //int wolol = 0;
        }
	}
}
