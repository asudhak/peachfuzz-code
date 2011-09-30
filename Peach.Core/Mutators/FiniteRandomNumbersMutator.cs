
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
    //[Mutator("Produce a finite number of random numbers for each <Number> element")]
    [Hint("FiniteRandomNumbersMutator-N", "Gets N by checking node for hint, or returns default (5000).")]
	public class FiniteRandomNumbersMutator : Mutator
	{
        // members
        //
        int n;
        int currentCount;
        long minValue;
        ulong maxValue;
        Number objAsNumber;

        // CTOR
        //
        public FiniteRandomNumbersMutator(DataElement obj)
        {
            objAsNumber = (Number)(obj);
            currentCount = 0;
            n = getN(obj, 2500);
            name = "FiniteRandomNumbersMutator";

            if (obj is Dom.String)
            {
                minValue = 0;
                maxValue = UInt32.MaxValue;
            }
            else
            {
                minValue = objAsNumber.MinValue;
                maxValue = objAsNumber.MaxValue;
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

            int maxAsInt = (((int)maxValue) + 1) * -1;
            int value = context.random.Next((int)minValue, maxAsInt);

            if (obj is Dom.String)
                obj.MutatedValue = new Variant(value.ToString());
            else
                obj.MutatedValue = new Variant(value);
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            int maxAsInt = (((int)maxValue) + 1) * -1;
            int value = context.random.Next((int)minValue, maxAsInt);

            if (obj is Dom.String)
                obj.MutatedValue = new Variant(value.ToString());
            else
                obj.MutatedValue = new Variant(value);
        }
	}
}
