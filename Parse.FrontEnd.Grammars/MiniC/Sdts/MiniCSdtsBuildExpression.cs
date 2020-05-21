using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.InterLanguages;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        private AstBuildResult BuildCommonAssign(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = baseSymbolTable;
            var result = new AstBuildResult(null, baseSymbolTable);

            // leftExp is always TreeTerminal so it has to only check rightExp.
            if (curNode[2] is AstNonTerminal)
                result = BuildExpressionNode((curNode[2] as AstNonTerminal), baseSymbolTable, blockLevel, offset);
            else
                BuildHalfExpression(curNode[2], baseSymbolTable, blockLevel, offset);

            // leftExp is always TreeTerminal
            if (ConnectSimpleVarCode(curNode[0] as AstTerminal))
            {
                var varData = (baseSymbolTable as MiniCSymbolTable).AllVarList.GetVarByName((curNode[0] as AstTerminal).Token.Input);

                if (curNode.SignPost.MeaningUnit == this.AddAssign)
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Add(ReservedLabel));
                else if (curNode.SignPost.MeaningUnit == this.SubAssign)
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Sub(ReservedLabel));
                else if (curNode.SignPost.MeaningUnit == this.MulAssign)
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Multiple(ReservedLabel));
                else if (curNode.SignPost.MeaningUnit == this.DivAssign)
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Div(ReservedLabel));
                else if (curNode.SignPost.MeaningUnit == this.ModAssign)
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Mod(ReservedLabel));

                curNode.ConnectedInterLanguage.Add(UCode.Command.Store(ReservedLabel, varData.DclData.BlockLevel,
                                                                                                            varData.Offset, varData.DclData.DclItemData.Name));
            }

            return result;
        }

        private AstBuildResult BuildAssign(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonAssign(curNode, baseSymbolTable, blockLevel, offset);

        private AstBuildResult BuildAddAssign(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonAssign(curNode, baseSymbolTable, blockLevel, offset);

        private AstBuildResult BuildSubAssign(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonAssign(curNode, baseSymbolTable, blockLevel, offset);

        private AstBuildResult BuildMulAssign(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonAssign(curNode, baseSymbolTable, blockLevel, offset);

        private AstBuildResult BuildDivAssign(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonAssign(curNode, baseSymbolTable, blockLevel, offset);

        private AstBuildResult BuildModAssign(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonAssign(curNode, baseSymbolTable, blockLevel, offset);

        private AstBuildResult BuildAdd(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildExpressionNode(curNode, baseSymbolTable, blockLevel, offset);

        private AstBuildResult BuildSub(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildExpressionNode(curNode, baseSymbolTable, blockLevel, offset);

        private AstBuildResult BuildMul(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildExpressionNode(curNode, baseSymbolTable, blockLevel, offset);

        private AstBuildResult BuildDiv(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildExpressionNode(curNode, baseSymbolTable, blockLevel, offset);

        private AstBuildResult BuildMod(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildExpressionNode(curNode, baseSymbolTable, blockLevel, offset);
    }
}
