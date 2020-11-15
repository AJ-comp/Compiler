using Parse.FrontEnd;
using Parse.FrontEnd.IRGenerator;
using System;
using System.Diagnostics;
using System.IO;

namespace ApplicationLayer.Common
{
    public class Builder
    {
        /// <summary>
        /// This function creates a bitcode.
        /// </summary>
        /// <param name="sdtsNode">The AST root node to create bitcode</param>
        /// <param name="bitCodeFullPath">The full path included bitcode name to create</param>
        public static void CreateBitCode(SdtsNode sdtsNode, string bitCodeFullPath)
        {
            var test = IRExpressionGenerator.GenerateLLVMExpression(sdtsNode);
            var instructionList = test.Build();

            string textCode = string.Empty;
            foreach (var instruction in instructionList) textCode += instruction.FullData + Environment.NewLine;

            File.WriteAllText(bitCodeFullPath, textCode);
        }


        /// <summary>
        /// This function creates a target code.
        /// </summary>
        /// <param name="bitCodeFullPath">The full path that included bitcode filename</param>
        /// <param name="targetCodeFullPath">The full path included target code name to create</param>
        /// <param name="mcpuType">The mcpu type of the target</param>
        public static void CreateAssem(string bitCodeFullPath, string targetCodeFullPath, string mcpuType)
        {
            var command = string.Format("-mtriple=arm-none-eabi -march=thumb -mattr=thumb2 -mcpu={0} {1} -o {2}",
                                                            mcpuType, bitCodeFullPath, targetCodeFullPath);

            // execute llc.exe
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = @"llc.exe",
                Arguments = command,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();
        }


        public static void CreateLinkerScript(string path, string fileName)
        {
            string code = "OUTPUT_FORMAT(\"elf32-littlearm\", \"elf32-littlearm\", \"elf32-littlearm\")" + Environment.NewLine +
                                "OUTPUT_ARCH(arm)" + Environment.NewLine +
                                Environment.NewLine +

                                "MEMORY" + Environment.NewLine +
                                "{" + Environment.NewLine +
                                "\trom(rx)   : ORIGIN = 0x08000000, LENGTH = 64K" + Environment.NewLine +
                                "\tram(rw!x) : ORIGIN = 0x20000000, LENGTH = 20K" + Environment.NewLine +
                                "}" + Environment.NewLine +
                                Environment.NewLine +

                                "SECTIONS" + Environment.NewLine +
                                "{" + Environment.NewLine +
                                "\t.text :" + Environment.NewLine +
                                "\t{" + Environment.NewLine +
                                "\t\tKEEP(*(.vectors.table))" + Environment.NewLine +
                                "\t\tKEEP(*(.vectors.code))" + Environment.NewLine +
                                "\t\t* (.text.text.*)" + Environment.NewLine +
                                "\t\t* (.rodata.rodata.*)" + Environment.NewLine +
                                "\t} > rom" + Environment.NewLine +
                                Environment.NewLine +

                                "\t. = ALIGN(4);" + Environment.NewLine +
                                "\t\t_etext = .;" + Environment.NewLine +
                                Environment.NewLine +

                                "\t.data : AT(_etext)" + Environment.NewLine +
                                "\t{" + Environment.NewLine +
                                "\t\t_sdata = .;" + Environment.NewLine +
                                Environment.NewLine +

                                "\t\t* (.data.data.*)" + Environment.NewLine +
                                Environment.NewLine +

                                "\t\t. = ALIGN(4);" + Environment.NewLine +
                                "\t\t_edata = .;" + Environment.NewLine +
                                Environment.NewLine +

                                "\t} > ram" + Environment.NewLine +
                                Environment.NewLine +

                                "\t.bss(NOLOAD) :" + Environment.NewLine +
                                "\t{" + Environment.NewLine +
                                "\t\t. = ALIGN(4);" + Environment.NewLine +
                                "\t\t_sbss = .;" + Environment.NewLine +
                                Environment.NewLine +

                                "\t\t*(.bss.bss.*)" + Environment.NewLine +
                                "\t\t* (COMMON)" + Environment.NewLine +
                                Environment.NewLine +

                                "\t\t. = ALIGN(4);" + Environment.NewLine +
                                "\t\t_ebss = .;" + Environment.NewLine +
                                "\t} > ram" + Environment.NewLine +
                                Environment.NewLine +

                                "\t. = ALIGN(4);" + Environment.NewLine +
                                Environment.NewLine +
                                "\tend = .;" + Environment.NewLine +
                                "\t_ram_end = ORIGIN(ram) + LENGTH(ram) - 0x1;" + Environment.NewLine +
                                "}";

            File.WriteAllText(Path.Combine(path, fileName), code);
        }


        /// <summary>
        /// This function executes a makefile.
        /// </summary>
        /// <param name="folderPath">The folder path that existing the makefile to execute</param>
        public static void ExecuteMakeFile(string folderPath)
        {
            // execute makefile with make.exe
            var process = new Process();
            process.StartInfo.FileName = "make.exe";
            process.StartInfo.Arguments = string.Format("-C {0} -f makefile", folderPath);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            // Synchronously read the standard output of the spawned process.
            StreamReader reader = process.StandardOutput;
            string output = reader.ReadToEnd();

            //                        process = Process.Start(startInfo);
            process.WaitForExit();
        }


        public static void CreateJLinkCommanderScript(string path, string binFileToLoad, string downloadAddress)
        {
            var fileContent = string.Format("si 1" + Environment.NewLine +
                                                          "r " + Environment.NewLine +
                                                          "erase " + Environment.NewLine +
                                                          "h " + Environment.NewLine +
                                                          "loadbin {0}, {1} " + Environment.NewLine +
                                                          "g" + Environment.NewLine +
                                                          "exit",
                                                           Path.Combine(path, binFileToLoad),
                                                           downloadAddress);

            File.WriteAllText(Path.Combine(path, "CommanderScript.jlink"), fileContent);
        }
    }
}
