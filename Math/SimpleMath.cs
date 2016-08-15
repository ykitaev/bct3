namespace YMath
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class SimpleMath
    {
        public static int GCD(int a, int b)
        {
            int Remainder;

            while (b != 0)
            {
                Remainder = a % b;
                a = b;
                b = Remainder;
            }

            return a;
        }
    }
}
