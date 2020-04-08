using ApplicationLayer.Common.Interfaces;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class FolderTreeNodeModel : PathTreeNodeModel
    {
        public IManagableElements ManagerTree
        {
            get
            {
                TreeNodeModel current = this;

                while (true)
                {
                    if (current == null) return null;
                    else if (current is ProjectTreeNodeModel) return current as ProjectTreeNodeModel;

                    current = current.Parent;
                }
            }
        }

        public override string DisplayName => FileName;

        public override string FullOnlyPath => System.IO.Path.Combine(Parent.FullOnlyPath, Path);

        public FolderTreeNodeModel(string path, string folderName) : base(path, folderName)
        {
        }
        public void Save() { }

        public override void RemoveChild(TreeNodeModel nodeToRemove) { }
    }
}
