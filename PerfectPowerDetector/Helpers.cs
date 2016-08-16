namespace PerfectPowerDetector
{
    using System;

    partial class Program
    {
        /// <summary>
        /// A mutex for shared structures and output
        /// </summary>
        private static object lock1 = new object();

        /// <summary>
        /// LOAD, SETUP, AND TEST
        /// </summary>
        private static void boot()
        {
            // LOAD, SETUP, AND TEST
            Console.WriteLine("Loading...");
            loadPrimeNumbers();

            Console.WriteLine("Generating qtable...");
            makeQtable();

            Console.Write("Testing Algorithm C... ");
            testAlgC();

            Console.Write("Testing Suitable Power Method... ");
            testIsSuitablePower();
        }
    }
}
