using ApplicationLayer.Common;
using System;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class CategoryTreeNodeModel : TreeNodeModel
    {
        public override string DisplayName { get; set; }

        public override string FullOnlyPath => throw new NotImplementedException();

        public override event EventHandler<FileChangedEventArgs> Changed;

        public CategoryTreeNodeModel(string categoryName)
        {
            DisplayName = categoryName;
        }

        public override void RemoveChild(TreeNodeModel nodeToRemove)
        {
        }
    }
}
