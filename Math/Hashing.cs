﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using DamienG.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;

namespace YMath
{
    public static class Hashing
    {
        public const int bill2 = 2147483000;
        private static Crc32 crc = new Crc32();

        private static BitArray qfilter;
        private static BloomFilter<BigInteger> filter1;
        private static BloomFilter<BigInteger> filter2;

        private static SemaphoreSlim filterLock = new SemaphoreSlim(1);

        internal static bool InHash(BigInteger t)
        {
            if (!qfilter[HashBigIntQuick(t)]) return false;
            else if (!filter1.Contains(t)) return false;
            else if (!filter2.Contains(t)) return false;
            else return true;
        }

        public static int HashBigInt1(BigInteger b)
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

        public static int HashBigIntQuick(BigInteger b)
        {
            return (int)(Math.Abs((long)b.GetHashCode()) % bill2);
        }

        internal static void Initialize(string fileName)
        {
            var t1 = Task.Run(() =>
            {
                Console.WriteLine("Initializing Bloom Filter 1");
                filter1 = new BloomFilter<BigInteger>(capacity: 178000000, errorRate: 0.004f, hashFunction: Hashing.HashBigInt1);
                filter1.LoadFromFile(@"m:\temp\bloom-dump.bin");
                Console.WriteLine("Initializing done for filter 1");
            });

            var t2 = Task.Run(() =>
            {
                Console.WriteLine("Initializing Bloom Filter 2");
                filter2 = new BloomFilter<BigInteger>(capacity: 178000000, errorRate: 0.004f, hashFunction: Hashing.HashBigInt2);
                filter2.LoadFromFile(@"m:\temp\bloom-dump2.bin");
                Console.WriteLine("Initializing done for filter 2");
            });

            var t3 = Task.Run(() =>
            {
                ///var cnt = 0;
                Console.WriteLine("Loading qhash...");
                qfilter = new BitArray(Hashing.bill2);
                Helper.LoadBitsFromFile(@"m:\temp\qhash-dump1.bin", qfilter);
                //// var ql = new SemaphoreSlim(1);
                //// Console.WriteLine("Building quick hash...");
                //// Parallel.ForEach(Powers.GenerateBaseAndExponentValues(), tup =>
                //// {
                ////     var num = BigInteger.Pow(tup.Item1, tup.Item2);
                ////     var h = Hashing.HashBigIntQuick(num);
                ////     ql.Wait();
                ////     qfilter[h] = true;
                ////     ++cnt;
                ////     if (cnt % 1000000 == 0)
                ////         Console.WriteLine("\tqhash has {0} mil", cnt/1000000);
                ////     ql.Release();
                //// });
                //// 
                //// Console.WriteLine("Building qhash done");
                //// Console.WriteLine("Dumping bits...");
                //// Helper.DumpBits(@"m:\temp\qhash-dump1.bin", qfilter);
                //// Console.WriteLine("Dumping bits done");
                Console.WriteLine("Loading qhash done");
            });

            Task.WhenAll(t1, t2, t3).Wait();

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
