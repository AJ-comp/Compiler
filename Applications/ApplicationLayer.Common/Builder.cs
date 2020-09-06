using Parse.FrontEnd;
using Parse.FrontEnd.IRGenerator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

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
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"llc.exe";
            startInfo.Arguments = command;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            var process = Process.Start(startInfo);
            process.WaitForExit();
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


        public static void CreateJLinkCommanderScript(string path, string binFileToLoad)
        {
            var fileContent = string.Format("si 1" + Environment.NewLine +
                                                          "r " + Environment.NewLine +
                                                          "erase " + Environment.NewLine +
                                                          "h " + Environment.NewLine +
                                                          "loadbin {0}, 0x08000000 " + Environment.NewLine +
                                                          "g" + Environment.NewLine +
                                                          "exit",
                                                           Path.Combine(path, binFileToLoad));

            File.WriteAllText(Path.Combine(path, "CommanderScript.jlink"), fileContent);
        }
    }
}
