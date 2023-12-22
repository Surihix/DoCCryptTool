# DoCCryptTool
This tool allows you to decrypt and encrypt the text bin files, class files and the kelstr.bin files from Dirge Of Cerberus Final Fantasy VII.

The program should be launched from command prompt with a function switch, a filetype switch and with the input file. a list of valid function and filetype switches are given below.

**Function Switches:**
<br>``-d`` - For decryption
<br>``-e`` - For encryption

**Filetype Switches:**
<br>``-txtbin`` - For handling text bin files
<br>``-script`` - For handling class files
<br>``-kelstr`` - For handling kelstr.bin files

<br>**Commandline usage examples:**
<br>For Text bin files:
<br>`` DoCCryptTool.exe -d -txtbin "string_us.bin" ``
<br>`` DoCCryptTool.exe -e -txtbin "string_us.bin" ``

For class files:
<br>`` DoCCryptTool.exe -d -script "gmap.class" ``
<br>`` DoCCryptTool.exe -e -script "gmap.class" ``

For kelstr.bin files:
<br>`` DoCCryptTool.exe -d -kelstr "kelstr.bin" ``
<br>`` DoCCryptTool.exe -e -kelstr "kelstr.bin" ``

## Credits
[Shademp](https://github.com/Shademp) - Decryption assembly algorithm, format research and general support
