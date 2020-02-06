using System.IO;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class FileHier : HierarchicalData, IChangeTrackable
    {
        public abstract bool IsChanged { get; }

        public abstract void Commit();
        public abstract void RollBack();
    }

    public class ErrorFileHier : FileHier
    {
        public override bool IsChanged => false;

        public override void Commit()
        {
        }

        public override void RollBack()
        {
        }
    }

    public class DefaultFileHier : FileHier
    {
        private string prevData;
        public string Data { get; set; }

        public override bool IsChanged => (prevData != Data);

        public void CreateFile()
        {
            if (File.Exists(this.FullPath)) return;

            Directory.CreateDirectory(this.BaseOPath);
            File.WriteAllText(this.FullPath, this.Data);
        }

        public override void Commit()
        {
            prevData = Data;
        }

        public override void RollBack()
        {
            Data = prevData;
        }
    }
}
