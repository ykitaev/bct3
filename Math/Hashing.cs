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
        private static BloomFilter<BigInteger> filter2;
        private static SemaphoreSlim filterLock = new SemaphoreSlim(1);

        internal static bool InHash(BigInteger t)
        {
            if (!filter.Contains(t)) return false;
            else if (!filter2.Contains(t)) return false;
            else return true;
        }

        public static int HashBigInt(BigInteger b)
        {
            var bytes = b.ToByteArray();
            var crcbytes = crc.ComputeHash(bytes);
            var hash = BitConverter.ToInt32(crcbytes, startIndex: 0);
            return hash;
        }

        public static int HashBigInt2(BigInteger b)
        {
            var b2 = BigInteger.Divide(b, 1000000);
            var bytes = b2.ToByteArray();
            var crcbytes = crc.ComputeHash(bytes);
            var hash = BitConverter.ToInt32(crcbytes, startIndex: 0);
            return hash;
        }

        internal static void Initialize(string fileName)
        {
            var t1 = Task.Run(() =>
            {
                Console.WriteLine("Initializing Bloom Filter 1");
                filter = new BloomFilter<BigInteger>(capacity: 178000000, errorRate: 0.004f, hashFunction: Hashing.HashBigInt);
                filter.LoadFromFile(@"m:\temp\bloom-dump.bin");
                Console.WriteLine("Initializing done for filter 1");
            });

            var t2 = Task.Run(() =>
            {
                Console.WriteLine("Initializing Bloom Filter 2");
                filter2 = new BloomFilter<BigInteger>(capacity: 178000000, errorRate: 0.004f, hashFunction: Hashing.HashBigInt2);
                filter2.LoadFromFile(@"m:\temp\bloom-dump2.bin");
                Console.WriteLine("Initializing done for filter 2");
            });

            Task.WhenAll(t1, t2).Wait();

            //// var cnt = 0;
            //// Parallel.ForEach(Powers.GenerateBaseAndExponentValues(), tup =>
            //// {
            ////     var a = tup.Item1;
            ////     var x = tup.Item2;
            ////     var ax = BigInteger.Pow(a, x);
            ////     filterLock.Wait();
            ////     filter2.Add(ax);
            ////     //if (!filter.Contains(ax)) throw new Exception("not deserialized right");
            ////     ++cnt;
            ////     if (cnt % 1000000 == 0)
            ////     {
            ////         Console.WriteLine("Checked {0}M in bloom filter", cnt / 1000000);
            ////     }
            ////     filterLock.Release();
            //// });
            //// 
            //// Console.WriteLine("Initializing done, truthiness = {0}", filter2.Truthiness);
            //// filter2.DumpBits(@"m:\temp\bloom-dump2.bin");
        }
    }
}
