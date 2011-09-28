
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
    //[Mutator("This is a straight up generation class. Produces values that have nothing to do with defaultValue")]
	public class NumericalEdgeCaseMutator : Mutator
	{
        // members
        //
        int[][] values;
        int[] allowedSizes;
        int currentCount;
        int selfCount;
        int minValue;
        uint maxValue;
        int size;
        int n;

        // CTOR
        //
        public NumericalEdgeCaseMutator(DataElement obj)
        {
            allowedSizes = new int[] { 8, 16, 32, 64 };
            name = "NumericalEdgeCaseMutator";
            n = getN(obj, 50);
            currentCount = 0;
            selfCount = 0;

            if (values == null)
                PopulateValues();

            if (obj is Dom.String)
            {
                size = 32;                      // size is what size of data we are dealing with
                minValue = Int32.MinValue;
                maxValue = UInt32.MaxValue;
            }
            else
            {
                minValue = values[0].Length;
                maxValue = (uint)(values[values.Length - 1].Length);
                //size = obj.size;
            }

            // if size is off, pick up the next largest one from allowedSizes
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
                        throw new PeachException("Expected numerical value for Hint named NumericalEdgeCaseMutator-N");
                    }
                }
            }

            return n;
        }

        // POPULATE_VALUES
        //
        private void PopulateValues()
        {
        }

        // NEXT
        //
        public override void next()
        {
            currentCount++;
            if (currentCount >= values.Length)
                throw new MutatorCompleted();
        }

        // COUNT
        //
        public override int count
        {
            get 
            {
                if (selfCount == 0)
                {
                    int cnt = 0;

                    for (int i = 0; i < values[size].Length; ++i)
                    {
                        if (values[size][i] < minValue || values[size][i] > maxValue)
                            continue;
                        cnt++;
                    }

                    selfCount = cnt;
                }

                return selfCount;
            }
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
            // verify the value against min/max values and skip invalid ones
            while (true)
            {
                int value = values[size][currentCount];

                if ((long)value <= minValue || (long)value >= maxValue)
                    break;

                if (obj is Dom.String)
                    obj.MutatedValue = new Variant(value.ToString());
                else
                    obj.MutatedValue = new Variant(value);

                try
                {
                    next();
                }
                catch
                {
                    break;
                }
            }

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
