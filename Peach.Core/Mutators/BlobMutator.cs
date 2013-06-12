
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
using System.IO;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;
using Peach.Core.IO;

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
        protected delegate BitwiseStream changeFcn(DataElement obj);
        protected List<changeFcn> changeFcns;

        protected delegate void generateFcn(BitwiseStream data, long size);
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
        private void getRange(long size, out long start, out long end, long delta = long.MaxValue)
        {
            start = context.Random.Next(size);
            end = context.Random.Next(size);

            if (start > end)
            {
                long temp = end;
                end = start;
                start = temp;
            }

            if ((end - start) > delta)
                end = start + delta;
        }

		/// <summary>
		/// Returns a new BitStream composed of the first 'length' bytes of src.
		/// When done, src is positioned at length bytes, and the returned
		/// BitStream is positioned at SeekOrigin.End
		/// </summary>
		/// <param name="src"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		private BitStream copyBytes(BitwiseStream src, long length)
		{
			if (length > src.Length)
				throw new ArgumentOutOfRangeException("length");

			var buf = new byte[BitwiseStream.BlockCopySize];
			var ret = new BitStream();
			src.Seek(0, SeekOrigin.Begin);

			while (length > 0)
			{
				int len = (int)Math.Min(length, buf.Length);
				len = src.Read(buf, 0, len);
				ret.Write(buf, 0, len);
				length -= len;
			}

			return ret;
		}

        // SEQUENTIAL_MUTATION
        //
        public override void sequentialMutation(DataElement obj)
        {
            // The sequential logic relies on expand being the 1st change function when we have generate functions
            System.Diagnostics.Debug.Assert(generateFcns.Count == 0 || changeFcns[0] == changeExpandBuffer);

            BitwiseStream bs;

            if (pos < generateFcns.Count)
                bs = changeExpandBuffer(obj, generateFcns[(int)pos]);
            else if (generateFcns.Count > 0)
                bs = changeFcns[(int)pos - generateFcns.Count + 1](obj);
            else
                bs = changeFcns[(int)pos](obj);

            bs.Seek(0, SeekOrigin.Begin);

            obj.MutatedValue = new Variant(bs);

            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
        }

        // RANDOM_MUTAION
        //
        public override void randomMutation(DataElement obj)
        {
            var bs = context.Random.Choice(changeFcns)(obj);
            bs.Seek(0, SeekOrigin.Begin);

            obj.MutatedValue = new Variant(bs);

            obj.mutationFlags = DataElement.MUTATE_DEFAULT;
            obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
        }

        // EXPAND_BUFFER
        //
        private BitwiseStream changeExpandBuffer(DataElement obj)
        {
            var how = context.Random.Choice(generateFcns);
            return changeExpandBuffer(obj, how);
        }

        private BitwiseStream changeExpandBuffer(DataElement obj, generateFcn generate)
        {
            // expand the size of our buffer
            var data = obj.Value;
            long size = context.Random.Next(256);
            long pos = context.Random.Next(data.Length);

            // Copy first 'pos' bytes
            var ret = copyBytes(data, pos);

            // Generate 'size' bytes
            generate(ret, size);

            // Copy from 'pos' onwards
            data.CopyTo(ret);

            return ret;
        }

        // REDUCE_BUFFER
        //
        private BitwiseStream changeReduceBuffer(DataElement obj)
        {
            // reduce the size of our buffer

            var data = obj.Value;
            long start = 0;
            long end = 0;

            getRange(data.Length, out start, out end);

            // Copy first 'start' bytes
            var ret = copyBytes(data, start);

            // Copy from end onwards
            data.Seek(end, SeekOrigin.Begin);
            data.CopyTo(ret);

            return ret;
        }

        // CHANGE_RANGE
        //
        private BitwiseStream changeChangeRange(DataElement obj)
        {
            // change a sequence of bytes in our buffer

            var data = obj.Value;
            long start = 0;
            long end = 0;

            getRange(data.Length, out start, out end, 100);

            data.Seek(start, SeekOrigin.Begin);

            while (data.Position < end)
                data.WriteByte((byte)context.Random.Next(256));

            return data;
        }

        // CHANGE_RANGE_SPECIAL
        //
        private BitwiseStream changeRangeSpecial(DataElement obj)
        {
            // change a sequence of bytes in our buffer to some special chars

            var data = obj.Value;
            long start = 0;
            long end = 0;
            byte[] special = { 0x00, 0x01, 0xFE, 0xFF };

            getRange(data.Length, out start, out end, 100);

            data.Seek(start, SeekOrigin.Begin);

            while (data.Position < end)
                data.WriteByte(context.Random.Choice(special));

            return data;
        }

        // NULL_RANGE
        //
        private BitwiseStream changeNullRange(DataElement obj)
        {
            // change a range of bytes to null

            var data = obj.Value;
            long start = 0;
            long end = 0;

            getRange(data.Length, out start, out end, 100);

            data.Seek(start, SeekOrigin.Begin);

            // Write null bytes until end
            while (data.Position < end)
                data.WriteByte(0);

            return data;
        }

        // UNNULL_RANGE
        //
        private BitwiseStream changeUnNullRange(DataElement obj)
        {
            // change all zeros in a range to something else

            var data = obj.Value;
            long start = 0;
            long end = 0;

            getRange(data.Length, out start, out end, 100);

            data.Seek(start, SeekOrigin.Begin);

            // Write bytes until end changing nulls to non-null
            while (data.Position < end)
            {
                int b = data.ReadByte();

                System.Diagnostics.Debug.Assert(b != -1);

                if (b == 0)
                    b = context.Random.Next(1, 256);

                data.Seek(-1, SeekOrigin.Current);
                data.WriteByte((byte)b);
            }

            return data;
        }

        // NEW_BYTES_SINGLE_RANDOM
        //
        private void generateNewBytesSingleRandom(BitwiseStream data, long size)
        {
            byte val = (byte)(context.Random.Next(256));

            while (size --> 0)
                data.WriteByte(val);
        }

        // NEW_BYTES_INCREMENTING
        //
        private void generateNewBytesIncrementing(BitwiseStream data, long size)
        {
            // Pick a starting value between [0, 255-size] and grow buffer by
            // a max of size bytes of incrementing values from [value,255]
            System.Diagnostics.Debug.Assert(size < 256);

            // Starting value is 0 - (256 - size) inclusive, but rand max is exclusive
            byte val = (byte)context.Random.Next((int)(257 - size));

            while (size --> 0)
                data.WriteByte(val++);
        }

        // NEW_BYTES_ZERO
        //
        private void generateNewBytesZero(BitwiseStream data, long size)
        {
            while (size --> 0)
                data.WriteByte(0);
        }

        // NEW_BYTES_ALL_RANDOM
        //
        private void generateNewBytesAllRandom(BitwiseStream data, long size)
        {
            while (size --> 0)
                data.WriteByte((byte)context.Random.Next(256));
        }
    }
}

// end
