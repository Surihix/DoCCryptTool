using DoCCryptTool.CryptoClasses;
using DoCCryptTool.SupportClasses;
using System;
using System.IO;
using System.Text;
using static DoCCryptTool.SupportClasses.ToolEnums;

namespace DoCCryptTool
{
    internal class CryptKelStr
    {
        public static void ProcessKelStr(CryptActions cryptAction, string inFile)
        {
            using (var inFileReader = new BinaryReader(File.Open(inFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var readValueBuffer = inFileReader.ReadBytes(28);
                var detectedHeader = Encoding.ASCII.GetString(readValueBuffer).Replace("\0", "");

                uint dwordBlockComputeVal = 0x01FE0024; // 0x01FE0024 works for all except Beta.

                if (detectedHeader == "KelStr 1.1 2005/07/11 14:55")
                {
                    dwordBlockComputeVal = 0x01FE8024; // 0x01FE8024 is used for Beta.
                }

                Console.WriteLine("Generating bitmask....");
                Console.WriteLine("");

                var bitMask = Generators.GenerateBitMask();


                switch (cryptAction)
                {
                    case CryptActions.e:
                        Console.WriteLine("Encrypting 'kelstr.bin'....");
                        Console.WriteLine("");

                        (inFile + ".enc").IfFileExistsDel();

                        File.Copy(inFile, inFile + ".enc");

                        using (var encryptedDataStream = new FileStream(inFile + ".enc", FileMode.Open, FileAccess.ReadWrite))
                        {
                            using (var encDataReader = new BinaryReader(encryptedDataStream))
                            {
                                using (var encDataWriter = new BinaryWriter(encryptedDataStream))
                                {
                                    encDataReader.BaseStream.Position = 0x20;
                                    var fileSectionSizeHelper = encDataReader.ReadUInt32();
                                    var dwordBlockCounterCap = ((fileSectionSizeHelper + 1) >> 0x0A) + 1;

                                    uint dwordBlock;

                                    for (int i = 0, n = 0x24; i < dwordBlockCounterCap; n += 4)
                                    {
                                        encDataReader.BaseStream.Position = n;
                                        dwordBlock = encDataReader.ReadUInt32();
                                        if (dwordBlock != 0)
                                        {
                                            dwordBlock -= dwordBlockComputeVal;

                                            encDataWriter.BaseStream.Position = n;
                                            encDataWriter.Write(dwordBlock);
                                        }

                                        i++;
                                    }

                                    encDataReader.BaseStream.Position = 0x1C;
                                    var fileSize = encDataReader.ReadUInt32() + 0x20;
                                    int bitMaskIterator = 0;
                                    byte byteToWrite;

                                    for (int n = 0x20; n < fileSize; n++)
                                    {
                                        bitMaskIterator &= 15;

                                        encDataReader.BaseStream.Position = n;
                                        byteToWrite = (byte)(bitMask[bitMaskIterator] ^ encDataReader.ReadByte());

                                        encDataWriter.BaseStream.Position = n;
                                        encDataWriter.Write(byteToWrite);

                                        bitMaskIterator++;
                                    }
                                }
                            }
                        }

                        inFileReader.Dispose();

                        inFile.CreateFinalFile(inFile + ".enc");

                        ExitType.Success.ExitProgram($"Finished encrypting '{Path.GetFileName(inFile)}'.");
                        break;

                    case CryptActions.d:
                        Console.WriteLine("Decrypting 'kelstr.bin'....");
                        Console.WriteLine("");

                        (inFile + ".dec").IfFileExistsDel();

                        using (var decryptedDataWriter = new BinaryWriter(File.Open(inFile + ".dec", FileMode.Append, FileAccess.Write)))
                        {
                            inFileReader.BaseStream.Seek(0, SeekOrigin.Begin);
                            inFileReader.BaseStream.ExCopyTo(decryptedDataWriter.BaseStream, 0x20);

                            inFileReader.BaseStream.Position = 0x1C;
                            var fileSize = inFileReader.ReadUInt32() + 0x20;
                            int bitMaskOffset = 0;

                            // The bitMask is applied from offset 0x20 and onwards
                            // until the end of the file.
                            // The game applies the XOR on 4-byte blocks at a time, but here we do
                            // one byte at a time.
                            inFileReader.BaseStream.Position = 0x20;

                            for (int n = 0x20; n < fileSize; n++)
                            {
                                bitMaskOffset &= 15;
                                decryptedDataWriter.Write((byte)(inFileReader.ReadByte() ^ bitMask[bitMaskOffset]));

                                bitMaskOffset++;
                            }
                        }

                        using (var decryptedDataStream = new FileStream(inFile + ".dec", FileMode.Open, FileAccess.ReadWrite))
                        {
                            using (var decryptedDataReader = new BinaryReader(decryptedDataStream))
                            {
                                using (var decryptedFileSectionWriter = new BinaryWriter(decryptedDataStream))
                                {
                                    // Retrieving the fileSectionSizeHelper from offset
                                    // 0x20 and deriving the dwordBlockCounter.
                                    decryptedDataReader.BaseStream.Position = 0x20;
                                    var fileSectionSizeHelper = decryptedDataReader.ReadUInt32();
                                    var dwordBlockCounterCap = ((fileSectionSizeHelper + 1) >> 0x0A) + 1;

                                    // Updating the file section header with dwordBlockComputeVal
                                    // to dwords that are not 0.
                                    uint dwordBlock;

                                    for (int i = 0, n = 0x24; i < dwordBlockCounterCap; n += 4)
                                    {
                                        decryptedDataReader.BaseStream.Position = n;
                                        dwordBlock = decryptedDataReader.ReadUInt32();

                                        if (dwordBlock != 0)
                                        {
                                            dwordBlock += dwordBlockComputeVal;

                                            decryptedFileSectionWriter.BaseStream.Position = n;
                                            decryptedFileSectionWriter.Write(dwordBlock);
                                        }

                                        i++;
                                    }
                                }
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