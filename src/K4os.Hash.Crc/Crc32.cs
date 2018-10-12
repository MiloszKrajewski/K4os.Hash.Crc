using System;
using System.Security.Cryptography;

namespace K4os.Hash.Crc
{
	public class Crc32
	{
		private uint _seed;

		private readonly Crc32Table _table;

		public Crc32(): this(Crc32Table.Default) { }

		public Crc32(Crc32Table table)
		{
			_table = table ?? Crc32Table.Default;
		}

		private static unsafe uint DigestOf(
			uint[] table, byte[] bytes, int index, int length, uint seed = 0)
		{
			if (length == 0)
				return seed;

			if (table == null || table.Length != 256)
				throw new ArgumentException("Invalid lookup table");

			if (bytes == null || index < 0 || length < 0 || index + length > bytes.Length)
				throw new ArgumentException("Invalid buffer boundaries");

			seed = ~seed;

			fixed (uint* tableP = table)
			fixed (byte* bytes0 = bytes)
			{
				var bytesP = bytes0 + index;

				while (length >= 8)
				{
					seed = (seed >> 8) ^ tableP[(bytesP[0] ^ seed) & 0xff];
					seed = (seed >> 8) ^ tableP[(bytesP[1] ^ seed) & 0xff];
					seed = (seed >> 8) ^ tableP[(bytesP[2] ^ seed) & 0xff];
					seed = (seed >> 8) ^ tableP[(bytesP[3] ^ seed) & 0xff];
					seed = (seed >> 8) ^ tableP[(bytesP[4] ^ seed) & 0xff];
					seed = (seed >> 8) ^ tableP[(bytesP[5] ^ seed) & 0xff];
					seed = (seed >> 8) ^ tableP[(bytesP[6] ^ seed) & 0xff];
					seed = (seed >> 8) ^ tableP[(bytesP[7] ^ seed) & 0xff];
					bytesP += 8;
					length -= 8;
				}

				if (length >= 4)
				{
					seed = (seed >> 8) ^ tableP[(bytesP[0] ^ seed) & 0xff];
					seed = (seed >> 8) ^ tableP[(bytesP[1] ^ seed) & 0xff];
					seed = (seed >> 8) ^ tableP[(bytesP[2] ^ seed) & 0xff];
					seed = (seed >> 8) ^ tableP[(bytesP[3] ^ seed) & 0xff];
					bytesP += 4;
					length -= 4;
				}
				
				if (length >= 2)
				{
					seed = (seed >> 8) ^ tableP[(bytesP[0] ^ seed) & 0xff];
					seed = (seed >> 8) ^ tableP[(bytesP[1] ^ seed) & 0xff];
					bytesP += 2;
					length -= 2;
				}

				if (length > 0)
				{
					seed = (seed >> 8) ^ tableP[(bytesP[0] ^ seed) & 0xff];
					// bytesP++;
					// length--;
				}
			}

			return ~seed;
		}

		public static uint DigestOf(Crc32Table table, byte[] bytes, int index, int length, uint seed = 0) =>
			DigestOf(table.Data, bytes, index, length, seed);

		public static uint DigestOf(byte[] bytes, int index, int length, uint seed = 0) =>
			DigestOf(Crc32Table.Default.Data, bytes, index, length, seed);

		public void Reset() =>
			_seed = 0;

		public void Update(byte[] bytes, int index, int length) =>
			_seed = DigestOf(_table, bytes, index, length, _seed);

		public uint Digest() => _seed;

		public byte[] DigestBytes() => BitConverter.GetBytes(_seed);

		public HashAlgorithm AsHashAlgorithm() =>
			new HashAlgorithmAdapter(sizeof(uint) << 3, Reset, Update, DigestBytes);
	}
}
