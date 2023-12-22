using DoCCryptTool.SupportClasses;
using System.IO;
using static DoCCryptTool.SupportClasses.ToolEnums;

namespace DoCCryptTool
{
    internal class Checks
    {
        public static void TxtBinState(CryptActions cryptActions, BinaryReader inFileReader)
        {
            var fileSize = (uint)inFileReader.BaseStream.Length;

            inFileReader.BaseStream.Position = 0;

            switch (cryptActions)
            {
                case CryptActions.e:
                    if (inFileReader.ReadUInt64() != 1)
                    {
                        ExitType.Error.ExitProgram("Specified file is not decrypted correctly or may not be a valid txt bin file.");
                    }

                    inFileReader.BaseStream.Position = 20;
                    if (inFileReader.ReadUInt32() != fileSize)
                    {
                        ExitType.Error.ExitProgram("Specified txt bin file is not decrypted correctly for encryption operation.");
                    }
                    break;

                case CryptActions.d:
                    if (inFileReader.ReadUInt64() != 10733845617377775685)
                    {
                        ExitType.Error.ExitProgram("Specified file is not a valid txt bin file.");
                    }

                    inFileReader.BaseStream.Position = 20;
                    if (inFileReader.ReadUInt32() == fileSize)
                    {
                        ExitType.Error.ExitProgram("Specified txt bin file is decrypted already.");
                    }
                    break;
            }
        }

        public static void ScriptState(CryptActions cryptActions, BinaryReader inFileReader, uint cryptBodySize)
        {
            inFileReader.BaseStream.Position = 0;
            if (inFileReader.ReadUInt32() != 1414812756)
            {
                ExitType.Error.ExitProgram("Specified file is not a valid class file.");
            }

            cryptBodySize -= 8;
            var cryptSizeOffset = (uint)inFileReader.BaseStream.Length - 8;

            switch (cryptActions)
            {
                case CryptActions.e:
                    inFileReader.BaseStream.Position = cryptSizeOffset;
                    if (inFileReader.ReadUInt32() != cryptBodySize)
                    {
                        ExitType.Error.ExitProgram("Specified class file is not decrypted correctly for encryption operation.");
                    }
                    break;

                case CryptActions.d:
                    inFileReader.BaseStream.Position = cryptSizeOffset;
                    if (inFileReader.ReadUInt32() == cryptBodySize)
                    {
                        ExitType.Error.ExitProgram("Specified class file is already decrypted.");
                    }
                    break;
            }
        }
    }
}