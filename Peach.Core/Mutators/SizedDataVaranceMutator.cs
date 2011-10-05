
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
    //[Mutator("Change the length of sized data to count - N to count + N. Size indicator will stay the same.")]
    [Hint("SizedDataVaranceMutator-N", "Gets N by checking node for hint, or returns default (50).")]
	public class SizedDataVaranceMutator : Mutator
	{
        // members
        //
        int n;
        int[] values;
        int currentCount;

        // CTOR
        //
        public SizedDataVaranceMutator(DataElement obj)
        {
            currentCount = 0;
            n = getN(obj, 50);
            name = "SizedDataVaranceMutator";
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
            if (obj.Hints.ContainsKey("SizedDataVaranceMutator-N"))
            {
                Hint h = null;
                if (obj.Hints.TryGetValue("SizedDataVaranceMutator-N", out h))
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
        public void performMutation(DataElement obj, int curr)
        {
            var sizeRelation = obj.GetSizeRelation();
            var objOf = sizeRelation.Of;
            var size = ((Number)obj).Size;
            var realSize = objOf.Value.LengthBytes;
            n = size + curr;

            //if (n == 0)
            //    objOf.Value = new Variant("");
            //else if (n < size)
            //    objOf.Value = objOf.getValue()[:n];
            //else if (size == 0)
            //    objOf.Value = "A" * n;
            //else
            //    objOf.Value = (objOf.getValue() * ((n/realSize)+1))[:n];
        }
	}
}
