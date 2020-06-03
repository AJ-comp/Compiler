using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using Parse.FrontEnd.Grammars.MiniC.Sdts;
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
            if (nodeInfo.CategoryVisible)
            {
                var astNonTerminal = nodeInfo.Node as AstNonTerminal;

                var model = FindMatchedModel(astNonTerminal[0], allNode);
                if (model != null) model.CategoryVisible = true;
            }
        }

        private static void CompoundStCategoryConf(UCodeDisplayModel nodeInfo, IReadOnlyList<UCodeDisplayModel> allNode)
        {
            if (nodeInfo.CategoryVisible)
            {
                var astNonTerminal = nodeInfo.Node as AstNonTerminal;

                var model = FindMatchedModel(astNonTerminal[0], allNode);
                if (model != null) model.CategoryVisible = true;

                model = FindMatchedModel(astNonTerminal[1], allNode);
                if (model != null) model.CategoryVisible = true;
            }
        }

        private static void IfStCategoryConf(UCodeDisplayModel nodeInfo, IReadOnlyList<UCodeDisplayModel> allNode)
        {
            var astNonTerminal = nodeInfo.Node as AstNonTerminal;

            nodeInfo.CategoryVisible = true;
            nodeInfo.CategoryName = "if ( " + (astNonTerminal[1] as AstNonTerminal).ConnectedParseTree.AllInputDatas + " )";

            var model = FindMatchedModel(astNonTerminal[2], allNode);
            if (model != null) model.CategoryVisible = true;
        }

        private static void WhileStCategoryConf(UCodeDisplayModel nodeInfo, IReadOnlyList<UCodeDisplayModel> allNode)
        {
            var astNonTerminal = nodeInfo.Node as AstNonTerminal;

            nodeInfo.CategoryVisible = true;
            nodeInfo.CategoryName = "while ( " + (astNonTerminal[1] as AstNonTerminal).ConnectedParseTree.AllInputDatas + " )";

            var model = FindMatchedModel(astNonTerminal[2], allNode);
            if (model != null) model.CategoryVisible = true;
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
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.ExpSt) ExpStCategoryConf(nodeInfo, allNode);
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.CompoundSt) CompoundStCategoryConf(nodeInfo, allNode);
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.IfSt) IfStCategoryConf(nodeInfo, allNode);
                else if (astNonTerminal.SignPost.MeaningUnit == sdts.WhileSt) WhileStCategoryConf(nodeInfo, allNode);
            }

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

        public UCodeDisplayModel(AstSymbol node)
        {
            Node = node;
        }

        public override string ToString()
        {
            string result = string.Empty;

            if(Node is AstTerminal)
            {
                var ast = Node as AstTerminal;
                result = ast.Token.Input;
            }
            else if(Node is AstNonTerminal)
            {
                var ast = Node as AstNonTerminal;
                result = ast.SignPost.Name;
            }

            return result;
        }
    }
}
