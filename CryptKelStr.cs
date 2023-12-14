using DoCCryptTool.CryptoClasses;
using DoCCryptTool.SupportClasses;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
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

                uint dwordBlockAdditionVal = 0x01FE0024; // 0x01FE0024 works for all except Beta.

                if (detectedHeader == "KelStr 1.1 2005/07/11 14:55")
                {
                    dwordBlockAdditionVal = 0x01FE8024; // 0x01FE8024 is used for Beta.
                }

                Console.WriteLine("Generating bitmask....");
                Console.WriteLine("");

                var bitMask = Generators.GenerateBitMask();


                switch (cryptAction)
                {
                    case CryptActions.e:
                        Console.WriteLine("Unimplemented");
                        break;

                    case CryptActions.d:
                        Console.WriteLine("Decrypting 'kelstr.bin'....");
                        Console.WriteLine("");

                        (inFile + ".dec").IfFileExistsDel();

                        File.Copy(inFile, inFile + ".dec");

                        using (var mmf = MemoryMappedFile.CreateFromFile(inFile + ".dec"))
                        {
                            using (var accessor = mmf.CreateViewAccessor())
                            {
                                var fileSize = accessor.ReadUInt32(0x1C) + 0x20;
                                int bitMaskIterator = 0;
                                byte byteToWrite;

                                // The bitMask is applied from offset 0x20 and onwards
                                // until the end of the file.
                                // The game applies the XOR on 4-byte blocks at a time, but here we do
                                // one byte at a time.
                                for (int n = 0x20; n < fileSize; n++)
                                {
                                    bitMaskIterator &= 15;

                                    byteToWrite = (byte)(accessor.ReadByte(n) ^ bitMask[bitMaskIterator]);
                                    accessor.Write(n, byteToWrite);

                                    bitMaskIterator++;
                                }

                                // Retrieving the fileSectionSizeHelper from offset
                                // 0x20 and deriving the dwordBlockCounter.
                                var fileSectionSizeHelper = accessor.ReadUInt32(0x20);
                                var dwordBlockCounterCap = ((fileSectionSizeHelper + 1) >> 0x0A) + 1;

                                // Updating the file section header with dwordBlockAdditionVal
                                // to dwords that are not 0.
                                for (int i = 0, n = 0x24; i < dwordBlockCounterCap; n += 4)
                                {
                                    var dwordBlock = accessor.ReadUInt32(n);

                                    if (dwordBlock != 0)
                                    {
                                        dwordBlock += dwordBlockAdditionVal;

                                        accessor.Write(n, dwordBlock);
                                    }
                                    i++;
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