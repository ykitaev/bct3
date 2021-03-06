﻿namespace Configuration
{
    public class Constants
    {
        // Don't use "const" for public varianbles 'cause these values may change and we don't want old inlined values to remain
        public static readonly string root = @"c:\temp";
        public static readonly string HopesFileNameFormat = root + @"\hopes-bloom-{0}.txt";
        public static readonly string CheckedHopedFilesFolderName = root + @"\checked\";
        public static readonly string UnCheckedHopedFilesFolderName = root + @"\unchecked\";
        public static readonly string BloomFilterDumpFileName1 = root + @"\bloom-dump1.bin";
        public static readonly string BloomFilterDumpFileName2 = root + @"\bloom-dump2.bin";
        public static readonly string QuickHashDumpFileName = root + @"\quick-hash-dump.bin";
        public static readonly string DreamsFileName = root + @"\dreams.txt";
        public static readonly string PrimesFileName = root + @"\generated-primes.txt";
        public static readonly string AzureConnectionStringFileName = root + @"\blob.azconf";
        public static readonly string StateFileName = root + @"\state.txt";
        public static readonly string ManualOverrideFileName = root + @"\override.txt";
    }
}
