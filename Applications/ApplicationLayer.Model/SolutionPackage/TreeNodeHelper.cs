using ApplicationLayer.Common.Interfaces;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class TreeNodeHelper
    {
        public static IManagableElements GetNearestManager(TreeNodeModel model)
        {
            IManagableElements result = null;
            if (model is SolutionTreeNodeModel) result = model as IManagableElements;
            else if (model is ProjectTreeNodeModel) result = model as IManagableElements;
            else if (model is FolderTreeNodeModel)
            {
                var folderNode = model as FolderTreeNodeModel;
                result = folderNode.ManagerTree;
            }
            else if (model is FilterTreeNodeModel)
            {
                var filterNode = model as FilterTreeNodeModel;
                result = filterNode.ManagerTree;
            }

            return result;
        }
    }
}
