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
        public static IReadOnlyList<UCodeDisplayModel> Convert(IReadOnlyList<AstSymbol> nodes, Grammar grammar)
        {
            if (grammar is null) return null;

            if (grammar.GetType() == typeof(MiniCGrammar))
            {
                return Convert(nodes, grammar.SDTS as MiniCSdts);
            }
            else return null;
        }

        private static IReadOnlyList<UCodeDisplayModel> Convert(IReadOnlyList<AstSymbol> nodes, MiniCSdts sdts)
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
                if (nodeInfo.Node is AstTerminal)
                {
                    nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                    continue;
                }

                var astNonTerminal = nodeInfo.Node as AstNonTerminal;
                if (astNonTerminal.SignPost.MeaningUnit == sdts.ParamDcl) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Up;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.Add) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.Sub) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.Mul) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.Div) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.Mod) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.Equal) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.NotEqual) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.LogicalNot) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.LogicalAnd) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.LogicalOr) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.GreaterThan) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.GreaterEqual) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.LessThan) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.LessEqual) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.PreInc) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.PreDec) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.PostInc) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.PostDec) nodeInfo.CategoryPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.IfSt)
                    nodeInfo.CategoryName = "if ( " + (astNonTerminal[1] as AstNonTerminal).ConnectedParseTree.AllInputDatas + ")";

                prevNode = nodeInfo;
            }

            return result;
        }
    }

    public class UCodeDisplayModel
    {
        public enum AttatchCategoryPosition { Up, Down, Own };

        public AstSymbol Node { get; }
        public AttatchCategoryPosition CategoryPos { get; set; } = AttatchCategoryPosition.Own;
        public string CategoryName { get; set; } = string.Empty;

        public UCodeDisplayModel(AstSymbol node)
        {
            Node = node;
        }
    }
}
