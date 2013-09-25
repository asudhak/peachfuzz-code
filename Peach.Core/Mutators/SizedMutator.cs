using System;
using System.IO;
using System.Collections.Generic;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Mutators
{
	public abstract class SizedMutator : Mutator
	{
		List<long> values = null;
		uint currentCount = 0;

		public SizedMutator(string name, DataElement obj)
			: base(obj)
		{
			this.name = name;

			int n = getN(obj, 50);
			values = GenerateValues(obj, n);
		}

		protected abstract NLog.Logger Logger { get; }
		protected abstract List<long> GenerateValues(DataElement obj, int n);
		protected abstract bool OverrideRelation { get; }

		protected virtual int getN(DataElement obj, int n)
		{
			Hint h;

			if (!obj.Hints.TryGetValue(name + "-N", out h))
				return n;

			if (!int.TryParse(h.Value, out n))
				throw new PeachException("Expected numerical value for Hint '" + h.Name + "'.");

			return n;
		}

		public override int count
		{
			get { return values.Count; }
		}

		public override uint mutation
		{
			get { return currentCount; }
			set { currentCount = value; }
		}

		public new static bool supportedDataElement(DataElement obj)
		{
			// verify data element has size relation
			if (obj.isMutable && obj.relations.hasFromSizeRelation)
				return true;

			return false;
		}

		public override void sequentialMutation(DataElement obj)
		{
			performMutation(obj, values[(int)currentCount]);
		}

		public override void randomMutation(DataElement obj)
		{
			performMutation(obj, context.Random.Choice(values));
		}

		private void performMutation(DataElement obj, long growBy)
		{
			Logger.Debug("performMutaton> Length: {0}, Grow By: {1}", obj.InternalValue, growBy);

			obj.mutationFlags = MutateOverride.Default;

			var sizeRelation = obj.relations.getFromSizeRelation();
			if (sizeRelation == null)
			{
				Logger.Error("Error, sizeRelation == null, unable to perform mutation.");
				return;
			}

			var objOf = sizeRelation.Of;
			if (objOf == null)
			{
				Logger.Error("Error, sizeRelation.Of == null, unable to perform mutation.");
				return;
			}

			objOf.mutationFlags = MutateOverride.Default;
			objOf.mutationFlags |= MutateOverride.TypeTransform;
			objOf.mutationFlags |= MutateOverride.Transformer;

			if (OverrideRelation)
			{
				// Indicate we are overrideing the relation
				objOf.mutationFlags |= MutateOverride.Relations;

				// Keep size indicator the same
				obj.MutatedValue = obj.InternalValue;
			}
			else if ((long)obj.InternalValue + growBy <= 0)
			{
				// Ensure we won't send the relation negative
				objOf.MutatedValue = new Variant(new BitStream());
				return;
			}

			var data = objOf.Value;

			if (sizeRelation.lengthType == LengthType.Bytes)
				data = GrowByBytes(data, growBy);
			else
				data = GrowByBits(data, growBy);

			objOf.MutatedValue = new Variant(data);
		}

		private static BitwiseStream GrowByBytes(BitwiseStream data, long growBy)
		{
			var dataLen = data.Length;
			var tgtLen = dataLen + growBy;

			if (tgtLen <= 0)
			{
				// Return empty if size is negative
				data = new BitStream();
			}
			else if (data.Length == 0)
			{
				// If objOf is a block, data is a BitStreamList
				data = new BitStream();

				// Fill with 'A' if we don't have any data
				while (--tgtLen > 0)
					data.WriteByte((byte)'A');
			}
			else
			{
				// Loop data over and over until we get to our target length

				var lst = new BitStreamList();

				while (tgtLen > dataLen)
				{
					lst.Add(data);
					tgtLen -= dataLen;
				}

				var buf = new byte[BitwiseStream.BlockCopySize];
				var dst = new BitStream();

				data.Seek(0, System.IO.SeekOrigin.Begin);

				while (tgtLen > 0)
				{
					int len = (int)Math.Min(tgtLen, buf.Length);
					len = data.Read(buf, 0, len);

					if (len == 0)
						data.Seek(0, System.IO.SeekOrigin.Begin);
					else
						dst.Write(buf, 0, len);

					tgtLen -= len;
				}

				lst.Add(dst);

				data = lst;
			}
			return data;
		}

		private static BitwiseStream GrowByBits(BitwiseStream data, long growBy)
		{
			var dataLen = data.LengthBits;
			var tgtLen = dataLen + growBy;

			if (tgtLen <= 0)
			{
				// Return empty if size is negative
				data = new BitStream();
			}
			else if (data.LengthBits == 0)
			{
				// If objOf is a block, data is a BitStreamList
				data = new BitStream();

				// Fill with 'A' if we don't have any data
				while (data.LengthBits < growBy)
					data.WriteByte((byte)'A');

				// Truncate to the correct bit length
				data.SetLengthBits(growBy);
			}
			else
			{
				// Loop data over and over until we get to our target length

				var lst = new BitStreamList();

				while (tgtLen > dataLen)
				{
					lst.Add(data);
					tgtLen -= dataLen;
				}

				var dst = new BitStream();

				data.Seek(0, System.IO.SeekOrigin.Begin);

				while (tgtLen > 0)
				{
					ulong bits;
					int len = data.ReadBits(out bits, (int)Math.Min(tgtLen, 64));

					if (len == 0)
						data.Seek(0, System.IO.SeekOrigin.Begin);
					else
						dst.WriteBits(bits, len);

					tgtLen -= len;
				}

				lst.Add(dst);

				data = lst;
			}
			return data;
		}
	}
}
