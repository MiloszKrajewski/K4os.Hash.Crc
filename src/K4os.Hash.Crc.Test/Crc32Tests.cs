using System;
using System.Text;
using Xunit;

namespace K4os.Hash.Crc.Test
{
	public class Crc32Tests
	{
		private static uint Hash(uint seed, byte[] bytes, int index, int length) => 
			Crc32.DigestOf(Crc32Table.Default, bytes, index, length, seed);

		[Theory]
		[InlineData(1)]
		[InlineData(0xffffffff)]
		[InlineData(0x98273645)]
		public void HashOfEmptyBufferDoesNotAffectHash(uint seed)
		{
			var actual = Hash(seed, new byte[0], 0, 0);
			Assert.Equal(seed, actual);
		}

		[Theory]
		[InlineData("a", 0xE8B7BE43)]
		[InlineData("123456789", 0xCBF43926)]
		[InlineData("Quick brown fox jumped over the lazy dog", 0x991DCCC7)]
		public void HashMatchesPrecalculatedValues(string text, uint hash)
		{
			var bytes = Encoding.UTF8.GetBytes(text);
			var actual = Hash(0, bytes, 0, bytes.Length);
			Assert.Equal(hash, actual);
		}

		[Theory]
		[InlineData("a")]
		[InlineData("123456789")]
		[InlineData("Quick brown fox jumped over the lazy dog")]
		public void HashCalculatesProperlyInTheMiddleOfBuffer(string text)
		{
			var padding = Guid.Empty.ToString("N");
			var bytes = Encoding.UTF8.GetBytes(text);
			var padded = Encoding.UTF8.GetBytes(padding + text + padding);
			var expected = Hash(0, bytes, 0, text.Length);
			var actual = Hash(0, padded, padding.Length, text.Length);
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("123456789", 3)]
		[InlineData("123456789", 7)]
		[InlineData("Quick brown fox jumped over the lazy dog", 3)]
		[InlineData("Quick brown fox jumped over the lazy dog", 7)]
		[InlineData("Quick brown fox jumped over the lazy dog", 8)]
		[InlineData("Quick brown fox jumped over the lazy dog", 9)]
		[InlineData("Quick brown fox jumped over the lazy dog", 11)]
		public void HashIsCalculatedInChunks(string text, int chunk)
		{
			var expected = Crc32.DigestOf(Crc32Table.Default, Encoding.ASCII.GetBytes(text), 0, text.Length);

			var actual = 0u;
			var index = 0;
			while (index < text.Length)
			{
				var length = Math.Max(text.Length - index, chunk);
				var bytes = Encoding.ASCII.GetBytes(text.Substring(index, length));
				actual = Hash(actual, bytes, index, length);
				index += length;
			}

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void CrcHasAllHashAlgorithmMetadata()
		{
			var crc = new Crc32().AsHashAlgorithm();
			Assert.True(crc.CanReuseTransform);
			Assert.True(crc.CanTransformMultipleBlocks);
			Assert.Equal(1, crc.InputBlockSize);
			Assert.Equal(1, crc.OutputBlockSize);
			Assert.Equal(32, crc.HashSize);
		}

		[Fact]
		public void SeedStartAs0()
		{
			var crc = new Crc32();
			Assert.Equal(32, crc.DigestBytes().Length * 8);
			Assert.Equal(0, BitConverter.ToInt32(crc.DigestBytes(), 0));
		}

		[Theory]
		[InlineData("123456789")]
		[InlineData("Quick brown fox jumped over the lazy dog")]
		public void HashAlgorithmComputesSameCrcAsRawAccess(string text)
		{
			var crc = new Crc32();
			var bytes = Encoding.ASCII.GetBytes(text);
			var expected = Crc32.DigestOf(bytes, 0, bytes.Length);
			var actual = crc.AsHashAlgorithm().ComputeHash(bytes);

			Assert.Equal(expected, BitConverter.ToUInt32(actual, 0));
		}
	}
}
