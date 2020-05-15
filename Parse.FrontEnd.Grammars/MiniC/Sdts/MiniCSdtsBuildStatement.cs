using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        private NodeBuildResult BuildCompoundStNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            var newSymbolTable = new MiniCSymbolTable(baseSymbolTable);
            curNode.ClearConnectedInfo();

            foreach (var item in curNode.Items)
            {
                // ident
                if (item is AstTerminal) continue;

                var astNonterminal = item as AstNonTerminal;
                if (astNonterminal.SignPost.MeaningUnit == this.DclList)
                {
                    var nodeCheckResult = this.BuildDclListNode(astNonterminal, baseSymbolTable, blockLevel, offset);
                    newSymbolTable = nodeCheckResult.symbolTable as MiniCSymbolTable;
                }
                else if (astNonterminal.SignPost.MeaningUnit == this.StatList)
                {
                    var nodeCheckResult = this.BuildStatListNode(astNonterminal, newSymbolTable, blockLevel, offset);
                }
            }

            curNode.ConnectedSymbolTable = newSymbolTable;

            return new NodeBuildResult(null, newSymbolTable);
        }

        // format summary
        // IfSt | IfElseSt | WhileSt | ExpSt
        private NodeBuildResult BuildStatListNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();

            foreach (var item in curNode.Items)
            {
                if (item is AstTerminal) continue;

                var astNonTerminal = item as AstNonTerminal;
                if (astNonTerminal.SignPost.MeaningUnit == this.IfSt)
                    this.BuildIfStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal.SignPost.MeaningUnit == this.IfElseSt)
                    this.BuildIfElseStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal.SignPost.MeaningUnit == this.WhileSt)
                    this.BuildWhileStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal.SignPost.MeaningUnit == this.ExpSt)
                    this.BuildExpStNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
            }

            return new NodeBuildResult(null, baseSymbolTable);
        }

        // format summary
        // (AddAssign | SubAssign | MulAssign | DivAssign) ;
        private NodeBuildResult BuildExpStNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();

            foreach (var item in curNode.Items)
            {
                if (item is AstTerminal) continue;

                var astNonTerminal = item as AstNonTerminal;
                if (astNonTerminal.SignPost.MeaningUnit == this.Assign)
                    this.BuildAssignNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal.SignPost.MeaningUnit == this.AddAssign)
                    this.BuildAddAssignNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal.SignPost.MeaningUnit == this.SubAssign)
                    this.BuildSubAssignNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal.SignPost.MeaningUnit == this.MulAssign)
                    this.BuildMulAssignNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal.SignPost.MeaningUnit == this.DivAssign)
                    this.BuildDivAssignNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal.SignPost.MeaningUnit == this.ModAssign)
                    this.BuildModAssignNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal.SignPost.MeaningUnit == this.Add)
                    this.BuildAddNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal.SignPost.MeaningUnit == this.Sub)
                    this.BuildSubNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal.SignPost.MeaningUnit == this.Mul)
                    this.BuildMulNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal.SignPost.MeaningUnit == this.Div)
                    this.BuildDivNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
                else if (astNonTerminal.SignPost.MeaningUnit == this.Mod)
                    this.BuildModNode(astNonTerminal, baseSymbolTable, blockLevel, offset);

            }

            return new NodeBuildResult(null, baseSymbolTable);
        }

        private NodeBuildResult BuildIfStNode(AstNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }

        private NodeBuildResult BuildIfElseStNode(AstNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }

        private NodeBuildResult BuildWhileStNode(AstNonTerminal node, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }
    }
}
