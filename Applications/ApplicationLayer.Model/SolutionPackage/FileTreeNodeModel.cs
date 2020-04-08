namespace ApplicationLayer.Models.SolutionPackage
{
    public class FileTreeNodeModel : PathTreeNodeModel
    {
        public TreeNodeModel ManagerTree
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

        public override string FullOnlyPath => Parent.FullOnlyPath;

        public FileTreeNodeModel(string path, string filename) : base(path, filename)
        {
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove) { }
    }
}
