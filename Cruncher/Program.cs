namespace Cruncher
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;
    using YMath;
    using Configuration;
    using System.Diagnostics;
    using System.Collections.Concurrent;

    class Program
    {
        private static SemaphoreSlim hopelock = new SemaphoreSlim(initialCount: 1);
        private static SemaphoreSlim outerBatchlock = new SemaphoreSlim(initialCount: 1);
        private static SemaphoreSlim innerBatchlock = new SemaphoreSlim(initialCount: 1);
        private static readonly int chunkSize = 1000;

        private static long innerBatchesDone = 0;
        private static Stopwatch stopwatch = new Stopwatch();
        private static TimeSpan last;

        static void Main(string[] args)
        {
            try
            {
               //  unchecked
               //  {
               //      var tbd = new BatchEnumerator();
               //      long serialSum = 0;
               //      foreach (var b in tbd.EnumerateBatches())
               //      {
               //          serialSum += b.Sum(i => i.Item1 + i.Item2);
               //      }
               // 
               //      Console.WriteLine("Serial sum: " + serialSum);
               //      for (var i = 0; i < 10; ++i)
               //      {
               //          var pbd = new BatchEnumerator();
               //          long psum = 0;
               //          Parallel.ForEach(pbd.EnumerateBatches(), batch =>
               //          {
               //              foreach (var b in batch)
               //                  Interlocked.Add(ref psum, b.Item1 + b.Item2);
               //          });
               // 
               //          Console.WriteLine("psum " + i + " : " + psum);
               //      }
               //  }
               //  Console.ReadKey();

                Hashing.Initialize();
               //  RunTests();
                stopwatch.Start();

                while (true)
                {
                    long outerLine = 0;

                    // Check if we aleady have a batch checked out
                    int batchIndex;
                    if (File.Exists(Constants.ManualOverrideFileName))
                    {
                        // Manual intervention to the operation of this method, at runtime. Check this before checking state.
                        // Probably used to recover from a crash of an older version that doesn't use state.txt
                        batchIndex = int.Parse(File.ReadAllText(Constants.ManualOverrideFileName));
                        Console.WriteLine("Manual intervention, processing batch: {0}", batchIndex);
                    }
                    else if (File.Exists(Constants.StateFileName))
                    {
                        var state = File.ReadAllText(Constants.StateFileName);
                        batchIndex = int.Parse(state);
                        Console.WriteLine("Batch index determined from state: {0}", batchIndex);
                    }
                    else
                    {
                        batchIndex = NetworkCoordinator.GetNextFreeBatchIndex().Result;
                        File.WriteAllText(Constants.StateFileName, batchIndex.ToString());
                        Console.WriteLine("Checked out batch " + batchIndex);
                    }

                    var skipLines = batchIndex * chunkSize;
                    var opg = new BatchEnumerator();
                    foreach (var outerCollection in opg.EnumerateBatches())
                    {
                        var outer = new List<Tuple<int, int, BigInteger>>(outerCollection);
                        Console.WriteLine("Outer loop starts line {0} with A^x = {1}^{2}", outerLine, outer.First().Item1, outer.First().Item2);
                        if (outerLine < skipLines)
                        {
                            outerLine += outer.Count;
                            continue;
                        }

                        stopwatch.Restart();
                        innerBatchesDone = 0;
                        last = new TimeSpan(0);
                        var hopesFileName = string.Format(Constants.HopesFileNameFormat, outerLine);
                        var hopesFileNameZip = hopesFileName.Replace(".txt", ".zip");

                        using (var streamWriter = new StreamWriter(hopesFileName))
                        {
                            var innerLine = 0;
                            var ipg = new BatchEnumerator();

                            // foreach (var inner in EnumerateChunks(innerBatchlock))
                            Parallel.ForEach(ipg.EnumerateBatches(), (inner, state) =>
                            {
                                Interlocked.Add(ref innerLine, inner.Count);

                                // Warning: It is possible that some other thread changes innerLine before the check below is done.
                                // It's ok though, because then we'll just process an extra chunk (false positive). Though if we have too many of these 
                                // races, then we should re-consider the approach. For now, it's simple enough to try.
                                //if (innerLine >= outerLine)
                                //{
                                cross(outer, inner, streamWriter);
                                // }
                                // else
                                // {
                                // Skipped
                                // }

                            });
                        }

                        File.WriteAllLines(@"M:\temp\bla-r3.txt", result);
                        Console.WriteLine("Lines written: " + result.Count);

                        Console.WriteLine("Compressing '{0}'", hopesFileName);
                        using (FileStream fs = new FileStream(hopesFileNameZip, FileMode.Create))
                        using (ZipArchive arch = new ZipArchive(fs, ZipArchiveMode.Create))
                        {
                            arch.CreateEntryFromFile(hopesFileName, hopesFileName.Split('\\').Last());
                        }

                        Console.WriteLine("Uploading '{0}' to blob storage...", hopesFileNameZip);
                        NetworkCoordinator.UploadHopesBlobAsync(hopesFileNameZip).Wait();

                        Console.WriteLine("Marking the batch as Crunched");
                        NetworkCoordinator.MarkCrunched(batchIndex).Wait();

                        Console.ReadKey();

                        // Try delete the override first. Only if failed, then try to delete state (we don't want to delete both)
                        if (File.Exists(Constants.ManualOverrideFileName))
                        {
                            Console.WriteLine("Override is cleared.");
                            File.Delete(Constants.ManualOverrideFileName);
                        }
                        else if (File.Exists(Constants.StateFileName))
                        {
                            File.Delete(Constants.StateFileName);
                            Console.WriteLine("Local state is cleared.");
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " - " + e.StackTrace);
                Console.ReadKey();
                Console.ReadKey();
            }
        }

        private static int RunTests()
        {
            Console.WriteLine("Running tests..");
            var r = new Random();
            var every = r.Next(100, 200);
            Console.WriteLine("Going to verify ever {0}th number", every);
            int i = 0;
            foreach (var tup in Powers.GenerateBaseAndExponentValues())
            {
                if (i % every == 0)
                {
                    var ax = BigInteger.Pow(tup.Item1, tup.Item2);
                    if (!Hashing.InHash(ax))
                        throw new Exception("testing inhash failed");
                }
                ++i;
            }
            Console.WriteLine("testing is complete");
            return i;
        }

        private static List<string> result = new List<string>(100000);

        private static void cross(List<Tuple<int, int, BigInteger>> first, List<Tuple<int, int, BigInteger>> second, StreamWriter sw)
        {
            var ibd = Interlocked.Increment(ref innerBatchesDone);
            if (ibd % 100 == 0)
            {
                Console.WriteLine("Done {0}, total elapsed: {1}, last 100 batches took {2}s", 
                    ibd, 
                    stopwatch.Elapsed.ToString(@"d\d\:h\h\:m\m\:s\s", System.Globalization.CultureInfo.InvariantCulture),
                    (int)((stopwatch.Elapsed - last).TotalSeconds));
                last = stopwatch.Elapsed;
            }

            for (var i = 0; i < first.Count; ++i)
            {
                var f = first[i];
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
                        var hope = string.Format("{0}\t{1}\t{2}\t{3}", a, x, b, y);
                        lock (hopelock)//.Wait(); 
                        {
                            sw.WriteLine(hope);
                            result.Add(hope);
                        }
                        //hopelock.Release();
                    }
                }
            }
        }


      private static IEnumerable<List<Tuple<int, int, BigInteger>>> EnumerateChunks(SemaphoreSlim batchLock)
      {
          batchLock.Wait();
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
                  var copy = new List<Tuple<int, int, BigInteger>>(chunk);
                  batchLock.Release();
                  yield return copy;
                  batchLock.Wait();
                  ctr = 0;
                  chunk = new List<Tuple<int, int, BigInteger>>();
              }
          }
      
          batchLock.Release();
          yield return chunk;
      }

        //private static IEnumerable<List<Tuple<int, int, BigInteger>>> EnumerateChunks2(SemaphoreSlim batchLock, PowersEnumerator pg)
        //{
        //    lock(batchLock)
        //    {
        //        var chunk = new List<Tuple<int, int, BigInteger>>(chunkSize);
        //        for (var i = 0; i < chunkSize && pg.HasMore(); ++i)
        //        {
        //            var t = pg.Next();
        //            chunk.Add(Tuple.Create(t.Item1, t.Item2, BigInteger.Pow(t.Item1, t.Item2)));
        //        }
        //        yield return chunk;
        //    }
        //}
    }
}
