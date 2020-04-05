namespace ApplicationLayer.Models.SolutionPackage
{
    public class FolderTreeNodeModel : PathTreeNodeModel
    {
        public override string DisplayName => FileName;

        public override string FullOnlyPath => System.IO.Path.Combine(Parent.FullOnlyPath, Path);

        public FolderTreeNodeModel(string path, string folderName) : base(path, folderName)
        {
        }
    }
}
