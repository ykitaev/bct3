using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PerfectPowerDetector
{
    partial class Program
    {
        /// <summary>
        /// This one tests algorithm C; it's good to run it each time to make sure nothing's broken
        /// </summary>
        private static void testAlgC()
        {
            for (int i = 12000; i < 12700; ++i)
            {
                for (int k = 3; k <= 13; ++k)
                {
                    BigInteger n = BigInteger.Pow(i, k);

                    // Check that it can at all find anything
                    int res = algc(n);
                    if (res < 2)
                        throw new Exception("AlgoC test fail! res < 2 at the first stage");

                    BigInteger baseNo = rootbi(n, res);
                    BigInteger poww = BigInteger.Pow(baseNo, res);
                    if (!BigInteger.Equals(poww, n))
                        throw new Exception("AlgoC test fail! res didn't work at the first stage");

                    // Negative test
                    BigInteger nm1 = n - 1;
                    res = algc(nm1);
                    if (res != 0)
                        throw new Exception("AlgoC test fail! found power that it shouldn't at the first stage");
                }
            }

            // A hard-coded tricky value to test against; small base, large exponent
            BigInteger threeto127 = BigInteger.Parse("3930061525912861057173624287137506221892737197425280369698987");
            int r = algc(threeto127);
            if (r != 127)
                throw new Exception("AlgoC test fail at stage 2! The exponent was supposed to be 127");

            // Large base, small exponent
            BigInteger largeto5 = BigInteger.Parse("999915002889950870417603580143");
            r = algc(largeto5);
            if (r != 5)
                throw new Exception("AlgoC test fail at stage 3! The exponent must be 5.");

            // If all passed...
            Console.WriteLine("Algo C tested ok.");
        }

        /// <summary>
        /// Tests how the Great Answer method is working.. similar to AlgC but broader
        /// </summary>
        private static void testIsSuitablePower()
        {

            for (int b = 7; b < 29; ++b)
            {
                for (int e = 3; e < 70; ++e)
                {
                    BigInteger n = BigInteger.Pow(b, e);
                    if (!isSuitablePower(n))
                    {
                        Console.WriteLine("\n" + n + " : SuitablePower method test failed at step 1");
                        throw new Exception("Suitable method test failed at step 1");
                    }

                    n = n - 1;
                    if (isSuitablePower(n))
                    {
                        Console.WriteLine("\n" + n + " : SuitablePower method test failed at step 2");
                        throw new Exception("Suitable method test failed at step 1.5");
                    }
                }
            }

            List<BigInteger> weirdTestCases = new List<BigInteger>();
            weirdTestCases.Add(BigInteger.Pow(2, 6));
            weirdTestCases.Add(BigInteger.Pow(2, 4));
            weirdTestCases.Add(BigInteger.Pow(6, 4));
            weirdTestCases.Add(BigInteger.Pow(324324235, 32));

            foreach (BigInteger n in weirdTestCases)
            {
                if (!isSuitablePower(n))
                    throw new Exception("Suitable method test failed at step 2");
            }

            // Negative test
            List<BigInteger> negativeTests = new List<BigInteger>();
            negativeTests.Add(BigInteger.Pow(1, 1));
            negativeTests.Add(BigInteger.Pow(2, 2));
            negativeTests.Add(BigInteger.Pow(3, 2));
            negativeTests.Add(BigInteger.Pow(121, 1));
            foreach (BigInteger n in negativeTests)
            {
                if (isSuitablePower(n))
                    throw new Exception("Suitable method test failed at step 3");
            }

            Console.WriteLine("OK!");
        }
    }
}
