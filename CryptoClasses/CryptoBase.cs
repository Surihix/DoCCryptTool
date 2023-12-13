using System;

namespace DoCCryptTool.CryptoClasses
{
    internal class CryptoBase
    {
        static uint BlockCounterEval { get; set; }

        static uint BlockCounterFval { get; set; }

        public static void BlockCounterSetup(uint blockCounter, ref uint xorTableOffset)
        {
            var blockCounterLowerMost = (ushort)(blockCounter & 0xFFFF);

            blockCounterLowerMost >>= 3;
            blockCounterLowerMost <<= 3;
            xorTableOffset = (uint)blockCounterLowerMost & 0xF8;

            long blockCounterABshiftVal = (long)blockCounter << 10;
            long blockCounterCDshiftVal = (long)blockCounter << 20;
            long blockCounterEFshiftVal = (long)blockCounter << 30;

            var blockCounterAval = (uint)(blockCounterABshiftVal & 0xFFFFFFFF);
            var blockCounterBval = (uint)(blockCounterABshiftVal >> 32);

            var blockCounterCval = (uint)(blockCounterCDshiftVal & 0xFFFFFFFF);
            var blockCounterDval = (uint)(blockCounterCDshiftVal >> 32);

            blockCounterCval |= blockCounterAval;
            blockCounterDval |= blockCounterBval;

            BlockCounterEval = (uint)(blockCounterEFshiftVal & 0xFFFFFFFF);
            BlockCounterFval = (uint)(blockCounterEFshiftVal >> 32);

            BlockCounterEval |= blockCounter;
            BlockCounterFval |= 0;

            BlockCounterEval |= blockCounterCval;
            BlockCounterFval |= blockCounterDval;
        }

        public static void XORblockSetup(byte[] xorTable, uint xorTableOffset, ref uint xorBlockLowerVal, ref uint xorBlockHigherVal)
        {
            var currentXORblock = new byte[] { xorTable[xorTableOffset + 0], xorTable[xorTableOffset + 1], 
                xorTable[xorTableOffset + 2], xorTable[xorTableOffset + 3], xorTable[xorTableOffset + 4], 
                xorTable[xorTableOffset + 5], xorTable[xorTableOffset + 6], xorTable[xorTableOffset + 7] };

            xorBlockLowerVal = BitConverter.ToUInt32(currentXORblock, 0);
            xorBlockHigherVal = BitConverter.ToUInt32(currentXORblock, 4);
        }

        public static void SpecialKeySetup(ref uint carryFlag, ref long specialKey1, ref long specialKey2)
        {
            if (BlockCounterEval > ~0xA1652347)
            {
                carryFlag = 1;
            }
            else
            {
                carryFlag = 0;
            }

            specialKey1 = (long)BlockCounterEval + 0xA1652347;
            specialKey2 = (long)BlockCounterFval + carryFlag;
        }
    }
}