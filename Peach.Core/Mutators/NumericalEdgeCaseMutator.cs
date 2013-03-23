
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
    [Mutator("This is a straight up generation class. Produces values that have nothing to do with defaultValue")]
    [Hint("NumericalEdgeCaseMutator-N", "Gets N by checking node for hint, or returns default (50).")]
    public class NumericalEdgeCaseMutator : Mutator
    {
        // members
        //
        Dictionary<int, long[]> values;
        List<int> allowedSizes;
        ulong[] ulongValues;
        int n;
        int size;
        uint currentCount;
        long minValue;
        ulong maxValue;
        bool signed;
        bool isULong;

        // CTOR
        //
        public NumericalEdgeCaseMutator(DataElement obj)
        {
            allowedSizes = new List<int>() { 8, 16, 24, 32, 64 };
            values = new Dictionary<int, long[]>();
            name = "NumericalEdgeCaseMutator";
            isULong = false;
            n = getN(obj, 50);
            currentCount = 0;

            if (obj is Dom.String)
            {
                size = 32;
                signed = false;
                minValue = Int32.MinValue;
                maxValue = UInt32.MaxValue;
            }
            else if (obj is Number || obj is Flag)
            {
                size = (int)((Number)obj).lengthAsBits;
                signed = ((Number)obj).Signed;
                minValue = ((Number)obj).MinValue;
                maxValue = ((Number)obj).MaxValue;

                if (size == 64 && !signed)
                    isULong = true;
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
            if (!isULong)
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
                    if (signed)
                    {
                        if (values[size][i] >= minValue && values[size][i] <= (long)maxValue)
                            listVals.Add(values[size][i]);
                    }
                    else
                    {
                        if (values[size][i] >= minValue && (ulong)values[size][i] <= maxValue)
                            listVals.Add(values[size][i]);
                    }
                }

                values[size] = listVals.ToArray();
            }
            else
            {
                ulong[] uEdges64 = NumberGenerator.GenerateBadPositiveUInt64(n);
                ulongValues = uEdges64;

                List<ulong> listUVals = new List<ulong>();
                for (int i = 0; i < ulongValues.Length; ++i)
                {
                    if (ulongValues[i] >= 0 && ulongValues[i] <= maxValue)
                        listUVals.Add(ulongValues[i]);
                }

                ulongValues = listUVals.ToArray();
            }
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
            get
            {
                if (isULong)
                    return ulongValues.Length;
                else
                    return values[size].Length;
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

        // SEQUENTIAL_MUTATION
        //
        public override void sequentialMutation(DataElement obj)
        {
            if (obj is Dom.String)
                obj.MutatedValue = new Variant(values[size][currentCount].ToString());
            else if (isULong)
                obj.MutatedValue = new Variant(ulongValues[currentCount]);
            else
                obj.MutatedValue = new Variant(values[size][currentCount]);

            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            if (obj is Dom.String)
                obj.MutatedValue = new Variant(context.Random.Choice(values[size]).ToString());
            else if (isULong)
                obj.MutatedValue = new Variant(context.Random.Choice(ulongValues));
            else
                obj.MutatedValue = new Variant(context.Random.Choice(values[size]));

            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
        }
    }
}

// end
