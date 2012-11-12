
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
    [Mutator("Remove nodes from a data tree")]
    public class DataElementRemoveMutator : Mutator
    {
        // CTOR
        //
        public DataElementRemoveMutator(DataElement obj)
        {
            name = "DataElementRemoveMutator";
        }

        // MUTATION
        //
        public override uint mutation
        {
            get { return 0; }
            set { }
        }

        // COUNT
        //
        public override int count
        {
            get { return 1; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if (obj.isMutable && !(obj is Flag))
                return true;

            return false;
        }

        // SEQUENTIAL_MUTATION
        //
        public override void sequentialMutation(DataElement obj)
        {
            // remove the element from the data model
            obj.parent.Remove(obj);
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            obj.parent.Remove(obj);
            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
        }
    }
}

// end
