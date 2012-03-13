
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
    [Mutator("Can perform more changes than BlobBitFlipper. We will grow the blob, shrink the blob, etc.")]
	public class BlobMutator : BlobBitFlipperMutator
	{
        // members
        //
        public delegate byte[] changeFcn(DataElement obj);
        changeFcn[] changeFcns = new changeFcn[6];

        public delegate byte[] generateFcn(int size);
        generateFcn[] generateFcns = new generateFcn[5];        

        // CTOR
        //
        public BlobMutator(DataElement obj) : base(obj)
        {
            name = "BlobMutator";

            changeFcns[0] = new changeFcn(changeExpandBuffer);
            changeFcns[1] = new changeFcn(changeReduceBuffer);
            changeFcns[2] = new changeFcn(changeChangeRange);
            changeFcns[3] = new changeFcn(changeRangeSpecial);
            changeFcns[4] = new changeFcn(changeNullRange);
            changeFcns[5] = new changeFcn(changeUnNullRange);

            generateFcns[0] = new generateFcn(generateNewBytes);
            generateFcns[1] = new generateFcn(generateNewBytesSingleRandom);
            generateFcns[2] = new generateFcn(generateNewBytesIncrementing);
            generateFcns[3] = new generateFcn(generateNewBytesZero);
            generateFcns[4] = new generateFcn(generateNewBytesAllRandom);
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
            if (obj is Dom.Blob && obj.isMutable)
                return true;

            return false;
        }

        // GET_RANGE
        //
        private void getRange(int size, out int start, out int end)
        {
            start = context.random.Next(size);
            end = context.random.Next(size);

            if (start > end)
            {
                int temp = end;
                end = start;
                start = temp;
            }
        }

        // GET_POSITION
        //
        private int getPosition(int size, int len = 0)
        {
            return context.random.Next(size - len);
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
            performMutation(obj);
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            performMutation(obj);
        }

        // PERFORM_MUTATION
        //
        private void performMutation(DataElement obj)
        {
            obj.MutatedValue = new Variant(context.random.Choice<changeFcn>(changeFcns)(obj));
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
        }

        // EXPAND_BUFFER
        //
        private byte[] changeExpandBuffer(DataElement obj)
        {
            // expand the size of our buffer

            List<byte> listData = new List<byte>();
            var data = obj.Value.Value;
            int size = context.random.Next(255);
            int pos = getPosition(size);

            var pt1 = ArrayExtensions.Slice(data, 0, pos);
            var pt2 = generateNewBytes(size);
            var pt3 = ArrayExtensions.Slice(data, pos, data.Length);

            return ArrayExtensions.Combine(pt1, pt2, pt3);
        }

        // REDUCE_BUFFER
        //
        private byte[] changeReduceBuffer(DataElement obj)
        {
            // reduce the size of our buffer

            var data = obj.Value.Value;
            int start = 0;
            int end = 0;

            getRange(data.Length, out start, out end);

            var pt1 = ArrayExtensions.Slice(data, 0, start);
            var pt2 = ArrayExtensions.Slice(data, end, data.Length);

            return ArrayExtensions.Combine(pt1, pt2);
        }

        // CHANGE_RANGE
        //
        private byte[] changeChangeRange(DataElement obj)
        {
            // change a sequence of bytes in our buffer

            var data = obj.Value.Value;
            int start = 0;
            int end = 0;

            getRange(data.Length, out start, out end);

            if (end > start + 100)
                end = start + 100;

            foreach (int i in ArrayExtensions.Range(start, end, 1))
            {
                var pt1 = ArrayExtensions.Slice(data, 0, i);
                byte[] pt2 = { (byte)(context.random.Next(255)) };
                var pt3 = ArrayExtensions.Slice(data, i + 1, data.Length);
                data = ArrayExtensions.Combine(pt1, pt2, pt3);
            }

            return data;
        }

        // CHANGE_RANGE_SPECIAL
        //
        private byte[] changeRangeSpecial(DataElement obj)
        {
            // change a sequence of bytes in our buffer to some special chars

            var data = obj.Value.Value;
            int start = 0;
            int end = 0;
            byte[] special = { 0x00, 0x01, 0xFE, 0xFF };

            getRange(data.Length, out start, out end);

            if (end > start + 100)
                end = start + 100;

            foreach (int i in ArrayExtensions.Range(start, end, 1))
            {
                var pt1 = ArrayExtensions.Slice(data, 0, i);
                byte[] pt2 = { context.random.Choice(special) };
                var pt3 = ArrayExtensions.Slice(data, i + 1, data.Length);
                data = ArrayExtensions.Combine(pt1, pt2, pt3);
            }

            return data;
        }

        // NULL_RANGE
        //
        private byte[] changeNullRange(DataElement obj)
        {
            // change a range of bytes to null

            var data = obj.Value.Value;
            int start = 0;
            int end = 0;

            getRange(data.Length, out start, out end);

            if (end > start + 100)
                end = start + 100;

            foreach (int i in ArrayExtensions.Range(start, end, 1))
            {
                var pt1 = ArrayExtensions.Slice(data, 0, i);
                byte[] pt2 = { 0x00 };
                var pt3 = ArrayExtensions.Slice(data, i + 1, data.Length);
                data = ArrayExtensions.Combine(pt1, pt2, pt3);
            }

            return data;
        }

        // UNNULL_RANGE
        //
        private byte[] changeUnNullRange(DataElement obj)
        {
            // change all zeros in a range to something else

            var data = obj.Value.Value;
            int start = 0;
            int end = 0;

            getRange(data.Length, out start, out end);

            if (end > start + 100)
                end = start + 100;

            foreach (int i in ArrayExtensions.Range(start, end, 1))
            {
                if (data[i] == 0)
                {
                    var pt1 = ArrayExtensions.Slice(data, 0, i);
                    byte[] pt2 = { (byte)(context.random.Next(1, 255)) };
                    var pt3 = ArrayExtensions.Slice(data, i + 1, data.Length);
                    data = ArrayExtensions.Combine(pt1, pt2, pt3);
                }
            }

            return data;
        }

        // NEW_BYTES
        //
        private byte[] generateNewBytes(int size)
        {
            // generate new bytes to inject into Blob

            return context.random.Choice<generateFcn>(generateFcns)(size);
        }

        // NEW_BYTES_SINGLE_RANDOM
        //
        private byte[] generateNewBytesSingleRandom(int size)
        {
            // generate a buffer of size bytes, each byte is the same random number

            List<byte> newData = new List<byte>();
            byte num = (byte)(context.random.Next(255));

            for (int i = 0; i < size; ++i)
                newData.Add(num);

            return newData.ToArray();
        }

        // NEW_BYTES_INCREMENTING
        //
        private byte[] generateNewBytesIncrementing(int size)
        {
            // generate a buffer of size bytes, each byte is incrementing from a random start

            List<byte> newData = new List<byte>();
            int x = context.random.Next(size);

            foreach (int i in ArrayExtensions.Range(0, size, 1))
            {
                if (i + x > 255)
                    return newData.ToArray();

                newData.Add((byte)(i + x));
            }

            return newData.ToArray();
        }

        // NEW_BYTES_ZERO
        //
        private byte[] generateNewBytesZero(int size)
        {
            // generate a buffer of size bytes, each byte is zero (NULL)

            List<byte> newData = new List<byte>();

            for (int i = 0; i < size; ++i)
                newData.Add(0x00);

            return newData.ToArray();
        }

        // NEW_BYTES_ALL_RANDOM
        //
        private byte[] generateNewBytesAllRandom(int size)
        {
            // generate a buffer of size bytes, each byte is randomly generated

            List<byte> newData = new List<byte>();

            for (int i = 0; i < size; ++i)
                newData.Add((byte)(context.random.Next(255)));

            return newData.ToArray();
        }
	}
}

// end
