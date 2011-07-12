
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
//using System.Random;

namespace Peach.Core.Mutators
{
    [Mutator("Changes the case of a string")]
	public partial class StringCaseMutator : Mutator
	{
        // members
        //
        public delegate void mutationType(Dom.DataElement obj);
        mutationType[] mutations = new mutationType[3]; // 1 - LowerCase
                                                        // 2 - UpperCase
                                                        // 3 - RandomCase
        uint index;

        // CTOR
        //
        public StringCaseMutator()
        {
            index = 0;
            mutations[0] = new mutationType(mutationLowerCase);
            mutations[1] = new mutationType(mutationUpperCase);
            mutations[2] = new mutationType(mutationRandomCase);
        }

        // NEXT
        //
        public override void next()
        {
            index++;
            if (index >= mutations.Length)
                throw new MutatorCompleted();
        }

        // COUNT
        //
        public override int count
        {
            get { return mutations.Length; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if (obj is Dom.String)
                return true;

            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(Dom.DataElement obj)
        {
            mutations[index](obj);
        }

        // RANDOM_MUTATION
        //
        public override void randomMutation(Dom.DataElement obj)
        {
            context.random.Choice<mutationType>(mutations)(obj);
        }

        // MUTATION_LOWER_CASE
        //
        public void mutationLowerCase(Dom.DataElement obj)
        {
            string str = (string)obj.InternalValue;
            obj.MutatedValue = new Variant(str.ToLower());
        }

        // MUTATION_UPPER_CASE
        //
        public void mutationUpperCase(Dom.DataElement obj)
        {
            string str = (string)obj.InternalValue;
            obj.MutatedValue = new Variant(str.ToUpper());
        }

        // MUTATION_RANDOM_CASE
        //
        public void mutationRandomCase(Dom.DataElement obj)
        {
            string str = (string)obj.InternalValue;
            string[] cases = new string[2];

            // check for strings over 20 chars to poll random indices from
            if (str.Length > 20)
            {
                foreach (int i in Sample(str.Length))
                {
                    char c = str[i];
                    cases[0] = c.ToString().ToLower();
                    cases[1] = c.ToString().ToUpper();
                    string s = context.random.Choice<string>(cases);
                    str = str.Substring(0, i) + s + str.Substring(i + 1, str.Length - (i + 1));
                }

                obj.MutatedValue = new Variant(str);
                return;
            }

            // loop through string one char at a time
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                cases[0] = c.ToString().ToLower();
                cases[1] = c.ToString().ToUpper();
                string s = context.random.Choice<string>(cases);
                str = str.Substring(0, i) + s + str.Substring(i + 1, str.Length - (i + 1));
            }

            obj.MutatedValue = new Variant(str);
            return;
        }

        // SAMPLE
        //
        private int[] Sample(int max)
        {
            int[] ret = new int[20];

            for (int i = 0; i < 20; ++i)
                ret[i] = context.random.Next(max);

            return ret;
        }
	}
}
