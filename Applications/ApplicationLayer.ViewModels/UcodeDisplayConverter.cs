using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Grammars.MiniC.Sdts;
using System;
using System.Collections.Generic;

namespace ApplicationLayer.ViewModels
{
    public static class UcodeDisplayConverter
    {
        public static IReadOnlyList<UCodeDisplayModel> Convert(IReadOnlyList<AstNonTerminal> nodes, Grammar grammar)
        {
            if (grammar is null) return null;

            if (grammar.GetType() == typeof(MiniCGrammar))
            {
                return Convert(nodes, grammar.SDTS as MiniCSdts);
            }
            else return null;
        }

        private static IReadOnlyList<UCodeDisplayModel> Convert(IReadOnlyList<AstNonTerminal> nodes, MiniCSdts sdts)
        {
            if (nodes is null) return null;
            if (sdts is null) return null;

            List<UCodeDisplayModel> result = new List<UCodeDisplayModel>();
            foreach (var node in nodes)
            {
                if (node == null) continue;
                result.Add(new UCodeDisplayModel(node));
            }

            UCodeDisplayModel prevNode = null;
            foreach(var nodeInfo in result)
            {
                if (nodeInfo.Node?.SignPost.MeaningUnit == sdts.ParamDcl) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Up;
                else if (nodeInfo.Node?.SignPost.MeaningUnit == sdts.Add) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (nodeInfo.Node?.SignPost.MeaningUnit == sdts.Equal) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (nodeInfo.Node?.SignPost.MeaningUnit == sdts.NotEqual) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (nodeInfo.Node?.SignPost.MeaningUnit == sdts.LogicalNot) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (nodeInfo.Node?.SignPost.MeaningUnit == sdts.LogicalAnd) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (nodeInfo.Node?.SignPost.MeaningUnit == sdts.LogicalOr) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (nodeInfo.Node?.SignPost.MeaningUnit == sdts.IfSt)
                    nodeInfo.CategoryName = "if ( " + (nodeInfo.Node[1] as AstNonTerminal).ConnectedParseTree.AllInputDatas + ")";

                prevNode = nodeInfo;
            }

            return result;
        }
    }

    public class UCodeDisplayModel
    {
        public enum AttatchCategoryPosition { Up, Down, Own };

        public AstNonTerminal Node { get; }
        public AttatchCategoryPosition CategoryPos { get; set; } = AttatchCategoryPosition.Own;
        public string CategoryName { get; set; } = string.Empty;

        public UCodeDisplayModel(AstNonTerminal node)
        {
            Node = node;
        }
    }
}
