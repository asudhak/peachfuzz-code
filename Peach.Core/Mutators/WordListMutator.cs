//
// Copyright (c) Mick Ayzenberg
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
//   Mick Ayzenberg (mick@dejavusecurity.com)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;

namespace Peach.Core.Mutators
{
    [Mutator("Allows a word list of different valid values to be specified")]
    [Hint("WordList", "Wordlist Containing newline seperated valid strings.")]
    public class WordListMutator : Mutator
    {
        // members
        //
        uint pos = 0;
        string[] values = new string[] { };

        // CTOR
        //
        public WordListMutator(DataElement obj)
        {
            pos = 0;
            name = "WordListMutator";
            generateValues(obj);
        }

        // GENERATE VALUES
        //
        public void generateValues(DataElement obj)
        {
            // 1. Get filename in hint
            // 2. Run function to add values in filename to list

            Hint h = null;
            if (obj.Hints.TryGetValue("WordList", out h))
            {
                AddListToValues(h.Value);                
            }
        }

        private void AddListToValues(string curfile)
        {
            var newvalues = new List<string>();
            if (System.IO.File.Exists(curfile))
            {
                newvalues.AddRange(System.IO.File.ReadAllLines(curfile));
            }
            else
            {
                throw new PeachException("Invalid Wordlist File: " + curfile);
            }
            newvalues.AddRange(values);
            values = newvalues.ToArray();
        }

        public override uint mutation
        {
            get { return pos; }
            set { pos = value; }
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
            if ((obj is Dom.String || obj is Dom.Number || obj is Dom.Blob) && obj.isMutable)
            {
                if (obj.Hints.ContainsKey("WordList"))
                    return true;
            }

            return false;
        }

        // SEQUENTIAL_MUTATION
        //
        public override void sequentialMutation(DataElement obj)
        {
            obj.MutatedValue = new Variant(values[pos]);
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
        }

        // RANDOM_MUTATION
        //
        public override void randomMutation(DataElement obj)
        {
            obj.MutatedValue = new Variant(context.Random.Choice(values));
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
        }
    }
}

// end
