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

                var keyblocksTableHeader = Generators.GenerateKeyblocksTable(headerSeedArray, false);
                var keyblocksTableBody = Generators.GenerateKeyblocksTable(bodySeedArray, false);


                uint readPos = 0;
                uint writePos = 0;
                uint blockCount = 0;

                switch (cryptAction)
                {
                    case CryptActions.d:
                        Console.WriteLine("Decrypting text bin file....");
                        Console.WriteLine("");

                        (inFile + ".dec").IfFileExistsDel();

                        using (var decryptedStream = new MemoryStream())
                        {
                            using (var decryptedStreamBinWriter = new BinaryWriter(decryptedStream))
                            {
                                // Header
                                Console.WriteLine("Decrypting header section....");
                                Decryption.DecryptBlocks(keyblocksTableHeader, 4, readPos, writePos, inFileReader, decryptedStreamBinWriter, false);
                                Console.WriteLine("");

                                using (var decryptedStreamBinReader = new BinaryReader(decryptedStream))
                                {
                                    decryptedStreamBinReader.BaseStream.Position = 12;
                                    uint decryptedFooterTxtSize = decryptedStreamBinReader.ReadUInt32();

                                    // Body
                                    var decryptionBodySize = new FileInfo(inFile).Length - decryptedFooterTxtSize - 32;

                                    ((uint)decryptionBodySize).CryptoLengthCheck();
                                    blockCount = (uint)decryptionBodySize / 8;

                                    readPos = 32;
                                    writePos = 32;

                                    Console.WriteLine("Decrypting body section....");
                                    Decryption.DecryptBlocks(keyblocksTableBody, blockCount, readPos, writePos, inFileReader, decryptedStreamBinWriter, false);
                                    Console.WriteLine("");

                                    using (var outFileStream = new FileStream(inFile + ".dec", FileMode.Append, FileAccess.Write))
                                    {
                                        decryptedStream.Seek(0, SeekOrigin.Begin);
                                        decryptedStream.CopyTo(outFileStream);

                                        inFileReader.BaseStream.Seek(decryptionBodySize + 32, SeekOrigin.Begin);
                                        inFileReader.BaseStream.CopyTo(outFileStream);
                                    }
                                }
                            }
                        }

                        inFileReader.Dispose();

                        inFile.CreateFinalFile(inFile + ".dec");

                        ExitType.Success.ExitProgram($"Finished decrypting '{Path.GetFileName(inFile)}'.");
                        break;

                    case CryptActions.e:
                        Console.WriteLine("Encrypting text bin file....");
                        Console.WriteLine("");

                        (inFile + ".tmp").IfFileExistsDel();
                        (inFile + ".enc").IfFileExistsDel();

                        File.Copy(inFile, inFile + ".tmp");

                        inFileReader.BaseStream.Position = 12;
                        var encryptionBodySize = (uint)new FileInfo(inFile).Length - inFileReader.ReadUInt32() - 32;

                        encryptionBodySize.CryptoLengthCheck();

                        blockCount = encryptionBodySize / 8;

                        using (var checkSumWriter = new BinaryWriter(File.Open(inFile + ".tmp", FileMode.Open, FileAccess.Write)))
                        {
                            var headerCheckSum = inFileReader.ComputeCheckSum(24 / 4, 0);
                            var bodyCheckSum = inFileReader.ComputeCheckSum((encryptionBodySize - 8) / 4, 32);

                            checkSumWriter.BaseStream.Position = 28;
                            checkSumWriter.Write(headerCheckSum);

                            checkSumWriter.BaseStream.Position = 32 + (encryptionBodySize - 4);
                            checkSumWriter.Write(bodyCheckSum);
                        }

                        using (var inFileTmpReader = new BinaryReader(File.Open(inFile + ".tmp", FileMode.Open, FileAccess.Read)))
                        {
                            using (var encryptedStreamBinWriter = new BinaryWriter(File.Open(inFile + ".enc", FileMode.Append, FileAccess.Write)))
                            {
                                Console.WriteLine("Encrypting header section....");
                                Encryption.EncryptBlocks(keyblocksTableHeader, 4, readPos, writePos, inFileTmpReader, encryptedStreamBinWriter, false);                                
                                Console.WriteLine("");

                                readPos = 32;
                                writePos = 32;

                                Console.WriteLine("Encrypting body section....");
                                Encryption.EncryptBlocks(keyblocksTableBody, blockCount, readPos, writePos, inFileTmpReader, encryptedStreamBinWriter, false);                               
                                Console.WriteLine("");

                                inFileTmpReader.BaseStream.Position = encryptionBodySize + 32;
                                inFileTmpReader.BaseStream.CopyTo(encryptedStreamBinWriter.BaseStream);
                            }
                        }

                        inFileReader.Dispose();

                        inFile.CreateFinalFile(inFile + ".enc");
                        (inFile + ".tmp").IfFileExistsDel();

                        ExitType.Success.ExitProgram($"Finished encrypting '{Path.GetFileName(inFile)}'.");
                        break;
                }
            }
        }
    }
}