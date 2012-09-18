
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
    [Hint("BlobMutator-How", "Comma seperated list of: ExpandSingleRandom,ExpandIncrementing,ExpandZero,ExpandAllRandom,Reduce,ChangeRange,RangeSpecial,NullRange,UnNullRange")]
    public class BlobMutator : Mutator
    {
        [Flags]
        public enum How
        {
            ExpandSingleRandom = 0x001,
            ExpandIncrementing = 0x002,
            ExpandZero = 0x004,
            ExpandAllRandom = 0x008,
            Reduce = 0x010,
            ChangeRange = 0x020,
            RangeSpecial = 0x040,
            NullRange = 0x080,
            UnNullRange = 0x100,
        }

        // members
        //
        protected delegate byte[] changeFcn(DataElement obj);
        protected List<changeFcn> changeFcns;

        protected delegate byte[] generateFcn(int size);
        protected List<generateFcn> generateFcns;

        uint pos;
        int _count;

        // CTOR
        //
        public BlobMutator(DataElement obj)
            : base(obj)
        {
            name = "BlobMutator";
            pos = 0;

            How how = 0;
            foreach (var flag in Enum.GetValues(typeof(How)))
                how |= (How)flag;

            Hint hint;
            if (obj.Hints.TryGetValue("BlobMutator-How", out hint))
            {
                if (!Enum.TryParse(hint.Value, out how))
                    throw new PeachException("Unexpected value for Hint named: " + hint.Name);
            }

            changeFcns = new List<changeFcn>();
            generateFcns = new List<generateFcn>();

            if ((how & How.ExpandSingleRandom) == How.ExpandSingleRandom)
                generateFcns.Add(new generateFcn(generateNewBytesSingleRandom));
            if ((how & How.ExpandIncrementing) == How.ExpandIncrementing)
                generateFcns.Add(new generateFcn(generateNewBytesIncrementing));
            if ((how & How.ExpandZero) == How.ExpandZero)
                generateFcns.Add(new generateFcn(generateNewBytesZero));
            if ((how & How.ExpandAllRandom) == How.ExpandAllRandom)
                generateFcns.Add(new generateFcn(generateNewBytesAllRandom));
            if ((how & How.Reduce) == How.Reduce)
                changeFcns.Add(new changeFcn(changeReduceBuffer));
            if ((how & How.ChangeRange) == How.ChangeRange)
                changeFcns.Add(new changeFcn(changeChangeRange));
            if ((how & How.RangeSpecial) == How.RangeSpecial)
                changeFcns.Add(new changeFcn(changeRangeSpecial));
            if ((how & How.NullRange) == How.NullRange)
                changeFcns.Add(new changeFcn(changeNullRange));
            if ((how & How.UnNullRange) == How.UnNullRange)
                changeFcns.Add(new changeFcn(changeUnNullRange));

            _count = changeFcns.Count + generateFcns.Count;

            // We only use expand when we have generate functions
            if (generateFcns.Count > 0)
                changeFcns.Insert(0, new changeFcn(changeExpandBuffer));
        }

        // MUTATION
        //
        public override uint mutation
        {
            get { return pos; }
            set { pos = value; }
        }

        // COUNT
        //
        public override int count
        {
            get { return _count; }
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
            start = context.Random.Next(size);
            end = context.Random.Next(size);

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
            return context.Random.Next(size - len);
        }

        // SEQUENCIAL_MUTATION
        //
        public override void sequencialMutation(DataElement obj)
        {
            // The sequencial logic relies on expand being thte 1st change function when we have generate functions
            System.Diagnostics.Debug.Assert(generateFcns.Count == 0 || changeFcns[0] == changeExpandBuffer);

            if (pos < generateFcns.Count)
                obj.MutatedValue = new Variant(changeExpandBuffer(obj, generateFcns[(int)pos]));
            else if (generateFcns.Count > 0)
                obj.MutatedValue = new Variant(changeFcns[(int)pos - generateFcns.Count + 1](obj));
            else
                obj.MutatedValue = new Variant(changeFcns[(int)pos](obj));

            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            obj.MutatedValue = new Variant(context.Random.Choice(changeFcns)(obj));

            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
        }

        // EXPAND_BUFFER
        //
        private byte[] changeExpandBuffer(DataElement obj)
        {
            var how = context.Random.Choice(generateFcns);
            return changeExpandBuffer(obj, how);
        }

        private byte[] changeExpandBuffer(DataElement obj, generateFcn how)
        {
            // expand the size of our buffer

            List<byte> listData = new List<byte>();
            var data = obj.Value.Value;
            int size = context.Random.Next(256);
            int pos = getPosition(size);

            var pt1 = ArrayExtensions.Slice(data, 0, pos);
            var pt2 = how(size);
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
                byte[] pt2 = { (byte)(context.Random.Next(256)) };
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
                byte[] pt2 = { context.Random.Choice(special) };
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
                    byte[] pt2 = { (byte)(context.Random.Next(1, 256)) };
                    var pt3 = ArrayExtensions.Slice(data, i + 1, data.Length);
                    data = ArrayExtensions.Combine(pt1, pt2, pt3);
                }
            }

            return data;
        }

        // NEW_BYTES_SINGLE_RANDOM
        //
        private byte[] generateNewBytesSingleRandom(int size)
        {
            // generate a buffer of size bytes, each byte is the same random number

            List<byte> newData = new List<byte>();
            byte num = (byte)(context.Random.Next(256));

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
            int x = context.Random.Next(size + 1);

            for (int i = 0; i < size && i + x <= 255; ++i)
                newData.Add((byte)(i + x));

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
                newData.Add((byte)(context.Random.Next(256)));

            return newData.ToArray();
        }
    }
}

// end
