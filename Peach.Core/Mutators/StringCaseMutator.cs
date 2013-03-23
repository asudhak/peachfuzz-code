
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
    [Mutator("Changes the case of a string")]
    public class StringCaseMutator : Mutator
    {
        // members
        //
        public delegate void mutationType(DataElement obj);
        mutationType[] mutations = new mutationType[3];
        uint index;

        // CTOR
        //
        public StringCaseMutator(DataElement obj)
        {
            index = 0;
            name = "StringCaseMutator";
            mutations[0] = new mutationType(mutationLowerCase);
            mutations[1] = new mutationType(mutationUpperCase);
            mutations[2] = new mutationType(mutationRandomCase);
        }

        // COUNT
        //
        public override int count
        {
            get { return mutations.Length; }
        }

        public override uint mutation
        {
            get { return index; }
            set { index = value; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if (obj is Dom.String && obj.isMutable)
                return true;

            return false;
        }

        // SEQUENTIAL_MUTATION
        //
        public override void sequentialMutation(DataElement obj)
        {
            // Only called via the Sequential mutation strategy, which should always have a consistent seed
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            mutations[index](obj);
        }

        // RANDOM_MUTATION
        //
        public override void randomMutation(DataElement obj)
        {
            if ((obj.mutationFlags & DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM) != 0)
                return;

            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            context.Random.Choice(mutations)(obj);
        }

        // MUTATION_LOWER_CASE
        //
        public void mutationLowerCase(DataElement obj)
        {
            string str = (string)obj.InternalValue;
            obj.MutatedValue = new Variant(str.ToLower());
        }

        // MUTATION_UPPER_CASE
        //
        public void mutationUpperCase(DataElement obj)
        {
            string str = (string)obj.InternalValue;
            obj.MutatedValue = new Variant(str.ToUpper());
        }

        // MUTATION_RANDOM_CASE
        //
        public void mutationRandomCase(DataElement obj)
        {
            StringBuilder builder = new StringBuilder((string)obj.InternalValue);
            char[] cases = new char[2];
            char c;

            foreach (int i in Sample(builder.Length))
            {
                c = builder[i];
                cases[0] = Char.ToLower(c);
                cases[1] = Char.ToUpper(c);

                builder[i] = context.Random.Choice(cases);
            }

            obj.MutatedValue = new Variant(builder.ToString());
            return;
        }

        /// <summary>
        /// Return a sampling of indexes based on max index.
        /// </summary>
        /// <remarks>
        /// For indexes &lt; 20 we return all indexes.  When
        /// over 20 we return a max of 20 samples.
        /// </remarks>
        /// <param name="max">Max index</param>
        /// <returns></returns>
        private int[] Sample(int max)
        {
            if (max < 20)
            {
                int[] ret = new int[max];

                for (int i = 0; i < ret.Length; i++)
                    ret[i] = i;

                return ret;
            }
            else
            {
                List<int> ret = new List<int>();
                int index;

                for (int i = 0; i < 20; ++i)
                {
                    do
                    {
                        index = context.Random.Next(max);
                    }
                    while (ret.Contains(index));

                    ret.Add(index);
                }

                return ret.ToArray();
            }
        }
    }
}

// end
