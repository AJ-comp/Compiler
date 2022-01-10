using Parse.BackEnd.Target;
using Parse.FrontEnd;
using System;
using System.Diagnostics;
using System.IO;

namespace Compile
{
    public class Builder
    {
        /***********************************************************************/
        /// <summary>
        /// This function creates a bitcode.
        /// </summary>
        /// <param name="buildNode">The build node to create bitcode</param>
        /// <param name="bitCodeFullPath">The full path included bitcode name to create</param>
        /***********************************************************************/
        public static void CreateBitCode(SdtsNode buildNode, string bitCodeFullPath)
        {
            buildNode.Compile(new CompileParameter(0, 0, true));

            /*
            IRExpressionGenerator.WriteData(buildNode);
            var instructionList = llvmExpr.Build();

            string textCode = string.Empty;
            foreach (var instruction in instructionList)
            {
                instruction.Save();
                textCode += instruction.Instruct + Environment.NewLine;
            }


            File.WriteAllText(bitCodeFullPath, textCode);
            */
        }


        /*************************************************************************/
        /// <summary>
        /// This function creates a target code.
        /// </summary>
        /// <param name="bitCodeFullPath">The full path that included bitcode filename</param>
        /// <param name="targetCodeFullPath">The full path included target code name to create</param>
        /// <param name="mcpuType">The mcpu type of the target</param>
        /*************************************************************************/
        public static void CreateAssem(string bitCodeFullPath, string targetCodeFullPath, string mcpuType)
        {
            var command = $"-mtriple=arm-none-eabi -march=thumb -mattr=thumb2 -mcpu={mcpuType} {bitCodeFullPath} -o {targetCodeFullPath}";

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


        /***********************************************************************/
        /// <summary>
        /// 링커스크립트를 생성합니다.
        /// </summary>
        /// <param name="path">링커스크립트를 생성할 경로</param>
        /// <param name="fileName">링커스크립트 파일 명</param>
        /***********************************************************************/
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


        /***********************************************************************/
        /// <summary>
        /// This function executes a makefile.
        /// </summary>
        /// <param name="folderPath">The folder path that existing the makefile to execute</param>
        /***********************************************************************/
        public static void ExecuteMakeFile(string folderPath)
        {
            // execute makefile with make.exe
            var process = new Process();
            process.StartInfo.FileName = "make.exe";
            process.StartInfo.Arguments = $"-C {folderPath} -f makefile";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            // Synchronously read the standard output of the spawned process.
            StreamReader reader = process.StandardOutput;
            string output = reader.ReadToEnd();

            //                        process = Process.Start(startInfo);
            process.WaitForExit();
        }


        /***********************************************************************/
        /// <summary>
        /// JLink 커맨드를 생성합니다.
        /// </summary>
        /// <param name="path">커맨드를 생성할 경로</param>
        /// <param name="binFileToLoad">바이너리 파일 명</param>
        /// <param name="downloadAddress">바이너리 코드가 다운로드 될 메모리 주소</param>
        /***********************************************************************/
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


        /*****************************************************/
        /// <summary>
        /// This function creates bootstrap and linker script.           <br/>
        /// 부트스트랩과 링커스크립트를 생성합니다.                   <br/>
        /// </summary>
        /// <param name="solutionBinFolderPath"></param>
        /// <param name="bootstrapName"></param>
        /// <param name="linkerScriptName"></param>
        /*****************************************************/
        public static void CreateStartingFile(string solutionBinFolderPath, string bootstrapName, string linkerScriptName, Target target)
        {
            // create bootstrap
            File.WriteAllText(Path.Combine(solutionBinFolderPath, bootstrapName), target.StartUpCode);

            // create linker script
            Builder.CreateLinkerScript(solutionBinFolderPath, linkerScriptName);
        }
    }
}
