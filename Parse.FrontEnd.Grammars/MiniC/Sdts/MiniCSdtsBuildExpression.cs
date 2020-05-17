using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.InterLanguages;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        private enum CalculateKind { None, Add, Sub, Mul, Div, Mod };
        private enum LogicalKind { None, Not, Or, And, EQ, NotEQ, GT, LT, GE, LE };

        // format summary
        // ( ident | (ident) ) = (ident | digit | Add | Sub | Mul | Div | Mod)
        private NodeBuildResult BuildAssignNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = baseSymbolTable;
            var result = new NodeBuildResult(null, baseSymbolTable);

            // leftExp is always TreeTerminal so it has to only check rightExp.
            if (curNode[2] is AstNonTerminal)
            {
                if(ConnectSimpleVarCode(curNode, 0))
                {
                    curNode.ConnectedInterLanguage.Clear();
                    var varData = baseSymbolTable.AllVarList.GetVarByName((curNode[0] as AstTerminal).Token.Input);
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Store(ReservedLabel, varData.DclData.BlockLevel, 
                                                                                                                varData.Offset, varData.DclData.DclItemData.Name));
                }

                var astNonTerminal = curNode[2] as AstNonTerminal;
                if(astNonTerminal.SignPost.MeaningUnit == this.Add)
                    result = BuildAddNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
            }
            else
                result = BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, CalculateKind.None, true);

            return result;
        }

        // format summary
        // ident += (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildAddAssignNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, CalculateKind.Add, true);

        // format summary
        // ident -= (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildSubAssignNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, CalculateKind.Sub, true);

        // format summary
        // ident *= (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildMulAssignNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, CalculateKind.Mul, true);

        // format summary
        // ident /= (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildDivAssignNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, CalculateKind.Div, true);

        // format summary
        // ident %= (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildModAssignNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, CalculateKind.Mod, true);

        private NodeBuildResult BuildAddNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, CalculateKind.Add);

        // format summary
        // (ident | digit) - (ident | digit)
        private NodeBuildResult BuildSubNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, CalculateKind.Sub);

        // format summary
        // (ident | digit) * (ident | digit)
        private NodeBuildResult BuildMulNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, CalculateKind.Mul);

        // format summary
        // (ident | digit) / (ident | digit)
        private NodeBuildResult BuildDivNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, CalculateKind.Div);

        // format summary
        // (ident | digit) % (ident | digit)
        private NodeBuildResult BuildModNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, CalculateKind.Mod);

        #region Build function related to Logical expression
        private NodeBuildResult BuildLogicalOr(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonLogicalNode(curNode, baseSymbolTable, blockLevel, offset, LogicalKind.Or);

        private NodeBuildResult BuildLogicalAnd(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonLogicalNode(curNode, baseSymbolTable, blockLevel, offset, LogicalKind.And);

        private NodeBuildResult BuildLogicalNot(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonLogicalNode(curNode, baseSymbolTable, blockLevel, offset, LogicalKind.Not);

        private NodeBuildResult BuildEqual(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonLogicalNode(curNode, baseSymbolTable, blockLevel, offset, LogicalKind.EQ);

        private NodeBuildResult BuildNotEqual(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonLogicalNode(curNode, baseSymbolTable, blockLevel, offset, LogicalKind.NotEQ);

        #endregion
    }
}
