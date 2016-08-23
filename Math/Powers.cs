using System;
using System.Collections.Generic;
using System.Text;

namespace YMath
{
    static class Powers
    {
        internal static IEnumerable<Tuple<int, int>> GenerateBaseAndExponentValues()
        {
            var k = 1;
            var done = false;
            do
            {
                var p = 3;
                for (var b = k; b >= 2; --b)
                {
                    yield return Tuple.Create(b, p);
                    ++p;

                    //if (b > 889283)
                    if (b > 3183)
                    {
                        done = true;
                        break;
                    }

                    if (p > 200)
                        break;
                }

                ++k;
            }
            while (!done);
        }
    }
}
