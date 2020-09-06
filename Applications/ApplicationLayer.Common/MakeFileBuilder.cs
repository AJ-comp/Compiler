using System;
using System.Collections.Generic;
using System.IO;

namespace ApplicationLayer.Common
{
    public class MakeFileBuilder
    {
        /// <summary>
        /// This function creates makefile code snippet that creating the object file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="targetCodeFile"></param>
        /// <param name="referenceFiles"></param>
        /// <returns></returns>
        public static MakeFileStruct CreateObjectSnippet(string path, string targetCodeFile, IEnumerable<string> referenceFiles)
        {
            var fileWithoutExt = Path.GetFileNameWithoutExtension(targetCodeFile);
            var fileFullPath = Path.Combine(path, fileWithoutExt);

            var cTargetCodeFile = new List<string>() { targetCodeFile };
            cTargetCodeFile.AddRange(referenceFiles);

            var content = string.Format("arm-none-eabi-as {0} -o {1}", fileFullPath + ".s", fileFullPath + ".o");

            return new MakeFileStruct(fileWithoutExt + ".o", cTargetCodeFile, content);
        }


        /// <summary>
        /// This function creates makefile code snippet that creating the binary file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="binFileName"></param>
        /// <param name="targetCodeFiles"></param>
        /// <returns></returns>
        public static MakeFileStruct CreateBinSnippet(string path, string binFileName, IEnumerable<string> targetCodeFiles)
        {
            var fileWithoutExt = Path.GetFileNameWithoutExtension(binFileName);
            var fileFullPath = Path.Combine(path, fileWithoutExt);
            var binFileNameToCreate = fileWithoutExt + ".bin";
            var elfFileName = fileFullPath + ".elf";

            var linkingParam = string.Empty;
            foreach (var refFile in targetCodeFiles) linkingParam += " " + refFile;

            List<string> contents = new List<string>();
            contents.Add(string.Format("arm-none-eabi-ld -Ttext=0x08000000 {0} -o {1}",
                                                        linkingParam, elfFileName));

            contents.Add(string.Format("arm-none-eabi-objdump -D {0}", elfFileName));
            contents.Add(string.Format("arm-none-eabi-objcopy {0} -O binary {1}",
                                                                elfFileName, binFileNameToCreate));

            return new MakeFileStruct(binFileNameToCreate, targetCodeFiles, contents);
        }


        /// <summary>
        /// This function creates makefile code snippet for clean block.
        /// </summary>
        /// <returns></returns>
        public static MakeFileStruct CreateCleanSnippet()
        {
            var result = new MakeFileStruct("clean", "rm -f *.o", "rm -f *.elf", "rm -f *.bin");
            result.StartingMessage = "Running target clean";

            return result;
        }


        /// <summary>
        /// This function creates makefile
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="makeFileBlocks"></param>
        public static void CreateMakeFile(string fullPath, IEnumerable<MakeFileStruct> makeFileBlocks)
        {
            string result = string.Empty;

            foreach (var block in makeFileBlocks)
                result += block.ToString() + Environment.NewLine;

            File.WriteAllText(fullPath, result);
        }
    }
}
