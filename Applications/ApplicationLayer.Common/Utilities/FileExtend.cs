using System.IO;

namespace ApplicationLayer.Common.Utilities
{
    public static class FileExtend
    {
        public static void CreateFile(string path, string fileName)
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

                File.Create(fullPath);
                break;
            }
        }
    }
}
