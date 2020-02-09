using ApplicationLayer.Common.Interfaces;
using System.IO;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class FileHier : HierarchicalData, ISaveAndChangeTrackable
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

        public override void Save()
        {
        }
    }

    public class DefaultFileHier : FileHier
    {
        private string originalData;
        public string Data { get; set; }

        public override bool IsChanged => (originalData != Data);

        public void CreateFile()
        {
            if (File.Exists(this.FullPath)) return;

            Directory.CreateDirectory(this.BaseOPath);
            File.WriteAllText(this.FullPath, this.Data);
        }

        public override void Commit()
        {
            originalData = Data;
        }

        public override void RollBack()
        {
            Data = originalData;
        }

        public override void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this.FullPath));
            File.WriteAllText(this.FullPath, Data);
        }
    }
}
