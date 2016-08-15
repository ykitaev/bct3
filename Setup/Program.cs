namespace Setup
{
    using Configuration;
    using System;
    using System.Collections;
    using System.Linq;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;
    using YMath;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Setup script running!");

            var filterLock = new SemaphoreSlim(initialCount: 1);
            var qfilter = new BitArray(Hashing.bill2); ;
            var filter1 = new BloomFilter<BigInteger>(capacity: 178000000, errorRate: 0.004f, hashFunction: Hashing.HashBigInt1);
            var filter2 = new BloomFilter<BigInteger>(capacity: 178000000, errorRate: 0.004f, hashFunction: Hashing.HashBigInt2);

            var counter = 0;
            Parallel.ForEach(Powers.GenerateBaseAndExponentValues(), tup =>
            {
                var a = tup.Item1;
                var x = tup.Item2;
                var ax = BigInteger.Pow(a, x);
                var quickHash = Hashing.HashBigIntQuick(ax);

                filterLock.Wait();
                {
                    qfilter[quickHash] = true;
                    filter1.Add(ax);
                    filter2.Add(ax);
                    ++counter;
                    if (counter % 1000000 == 0)
                        Console.WriteLine("{0} million processed", counter / 1000000);
                }
                filterLock.Release();
            });
            
            Console.WriteLine("Building hashes done, dumping bits");
            
            Helper.DumpBits(Constants.QuickHashDumpFileName, qfilter);
            filter1.DumpBits(Constants.BloomFilterDumpFileName1);
            filter2.DumpBits(Constants.BloomFilterDumpFileName2);

            Console.WriteLine("Checking truthiness...");
            Console.WriteLine("Filter 1: {0}", filter1.Truthiness);
            Console.WriteLine("Filter 2: {0}", filter2.Truthiness);

            Console.WriteLine("Dumping bits done for all hashes, safe to use.");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
