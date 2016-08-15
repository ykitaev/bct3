using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cruncher
{
    class Helpers
    {
        static long countLines(string fileName)
        {
            long ctr = 0;
            long len = 0;
            using (StreamReader sr = new StreamReader(fileName))
            {
                while (!sr.EndOfStream)
                {
                    var l = sr.ReadLine();
                    ++ctr;
                    len += l.Length;
                    if (ctr % 1000000 == 0)
                        Console.WriteLine("Counted: " + ctr + ", len = " + len / 1000000 + " megs");
                }
            }

            return len;
        }
    }
}
