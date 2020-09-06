using System.IO;

namespace ApplicationLayer.Common
{
    public class FileHelper
    {
        public static string ConvertFileName(string fileName, string extension)
            => Path.GetFileNameWithoutExtension(fileName) + extension;

        /// <summary>
        /// This function converts file name to the file name that has .o extension.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ConvertObjectFileName(string fileName) => ConvertFileName(fileName, ".o");


        /// <summary>
        /// This function converts file name to the file name that has .s extension.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ConvertTargetFileName(string fileName) => ConvertFileName(fileName, ".s");


        /// <summary>
        /// This function converts file name to the file name that has .bin extension.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ConvertBinaryFileName(string fileName) => ConvertFileName(fileName, ".bin");


        /// <summary>
        /// This function converts file name to the file name that has .elf extension.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ConvertElfFileName(string fileName) => ConvertFileName(fileName, ".elf");

    }
}
