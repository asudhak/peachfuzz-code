using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using Peach.Core.Dom;
using Peach.Core.IO;

namespace Peach.Core.Transformers
{
	[Description("Null transformer. Returns that data unaltered.")]
	[Transformer("Null", true)]
	[Serializable]
	public class Null : Transformer
	{
		public Null(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		protected override BitwiseStream internalEncode(BitwiseStream data)
		{
			return data;
		}

		protected override BitStream internalDecode(BitStream data)
		{
			return data;
		}
	}
}