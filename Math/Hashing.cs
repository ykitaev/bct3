using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using DamienG.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using Configuration;

namespace YMath
{
    public static class Hashing
    {
        public const int bill2 = 2147483000;

        private static BitArray qfilter;
        private static BloomFilter<BigInteger> filter1;
        private static BloomFilter<BigInteger> filter2;

        private static SemaphoreSlim filterLock = new SemaphoreSlim(initialCount: 1);
        private static SemaphoreSlim xlock = new SemaphoreSlim(initialCount: 1);


        internal static bool InHash(BigInteger t)
        {
            if (!qfilter[HashBigIntQuick(t)]) return false;
            else if (!filter1.Contains(t)) return false;
            else if (!filter2.Contains(t)) return false;
            else return true;
        }

        public static int HashBigInt1(BigInteger b)
        {
       
            var crc = new Crc32();
            var bytes = b.ToByteArray();
            var crcbytes = crc.ComputeHash(bytes);
            var hash = BitConverter.ToInt32(crcbytes, startIndex: 0);
            return hash;
        }

        public static int HashBigInt2(BigInteger b)
        {
            var crc = new Crc32();
            var b2 = BigInteger.Divide(b, 1000000);
            var bytes = b2.ToByteArray();
            var crcbytes = crc.ComputeHash(bytes);
            var hash = BitConverter.ToInt32(crcbytes, startIndex: 0);
            return hash;
        }

        public static int HashBigIntQuick(BigInteger b)
        {
            return (int)(Math.Abs((long)b.GetHashCode()) % bill2);
        }

        internal static void Initialize()
        {
            var t1 = Task.Run(() =>
            {
                Console.WriteLine("Initializing Bloom Filter 1");
                filter1 = new BloomFilter<BigInteger>(capacity: 178000000, errorRate: 0.004f, hashFunction: Hashing.HashBigInt1);
                filter1.LoadFromFile(Constants.BloomFilterDumpFileName1);
                Console.WriteLine("Initializing done for filter 1");
            });

            var t2 = Task.Run(() =>
            {
                Console.WriteLine("Initializing Bloom Filter 2");
                filter2 = new BloomFilter<BigInteger>(capacity: 178000000, errorRate: 0.004f, hashFunction: Hashing.HashBigInt2);
                filter2.LoadFromFile(Constants.BloomFilterDumpFileName2);
            Console.WriteLine("Initializing done for filter 2");
            });

            var t3 = Task.Run(() =>
            {
                ///var cnt = 0;
                Console.WriteLine("Loading qhash...");
                qfilter = new BitArray(Hashing.bill2);
                Helper.LoadBitsFromFile(Constants.QuickHashDumpFileName, qfilter);
                Console.WriteLine("Loading qhash done");
            });

            Task.WhenAll(t1, t2, t3).Wait();
        }
    }
}
