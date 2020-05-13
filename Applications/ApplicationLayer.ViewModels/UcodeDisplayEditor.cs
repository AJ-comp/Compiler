using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Grammars.MiniC.Sdts;
using System;
using System.Collections.Generic;

namespace ApplicationLayer.ViewModels
{
    public static class UcodeDisplayEditor
    {
        public static IReadOnlyList<UCodeDisplayModel> Edit(IReadOnlyList<TreeNonTerminal> nodes, Grammar grammar)
        {
            if (grammar is null) return null;

            if (grammar.GetType() == typeof(MiniCGrammar))
            {
                return Edit(nodes, grammar.SDTS as MiniCSdts);
            }
            else return null;
        }

        private static IReadOnlyList<UCodeDisplayModel> Edit(IReadOnlyList<TreeNonTerminal> nodes, MiniCSdts sdts)
        {
            if (nodes is null) return null;
            if (sdts is null) return null;

            List<UCodeDisplayModel> result = new List<UCodeDisplayModel>();
            foreach (var node in nodes)
                result.Add(new UCodeDisplayModel(node));

            UCodeDisplayModel prevNode = null;
            foreach(var nodeInfo in result)
            {
                if (nodeInfo.Node._signPost.MeaningUnit == sdts.ParamDcl) nodeInfo.CategoryVisible = false;

                prevNode = nodeInfo;
            }

            return result;
        }
    }

    public class UCodeDisplayModel
    {
        public TreeNonTerminal Node { get; }
        public bool CategoryVisible { get; set; } = true;

        public UCodeDisplayModel(TreeNonTerminal node)
        {
            Node = node;
        }
    }
}
