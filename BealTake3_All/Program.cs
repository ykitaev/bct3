using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YMath;

namespace Cruncher
{
    class Program
    {
        private static SemaphoreSlim hopelock = new SemaphoreSlim(1);
        private static readonly int chunkSize = 1000;

        /// ************ CONFIG **************
        const string root = @"m:\temp";
        //static string hashFileName = root + @"\hashes2x.bin";
        static long skipLines = chunkSize * 0;
        static string hopesFormat = root + @"\hopes-bloom-{0}.txt";

        static void Main(string[] args)
        {
            Hashing.Initialize(null);
            Console.WriteLine("Starting the outer loop");
            long outerLine = 0;
            foreach (var outer in EnumerateChunks())
            {
                Console.WriteLine("Outer loop starts line {0} with A^x = {1}^{2}", outerLine, outer.First().Item1, outer.First().Item2);
                if (outerLine < skipLines)
                {
                    outerLine += outer.Count;
                    continue;
                }

                var hopesFileName = string.Format(hopesFormat, outerLine);
                using (var sw = new StreamWriter(hopesFileName))
                {
                    Parallel.ForEach(EnumerateChunks(), (inner, state) =>
                    {
                        cross(outer, inner, sw);
                    });
                }
                outerLine += outer.Count;
            }
        }


        private static void cross(List<Tuple<int, int, BigInteger>> first, List<Tuple<int, int, BigInteger>> second, StreamWriter sw)
        {
            Console.Write("-");
            foreach (var f in first)
            {
                foreach (var s in second)
                {
                    var t = f.Item3 + s.Item3;
                    if (Hashing.InHash(t))
                    {
                        var a = f.Item1;
                        var b = s.Item1;
                        if (BigInteger.GreatestCommonDivisor(a, b) > 1)
                            continue;

                        if (BigInteger.GreatestCommonDivisor(t, a) > 1)
                            continue;

                        if (BigInteger.GreatestCommonDivisor(t, b) > 1)
                            continue;

                        var h = string.Format("{0}\t{1}\t{2}\t{3}", f.Item1, f.Item2, s.Item1, s.Item2);
                        hopelock.Wait();
                        sw.WriteLine(h);
                        hopelock.Release();
                    }
                }
            }
        }


        private static IEnumerable<List<Tuple<int, int, BigInteger>>> EnumerateChunks()
        {
            var chunk = new List<Tuple<int, int, BigInteger>>(chunkSize);
            long ctr = 0;

            foreach (var tup in Powers.GenerateBaseAndExponentValues())
            {
                chunk.Add(Tuple.Create(tup.Item1, tup.Item2, BigInteger.Pow(tup.Item1, tup.Item2)));
                ++ctr;
                if (ctr == chunkSize)
                {
                    yield return chunk;
                    ctr = 0;
                    chunk = new List<Tuple<int, int, BigInteger>>();
                }

                if (ctr > chunkSize)
                {
                    throw new InvalidOperationException("panic");
                }
            }
        }

    }
}
