using DoCCryptTool.SupportClasses;
using System;
using System.IO;
using static DoCCryptTool.SupportClasses.ToolEnums;

namespace DoCCryptTool
{
    internal class CryptAT3
    {
        public static void ProcessAT3(CryptActions cryptAction, string inFile)
        {
            using (var inFileReader = new BinaryReader(File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var at3keys = new byte[]
                {
                    0xA3, 0xDD, 0x51, 0x46, 0x7C, 0x49, 0x5B, 0x1C, 0x81, 0x25, 0x81, 0x55, 0x9A, 0x6B, 0xAF, 0xD5,
                    0x21, 0xF2, 0x88, 0x3E, 0x47, 0xDA, 0x51, 0xE1, 0x2F, 0x9B, 0x81, 0xA7, 0x1F, 0xCB, 0x00, 0x01,
                    0xA5, 0x47, 0x00, 0xAC, 0xBF, 0x32, 0x03, 0x44, 0xAB, 0x44, 0x7B, 0x9A, 0xA8, 0xA0, 0x4E, 0xDF,
                    0x06, 0x73, 0xD1, 0x49, 0x26, 0xCB, 0xB4, 0xF4, 0x0D, 0x5F, 0x66, 0xDF, 0x32, 0xCA, 0xB9, 0x27,
                    0x98, 0x91, 0x4D, 0x20, 0x80, 0x6A, 0x38, 0x60, 0x9E, 0x79, 0xC4, 0x8A, 0x08, 0x07, 0xD3, 0x87,
                    0x23, 0xE1, 0x28, 0xD4, 0x20, 0xB5, 0x52, 0xAC, 0x35, 0x4C, 0x45, 0x2F, 0xF8, 0x60, 0x8C, 0x3A
                };


                switch (cryptAction)
                {
                    case CryptActions.e:
                        Console.WriteLine($"Encrypting '{Path.GetFileName(inFile)}'....");
                        Console.WriteLine("");

                        (inFile + ".enc").IfFileExistsDel();

                        File.Copy(inFile, inFile + ".enc");

                        using (var encDataWriter = new BinaryWriter(File.Open(inFile + ".enc", FileMode.Open, FileAccess.Write)))
                        {
                            long readAmount = inFileReader.BaseStream.Length;
                            long bytesToRead = 96;
                            long readPos = 0;

                            while (bytesToRead != 0)
                            {
                                bytesToRead = Math.Min(readAmount, 96);
                                readAmount -= bytesToRead;

                                inFileReader.BaseStream.Position = readPos;
                                var bytesToProcessPreXOR = inFileReader.ReadBytes((int)bytesToRead);

                                var bytesToProcessXOR = new byte[bytesToRead];
                                Array.Copy(bytesToProcessPreXOR, bytesToProcessXOR, bytesToRead);

                                for (int i = 0; i < bytesToRead; i++)
                                {
                                    bytesToProcessXOR[i] ^= at3keys[i];

                                    for (int j = 0; j < i / 16; j++)
                                    {
                                        bytesToProcessXOR[i] ^= bytesToProcessPreXOR[i - (j + 1) * 16];
                                    }
                                }

                                encDataWriter.Write(bytesToProcessXOR);

                                readPos += bytesToRead;
                            }
                        }

                        inFileReader.Dispose();

                        inFile.CreateFinalFile(inFile + ".enc");

                        ExitType.Success.ExitProgram($"Finished encrypting '{Path.GetFileName(inFile)}'.");
                        break;

                    case CryptActions.d:
                        if (inFileReader.ReadByte() == 0xA2)
                        {
                            ExitType.Error.ExitProgram("File is already decrypted");
                        }

                        Console.WriteLine($"Decrypting '{Path.GetFileName(inFile)}'....");
                        Console.WriteLine("");

                        (inFile + ".dec").IfFileExistsDel();

                        using (var decryptedDataWriter = new BinaryWriter(File.Open(inFile + ".dec", FileMode.Append, FileAccess.Write)))
                        {
                            long readAmount = inFileReader.BaseStream.Length;
                            long bytesToRead = 96;
                            long readPos = 0;

                            while (bytesToRead != 0)
                            {
                                bytesToRead = Math.Min(readAmount, 96);
                                readAmount -= bytesToRead;

                                inFileReader.BaseStream.Position = readPos;
                                var bytesToProcess = inFileReader.ReadBytes((int)bytesToRead);

                                for (int i = 0; i < bytesToRead; i++)
                                {
                                    bytesToProcess[i] ^= at3keys[i];

                                    for (int j = 0; j < i / 16; j++)
                                    {
                                        bytesToProcess[i] ^= bytesToProcess[i - (j + 1) * 16];
                                    }
                                }

                                decryptedDataWriter.Write(bytesToProcess);

                                readPos += bytesToRead;
                            }
                        }

                        inFileReader.Dispose();

                        inFile.CreateFinalFile(inFile + ".dec");

                        ExitType.Success.ExitProgram($"Finished decrypting '{Path.GetFileName(inFile)}'.");
                        break;
                }
            }
        }
    }
}