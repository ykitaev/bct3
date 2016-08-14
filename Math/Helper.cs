using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YMath
{
    internal static class Helper
    {
        internal static void DumpBits(string fileName, BitArray bits)
        {
            using (var bw = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                for (var i = 0; i < bits.Count; ++i)
                {
                    byte b = bits[i] ? (byte)1 : (byte)0;
                    bw.Write(b);
                }
            }
        }

        internal static void LoadBitsFromFile(string fileName, BitArray target)
        {
            using (var br = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                for (var i = 0; i < target.Count; ++i)
                {
                    byte b = br.ReadByte();
                    target[i] = (b == 1);
                }
            }
        }
    }
}
