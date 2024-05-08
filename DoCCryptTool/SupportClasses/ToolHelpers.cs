using System;
using System.IO;
using static DoCCryptTool.SupportClasses.ToolEnums;

namespace DoCCryptTool.SupportClasses
{
    internal static class ToolHelpers
    {
        public static void ExitProgram(this ExitType exitType, string exitMsg)
        {
            var exitMsgType = "";
            var exitCode = 0;

            switch (exitType)
            {
                case ExitType.Success:
                    exitMsgType = "Success: ";
                    break;

                case ExitType.Error:
                    exitMsgType = "Error: ";
                    exitCode = 1;
                    break;
            }

            Console.WriteLine("");
            Console.WriteLine($"{exitMsgType}{exitMsg}");

            if (exitCode == 1)
            {
                Console.ReadLine();
            }

            Environment.Exit(exitCode);
        }

        public static void ExCopyTo(this Stream inStream, Stream outStream, long size)
        {
            int bufferSize = 81920;
            long amountRemaining = size;

            while (amountRemaining > 0)
            {
                long arraySize = Math.Min(bufferSize, amountRemaining);
                byte[] copyArray = new byte[arraySize];

                _ = inStream.Read(copyArray, 0, (int)arraySize);
                outStream.Write(copyArray, 0, (int)arraySize);

                amountRemaining -= arraySize;
            }
        }

        public static void IfFileExistsDel(this string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        public static void CreateFinalFile(this string ogFile, string processedFile)
        {
            var ogFileName = Path.GetFileName(ogFile);
            var ogFileDir = Path.GetDirectoryName(ogFile);
            var newFile = Path.Combine(ogFileDir, ogFileName);

            File.Delete(ogFile);
            File.Move(processedFile, newFile);
        }
    }
}