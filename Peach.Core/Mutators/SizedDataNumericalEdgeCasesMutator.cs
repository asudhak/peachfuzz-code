
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
    [Mutator("Change the length of sized data to numerical edge cases")]
    [Hint("SizedDataNumericalEdgeCasesMutator-N", "Gets N by checking node for hint, or returns default (50).")]
    public class SizedDataNumericalEdgeCasesMutator : Mutator
    {
        static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        // members
        //
        int n;
        long[] values;
        uint currentCount;
        //long originalDataLength;

        // CTOR
        //
        public SizedDataNumericalEdgeCasesMutator(DataElement obj)
        {
            currentCount = 0;
            n = getN(obj, 50);
            name = "SizedDataNumericalEdgeCasesMutator";
            //originalDataLength = (long)obj.GenerateInternalValue();
            PopulateValues(obj);
        }

        // POPULATE_VALUES
        //
        private void PopulateValues(DataElement obj)
        {
            int size = 0;

            if (obj is Number || obj is Flag)
            {
                size = (int)obj.lengthAsBits;
            }
            else
            {
                size = 64;
            }

            if (size < 16)
                values = NumberGenerator.GenerateBadNumbers(8, n);
            else
                values = NumberGenerator.GenerateBadNumbers(16, n);

            // this will weed out invalid values that would cause the length to be less than 0
            List<long> newVals = new List<long>(values);
            newVals.RemoveAll(RemoveInvalid);
            values = newVals.ToArray();
        }

        private bool RemoveInvalid(long n)
        {
#if MONO
			return n < 0 || n > 1000;
#else
			return n < 0;
#endif
		}

        // GET N
        //
        public int getN(DataElement obj, int n)
        {
            // check for hint
            if (obj.Hints.ContainsKey("SizedDataNumericalEdgeCasesMutator-N"))
            {
                Hint h = null;
                if (obj.Hints.TryGetValue("SizedDataNumericalEdgeCasesMutator-N", out h))
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
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            performMutation(obj, context.Random.Choice(values));
        }

        // PERFORM_MUTATION
        //
        private void performMutation(DataElement obj, long curr)
        {
            logger.Debug("performMutation(curr=" + curr + ")");

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

			n = (int)curr;

            obj.MutatedValue = null;
            objOf.MutatedValue = null;

            // make sure the data hasn't changed somewhere along the line
            //if (originalDataLength != size)
            //PopulateValues(obj);

            // Set all mutation flags up front
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_RELATIONS;
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
            objOf.mutationFlags |= DataElement.MUTATE_OVERRIDE_RELATIONS;
            objOf.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;

            // keep size indicator the same
            obj.MutatedValue = new Variant(obj.Value);

            byte[] data = objOf.Value.Value;
            BitStream newData = new BitStream();

            if (n <= 0)
            {
                objOf.MutatedValue = new Variant(new byte[0]);
                return;
            }
            else if (n < data.Length)
            {
                // shorten the size
                for (int i = 0; i < n; ++i)
                    newData.WriteByte(data[i]);
            }
            else if (data.Length == 0)
            {
                // fill in with A's
                for (int i = 0; i < n; ++i)
                    newData.WriteByte((byte)('A'));
            }
            else
            {
                // wrap the data to fill size
                logger.Debug("Expanding data from " + data.Length + " to " + n + " bytes");
                while (newData.LengthBytes < n)
                {
                    if ((newData.LengthBytes + data.Length) < n)
                    {
                        newData.WriteBytes(data);
                    }
                    else
                    {
                        for (int i = 0; i < data.Length && newData.LengthBytes < n; ++i)
                            newData.WriteByte(data[i]);
                    }
                }
            }

            logger.Debug("Setting MutatedValue");
            objOf.MutatedValue = new Variant(newData);
        }
    }
}

// end
