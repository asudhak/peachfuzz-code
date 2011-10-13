
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
    //[Mutator("Swap two nodes in the data model that are near each other")]
	public class DataElementSwapNearNodesMutator : Mutator
	{
        // members
        //

        // CTOR
        //
        public DataElementSwapNearNodesMutator(DataElement obj)
        {
            name = "DataElementSwapNearNodesMutator";
        }

        // NEXT
        //
        public override void next()
        {
            throw new MutatorCompleted();
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
            if (obj.isMutable)
                return true;

            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
            var nextNode = obj.nextSibling();
            if (nextNode != null)
            {
                var v1 = obj.Value.Value;
                var v2 = nextNode.Value.Value;

                obj.MutatedValue = new Variant(v2);
                nextNode.MutatedValue = new Variant(v1);
            }
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            sequencialMutation(obj);
        }

        // NEXT_NODE
        //
        private void nextNode(DataElement obj)
        {
            // TODO ???

            // walk down node tree

            // walk over or up if we can
        }

        // MOVE_NEXT
        //
        private void moveNext(DataElement obj)
        {
            // TODO ???

            // check if we are top

            // get sibling

            // get sibling of parent
        }
	}
}
