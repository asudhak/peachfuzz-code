
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
    //[Mutator("Performs the W3C parser tests. Only works on <String> elements with a <Hint name=\"type\" value=\"xml\">")]
    //[Hint("type", "Allows string to be mutated by the XmlW3CMutator.")]
	public class XmlW3CMutator : Mutator    // might inherit from SimpleGenerator???
	{
        // members
        //

        // CTOR
        //
        public XmlW3CMutator(DataElement obj)
        {

        }

        // NEXT
        //
        public override void next()
        {
            
        }

        // COUNT
        //
        public override int count
        {
            get { return 0; }
        }

        // SUPPORTED
        //
        public new static bool supportedDataElement(DataElement obj)
        {
            if (obj is Dom.String && obj.isMutable)
            {
                Hint h = null;
                if (obj.Hints.TryGetValue("type", out h))
                {
                    if (h.Value == "xml")
                        return true;
                }
            }

            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(Dom.DataElement obj)
        {
            
        }

        // RANDOM_MUTATION
        //
        public override void randomMutation(Dom.DataElement obj)
        {

        }
	}
}
