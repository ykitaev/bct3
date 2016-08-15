namespace Setup
{
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
            // TODO: test if this works
            // TODO: unify file names, probably by factoring out to some config
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
            
            Helper.DumpBits(@"m:\temp\qhash-dump-1.bin", qfilter);
            filter1.DumpBits(@"m:\temp\bloom-dump-1.bin");
            filter2.DumpBits(@"m:\temp\bloom-dump-2.bin");

            Console.WriteLine("Checking truthiness...");
            Console.WriteLine("Filter 1: {0}", filter1.Truthiness);
            Console.WriteLine("Filter 2: {0}", filter2.Truthiness);

            Console.WriteLine("Dumping bits done for all hashes, safe to use.");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
