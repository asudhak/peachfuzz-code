using System;
using System.Text;

namespace Peach.Core
{
	#region Encoding

	[Serializable]
	public abstract class Encoding
	{
		#region Decoder Helper

		protected class BlockDecoder : Decoder
		{
			private Decoder decoder;
			private int minBytesPerChar;
			private byte[] leftOvers;
			private int leftOversCount;

			public BlockDecoder(Encoding encoding)
			{
				this.decoder = encoding.encoding.GetDecoder();
				this.minBytesPerChar = encoding.minBytesPerChar;
				this.leftOvers = new byte[this.minBytesPerChar];
				this.leftOversCount = 0;
			}

			public override int GetCharCount(byte[] bytes, int index, int count)
			{
				throw new NotImplementedException();
			}

			public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
			{
				CheckParams(bytes, byteIndex, byteCount);

				if (minBytesPerChar == 1)
					return decoder.GetChars(bytes, byteIndex, byteCount, chars, charIndex);

				int ret = 0;

				if (leftOversCount > 0)
				{
					int count = Math.Min(leftOvers.Length - leftOversCount, byteCount);
					Buffer.BlockCopy(bytes, byteIndex, leftOvers, leftOversCount, count);

					leftOversCount += count;
					byteIndex += count;
					byteCount -= count;

					if (leftOversCount == leftOvers.Length)
					{
						ret += decoder.GetChars(leftOvers, 0, leftOversCount, chars, charIndex);
						charIndex += ret;
						leftOversCount = 0;
					}
				}

				int remain = byteCount % minBytesPerChar;

				if (remain > 0)
				{
					byteCount -= remain;
					Buffer.BlockCopy(bytes, byteIndex + byteCount, leftOvers, leftOversCount, remain);
					leftOversCount += remain;
				}

				ret += decoder.GetChars(bytes, byteIndex, byteCount, chars, charIndex);

				return ret;
			}
		}

		#endregion

		#region Protected Members

		protected Encoding(System.Text.Encoding encoding, int minBytesPerChar)
		{
			if (encoding.IsReadOnly)
				this.encoding = encoding.Clone() as System.Text.Encoding;
			else
				this.encoding = encoding;

			this.encoding.DecoderFallback = new DecoderExceptionFallback();
			this.encoding.EncoderFallback = new EncoderExceptionFallback();

			// TODO: Enable fallback exception once EncodingTests.EncodeBadUtf* tests pass
			if (!encoding.IsSingleByte)
				this.encoding.EncoderFallback = new EncoderReplacementFallback();

			this.minBytesPerChar = minBytesPerChar;
		}

		protected static void CheckParams<T>(T[] array, int index, int count)
		{
			if (index < 0 || index > array.Length)
				throw new ArgumentOutOfRangeException("index");
			if (count < 0 || (index + count) > array.Length)
				throw new ArgumentOutOfRangeException("count");
		}

		protected System.Text.Encoding encoding;
		protected int minBytesPerChar;

		#endregion

		#region byte[] <-> char[] Conversion Methods

		public virtual char[] GetChars(byte[] bytes, int index, int count)
		{
			CheckParams(bytes, index, count);

			if (IsSingleByte)
				return encoding.GetChars(bytes, index, count);

			if (count == 0)
				return new char[0];

			char[] ret = new char[GetCharCount(bytes, index, count)];

			--count;

			var dec = GetDecoder();
			int conv1 = dec.GetChars(bytes, index, count, ret, 0);
			int conv2 = dec.GetChars(bytes, index + count, 1, ret, conv1);

			if (conv2 != 1 || (conv1 + conv2) != ret.Length)
				throw new DecoderFallbackException();

			return ret;
		}

		public virtual byte[] GetBytes(char[] chars, int index, int count)
		{
			CheckParams(chars, index, count);
			return encoding.GetBytes(chars, index, count);
		}

		public virtual int GetByteCount(char[] chars, int index, int count)
		{
			CheckParams(chars, index, count);
			return encoding.GetByteCount(chars, index, count);
		}

		public virtual int GetCharCount(byte[] bytes, int index, int count)
		{
			CheckParams(bytes, index, count);
			return encoding.GetCharCount(bytes, index, count);
		}

		#endregion

		#region Public Helpers

		public bool IsSingleByte
		{
			get
			{
				return encoding.IsSingleByte;
			}
		}

		public int CodePage
		{
			get
			{
				return encoding.CodePage;
			}
		}

		public string HeaderName
		{
			get
			{
				return encoding.HeaderName;
			}
		}

		public char[] GetChars(byte[] bytes)
		{
			return GetChars(bytes, 0, bytes.Length);
		}

		public string GetString(byte[] bytes, int index, int count)
		{
			return new string(GetChars(bytes, index, count));
		}

		public string GetString(byte[] bytes)
		{
			return GetString(bytes, 0, bytes.Length);
		}

		public byte[] GetBytes(char[] chars)
		{
			return GetBytes(chars, 0, chars.Length);
		}

		public byte[] GetBytes(string s)
		{
			return GetBytes(s.ToCharArray());
		}

		public int GetByteCount(char[] chars)
		{
			return GetByteCount(chars, 0, chars.Length);
		}

		public int GetByteCount(string s)
		{
			return GetByteCount(s.ToCharArray());
		}

		public int GetCharCount(byte[] bytes)
		{
			return GetCharCount(bytes, 0, bytes.Length);
		}

