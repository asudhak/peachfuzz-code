
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
    [Mutator("Generates bad UTF-8 strings")]
	public partial class UnicodeBadUtf8Mutator : Mutator
	{
        // members
        //
        uint pos = 0;

        // CTOR
        //
        public UnicodeBadUtf8Mutator(DataElement obj)
        {
            pos = 0;
        }

        // BINARY_FORMATTER
        //
        public void binaryFormatter(int num, int bits, bool strip = false)
        {

        }

        // ONE_BYTE
        //
        public void utf8OneByte(char c)
        {

        }

        // TWO_BYTES
        //
        public void utf8TwoByte(char c, short mask)
        {

        }

        // THREE_BYTES
        //
        public void utf8ThreeByte(char c, int mask)
        {

        }

        // FOUR_BYTES
        //
        public void utf8FourByte(char c, int mask)
        {

        }

        // FIVE_BYTES
        //
        public void utf8FiveByte(char c, int mask)
        {

        }

        // SIX_BYTES
        //
        public void utf8SixByte(char c, int mask)
        {

        }

        // SEVEN_BYTES
        //
        public void utf8SevenByte(char c, int mask)
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
            if (obj is Dom.String)
                return true;

            return false;
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(Dom.DataElement obj)
        {
            //obj.MutatedValue = new Variant(values[pos]);
        }

        // RANDOM_MUTATION
        //
        public override void randomMutation(Dom.DataElement obj)
        {
            //obj.MutatedValue = new Variant(context.random.Choice<string>(values));
        }
	}
}
