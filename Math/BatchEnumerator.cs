using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;

namespace YMath
{
    class BatchEnumerator
    {
        const int BatchSize = 1000;
        private PowersEnumerator pg;
        private SemaphoreSlim ss;
        private bool hasMore;

        public BatchEnumerator()
        {
            this.pg = new PowersEnumerator();
            this.ss = new SemaphoreSlim(initialCount: 1);
            this.hasMore = true;
        }

        private bool HasMore()
        {
            return this.hasMore;
        }

        private List<Tuple<int, int, BigInteger>> Next()
        {
            ss.Wait();
            var list = new List<Tuple<int, int, BigInteger>>(BatchSize);
            if (!HasMore()) return list;
            for (var i = 0; i < BatchSize && pg.HasMore(); ++i)
            {
                var tup = pg.Next();
                var a = tup.Item1;
                var x = tup.Item2;
                var ax = BigInteger.Pow(a, x);
                list.Add(Tuple.Create(a, x, ax));
            }

            this.hasMore = pg.HasMore();
            ss.Release();
            return list;
        }

        public IEnumerable<List<Tuple<int, int, BigInteger>>> EnumerateBatches()
        {
            while(this.HasMore())
            {
                yield return Next();
            }
        }
    }
}
