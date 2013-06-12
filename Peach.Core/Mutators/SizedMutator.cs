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

			obj.mutationFlags = DataElement.MUTATE_DEFAULT;

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

			objOf.mutationFlags = DataElement.MUTATE_DEFAULT;
			objOf.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;

			if (OverrideRelation)
			{
				// Indicate we are overrideing the relation
				objOf.mutationFlags |= DataElement.MUTATE_OVERRIDE_RELATIONS;

				// Keep size indicator the same
				obj.MutatedValue = obj.InternalValue;
			}
			else if ((long)obj.InternalValue + growBy <= 0)
			{
				// Ensure we won't send the relation negative
				objOf.MutatedValue = new Variant(BitwiseStream.Null);
				return;
			}

			var data = objOf.Value;
			var dataLen = data.Length;
			var tgtLen = dataLen + growBy;

			if (tgtLen <= 0)
			{
				// Return empty if size is negative
				data = BitwiseStream.Null;
			}
			else if (data.Length == 0)
			{
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

			objOf.MutatedValue = new Variant(data);
		}
	}
}
