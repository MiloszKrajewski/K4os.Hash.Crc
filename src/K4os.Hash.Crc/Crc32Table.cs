namespace K4os.Hash.Crc
{
	public class Crc32Table
	{
		public static readonly Crc32Table Default = new Crc32Table();

		public uint[] Data { get; }

		public Crc32Table(uint polynomial)
		{
			Data = Build(polynomial);
		}

		public Crc32Table(): this(0xEDB88320) { }

		private static uint[] Build(uint polynomial)
		{
			var data = new uint[256];

			for (uint i = 0; i <= 255; i++)
			{
				var remainder = i;
				for (var j = 8; j > 0; --j)
					remainder = (remainder >> 1) ^ ((remainder & 1) != 0 ? polynomial : 0);

				data[i] = remainder;
			}

			return data;
		}
	}
}
