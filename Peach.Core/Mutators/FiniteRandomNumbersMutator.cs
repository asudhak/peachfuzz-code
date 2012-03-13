
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

        // CTOR
        //
        public FiniteRandomNumbersMutator(DataElement obj)
        {
            name = "FiniteRandomNumbersMutator";
            currentCount = 0;
            n = getN(obj, 5000);

            if (obj is Dom.String)
            {
                signed = false;
                size = 32;
                minValue = 0;
                maxValue = UInt32.MaxValue;
            }
            else if (obj is Number)
            {
                signed = ((Number)obj).Signed;
                size = (int) ((Number)obj).lengthAsBits;
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
            if (currentCount >= n)
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
            if (obj is Number && obj.isMutable)
                if (((Number)obj).lengthAsBits > 8)
                    return true;

            if (obj is Flag && obj.isMutable)
                if (((Flag)obj).size > 8)
                    return true;

            if (obj is Dom.String && obj.isMutable)
                if (obj.Hints.ContainsKey("NumericalString"))
                    return true;

            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
            context.random.Seed = currentCount;

            // handle strings
            if (obj is Dom.String)
            {
                UInt32 value = context.random.NextUInt32();
                obj.MutatedValue = new Variant(value.ToString());
                return;
            }

            if (signed)
            {
                if (size <= 32)
                {
                    Int32 value = context.random.Next((int)minValue, (int)maxValue);
                    obj.MutatedValue = new Variant(value);
                }
                else
                {
                    Int64 value = context.random.NextInt64();
                    obj.MutatedValue = new Variant(value);
                }
            }
            else
            {
                if (size <= 32)
                {
                    UInt32 value = context.random.NextUInt32();
                    obj.MutatedValue = new Variant(value);
                }
                else
                {
                    UInt64 value = context.random.NextUInt64();
                    obj.MutatedValue = new Variant(value);
                }
            }
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            // handle strings
            if (obj is Dom.String)
            {
                UInt32 value = context.random.NextUInt32();
                obj.MutatedValue = new Variant(value.ToString());
                return;
            }

            if (signed)
            {
                if (size <= 32)
                {
                    Int32 value = context.random.Next((int)minValue, (int)maxValue);
                    obj.MutatedValue = new Variant(value);
                }
                else
                {
                    Int64 value = context.random.NextInt64();
                    obj.MutatedValue = new Variant(value);
                }
            }
            else
            {
                if (size <= 32)
                {
                    UInt32 value = context.random.NextUInt32();
                    obj.MutatedValue = new Variant(value);
                }
                else
                {
                    UInt64 value = context.random.NextUInt64();
                    obj.MutatedValue = new Variant(value);
                }
            }
        }
	}
}

// end
