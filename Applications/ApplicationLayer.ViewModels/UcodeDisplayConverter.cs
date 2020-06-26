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
        private static UCodeDisplayModel FindMatchedModel(AstSymbol nodeToFind, IReadOnlyList<UCodeDisplayModel> modelList)
        {
            UCodeDisplayModel result = null;

            foreach (var model in modelList)
            {
                if (model.Node.Equals(nodeToFind))
                {
                    result = model;
                    break;
                }
            }

            return result;
        }

        private static void ConfCategoryVisible(AstSymbol astSymbol, IReadOnlyList<UCodeDisplayModel> allNode)
        {
            var model = FindMatchedModel(astSymbol, allNode);
            if (model != null)
            {
                model.CategoryVisible = true;
                model.CategoryConfAction?.Invoke(model, allNode);
            }
        }

        /// <summary>
        /// This function removes dummy nodes
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns>returns nodes dummy node removed</returns>
        private static IReadOnlyList<UCodeDisplayModel> RemoveUselessNodes(IReadOnlyList<UCodeDisplayModel> nodes)
        {
            List<UCodeDisplayModel> result = new List<UCodeDisplayModel>();

            foreach (var node in nodes)
            {
                if (node.Node.IsDummy) continue;

                result.Add(node);
            }

            return result;
        }

        private static void ExpStCategoryConf(UCodeDisplayModel nodeInfo, IReadOnlyList<UCodeDisplayModel> allNode)
        {
            if (nodeInfo.CategoryVisible == false) return;

            var astNonTerminal = nodeInfo.Node as AstNonTerminal;

            ConfCategoryVisible(astNonTerminal[0], allNode);
        }

        private static void AllChangeableCategoryConf(UCodeDisplayModel nodeInfo, IReadOnlyList<UCodeDisplayModel> allNode)
        {
            if (nodeInfo.CategoryVisible == false) return;

            var astNonTerminal = nodeInfo.Node as AstNonTerminal;

            foreach (var item in astNonTerminal) ConfCategoryVisible(item, allNode);
        }

        private static void CompoundStCategoryConf(UCodeDisplayModel nodeInfo, IReadOnlyList<UCodeDisplayModel> allNode)
        {
            if (nodeInfo.CategoryVisible == false) return;

            var astNonTerminal = nodeInfo.Node as AstNonTerminal;

            ConfCategoryVisible(astNonTerminal[0], allNode);
            ConfCategoryVisible(astNonTerminal[1], allNode);
        }

        private static void IfStCategoryConf(UCodeDisplayModel nodeInfo, IReadOnlyList<UCodeDisplayModel> allNode)
        {
            var astNonTerminal = nodeInfo.Node as AstNonTerminal;

            nodeInfo.CategoryVisible = true;
            nodeInfo.CategoryName = "if ( " + (astNonTerminal[1] as AstNonTerminal).ConnectedParseTree.AllInputDatas + " )";

            ConfCategoryVisible(astNonTerminal[2], allNode);
        }

        private static void WhileStCategoryConf(UCodeDisplayModel nodeInfo, IReadOnlyList<UCodeDisplayModel> allNode)
        {
            var astNonTerminal = nodeInfo.Node as AstNonTerminal;

            nodeInfo.CategoryVisible = true;
            nodeInfo.CategoryName = "while ( " + (astNonTerminal[1] as AstNonTerminal).ConnectedParseTree.AllInputDatas + " )";

            ConfCategoryVisible(astNonTerminal[2], allNode);
        }

        private static IReadOnlyList<UCodeDisplayModel> Convert(IReadOnlyList<AstSymbol> nodes, MiniCSdts sdts)
        {
            if (nodes is null) return null;
            if (sdts is null) return null;

            List<UCodeDisplayModel> allNode = new List<UCodeDisplayModel>();
            foreach (var node in nodes)
            {
                if (node == null) continue;
                allNode.Add(new UCodeDisplayModel(node));
            }

            foreach(var nodeInfo in allNode)
            {
                if (nodeInfo.Node is AstTerminal)
                {
                    nodeInfo.CategoryAttachPos = UCodeDisplayModel.AttatchCategoryPosition.Down;
                    continue;
                }

                var astNonTerminal = nodeInfo.Node as AstNonTerminal;
                if (astNonTerminal.SignPost.MeaningUnit == sdts.FuncHead) nodeInfo.CategoryVisible = true;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.Dcl) nodeInfo.CategoryVisible = true;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.ParamDcl) nodeInfo.CategoryAttachPos = UCodeDisplayModel.AttatchCategoryPosition.Up;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.DclList) nodeInfo.CategoryConfAction = AllChangeableCategoryConf;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.StatList) nodeInfo.CategoryConfAction = AllChangeableCategoryConf;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.ExpSt) nodeInfo.CategoryConfAction = ExpStCategoryConf;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.CompoundSt) nodeInfo.CategoryConfAction = CompoundStCategoryConf;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.IfSt) nodeInfo.CategoryConfAction = IfStCategoryConf;
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.WhileSt) nodeInfo.CategoryConfAction = WhileStCategoryConf;
            }

            foreach (var nodeInfo in allNode)
                nodeInfo.CategoryConfAction?.Invoke(nodeInfo, allNode);

            return RemoveUselessNodes(allNode);
        }

        public static IReadOnlyList<UCodeDisplayModel> Convert(IReadOnlyList<AstSymbol> nodes, Grammar grammar)
        {
            if (grammar is null) return null;

            if (grammar.GetType() == typeof(MiniCGrammar))
            {
                return Convert(nodes, grammar.SDTS as MiniCSdts);
            }
            else return null;
        }
    }

    public class UCodeDisplayModel
    {
        public enum AttatchCategoryPosition { Up, Down };

        public AstSymbol Node { get; }
        public AttatchCategoryPosition CategoryAttachPos { get; set; } = AttatchCategoryPosition.Down;
        public bool CategoryVisible { get; set; } = false;
        public string CategoryName { get; set; } = string.Empty;

        public Action<UCodeDisplayModel, IReadOnlyList<UCodeDisplayModel>> CategoryConfAction { get; set; }

        public UCodeDisplayModel(AstSymbol node)
        {
            Node = node;
        }

        public override string ToString()
        {
            string result0 = string.Empty;
            string format = "{0}, {1}";

            if(Node is AstTerminal)
            {
                var ast = Node as AstTerminal;
                result0 = ast.Token.Input;
            }
            else if(Node is AstNonTerminal)
            {
                var ast = Node as AstNonTerminal;
                result0 = ast.SignPost.Name;
            }

            var result1 = CategoryVisible;

            return string.Format(format, result0, result1);
        }
    }
}
