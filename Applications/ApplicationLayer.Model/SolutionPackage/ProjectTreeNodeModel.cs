namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class ProjectTreeNodeModel : PathTreeNodeModel
    {
        public abstract string ProjectType { get; }
        public override string DisplayName => System.IO.Path.GetFileNameWithoutExtension(this.FileName);
        public override string FullOnlyPath => System.IO.Path.Combine(Parent.FullOnlyPath, Path);

        public ProjectTreeNodeModel() : base(string.Empty, string.Empty)
        { }

        public ProjectTreeNodeModel(string path, string projName) : base(path, projName)
        {
        }

        public abstract void LoadElement();
    }
}
