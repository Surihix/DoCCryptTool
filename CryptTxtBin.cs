using DoCCryptTool.CryptoClasses;
using DoCCryptTool.SupportClasses;
using System;
using System.IO;
using static DoCCryptTool.SupportClasses.ToolEnums;

namespace DoCCryptTool
{
    internal class CryptTxtBin
    {
        public static void ProcessFilelist(CryptActions cryptAction, string inFile)
        {
            using (var inFileReader = new BinaryReader(File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                Console.WriteLine("Performing initial setup....");
                Console.WriteLine("");

                Checks.TxtBinState(cryptAction, inFileReader);

                Console.WriteLine("Generating KeyBlocks tables....");
                Console.WriteLine("");

                var headerSeedArray = new byte[] { 136, 86, 49, 149, 241, 163, 137, 87 };
                var bodySeedArray = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

                var keyblocksTableHeader = Generator.GenerateKeyblocksTable(headerSeedArray, false);
                var keyblocksTableBody = Generator.GenerateKeyblocksTable(bodySeedArray, false);


                uint readPos = 0;
                uint writePos = 0;

                switch (cryptAction)
                {
                    case CryptActions.d:
                        Console.WriteLine("Decrypting text bin file....");
                        Console.WriteLine("");

                        (inFile + ".dec").IfFileExistsDel();

                        using (var decryptedStreamBinWriter = new BinaryWriter(File.Open(inFile + ".dec", FileMode.Append, FileAccess.Write)))
                        {

                        }

                        inFileReader.Dispose();

                        inFile.CreateFinalFile(inFile + ".dec");

                        ExitType.Success.ExitProgram($"Finished decrypting '{Path.GetFileName(inFile)}'.");
                        break;

                    case CryptActions.e:
                        Console.WriteLine("Encrypting text bin file....");
                        Console.WriteLine("");

                        (inFile + ".enc").IfFileExistsDel();
                        (inFile + ".tmp").IfFileExistsDel();

                        File.Copy(inFile, inFile + ".tmp");

                        using (var encryptedStreamBinWriter = new BinaryWriter(File.Open(inFile + ".enc", FileMode.Append, FileAccess.Write)))
                        {

                        }

                        inFileReader.Dispose();

                        inFile.CreateFinalFile(inFile + ".enc");

                        ExitType.Success.ExitProgram($"Finished encrypting '{Path.GetFileName(inFile)}'.");
                        break;
                }
            }
        }
    }
}