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
        private bool hasMore;

        public PowersEnumerator()
        {
            b = 2;
            e = 3;
            k = 2;
            hasMore = true;
        }

        public bool HasMore()
        {
            return hasMore;
        }

        public Tuple<int, int> Next()
        {
            if (!HasMore())
                throw new IndexOutOfRangeException("No more powers");
            var t = Tuple.Create(b, e);
            --b;
            ++e;
            if (b == 1 || e == 201)
            {
                ++k;
                b = k;
                e = 3;

                if (b > 3183)
                    hasMore = false;
            }

            return t;
        }
    }
}
