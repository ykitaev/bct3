namespace Cruncher
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;
    using YMath;
    using Configuration;

    class Program
    {
        private static SemaphoreSlim hopelock = new SemaphoreSlim(initialCount: 1);
        private static readonly int chunkSize = 1000;

        /// ************ CONFIG **************
        static long skipLines = chunkSize * 100;


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

                var hopesFileName = string.Format(Constants.HopesFormat, outerLine);
                using (var sw = new StreamWriter(hopesFileName))
                {
                    var innerLine = 0;
                    Parallel.ForEach(EnumerateChunks(), (inner, state) =>
                    {
                        Interlocked.Add(ref innerLine, inner.Count);

                        // Warning: It is possible that some other thread changes innerLine before the check below is done.
                        // It's ok though, because then we'll just process an extra chunk (false positive). Though if we have too many of these 
                        // races, then we should re-consider the approach. For now, it's simple enough to try.
                        if (innerLine >= outerLine)
                        {
                            cross(outer, inner, sw);
                        }
                        else
                        {
                            // Skipped
                            Console.Write(@"/");
                        }

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
                var a = f.Item1;
                var x = f.Item2;
                var ax = f.Item3;

                foreach (var s in second)
                {
                    var b = s.Item1;
                    var y = s.Item2;
                    var by = s.Item3;

                    if (SimpleMath.GCD(a, b) > 1)
                        continue;

                    var cz = ax + by;
                    if (Hashing.InHash(cz))
                    {
                        var h = string.Format("{0}\t{1}\t{2}\t{3}", a, x, b, y);
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
                var a = tup.Item1;
                var x = tup.Item2;
                var ax = BigInteger.Pow(a, x);
                chunk.Add(Tuple.Create(a, x, ax));
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
