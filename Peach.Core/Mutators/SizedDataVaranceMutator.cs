
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
using Peach.Core.IO;
using NLog;

namespace Peach.Core.Mutators
{
    [Mutator("Change the length of sized data to count - N to count + N. Size indicator will stay the same.")]
    [Hint("SizedDataVaranceMutator-N", "Gets N by checking node for hint, or returns default (50).")]
    public class SizedDataVaranceMutator : Mutator
    {
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		// members
        //
        int n;
        int[] values;
        uint currentCount;
        long originalDataLength;

        // CTOR
        //
        public SizedDataVaranceMutator(DataElement obj)
        {
            currentCount = 0;
            n = getN(obj, 50);
            name = "SizedDataVaranceMutator";
            originalDataLength = (long)obj.InternalValue;
            PopulateValues(originalDataLength);
        }

        // POPULATE_VALUES
        //
        private void PopulateValues(long length)
        {
            // generate values from [-n, n]
            List<int> temp = new List<int>();

            for (int i = -n; i <= n; ++i)
            {
                // only add valid n-values
                if (length + i <= 0)
                    continue;
                temp.Add(i);
            }

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
            get { return values.Length; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            // verify data element has size relation
            if (obj.isMutable && obj.relations.hasFromSizeRelation)
                return true;

            return false;
        }

        // SEQUENTIAL_MUTATION
        //
        public override void sequentialMutation(DataElement obj)
        {
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            performMutation(obj, values[currentCount]);
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            performMutation(obj, context.Random.Choice(values));
        }

        // PERFORM_MUTATION
        //
        private void performMutation(DataElement obj, int curr)
        {
            var sizeRelation = obj.relations.getFromSizeRelation();
			if (sizeRelation == null)
			{
				logger.Error("Error, sizeRelation == null, unable to perform mutation.");
				return;
			}

			var objOf = sizeRelation.Of;
			if (objOf == null)
			{
				logger.Error("Error, sizeRelation.Of == null, unable to perform mutation.");
				return;
			}

			var size = (long)obj.InternalValue;
            var realSize = objOf.Value.LengthBytes;
            n = (int)size + curr;

            // keep size indicator the same
            obj.MutatedValue = obj.InternalValue;

            byte[] data = objOf.Value.Value;
            List<byte> newData = new List<byte>();

            objOf.mutationFlags = DataElement.MUTATE_DEFAULT;
            objOf.mutationFlags |= DataElement.MUTATE_OVERRIDE_RELATIONS;
            objOf.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;

            if (n < 0)
            {
                return;
            }
            else if (n == 0)
            {
                objOf.MutatedValue = new Variant(new byte[0]);
                return;
            }
            else if (n < realSize)
            {
                // shorten the size
                for (int i = 0; i < n; ++i)
                    newData.Add(data[i]);
            }
            else if (data.Length == 0)
            {
                // fill in with A's
                for (int i = 0; i < n; ++i)
                    newData.Add((byte)('A'));
            }
            else
            {
                // wrap the data to fill size
                int cnt = 0;

                while (cnt < n)
                {
                    for (int i = 0; i < data.Length; ++i)
                    {
                        newData.Add(data[i]);
                        cnt++;

                        if (cnt >= n)
                            break;
                    }
                }
            }

            objOf.MutatedValue = new Variant(newData.ToArray());
        }
    }
}

// end
