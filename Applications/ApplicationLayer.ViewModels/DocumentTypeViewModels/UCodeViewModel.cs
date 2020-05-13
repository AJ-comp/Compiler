using ApplicationLayer.Models;
using ApplicationLayer.Models.SolutionPackage;
using Parse.FrontEnd.InterLanguages;
using System.Collections.Generic;

namespace ApplicationLayer.ViewModels.DocumentTypeViewModels
{
    public class UCodeViewModel : DocumentViewModel
    {
        public CategoryTreeNodeModel Root { get; }

        public UCodeViewModel(IReadOnlyList<UCodeDisplayModel> trees, string title) : base(title)
        {
            if (trees is null) return;

            Root = new CategoryTreeNodeModel("root");
            var parentNode = Root;
            foreach(var nodeInfo in trees)
            {
                var node = nodeInfo.Node;

                if(nodeInfo.CategoryVisible)
                {
                    parentNode = new CategoryTreeNodeModel(node.AllInputDatas);
                    Root.AddChildren(parentNode);
                }

                foreach (var statement in node.ConnectedInterLanguage)
                {
                    var ucodeNode = new UCodeTreeNodeModel();

                    var format = statement as UCode.Format;
                    ucodeNode.Label = format.Label;
                    ucodeNode.OpCode = format.OpCode;
                    ucodeNode.Comment = format.Comment;

                    ucodeNode.Operand1 = (format.Operands.Count > 0) ? format.Operands[0] : string.Empty;
                    ucodeNode.Operand2 = (format.Operands.Count > 1) ? format.Operands[1] : string.Empty;
                    ucodeNode.Operand3 = (format.Operands.Count > 2) ? format.Operands[2] : string.Empty;

                    parentNode.AddChildren(ucodeNode);
                }
            }
        }
    }
}
