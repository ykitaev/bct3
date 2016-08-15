namespace Configuration
{
    using System;

    public class Constants
    {
        private const string root = @"m:\temp";
        public static readonly string HopesFormat = root + @"\hopes-bloom-{0}.txt";
        public static readonly string BloomFilterDumpFileName1 = root + @"\bloom-dump1.bin";
        public static readonly string BloomFilterDumpFileName2 = root + @"\bloom-dump2.bin";
        public static readonly string QuickHashDumpFileName = root + @"\quick-hash-dump.bin";
    }
}
