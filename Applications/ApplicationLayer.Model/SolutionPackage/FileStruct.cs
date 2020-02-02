using System.IO;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class FileStruct : HirStruct
    {
    }

    public class ErrorFileStruct : FileStruct
    {
    }

    public class DefaultFileStruct : FileStruct
    {
        public string Data { get; set; }

        public void CreateFile()
        {
            if (File.Exists(this.FullPath)) return;

            Directory.CreateDirectory(this.BaseOPath);
            File.WriteAllText(this.FullPath, this.Data);
        }
    }
}
