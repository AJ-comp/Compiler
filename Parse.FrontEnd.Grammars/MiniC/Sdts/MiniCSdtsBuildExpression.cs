using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.Grammars.Properties;
using Parse.FrontEnd.InterLanguages;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        private enum Value { NotInitialized, UnKnown }

        private AstBuildResult BuildCommonAssign(AstNonTerminal curNode, SymbolTable baseSymbolTable,
                                                                        int blockLevel, int offset, AstBuildOption buildOption)
        {
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = baseSymbolTable;
            var rhsResult = new AstBuildResult(null, baseSymbolTable);

            // semantic parsing for right exp
            rhsResult = (curNode[2] as AstNonTerminal).BuildLogic(baseSymbolTable, blockLevel, offset, buildOption);

            // semantic parsing for left exp
            // if leftExp is AstNonTerminal it only be PreInc or PreDec or Index.
            var lhsResult = new AstBuildResult(null, baseSymbolTable);
            var node0 = curNode[0] as AstNonTerminal;
            var node0MeaningUnit = node0.SignPost.MeaningUnit;

            if (node0MeaningUnit == this.PreInc || node0MeaningUnit == this.PreDec)
                lhsResult = BuildCommonIncDec(node0, baseSymbolTable, blockLevel, offset, buildOption);
            else if (node0MeaningUnit == this.Index)
                lhsResult = BuildIndex(node0, baseSymbolTable, blockLevel, offset);
            else
                node0.ConnectedErrInfoList.Add(new MeaningErrInfo(node0, AlarmCodes.MCL0004));

            TypeChecker.Check(curNode, lhsResult.Data as VarData, rhsResult.Data);

            // semantic parsing for operator
            var varData = lhsResult.Data as VarData;

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

            return lhsResult;
        }

        // [0] : (expression | ident | digit) (AstNonTerminal)
        private AstBuildResult BuildCommonIncDec(AstNonTerminal curNode, SymbolTable baseSymbolTable,
                                                                        int blockLevel, int offset, AstBuildOption buildOption)
        {
            VarData varData = null;
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = baseSymbolTable;

            try
            {
                (curNode[0] as AstNonTerminal).BuildLogic(baseSymbolTable, blockLevel, offset, buildOption);
                varData = (baseSymbolTable as MiniCSymbolTable).AllVarList.GetVarByName((curNode[0] as AstTerminal).Token.Input);

                if (curNode.SignPost.MeaningUnit == this.PreInc)
                {
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Increment(ReservedLabel));
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Store(ReservedLabel, varData.DclData.BlockLevel, varData.Offset));
                }
                else if (curNode.SignPost.MeaningUnit == this.PreDec)
                {
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Decrement(ReservedLabel));
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Store(ReservedLabel, varData.DclData.BlockLevel, varData.Offset));
                }
                else if (curNode.SignPost.MeaningUnit == this.PostInc)
                {
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Duplicate(ReservedLabel));
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Increment(ReservedLabel));
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Store(ReservedLabel, varData.DclData.BlockLevel, varData.Offset));
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Pop(ReservedLabel));
                }
                else if (curNode.SignPost.MeaningUnit == this.PostDec)
                {
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Duplicate(ReservedLabel));
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Decrement(ReservedLabel));
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Store(ReservedLabel, varData.DclData.BlockLevel, varData.Offset));
                    curNode.ConnectedInterLanguage.Add(UCode.Command.Pop(ReservedLabel));
                }

                return new AstBuildResult(varData, baseSymbolTable, true);
            }
            catch
            {
                curNode.ConnectedInterLanguage.Clear();
                return new AstBuildResult(null, baseSymbolTable);
            }
        }

        // format summary
        // [0] : VarNode (AstNonTerminal)
        // [1] : Exp (AstNonTerminal)
        private AstBuildResult BuildIndex(AstNonTerminal curNode, SymbolTable baseSymbolTable,
                                                        int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
        {
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = baseSymbolTable;

            try
            {
                var rightResult = (curNode[1] as AstNonTerminal).BuildLogic(baseSymbolTable, blockLevel, offset, buildOption);   // semantic parsing for index
                var leftResult = (curNode[0] as AstNonTerminal).BuildLogic(baseSymbolTable, blockLevel, offset, AstBuildOption.Reference); // semantic parsing for left exp

                var varData = leftResult.Data as VarData;
                var literalData = rightResult.Data as LiteralData;

                if ((literalData is IntLiteralData) == false) throw new Exception(AlarmCodes.MCL0006);

                var intLiteralData = literalData as IntLiteralData;
                if (varData.DclData.DclItemData.Dimension <= intLiteralData.Value)
                {
                    int canByte = varData.DclData.DclItemData.Dimension;
                    int factByte = intLiteralData.Value;
                    curNode.ConnectedErrInfoList.Add(new MeaningErrInfo(curNode, string.Format(AlarmCodes.MCL0007, varData.VarName, canByte, factByte)));
                }
                curNode.ConnectedInterLanguage.Add(UCode.Command.Add(ReservedLabel));
                curNode.ConnectedInterLanguage.Add(UCode.Command.Sti(ReservedLabel));

                return new AstBuildResult(varData, null, leftResult.Result);
            }
            catch(Exception ex)
            {
                curNode.ConnectedErrInfoList.Add(new MeaningErrInfo(curNode, ex.Message, ErrorType.Error));
                curNode.ConnectedInterLanguage.Clear();
                return new AstBuildResult(null, null);
            }
        }

        private AstBuildResult BuildAssign(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildCommonAssign(curNode, baseSymbolTable, blockLevel, offset, buildOption);

        private AstBuildResult BuildAddAssign(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildCommonAssign(curNode, baseSymbolTable, blockLevel, offset, buildOption);

        private AstBuildResult BuildSubAssign(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildCommonAssign(curNode, baseSymbolTable, blockLevel, offset, buildOption);

        private AstBuildResult BuildMulAssign(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildCommonAssign(curNode, baseSymbolTable, blockLevel, offset, buildOption);

        private AstBuildResult BuildDivAssign(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildCommonAssign(curNode, baseSymbolTable, blockLevel, offset, buildOption);

        private AstBuildResult BuildModAssign(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildCommonAssign(curNode, baseSymbolTable, blockLevel, offset, buildOption);

        private AstBuildResult BuildAdd(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildOpNode(curNode, baseSymbolTable, blockLevel, offset, buildOption);

        private AstBuildResult BuildSub(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildOpNode(curNode, baseSymbolTable, blockLevel, offset, buildOption);

        private AstBuildResult BuildMul(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildOpNode(curNode, baseSymbolTable, blockLevel, offset, buildOption);

        private AstBuildResult BuildDiv(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildOpNode(curNode, baseSymbolTable, blockLevel, offset, buildOption);

        private AstBuildResult BuildMod(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildOpNode(curNode, baseSymbolTable, blockLevel, offset, buildOption);

        private AstBuildResult BuildPreInc(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildCommonIncDec(curNode, baseSymbolTable, blockLevel, offset, buildOption);

        private AstBuildResult BuildPreDec(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildCommonIncDec(curNode, baseSymbolTable, blockLevel, offset, buildOption);

        private AstBuildResult BuildPostInc(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildCommonIncDec(curNode, baseSymbolTable, blockLevel, offset, buildOption);

        private AstBuildResult BuildPostDec(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset, AstBuildOption buildOption = AstBuildOption.None)
            => BuildCommonIncDec(curNode, baseSymbolTable, blockLevel, offset, buildOption);
    }
}
