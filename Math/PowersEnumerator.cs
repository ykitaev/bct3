using System;
using System.Collections.Generic;
using System.Text;

namespace YMath
{
    class PowersEnumerator
    {
        private int b;
        private int e;
        private int k;

        public PowersEnumerator()
        {
            b = 2;
            e = 3;
            k = 2;
        }

        public bool HasMore()
        {
            return b < 3183;
        }

        public Tuple<int, int> Next()
        {
            var t = Tuple.Create(b, e);
            --b;
            ++e;
            if (b == 1 || e == 201)
            {
                ++k;
                b = k;
                e = 3;
            }

            return t;
        }
    }
}
