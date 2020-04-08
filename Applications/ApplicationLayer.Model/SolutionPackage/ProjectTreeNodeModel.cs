using ApplicationLayer.Common.Interfaces;
using System;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class ProjectTreeNodeModel : PathTreeNodeModel, IManagableElements
    {
        public abstract string ProjectType { get; }
        public override string DisplayName => System.IO.Path.GetFileNameWithoutExtension(this.FileName);
        public override string FullOnlyPath => System.IO.Path.Combine(Parent.FullOnlyPath, Path);

        public abstract bool IsChanged { get; }

        public ProjectTreeNodeModel() : base(string.Empty, string.Empty)
        { }

        public ProjectTreeNodeModel(string path, string projName) : base(path, projName)
        {
        }

        public abstract void LoadElement();
        public abstract void Save();
        public abstract void SyncWithLoadValue();
        public abstract void SyncWithCurrentValue();
    }
}
