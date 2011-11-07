
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
    [Mutator("Produce a finite number of random numbers for each <Number> element")]
    [Hint("FiniteRandomNumbersMutator-N", "Gets N by checking node for hint, or returns default (5000).")]
	public class FiniteRandomNumbersMutator : Mutator
	{
        // members
        //
        int n;
        int size;
        bool signed;
        int currentCount;
        long minValue;
        ulong maxValue;

        //int i32min = Int32.MinValue;
        //int i32max = Int32.MaxValue;
        //uint ui32max = UInt32.MaxValue;

        //long i64min = Int64.MinValue;
        //long i64max = Int64.MaxValue;
        //ulong ui64max = UInt64.MaxValue;

        // CTOR
        //
        public FiniteRandomNumbersMutator(DataElement obj)
        {
            currentCount = 0;
            n = getN(obj, 5000);
            name = "FiniteRandomNumbersMutator";

            if (obj is Dom.String)
            {
                signed = false;
                size = 32;
                minValue = 0;
                maxValue = UInt32.MaxValue;
            }
            else
            {
                signed = ((Number)obj).Signed;
                size = ((Number)obj).Size;
                minValue = ((Number)obj).MinValue;
                maxValue = ((Number)obj).MaxValue;
            }
        }

        // GET N
        //
        public int getN(DataElement obj, int n)
        {
            // check for hint
            if (obj.Hints.ContainsKey("FiniteRandomNumbersMutator-N"))
            {
                Hint h = null;
                if (obj.Hints.TryGetValue("FiniteRandomNumbersMutator-N", out h))
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
            if (currentCount > n)
                throw new MutatorCompleted();
        }

        // COUNT
        //
        public override int count
        {
            get { return n; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if ((obj is Dom.Number || obj is Dom.Flag) && obj.isMutable)
                if (((Number)obj).Size > 8)
                    return true;

            if (obj is Dom.String && obj.isMutable)
            {
                if (obj.Hints.ContainsKey("NumericalString"))
                    return true;
            }

            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
            context.random.Seed = currentCount;

            //byte[] value = GenerateValue();
            //int value = context.random.Next((int)minValue, (int)maxValue);
            int value = context.random.Next((int)minValue, Int32.MaxValue);

            if (obj is Dom.String)
                obj.MutatedValue = new Variant(value.ToString());
            else
                obj.MutatedValue = new Variant(value);
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            //byte[] value = GenerateValue();
            //int value = context.random.Next((int)minValue, (int)maxValue);
            int value = context.random.Next((int)minValue, Int32.MaxValue);

            if (obj is Dom.String)
                obj.MutatedValue = new Variant(value.ToString());
            else
                obj.MutatedValue = new Variant(value);
        }

        // GENERATE_VALUE
        //
        private byte[] GenerateValue()
        {
            if (size <= 32)
            {
                if (signed)
                {
                    int value = context.random.Next((int)minValue, (int)maxValue);
                    return BitConverter.GetBytes(value);
                }
                else
                {
                    uint value = context.random.NextUInt32();
                    return BitConverter.GetBytes(value);
                }
            }
            else
            {
                if (signed)
                {
                    long value = context.random.NextInt64((long)minValue, (long)maxValue);
                    return BitConverter.GetBytes(value);
                }
                else
                {
                    ulong value = context.random.NextUInt64();
                    return BitConverter.GetBytes(value);
                }
            }
        }
	}
}
