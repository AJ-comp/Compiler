using ApplicationLayer.Models;
using ApplicationLayer.Models.SolutionPackage;
using Parse.FrontEnd.InterLanguages;
using System.Collections.Generic;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class UCodeViewModel : DocumentViewModel
    {
        public CategoryTreeNodeModel Root { get; }

        private UCodeTreeNodeModel CreateUcodeTreeNodeModel(UCode.Format data)
        {
            var ucodeNode = new UCodeTreeNodeModel();

            ucodeNode.Label = data.Label;
            ucodeNode.OpCode = data.OpCode;
            ucodeNode.Comment = data.Comment;

            ucodeNode.Operand1 = (data.Operands.Count > 0) ? data.Operands[0] : string.Empty;
            ucodeNode.Operand2 = (data.Operands.Count > 1) ? data.Operands[1] : string.Empty;
            ucodeNode.Operand3 = (data.Operands.Count > 2) ? data.Operands[2] : string.Empty;

            return ucodeNode;
        }

        public UCodeViewModel(IReadOnlyList<UCodeDisplayModel> trees, string title) : base(title)
        {
            if (trees is null) return;

            List<object> leftUnDoneList = new List<object>();
            Root = new CategoryTreeNodeModel("root");
            var parentNode = Root;

            for (int i = 0; i < trees.Count; i++)
            {
                var nodeInfo = trees[i];
                var node = nodeInfo.Node;

                if(nodeInfo.CategoryPos == UCodeDisplayModel.AttatchCategoryPosition.Own)
                {
                    var data = (string.IsNullOrEmpty(nodeInfo.CategoryName))
                                ? node.ConnectedParseTree?.AllInputDatas : nodeInfo.CategoryName;

                    parentNode = new CategoryTreeNodeModel(data);
                    Root.AddChildren(parentNode);
                }
                else if(nodeInfo.CategoryPos == UCodeDisplayModel.AttatchCategoryPosition.Down)
                {
                    // if current node is not a last node
                    if(i != trees.Count - 1)
                    {
                        leftUnDoneList.AddRange(node.ConnectedInterLanguage);
                        continue;
                    }
                }

                // add a leftundone list
                foreach (var statement in leftUnDoneList)
                    parentNode.AddChildren(CreateUcodeTreeNodeModel(statement as UCode.Format));
                leftUnDoneList.Clear();

                // add a list on current node
                foreach (var statement in node.ConnectedInterLanguage)
                    parentNode.AddChildren(CreateUcodeTreeNodeModel(statement as UCode.Format));
            }
        }
    }
}
