using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.InterLanguages;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        private enum ExpressionKind { None, Add, Sub, Mul, Div, Mod };

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
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Store(varData.DclData.BlockLevel, varData.Offset, varData.DclData.DclItemData.Name));
                }

                var astNonTerminal = curNode[2] as AstNonTerminal;
                if(astNonTerminal.SignPost.MeaningUnit == this.Add)
                    result = BuildAddNode(astNonTerminal, baseSymbolTable, blockLevel, offset);
            }
            else
                result = BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, ExpressionKind.None, true);

            return result;
        }

        // format summary
        // ident += (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildAddAssignNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, ExpressionKind.Add, true);

        // format summary
        // ident -= (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildSubAssignNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, ExpressionKind.Sub, true);

        // format summary
        // ident *= (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildMulAssignNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, ExpressionKind.Mul, true);

        // format summary
        // ident /= (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildDivAssignNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, ExpressionKind.Div, true);

        // format summary
        // ident %= (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildModAssignNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, ExpressionKind.Mod, true);

        /// <summary>
        /// This function builds an AddNode following the below process.
        /// 1. The type of the 0 index may be TreeNonTerminal or TreeTerminal.
        /// 2. The type of the 2 index may be TreeNonTerminal or TreeTerminal.
        /// 3. Token type of the TreeTerminal may be Identifier or Digit type.
        /// 4. If 0 or 2 index both is TreeTerminal type call BuildCommonCalculateNode function.
        /// 5. If 0 or 2 index is TreeNonTerminal type call BuildCalculateNode function.
        /// 6. If 0 or 2 index is TreeTerminal type call ConnectVarOrDigitUCode function.
        /// </summary>
        /// <param name="curNode"></param>
        /// <param name="baseSymbolTable"></param>
        /// <param name="blockLevel"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private NodeBuildResult BuildAddNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, ExpressionKind.Add);

        // format summary
        // (ident | digit) - (ident | digit)
        private NodeBuildResult BuildSubNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, ExpressionKind.Sub);

        // format summary
        // (ident | digit) * (ident | digit)
        private NodeBuildResult BuildMulNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, ExpressionKind.Mul);

        // format summary
        // (ident | digit) / (ident | digit)
        private NodeBuildResult BuildDivNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, ExpressionKind.Div);

        // format summary
        // (ident | digit) % (ident | digit)
        private NodeBuildResult BuildModNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, blockLevel, offset, ExpressionKind.Mod);
    }
}
