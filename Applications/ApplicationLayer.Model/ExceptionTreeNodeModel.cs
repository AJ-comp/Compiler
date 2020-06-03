using ApplicationLayer.Common;
using ApplicationLayer.Models.SolutionPackage;
using System;

namespace ApplicationLayer.Models
{
    public class ExceptionTreeNodeModel : TreeNodeModel
    {
        public override string DisplayName { get; set; }

        public override string FullOnlyPath => throw new NotImplementedException();

        public override event EventHandler<FileChangedEventArgs> Changed;

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
            throw new NotImplementedException();
        }
    }
}
