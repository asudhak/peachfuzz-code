
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
    [Mutator("Produce numbers that are defaultValue - N to defaultValue + N")]
    [Hint("NumericalVarianceMutator-N", "Gets N by checking node for hint, or returns default (50).")]
	public class NumericalVarianceMutator : Mutator
	{
        // members
        //
        int n;
        int currentCount;
        int valuesLength;
        long defaultValue;
        long minValue;
        ulong maxValue;
        long[] values;
        bool signed;

        // CTOR
        //
        public NumericalVarianceMutator(DataElement obj)
        {
            name = "NumericalVarianceMutator";
            currentCount = 0;
            defaultValue = (long)obj.DefaultValue;
            n = getN(obj, 50);

            if (obj is Dom.String)
            {
                signed = false;
                minValue = Int32.MinValue;
                maxValue = UInt32.MaxValue;
            }
            else if (obj is Number)
            {
                signed = ((Number)obj).Signed;
                minValue = ((Number)obj).MinValue;
                maxValue = ((Number)obj).MaxValue;
            }
            else if (obj is Flag)
            {
                signed = false;
                minValue = 0;
                maxValue = UInt32.MaxValue;
            }

            PopulateValues(defaultValue);
        }

        // POPULATE_VALUES
        //
        private void PopulateValues(long val)
        {
            // catch n == 0
            if (n == 0)
            {
                valuesLength = 0;
                return;
            }

            // generate values from [-n, n]
            List<long> temp = new List<long>();
            for (int i = -n; i <= n; ++i)
            {
                if ((val + i) >= minValue && (val + i) <= (long)maxValue)
                    temp.Add(i);
            }

            // setup values
            values = temp.ToArray();
            valuesLength = values.Length;
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
            if (currentCount >= count)
                throw new MutatorCompleted();
        }

        // COUNT
        //
        public override int count
        {
            get { return valuesLength; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if (obj is Dom.String && obj.isMutable)
                if (obj.Hints.ContainsKey("NumericalString"))
                    return true;

            if (obj is Number && obj.isMutable)
                if (((Number)obj).lengthAsBits > 8)
                    return true;

            if (obj is Flag && obj.isMutable)
                if (((Flag)obj).size > 8)
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

            long value = (long)obj.DefaultValue + values[currentCount];

            if (obj is Dom.String)
                obj.MutatedValue = new Variant(value.ToString());
            else
                obj.MutatedValue = new Variant(value);
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            if (currentCount >= count)
                return;

            try
            {
                var rand = new Random(context.random.Seed + context.IterationCount + obj.fullName.GetHashCode());
                long value = rand.Choice(values);
                long finalValue = (long)obj.DefaultValue + value;
                if (obj is Dom.String)
                    obj.MutatedValue = new Variant(finalValue.ToString());
                else
                    obj.MutatedValue = new Variant(finalValue);
            }
            catch
            {
                // OK to skip, another mutator probably changes this value already - (such as datatree)
                return;
            }
        }
	}
}

// end