		public Decoder GetDecoder()
		{
			return new BlockDecoder(this);
		}

		public static Encoding GetEncoding(string name)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			string converted = name.ToLowerInvariant().Replace('-', '_');

			switch (converted)
			{
				case "ascii":
					return ASCII;

				case "utf7":
				case "utf_7":
					return UTF7;

				case "utf8":
				case "utf_8":
					return UTF8;

				case "utf16":
				case "utf_16":
				case "utf_16le":
					return Unicode;

				case "utf16be":
				case "utf_16be":
					return BigEndianUnicode;

				case "utf32":
				case "utf_32":
				case "utf_32le":
					return UTF32;

				case "utf32be":
				case "utf_32be":
					return BigEndianUTF32;

				case "iso_8859_1":
				case "latin1":
					return ISOLatin1;

				default:
					throw new ArgumentException("Encoding name '" + name + "' not supported.", "name");
			}
		}

		#endregion

		#region Static Properties

		static volatile Encoding asciiEncoding;
		static volatile Encoding utf7Encoding;
		static volatile Encoding utf8Encoding;
		static volatile Encoding unicodeEncoding;
		static volatile Encoding bigEndianEncoding;
		static volatile Encoding utf32Encoding;
		static volatile Encoding bigEndianUTF32Encoding;
		static volatile Encoding isoLatin1Encoding;

		static readonly object lockobj = new object();

		public static Encoding ASCII
		{
			get
			{
				if (asciiEncoding == null)
				{
					lock (lockobj)
					{
						if (asciiEncoding == null)
						{
							asciiEncoding = new ASCIIEncoding();
						}
					}
				}

				return asciiEncoding;
			}
		}

		public static Encoding UTF7
		{
			get
			{
				if (utf7Encoding == null)
				{
					lock (lockobj)
					{
						if (utf7Encoding == null)
						{
							utf7Encoding = new UTF7Encoding();
						}
					}
				}

				return utf7Encoding;
			}
		}

		public static Encoding UTF8
		{
			get
			{
				if (utf8Encoding == null)
				{
					lock (lockobj)
					{
						if (utf8Encoding == null)
						{
							utf8Encoding = new UTF8Encoding();
						}
					}
				}

				return utf8Encoding;
			}
		}

		public static Encoding Unicode
		{
			get
			{
				if (unicodeEncoding == null)
				{
					lock (lockobj)
					{
						if (unicodeEncoding == null)
						{
							unicodeEncoding = new UnicodeEncoding(false);
						}
					}
				}

				return unicodeEncoding;
			}
		}

		public static Encoding BigEndianUnicode
		{
			get
			{
				if (bigEndianEncoding == null)
				{
					lock (lockobj)
					{
						if (bigEndianEncoding == null)
						{
							bigEndianEncoding = new UnicodeEncoding(true);
						}
					}
				}

				return bigEndianEncoding;
			}
		}

		public static Encoding UTF32
		{
			get
			{
				if (utf32Encoding == null)
				{
					lock (lockobj)
					{
						if (utf32Encoding == null)
						{
							utf32Encoding = new UTF32Encoding(false);
						}
					}
				}

				return utf32Encoding;
			}
		}

		public static Encoding BigEndianUTF32
		{
			get
			{
				if (bigEndianUTF32Encoding == null)
				{
					lock (lockobj)
					{
						if (bigEndianUTF32Encoding == null)
						{
							bigEndianUTF32Encoding = new UTF32Encoding(true);
						}
					}
				}

				return bigEndianUTF32Encoding;
			}
		}

		public static Encoding ISOLatin1
		{
			get
			{
				if (isoLatin1Encoding == null)
				{
					lock (lockobj)
					{
						if (isoLatin1Encoding == null)
						{
							isoLatin1Encoding = new Latin1Encoding();
						}
					}
				}

				return isoLatin1Encoding;
			}
		}

		#endregion
	}

	#endregion

	#region ASCIIEncoding

	[Serializable]
	public class ASCIIEncoding : Encoding
	{
		public ASCIIEncoding()
			: base(new System.Text.ASCIIEncoding(), 1)
		{
		}
	}

	#endregion

	#region UTF7Encoding

	[Serializable]
	public class UTF7Encoding : Encoding
	{
		public UTF7Encoding()
			: base(new System.Text.UTF7Encoding(true), 1)
		{
		}
	}

	#endregion

	#region UTF8Encoding

	[Serializable]
	public class UTF8Encoding : Encoding
	{
		public UTF8Encoding()
			: base(new System.Text.UTF8Encoding(false, true), 1)
		{
		}
	}

	#endregion

	#region UnicodeEncoding

	[Serializable]
	public class UnicodeEncoding : Encoding
	{
		public UnicodeEncoding(bool bigEndian)
			: base(new System.Text.UnicodeEncoding(bigEndian, false, true), 2)
		{
		}
	}

	#endregion

	#region UTF32Encoding

	[Serializable]
	public class UTF32Encoding : Encoding
	{
		public UTF32Encoding(bool bigEndian)
			: base(new System.Text.UTF32Encoding(bigEndian, false, true), 4)
		{
		}
	}

	#endregion

	#region Latin1Encoding

	[Serializable]
	public class Latin1Encoding : Encoding
	{
		public Latin1Encoding()
			: base(System.Text.Encoding.GetEncoding("ISO-8859-1"), 1)
		{
		}
	}

	#endregion
}
