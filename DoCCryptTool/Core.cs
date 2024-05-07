using DoCCryptTool.SupportClasses;
using System;
using System.IO;
using static DoCCryptTool.SupportClasses.ToolEnums;

namespace DoCCryptTool
{
    internal class Core
    {
        static void Main(string[] args)
        {
            var exampleMsgArray = new string[]
            {
                "Examples:",
                "To decrypt a text bin file: DoCCryptTool.exe -d -txtbin \"string_us.bin\"",
                "To decrypt a class file: DoCCryptTool.exe -d -script \"gmap.class\"",
                "To decrypt kelstr.bin file: DoCCryptTool.exe -d -kelstr \"kelstr.bin\"",
                "To decrypt an at3 bgm file: DoCCryptTool.exe -d -at3 \"bgm_004.at3\"", "",
                "To encrypt a text bin file: DoCCryptTool.exe -e -txtbin \"string_us.bin\"",
                "To encrypt a class file: DoCCryptTool.exe -e -script \"gmap.class\"",
                "To encrypt kelstr.bin file: DoCCryptTool.exe -e -kelstr \"kelstr.bin\"",
                "To encrypt an at3 bgm file: DoCCryptTool.exe -d -at3 \"bgm_004.at3\"", "",
                "Important:", "Change the filename mentioned in the example to the name or path of" +
                "\nthe file that you are trying to decrypt or encrypt.", ""
            };

            var actionSwitchesMsgArray = new string[]
            {
                "Action Switches:", "-d = To Decrypt", "-e = To Encrypt"
            };

            var cryptTypeSwitchesMsgArray = new string[]
            {
                "Crypt Type Switches:", "-txtbin = For text bin files", "-script = For class files", "-kelstr = For kelstr.bin files", "-at3 = For at3 bgm files"
            };


            // Check length
            if (args.Length < 2)
            {
                ExitType.Error.ExitProgram($"Enough arguments not specified\n\n{string.Join("\n", actionSwitchesMsgArray)}\n\n{string.Join("\n", exampleMsgArray)}");
            }

            // Set CryptAction
            var cryptAction = new CryptActions();
            if (Enum.TryParse(args[0].Replace("-", ""), false, out CryptActions convertedActionSwitch))
            {
                cryptAction = convertedActionSwitch;
            }
            else
            {
                ExitType.Error.ExitProgram($"Invalid or no action switch specified\n\n{string.Join("\n", actionSwitchesMsgArray)}");
            }

            // Set CryptType
            var cryptType = new CryptType();
            if (Enum.TryParse(args[1].Replace("-", ""), false, out CryptType convertedTypeSwitch))
            {
                cryptType = convertedTypeSwitch;
            }
            else
            {
                ExitType.Error.ExitProgram($"Invalid or no crypt type switch specified\n\n{string.Join("\n", cryptTypeSwitchesMsgArray)}");
            }

            // Set file
            var inFile = args[2];

            if (!File.Exists(inFile))
            {
                ExitType.Error.ExitProgram("Specified file is missing");
            }

            Console.WriteLine("");

            try
            {
                switch (cryptType)
                {
                    case CryptType.txtbin:
                        CryptTxtBin.ProcessFilelist(cryptAction, inFile);
                        break;

                    case CryptType.script:
                        CryptScript.ProcessScript(cryptAction, inFile);
                        break;

                    case CryptType.kelstr:
                        CryptKelStr.ProcessKelStr(cryptAction, inFile);
                        break;

                    case CryptType.at3:
                        CryptAT3.ProcessAT3(cryptAction, inFile);
                        break;
                }
            }
            catch (Exception ex)
            {
                ExitType.Error.ExitProgram($"An Exception has occured\n{ex}");
            }
        }

        enum CryptType
        {
            txtbin,
            script,
            kelstr,
            at3
        }
    }
}