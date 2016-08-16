namespace PerfectPowerDetector
{
    using Configuration;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    partial class Program
    {
        private static Random r = new Random();
        private static DateTime startTime = DateTime.Now;

        /// <summary>
        /// Scans a file and checks if it has any perfect powers
        /// If if fines any, writes a, x, b, and y to dreams.txt
        /// </summary>
        /// <param name="args">Not used for now</param>
        /// <remarks>Code copied over from my work on this back in 2013, so the coding style might be different</remarks>
        static void Main(string[] args)
        {
            boot();
            var cnt = 0;
            Console.WriteLine("Computing cz and checking if it's a perfect power...");
            var fileName = @"M:\temp\test.txt";
            var lines = File.ReadAllLines(fileName);
            foreach (var l in lines)
            {
                var parts = l.Split('\t');
                var a = int.Parse(parts[0]);
                var x = int.Parse(parts[1]);
                var b = int.Parse(parts[2]);
                var y = int.Parse(parts[3]);

                var cz = BigInteger.Pow(a, x) + BigInteger.Pow(b, y);
                var res = isSuitablePower(cz);
                if (res)
                {
                    var str = l + Environment.NewLine;
                    File.AppendAllText(Constants.DreamsFileName, str);
                }

                cnt++;
                if (cnt % 10000 == 0)
                {
                    Console.WriteLine("Done {0}/{1}", cnt, lines.Count());
                }
            }
        }
    }
}
