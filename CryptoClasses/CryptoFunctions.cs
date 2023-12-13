﻿using DoCCryptTool.SupportClasses;
using System;
using System.IO;
using static DoCCryptTool.SupportClasses.ToolEnums;

namespace DoCCryptTool.CryptoClasses
{
    internal static class CryptoFunctions
    {
        public static void CryptoLengthCheck(this uint bodySize)
        {
            if (bodySize % 8 != 0)
            {
                ExitType.Error.ExitProgram("Length of the body to decrypt/encrypt is not valid");
            }
        }

        public static uint LoopAByte(this uint decryptedByte, byte[] xorTable, uint xorTableOffset)
        {
            var byteIterator = 0;

            while (byteIterator < 8)
            {
                int integerVal = IntegersArray.Integers[decryptedByte];

                var xorTableByte = xorTable[xorTableOffset + byteIterator];
                var computedValue = integerVal - xorTableByte;

                if (computedValue < 0)
                {
                    decryptedByte = (uint)computedValue & 0xFF;
                }
                else
                {
                    decryptedByte = (uint)computedValue;
                }

                byteIterator++;
            }

            return decryptedByte;
        }

        public static long ArrayToFFNum(this byte[] byteArray)
        {
            var hexValue = "FFFFFFFF";
            hexValue += byteArray[0].ToString("X2") + "" + byteArray[1].ToString("X2") + "" + byteArray[2].ToString("X2") +
                "" + byteArray[3].ToString("X2");

            return Convert.ToInt64(hexValue, 16);
        }

        public static uint LoopAByteReverse(this byte byteToEncrypt, byte[] xorTable, uint xorTableOffset)
        {
            var byteIterator = 7;

            while (byteIterator > -1)
            {
                var xorTableByte = xorTable[xorTableOffset + byteIterator];
                var integerValUsed = xorTableByte + byteToEncrypt;

                if (integerValUsed > 255)
                {
                    var negativeHexVal = "FFFFFF";
                    negativeHexVal += byteToEncrypt.ToString("X2");

                    integerValUsed = Convert.ToInt32(negativeHexVal, 16) + xorTableByte;
                    byteToEncrypt = (byte)Array.IndexOf(IntegersArray.Integers, (byte)integerValUsed);
                }
                else
                {
                    byteToEncrypt = (byte)Array.IndexOf(IntegersArray.Integers, (byte)integerValUsed);
                }

                byteIterator--;
            }

            return byteToEncrypt;
        }

        public static uint ComputeCheckSum(this BinaryReader readerName, uint blocks, uint readPos)
        {
            uint chkSumVal = 0;
            uint totalChkSum = 0;
            byte currentVal;

            for (int i = 0; i < blocks; i++)
            {
                readerName.BaseStream.Position = readPos;
                currentVal = readerName.ReadByte();

                totalChkSum = chkSumVal + currentVal;

                chkSumVal = totalChkSum;
                readPos += 4;
            }

            return totalChkSum;
        }
    }
}