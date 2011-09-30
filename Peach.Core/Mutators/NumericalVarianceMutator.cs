
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
    [Mutator("Produce numbers that are defaultValue - N to defaultValue + N")]
    [Hint("NumericalVarianceMutator-N", "Gets N by checking node for hint, or returns default (50).")]
	public class NumericalVarianceMutator : Mutator
	{
        // members
        //
        int n;
        long minValue;
        ulong maxValue;
        int currentCount;
        int[] values;
        Number objAsNumber;

        // CTOR
        //
        public NumericalVarianceMutator(DataElement obj)
        {
            objAsNumber = (Number)(obj);
            currentCount = 0;
            n = getN(obj, 50);
            name = "NumericalVarianceMutator";
            PopulateValues();

            if (obj is Dom.String)
            {
                minValue = Int32.MinValue;
                maxValue = UInt32.MaxValue;
            }
            else
            {
                minValue = objAsNumber.MinValue;
                maxValue = objAsNumber.MaxValue;
            }
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
            if (obj.Hints.ContainsKey("NumericalVarianceMutator-N"))
            {
                Hint h = null;
                if (obj.Hints.TryGetValue("NumericalVarianceMutator-N", out h))
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
            if (currentCount > count)
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
            if (obj is Dom.String && obj.isMutable)
            {
                if (obj.Hints.ContainsKey("NumericalString"))
                    return true;
            }

            // Disable for 8-bit ints, we've tried all values already
            if ((obj is Dom.Number || obj is Dom.Flag) && obj.isMutable)
                if (((Number)obj).Size > 8)
                    return true;

            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
            // verify the value against min/max values and skip invalid ones
            if (currentCount >= count)
                return;

            long value = ((long)((Variant)objAsNumber.DefaultValue)) - values[currentCount];

            if (value >= minValue)
            {
                if (value >= 0 && (ulong)value >= maxValue)
                    return;
                else if (obj is Dom.String)
                    obj.MutatedValue = new Variant(value.ToString());
                else
                    obj.MutatedValue = new Variant(value);
            }
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            try
            {
                int value = context.random.Choice(values);
                long finalValue = ((long)((Variant)objAsNumber.DefaultValue)) - value;

                if (obj is Dom.String)
                    obj.MutatedValue = new Variant(finalValue.ToString());
            }
            catch
            {
                // OK to skip, another mutator probably changes this value already - (such as datatree)
                return;
            }
        }
	}
}
