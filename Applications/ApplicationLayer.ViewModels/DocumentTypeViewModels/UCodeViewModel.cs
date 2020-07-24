using ApplicationLayer.Models;
using ApplicationLayer.Models.SolutionPackage;
using Parse.FrontEnd;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR;
using System.Collections.Generic;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class UCodeViewModel : DocumentViewModel
    {
        public CategoryTreeNodeModel Root { get; }

        private TreeNodeModel CreateTreeNodeModel(object data)
        {
            if (data is MeaningErrInfo)
            {
                var cData = (data as MeaningErrInfo).Message;
                return CreateTreeNodeModel(cData);
            }
            else
                return CreateTreeNodeModel(data as IRUnit);
        }

        private UCodeTreeNodeModel CreateTreeNodeModel(IRUnit data)
        {
            var result = new UCodeTreeNodeModel
            {
                Comment = data.Comment,

                Operand2 = data.ToString()
            };

            return result;
        }

        private ExceptionTreeNodeModel CreateTreeNodeModel(string exception) => new ExceptionTreeNodeModel() { DisplayName = exception };

        public UCodeViewModel(IReadOnlyList<UCodeDisplayModel> trees, string title) : base(title)
        {
            /*
            if (trees is null) return;

            List<object> leftUnDoneList = new List<object>();
            Root = new CategoryTreeNodeModel("root");
            var parentNode = Root;

            for (int i = 0; i < trees.Count; i++)
            {
                var nodeInfo = trees[i];
                var node = nodeInfo.Node;

                if (node is AstTerminal)
                {
                    if (node.ConnectedErrInfoList.Count > 0)
                        leftUnDoneList.AddRange(node.ConnectedErrInfoList);
                    else
                        leftUnDoneList.Add(node.ConnectedIrUnit);

                    continue;
                }

                var astNonTerminal = node as AstNonTerminal;

                if (nodeInfo.CategoryVisible == false)
                {
                    if(nodeInfo.CategoryAttachPos == UCodeDisplayModel.AttatchCategoryPosition.Down)
                        leftUnDoneList.Add(node.ConnectedIrUnit);
                    else
                    {
                        // add a IL of the current node to the parent node
                        IRBlock block = node.ConnectedIrUnit.Unit as IRBlock;
                        foreach (var il in block)
                            parentNode.AddChildren(CreateTreeNodeModel(il));
                    }

                }
                else
                {
                    var data = (string.IsNullOrEmpty(nodeInfo.CategoryName))
                        ? astNonTerminal.ConnectedParseTree?.AllInputDatas : nodeInfo.CategoryName;

                    parentNode = new CategoryTreeNodeModel(data);

                    // add a leftundone list
                    foreach (var statement in leftUnDoneList)
                        parentNode.AddChildren(CreateTreeNodeModel(statement));
                    leftUnDoneList.Clear();

                    // add a IL of the current node to the parent node
                    IRBlock block = node.ConnectedIrUnit.Unit as IRBlock;
                    foreach (var il in block)
                        parentNode.AddChildren(CreateTreeNodeModel(il));

                    Root.AddChildren(parentNode);
                }
            }

            // add a leftundone list
            foreach (var statement in leftUnDoneList)
                parentNode.AddChildren(CreateTreeNodeModel(statement));
            leftUnDoneList.Clear();
            */
        }
    }
}
