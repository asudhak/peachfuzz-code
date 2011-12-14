
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
    [Mutator("This is a straight up generation class. Produces values that have nothing to do with defaultValue")]
    [Hint("NumericalEdgeCaseMutator-N", "Gets N by checking node for hint, or returns default (50).")]
	public class NumericalEdgeCaseMutator : Mutator
	{
        // members
        //
        Dictionary<int, long[]> values;
        List<int> allowedSizes;
        int n;
        int size;
        int currentCount;
        long minValue;
        ulong maxValue;
        bool signed;

        // CTOR
        //
        public NumericalEdgeCaseMutator(DataElement obj)
        {
            allowedSizes = new List<int>() { 8, 16, 24, 32, 64 };
            values = new Dictionary<int, long[]>();
            name = "NumericalEdgeCaseMutator";
            n = getN(obj, 50);
            currentCount = 0;

            if (obj is Dom.String)
            {
                size = 32;
                signed = false;
                minValue = Int32.MinValue;
                maxValue = UInt32.MaxValue;
            }
            else if (obj is Number)
            {
                size = ((Number)obj).Size;
                signed = ((Number)obj).Signed;
                minValue = ((Number)obj).MinValue;
                maxValue = ((Number)obj).MaxValue;
            }
            else if (obj is Flag)
            {
                signed = false;
                size = ((Flag)obj).size;
                minValue = 0;
                maxValue = UInt32.MaxValue;
            }

            // if size is off, pick up the next largest one from allowedSizes
            if (!allowedSizes.Contains(size))
            {
                foreach (int sz in allowedSizes)
                {
                    if (size <= sz)
                    {
                        size = sz;
                        break;
                    }
                }
            }

            PopulateValues();
        }

        // POPULATE_VALUES
        //
        private void PopulateValues()
        {
            // generate numbers
            long[] edges8 = NumberGenerator.GenerateBadNumbers(8, n);
            long[] edges16 = NumberGenerator.GenerateBadNumbers(16, n);
            long[] edges24 = NumberGenerator.GenerateBadNumbers(24, n);
            long[] edges32 = NumberGenerator.GenerateBadNumbers(32, n);
            long[] edges64 = NumberGenerator.GenerateBadNumbers(64, n);

            values[8] = edges8;
            values[16] = edges16;
            values[24] = edges24;
            values[32] = edges32;
            values[64] = edges64;

            // setup values
            List<long> listVals = new List<long>();

            for (int i = 0; i < values[size].Length; ++i)
            {
                if (values[size][i] >= minValue && values[size][i] <= (long)maxValue)
                    listVals.Add(values[size][i]);
            }

            values[size] = listVals.ToArray();
        }

        // GET N
        //
        public int getN(DataElement obj, int n)
        {
            // check for hint
            if (obj.Hints.ContainsKey("NumericalEdgeCaseMutator-N"))
            {
                Hint h = null;
                if (obj.Hints.TryGetValue("NumericalEdgeCaseMutator-N", out h))
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
            get { return values[size].Length; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if (obj is Dom.String && obj.isMutable)
            {
                if (obj.Hints.ContainsKey("NumericalString"))
                    return true;
            }

            if ((obj is Dom.Number || obj is Dom.Flag) && obj.isMutable)
                return true;

            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
            // value should be valid by this point
            if (currentCount >= count)
                return;

            if (obj is Dom.String)
                obj.MutatedValue = new Variant(values[size][currentCount].ToString());
            else
                obj.MutatedValue = new Variant(values[size][currentCount]);
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            if (obj is Dom.String)
                obj.MutatedValue = new Variant(context.random.Choice(values[size]).ToString());
            else
                obj.MutatedValue = new Variant(context.random.Choice(values[size]));
        }
	}
}
