using ApplicationLayer.Common.Interfaces;
using System.Collections.Generic;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class ErrorProjectTreeNodeModel : ProjectTreeNodeModel, IManagedable
    {
        public override bool IsChanged => false;
        public override string FullPath => System.IO.Path.Combine(this.FullOnlyPath, this.FileName);
        public override IEnumerable<FileReferenceInfo> FileReferenceInfos => throw new System.NotImplementedException();
        public override string LanguageType => throw new System.NotImplementedException();

        public ErrorProjectTreeNodeModel(string path, string projName)
            : base(new ProjectData(path, projName, ProjectKinds.Unknown))
        {
            this.IsEditable = false;
        }

        public override void LoadElement()
        {
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
        }

        public override void Save()
        {
        }

        public override void SyncWithCurrentValue()
        {
        }

        public override void SyncWithLoadValue()
        {
        }

        public override ProjectProperty GetProjectProperty(ProjectProperty.Configure configure)
        {
            throw new System.NotImplementedException();
        }
    }
}
