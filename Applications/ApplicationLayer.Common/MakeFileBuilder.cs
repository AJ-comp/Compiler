using System;
using System.Collections.Generic;
using System.IO;

namespace ApplicationLayer.Common
{
    public class MakeFileBuilder
    {
        public HashSet<string> CleanExtensions { get; }

        public HashSet<MakeFileSectionStruct> Sections = new HashSet<MakeFileSectionStruct>();

        /// <summary>
        /// This function creates makefile code snippet that creating the object file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="targetCodeFile"></param>
        /// <param name="referenceFiles"></param>
        /// <returns></returns>
        public static MakeFileSectionStruct CreateObjectSnippet(string path, string targetCodeFile, IEnumerable<string> referenceFiles)
        {
            var fileWithoutExt = Path.GetFileNameWithoutExtension(targetCodeFile);
            var fileFullPath = Path.Combine(path, fileWithoutExt);

            var cTargetCodeFile = new List<string>() { targetCodeFile };
            cTargetCodeFile.AddRange(referenceFiles);

            var content = string.Format("arm-none-eabi-as {0} -o {1}", fileFullPath + ".s", fileFullPath + ".o");

            return new MakeFileSectionStruct(fileWithoutExt + ".o", cTargetCodeFile, content);
        }


        public static MakeFileSectionStruct CreateObjectSnippet(string path, string targetCodeFile)
        {
            var fileWithoutExt = Path.GetFileNameWithoutExtension(targetCodeFile);
            var fileFullPath = Path.Combine(path, fileWithoutExt);

            var cTargetCodeFile = new List<string>() { targetCodeFile };
            var content = string.Format("arm-none-eabi-as {0} -o {1}", fileFullPath + ".s", fileFullPath + ".o");

            return new MakeFileSectionStruct(fileWithoutExt + ".o", cTargetCodeFile, content);
        }


        /// <summary>
        /// This function creates makefile code snippet that creating the binary file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="binFileName"></param>
        /// <param name="targetCodeFiles"></param>
        /// <returns></returns>
        public static MakeFileSectionStruct CreateBinSnippet(string path, 
                                                                                    string binFileName, 
                                                                                    string linkerFullPath, 
                                                                                    IEnumerable<string> targetCodeFiles)
        {
            var fileWithoutExt = Path.GetFileNameWithoutExtension(binFileName);
            var fileFullPath = Path.Combine(path, fileWithoutExt);
            var binFullPath = fileWithoutExt + ".bin";
            var mapFullPath = fileFullPath + ".map";
            var disassemFullPath = fileFullPath + ".dis";

            var linkingParam = string.Empty;
            foreach (var refFile in targetCodeFiles) linkingParam += " " + refFile;

            List<string> contents = new List<string>
            {
                string.Format("arm-none-eabi-ld {0} -T{1} -Map {2} --gc-sections -o {3}",
                                                        linkingParam, linkerFullPath, mapFullPath, fileFullPath),

                string.Format("arm-none-eabi-objdump -hD {0} > {1}", fileFullPath, disassemFullPath),
                string.Format("arm-none-eabi-objcopy {0} -O binary {1}", fileFullPath, binFullPath)
            };

            return new MakeFileSectionStruct(binFullPath, targetCodeFiles, contents);
        }


        /// <summary>
        /// This function creates makefile code snippet for clean block.
        /// </summary>
        /// <returns></returns>
        public static MakeFileSectionStruct CreateCleanSnippet()
        {
            var result = new MakeFileSectionStruct("clean", "rm -f *.o", "rm -f *.elf", "rm -f *.bin");
            result.StartingMessage = "Running target clean";

            return result;
        }


        /// <summary>
        /// This function creates makefile
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="makeFileBlocks"></param>
        public static void CreateMakeFile(string fullPath, IEnumerable<MakeFileSectionStruct> makeFileBlocks)
        {
            string result = string.Empty;

            foreach (var block in makeFileBlocks)
                result += block.ToString() + Environment.NewLine;

            File.WriteAllText(fullPath, result);
        }


        private Dictionary<string, string> _pathVar;
    }
}
