using System.IO;

namespace ApplicationLayer.Common.Utilities
{
    public static class FileExtend
    {
        /// <summary>
        /// This function creates File after modify fileName if fileName already exists.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <returns>Returns a generated file name</returns>
        public static string CreateFile(string path, string fileName, string data)
        {
            var extension = Path.GetExtension(fileName);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

            int index = 1;
            while (true)
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                {
                    fileNameWithoutExt += index++;
                    fileName = fileNameWithoutExt + extension;

                    continue;
                }

                File.WriteAllText(fullPath, data);
                break;
            }

            return fileName;
        }
    }
}
