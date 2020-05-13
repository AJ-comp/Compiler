using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        private NodeBuildResult BuildCompoundStNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            var newSymbolTable = new MiniCSymbolTable(baseSymbolTable);
            curNode.ClearConnectedInfo();
            if (curNode.HasVirtualChild) return new NodeBuildResult(null, newSymbolTable);

            foreach (var item in curNode.Items)
            {
                // ident
                if (item is TreeTerminal) continue;

                var astNonterminal = item as TreeNonTerminal;
                if (astNonterminal._signPost.MeaningUnit == this.DclList)
                {
                    var nodeCheckResult = this.BuildDclListNode(astNonterminal, baseSymbolTable, blockLevel, offset);
                    newSymbolTable = nodeCheckResult.symbolTable as MiniCSymbolTable;
                }
                else if (astNonterminal._signPost.MeaningUnit == this.StatList)
                {
                    var nodeCheckResult = this.BuildStatListNode(astNonterminal, newSymbolTable, blockLevel, offset);
                }
            }

            curNode.ConnectedSymbolTable = newSymbolTable;

            return new NodeBuildResult(null, newSymbolTable);
        }

        // format summary
        // IfSt | IfElseSt | WhileSt | ExpSt
        private NodeBuildResult BuildStatListNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            string result = string.Empty;
            curNode.ClearConnectedInfo();
            if (curNode.HasVirtualChild) return new NodeBuildResult(null, baseSymbolTable);

            foreach (var item in curNode.Items)
            {
                if (item is TreeTerminal) continue;

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.IfSt)
                    result += this.BuildIfStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.IfElseSt)
                    result += this.BuildIfElseStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.WhileSt)
                    result += this.BuildWhileStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.ExpSt)
                    result += this.BuildExpStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
            }

            return new NodeBuildResult(null, baseSymbolTable);
        }

        // format summary
        // (AddAssign | SubAssign | MulAssign | DivAssign) ;
        private NodeBuildResult BuildExpStNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            string result = string.Empty;
            curNode.ClearConnectedInfo();
            if (curNode.HasVirtualChild) return new NodeBuildResult(null, baseSymbolTable);

            foreach (var item in curNode.Items)
            {
                if (item is TreeTerminal) continue;

                var astNonTerminal = item as TreeNonTerminal;
                if (astNonTerminal._signPost.MeaningUnit == this.Assign)
                    result += this.BuildAssignNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.AddAssign)
                    result += this.BuildAddAssignNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.SubAssign)
                    result += this.BuildSubAssignNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.MulAssign)
                    result += this.BuildMulAssignNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.DivAssign)
                    result += this.BuildDivAssignNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.ModAssign)
                    result += this.BuildModAssignNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.Add)
                    result += this.BuildAddNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.Sub)
                    result += this.BuildSubNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.Mul)
                    result += this.BuildMulNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.Div)
                    result += this.BuildDivNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal._signPost.MeaningUnit == this.Mod)
                    result += this.BuildModNode(astNonTerminal, baseSymbolTable, blockLevel, offset);

            }

            return new NodeBuildResult(null, baseSymbolTable);
        }

        private NodeBuildResult BuildIfStNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }

        private NodeBuildResult BuildIfElseStNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }

        private NodeBuildResult BuildWhileStNode(TreeNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }
    }
}
