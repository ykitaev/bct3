namespace PerfectPowerDetector
{
    using Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// This parts is responsible for dealing with perfect powers, i.e. deteciting if a number is a perfect power
    /// </summary>
    /// <remarks>
    /// Kind of legacy code. Code copied over from my work on this back in 2013, so the coding style might be different.
    /// Using this research paper as source of ideas, Algorithm C: http://cr.yp.to/lineartime/powers2-20060914-ams.pdf
    /// </remarks>
    partial class Program
    {
        /// <summary>
        /// The array of primes - necessary for the math stuff to work (qtable generation and Algorithm C)
        /// </summary>
        static List<int> primes = new List<int>(80000000);

        /// <summary>
        /// QTable is necessary for Algorithm C to wrok
        /// </summary>
        static Dictionary<int, List<int>> qtable = new Dictionary<int, List<int>>();

        /// <summary>
        /// Loads the prime numbers and stores them in the "primes" array
        /// </summary>
        private static void loadPrimeNumbers()
        {
            string line;
            using (var file = new System.IO.StreamReader(Constants.PrimesFileName))
            {
                int index = 0;
                while ((line = file.ReadLine()) != null)
                {
                    int pr = Int32.Parse(line);
                    primes.Add(pr);
                    index++;
                }
            }
        }

        /// <summary>
        /// Hardcore algorithm that for given N determines if there exists such numbers a and p such that a^p = N
        /// </summary>
        /// <param name="n">The number to check if it's a perfect power</param>
        /// <returns>The power (smallest) to which you can raise some 'a' to get N</returns>
        /// <remarks>Requires an array of primes and a QTable to work
        /// Implemented back in 2013 using this research paper as source of ideas:
        /// http://cr.yp.to/lineartime/powers2-20060914-ams.pdf
        /// Haven't reviewed this code in a while; I seem to have changed the style of coding since then a bit. 
        /// To make sense of it, it's best to follow the Algorithm C from the research paper above.
        /// So there is a test code that tests this on every run to make sure it works.. I'd just leave it as-is for now
        /// </remarks>
        private static int algc(BigInteger n)
        {
            double logn2 = BigInteger.Log(n, 2);
            double lnlogn2 = Math.Log(logn2);
            // Compute the smallest integer b such that ...
            int b = (int)logn2;
            int leftb = 1;
            int rightb = b;
            int midb = (leftb + rightb) / 2;
            while (leftb != midb && rightb != midb)
            {
                int bcand = midb;
                double logbcand = Math.Log(bcand, 2);
                double lcrit = bcand * logbcand * logbcand;
                double rcrit = logn2;
                if (lcrit < rcrit)
                {
                    leftb = midb;
                    midb = (leftb + rightb) / 2;
                    continue;
                }
                else
                {
                    rightb = midb;
                    midb = (leftb + rightb) / 2;
                    continue;
                }
            }
            b = rightb;
            // S  <--  {p : p log n/ log b}
            int plimit = (int)Math.Round((BigInteger.Log(n, 2) / Math.Log(b, 2)));
            HashSet<int> s = new HashSet<int>();
            for (int i = 0; i < primes.Count; ++i)
            {
                int p = primes[i];
                if (p > plimit)
                    break;
                s.Add(p);
            }

            // Trial division
            BigInteger rem = 0;
            for (int i = 0; i < primes.Count; ++i)
            {
                int r = primes[i];
                if (r > b)
                    break;
                BigInteger.DivRem(n, r, out rem);
                if (rem.IsZero)
                {
                    int e = 0;
                    BigInteger renrem = 0;
                    BigInteger re2nrem = 0;
                    do
                    {
                        e++;
                        BigInteger rtoe = BigInteger.Pow(r, e);
                        BigInteger rtoe2 = rtoe * r;
                        BigInteger.DivRem(n, rtoe, out renrem);
                        BigInteger.DivRem(n, rtoe2, out re2nrem);
                    } while (renrem.IsZero && re2nrem.IsZero);
                    s.Clear();
                    for (int j = 0; j < primes.Count; ++j)
                    {
                        int p = primes[j];
                        if (p > e)
                            break;
                        if (e % p == 0)
                            s.Add(p);
                    }
                    break;
                }
            }

            while (s.Count > 0)
            {
                int p = s.Min();
                //Perform up to d2 log log n/ log pe sieve tests on n for p:
                bool passedall = true;
                int numberOfTests = (int)Math.Ceiling(2 * Math.Log(logn2, 2) / Math.Log(p, 2));
                int largestStieveModulus = (int)Math.Ceiling(logn2 * lnlogn2 * lnlogn2);

                for (int i = 0; i < numberOfTests; ++i)
                {
                    int q = qtable[p][i];
                    if (q > largestStieveModulus)
                        break;

                    int pow = (q - 1) / p;
                    BigInteger residue = BigInteger.ModPow(n, pow, q);
                    if (!residue.IsOne && !residue.IsZero)
                    {
                        passedall = false;
                        break;
                    }
                    BigInteger qrem = 0;
                    BigInteger.DivRem(n, q, out qrem);
                    if (qrem.IsZero)
                    {
                        int e = 0;
                        BigInteger qenrem = 0;
                        BigInteger qe2nrem = 0;
                        do
                        {
                            e++;
                            BigInteger qtoe = BigInteger.Pow(q, e);
                            BigInteger qtoe2 = qtoe * q;
                            BigInteger.DivRem(n, qtoe, out qenrem);
                            BigInteger.DivRem(n, qtoe2, out qe2nrem);
                        } while (qenrem.IsZero && qe2nrem.IsZero);
                        HashSet<int> newset = new HashSet<int>();
                        int newsetlim = e;
                        for (int j = 0; j < primes.Count; ++j)
                        {
                            int pp = primes[j];
                            if (pp > newsetlim)
                                break;
                            if (e % pp == 0)
                                newset.Add(pp);
                        }
                        s.IntersectWith(newset);
                    }
                }

                if (passedall && s.Contains(p))
                {
                    BigInteger rt = rootbi(n, p);
                    if (BigInteger.Pow(rt, p).Equals(n))
                        return p;
                }
                s.Remove(p);
            }
            return 0;
        }

        /// <summary>
        /// Computes rth root of number using bisection; not supposed to be called very frequently (slow)
        /// </summary>
        /// <param name="number">The number for which we compute root</param>
        /// <param name="r">The 'th' part of the root</param>
        /// <returns>The rth root of number, rounded/truncated if not integer - don't trust this case much</returns>
        private static BigInteger rootbi(BigInteger number, int r)
        {
            BigInteger left = 1;
            BigInteger right = number / 2;
            BigInteger middle = right / 2;
            while (!left.Equals(right))
            {
                BigInteger middletopow = BigInteger.Pow(middle, r);
                if (middletopow.Equals(number))
                    return middle;
                else if (middletopow > number)
                    right = middle;
                else
                    left = middle;

                middle = (right + left) / 2;
                if ((right - left).IsOne)
                {
                    BigInteger rightex = BigInteger.Pow(right, r) - number;
                    BigInteger leftex = number - BigInteger.Pow(left, r);
                    if (rightex > leftex)
                        return left;
                    else
                        return right;
                }
            }
            return left;
        }

        /// <summary>
        /// Generates the QTable for the AlgorithmC method; some internal thing needed for the code to work
        /// </summary>
        /// <remarks>Requires an array of primes to work</remarks>
        private static void makeQtable()
        {
            for (int ip = 0; ip < primes.Count; ++ip)
            {
                int p = primes[ip];
                if (p > 40000)
                    break;

                qtable[p] = new List<int>();
                for (int iq = 0; iq < primes.Count; ++iq)
                {
                    int q = primes[iq];
                    if ((q - 1) % p == 0)
                        qtable[p].Add(q);

                    if (q > 3000000)
                        break;
                }
            }
        }

        /// <summary>
        /// When Algorithm C stumbles on a 4th power or any other high even power: it returns 2, because every even power is also 2nd power.
        /// However, 2 is not enough, we need 3 or higher, so we need to check - maybe the number is also some other power
        /// </summary>
        /// <param name="n">The number to check. This should be at least 2nd power or higher</param>
        /// <returns>True if it's also a fourth power, false otherwise</returns>
        private static int getPowOfRoot(BigInteger n)
        {
            BigInteger root = rootbi(n, 2);
            return algc(root);
        }

        /// <summary>
        /// Combines all methods above to give a definite answer: is number N a perfect power with exponent greater than 2
        /// </summary>
        /// <param name="n">The number we are checking!</param>
        /// <returns>True N is a perfect power with exponent greater than 2, false otherwise</returns>
        private static bool isSuitablePower(BigInteger n)
        {
            int pow = algc(n);
            if (pow == 0)
            {
                return false;
            }
            else if (pow == 2)
            {
                int powOfRoot = getPowOfRoot(n);
                if (powOfRoot >= 2)
                    return true;
                else
                    return false;
            }
            else
            {
                return true; // Yay!
            }
        }
    }
}
