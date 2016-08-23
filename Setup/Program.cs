namespace Setup
{
    using Configuration;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
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

           /// Console.WriteLine("Generating prime numers");
           /// var primesMax = 50847534;
           /// var primes = new List<int>(primesMax) { 2 };
           /// var primesGenerated = 1;
           /// var candidate = 3;
           /// while (primesGenerated < primesMax)
           /// {
           ///     var boundary = Math.Ceiling(Math.Sqrt(candidate));
           ///     var good = true;
           ///     var done = false;
           ///     while (!done)
           ///     {
           ///         foreach (var p in primes)
           ///         {
           ///             if (p > boundary)
           ///             {
           ///                 good = true;
           ///                 done = true;
           ///                 break;
           ///             }
           ///             else
           ///             {
           ///                 if (candidate % p == 0)
           ///                 {
           ///                     done = true;
           ///                     good = false;
           ///                     break;
           ///                 }
           ///             }
           ///         }
           ///         if (good)
           ///         {
           ///             primes.Add(candidate);
           ///             ++primesGenerated;
           ///
           ///             if (primesGenerated % 1000000 == 0)
           ///                 Console.WriteLine("Done {0}M", primesGenerated/1000000);
           ///         }
           ///
           ///         candidate += 2;
           ///     }
           /// }
           ///
           /// Console.WriteLine("Prime number generation done, writing to file...");
           /// File.WriteAllLines(Constants.PrimesFileName, primes.Select(p => p.ToString()));
           /// Console.WriteLine("Primary numers written");

            var qfilter = new BitArray(Hashing.bill2); ;
            var filter1 = new BloomFilter<BigInteger>(capacity: 178000000, errorRate: 0.004f, hashFunction: Hashing.HashBigInt1);
            var filter2 = new BloomFilter<BigInteger>(capacity: 178000000, errorRate: 0.004f, hashFunction: Hashing.HashBigInt2);

            var counter = 0;
            foreach(var tup in Powers.GenerateBaseAndExponentValues())
            {
                var a = tup.Item1;
                var x = tup.Item2;
                var ax = BigInteger.Pow(a, x);
                var quickHash = Hashing.HashBigIntQuick(ax);

                {
                    qfilter[quickHash] = true;
                    filter1.Add(ax);
                    filter2.Add(ax);
                    ++counter;
                    if (counter % 1000000 == 0)
                        Console.WriteLine("{0} million processed", counter / 1000000);
                }
            }
            
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
