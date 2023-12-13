using DoCCryptTool.CryptoClasses;
using DoCCryptTool.SupportClasses;
using System;
using System.IO;
using static DoCCryptTool.SupportClasses.ToolEnums;

namespace DoCCryptTool
{
    internal class CryptScript
    {
        public static void ProcessScript(CryptActions cryptAction, string inFile)
        {
            using (var inFileReader = new BinaryReader(File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                Console.WriteLine("Performing initial setup....");
                Console.WriteLine("");

                var cryptBodySize = (uint)inFileReader.BaseStream.Length - 8;

                Checks.ScriptState(cryptAction, inFileReader, cryptBodySize);

                Console.WriteLine("Generating Keyblocks table....");
                Console.WriteLine("");

                inFileReader.BaseStream.Position = 0;
                var seedArray = inFileReader.ReadBytes(8);
                Array.Reverse(seedArray);

                var keyblocksTable = Generator.GenerateKeyblocksTable(seedArray, false);


                uint readPos = 8;
                uint writePos = 8;

                switch (cryptAction)
                {
                    case CryptActions.d:
                        Console.WriteLine("Decrypting class file....");
                        Console.WriteLine("");

                        (inFile + ".dec").IfFileExistsDel();

                        using (var decryptedStreamBinWriter = new BinaryWriter(File.Open(inFile + ".dec", FileMode.Append, FileAccess.Write)))
                        {
                            inFileReader.BaseStream.Position = 0;
                            inFileReader.BaseStream.ExCopyTo(decryptedStreamBinWriter.BaseStream, readPos);

                            cryptBodySize.CryptoLengthCheck();
                            var blockCount = cryptBodySize / 8;
                            Decryption.DecryptBlocks(keyblocksTable, blockCount, readPos, writePos, inFileReader, decryptedStreamBinWriter, false);
                        }

                        inFileReader.Dispose();

                        inFile.CreateFinalFile(inFile + ".dec");

                        ExitType.Success.ExitProgram($"Finished decrypting '{Path.GetFileName(inFile)}'.");
                        break;

                    case CryptActions.e:
                        Console.WriteLine("Encrypting class file....");
                        Console.WriteLine("");

                        cryptBodySize.CryptoLengthCheck();

                        (inFile + ".enc").IfFileExistsDel();
                        (inFile + ".tmp").IfFileExistsDel();

                        File.Copy(inFile, inFile + ".tmp");

                        using (var checkSumWriter = new BinaryWriter(File.Open(inFile + ".tmp", FileMode.Open, FileAccess.Write)))
                        {
                            var checkSum = inFileReader.ComputeCheckSum((cryptBodySize - 8) / 4, 8);

                            checkSumWriter.BaseStream.Position = inFileReader.BaseStream.Length - 4;
                            checkSumWriter.Write(checkSum);
                        }

                        inFileReader.Dispose();

                        using (var inFileTmpReader = new BinaryReader(File.Open(inFile + ".tmp", FileMode.Open, FileAccess.Read)))
                        {
                            using (var encryptedStreamBinWriter = new BinaryWriter(File.Open(inFile + ".enc", FileMode.Append, FileAccess.Write)))
                            {
                                inFileTmpReader.BaseStream.Position = 0;
                                inFileTmpReader.BaseStream.ExCopyTo(encryptedStreamBinWriter.BaseStream, writePos);

                                var blockCount = cryptBodySize / 8;
                                Encryption.EncryptBlocks(keyblocksTable, blockCount, readPos, writePos, inFileTmpReader, encryptedStreamBinWriter, false);
                            }
                        }

                        inFile.CreateFinalFile(inFile + ".enc");
                        (inFile + ".tmp").IfFileExistsDel();

                        ExitType.Success.ExitProgram($"Finished encrypting '{Path.GetFileName(inFile)}'.");
                        break;
                }
            }
        }
    }
}