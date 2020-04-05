namespace ApplicationLayer.Models.SolutionPackage
{
    public class FileTreeNodeModel : PathTreeNodeModel
    {
        public override string DisplayName => FileName;

        public override string FullOnlyPath => Parent.FullOnlyPath;

        public FileTreeNodeModel(string path, string filename) : base(path, filename)
        {
        }
    }
}
