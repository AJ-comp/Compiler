using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AJ.Common.Helpers
{
    public static class DirectoryHelper
    {
        public static void DeleteAllFiles(string dirPath)
        {
            DirectoryInfo di = new DirectoryInfo(dirPath);
            FileInfo[] files = di.GetFiles();
            foreach (FileInfo file in files) file.Delete();
        }
    }
}
