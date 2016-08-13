using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using DamienG.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading;

namespace YMath
{
    public static class Hashing
    {
        public const int bill2 = 2147483000;
        private static Crc32 crc = new Crc32();
        private static BloomFilter<BigInteger> filter;
        private static SemaphoreSlim filterLock = new SemaphoreSlim(1);

        internal static bool InHash(BigInteger t)
        {
            return filter.Contains(t);
        }

        public static int HashBigInt(BigInteger b)
        {
            var bytes = b.ToByteArray();
            var crcbytes = crc.ComputeHash(bytes);
            var hash = BitConverter.ToInt32(crcbytes, startIndex: 0);
            return hash;
        }

        internal static void Initialize(string fileName)
        {
            Console.WriteLine("Initializing Bloom Filter");
            filter = new BloomFilter<BigInteger>(capacity: 178000000, errorRate: 0.004f, hashFunction: Hashing.HashBigInt);
            var cnt = 0;
            Parallel.ForEach(Powers.GenerateBaseAndExponentValues(), tup =>
            {
                var a = tup.Item1;
                var x = tup.Item2;
                var ax = BigInteger.Pow(a, x);
                filterLock.Wait();
                filter.Add(ax);
                ++cnt;
                if (cnt % 1000000 == 0)
                {
                    Console.WriteLine("Added {0}M to bloom filter", cnt / 1000000);
                }
                filterLock.Release();
            });

            Console.WriteLine("Initializing done, truthiness = {0}", filter.Truthiness);
            filter.DumpBits(@"m:\temp\bloom-dump.bin");
        }
    }
}
