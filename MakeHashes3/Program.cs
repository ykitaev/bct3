using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YMath;
using DamienG.Security.Cryptography;

namespace MakeHashes3
{
    class Program
    {
        private static SemaphoreSlim xl = new SemaphoreSlim(1);
        
        static void Main(string[] args)
        {
            long cnt = 0;
            var crc = new Crc32();
            using (var bw = new BinaryWriter(File.Open(@"m:\temp\hashes3xx.bin", FileMode.Create)))
            {
                Parallel.ForEach(Powers.GenerateBaseAndExponentValues(), tup =>
                {
                    var a = tup.Item1;
                    var x = tup.Item2;
                    var ax = BigInteger.Pow(a, x);
                    var h1 = ax.GetHashCode();
                    var h2 = Hashing.HashBigInt1(ax);

                    xl.Wait();

                    bw.Write(a);
                    bw.Write(x);
                    bw.Write(h1);
                    bw.Write(h2);
                    ++cnt;
                    if (cnt % 1000000 == 0)
                    {
                        Console.WriteLine(cnt / 1000000 + " million hashes written..");
                    }

                    xl.Release();
                });
            }
        }
    }
}
