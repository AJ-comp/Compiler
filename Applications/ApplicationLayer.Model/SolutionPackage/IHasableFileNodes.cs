using System.Collections.Generic;

namespace ApplicationLayer.Models.SolutionPackage
{
    public interface IHasableFileNodes
    {
        IEnumerable<FileTreeNodeModel> AllFileNodes { get; }
    }
}
