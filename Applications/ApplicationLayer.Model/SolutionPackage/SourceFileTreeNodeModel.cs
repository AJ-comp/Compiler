using Parse.FrontEnd;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class SourceFileTreeNodeModel : FileTreeNodeModel
    {
        public SdtsNode Ast { get; set; }

        public SourceFileTreeNodeModel(string path, string filename) : base(path, filename)
        {
        }
    }
}
