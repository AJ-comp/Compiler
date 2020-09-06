using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class TreeNodeModelLogics
    {
        public static IEnumerable<FileTreeNodeModel> GetAllFileNodes(TreeNodeModel model)
        {
            List<FileTreeNodeModel> result = new List<FileTreeNodeModel>();

            foreach (var item in model.Children)
            {
                if (item is FileTreeNodeModel) result.Add(item as FileTreeNodeModel);
                if (item is IHasableFileNodes) result.AddRange((item as IHasableFileNodes).AllFileNodes);
            }

            return result;
        }
    }
}
