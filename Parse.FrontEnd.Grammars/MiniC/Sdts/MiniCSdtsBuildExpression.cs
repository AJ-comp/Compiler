using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat;
using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.Grammars.Properties;
using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        private enum Value { NotInitialized, UnKnown }

        private LiteralData CommonCalculateAssign(AstNonTerminal curNode, VarData left, LiteralData right)
        {
            var result = right.Clone() as LiteralData;

            if (curNode.SignPost.MeaningUnit == this.AddAssign)
            {
                result = left.Value.Add(right) as LiteralData;
                ConnectBinOpToNode(curNode, left, right, IROperation.Add);
            }
            else if (curNode.SignPost.MeaningUnit == this.SubAssign)
            {
                result = left.Value.Sub(right) as LiteralData;
                ConnectBinOpToNode(curNode, left, right, IROperation.Sub);
            }
            else if (curNode.SignPost.MeaningUnit == this.MulAssign)
            {
                result = left.Value.Mul(right) as LiteralData;
                ConnectBinOpToNode(curNode, left, right, IROperation.Mul);
            }
            else if (curNode.SignPost.MeaningUnit == this.DivAssign)
            {
                result = left.Value.Div(right) as LiteralData;
                ConnectBinOpToNode(curNode, left, right, IROperation.Div);
            }
            else if (curNode.SignPost.MeaningUnit == this.ModAssign)
            {
                result = left.Value.Mod(right) as LiteralData;
                ConnectBinOpToNode(curNode, left, right, IROperation.Mod);
            }

            return result;
        }

        // [0] : ExpSt (AstNonTerminal)
        // [1] : ExpSt (AstNonTerminal)
        private AstBuildResult BuildCommonAssign(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = p.SymbolTable;

            // semantic parsing for right exp
            var rhsResult = (curNode[1] as AstNonTerminal).BuildLogic(p, astNodes);

            // semantic parsing for left exp
            // if leftExp is AstNonTerminal it only be PreInc or PreDec or Index.
            var node0 = curNode[0] as AstNonTerminal;
            var node0MeaningUnit = node0.SignPost.MeaningUnit;

            List<MeaningUnit> canList = new List<MeaningUnit>()
            { this.PreInc, this.PreDec, this.Index, this.VariableNode };

            var lhsResult = node0.BuildLogic(p, astNodes);
            if (canList.Contains(node0MeaningUnit) == false)
                node0.ConnectedErrInfoList.Add(new MeaningErrInfo(node0, nameof(AlarmCodes.MCL0004), AlarmCodes.MCL0004));

            // semantic parsing for operator
            var left = lhsResult.Data as VarData;
            var right = (rhsResult.Data is VarData) ? (rhsResult.Data as VarData).Value : rhsResult.Data as LiteralData;
            TypeChecker.Check(curNode, left, right);

            var calResult = CommonCalculateAssign(curNode, left, right);

            left.Value = calResult;
            IROptions option = new IROptions(ReservedLabel);
            curNode.ConnectedIrUnit = IRBuilder.CreateAssign(option, left, right);
            astNodes.Add(curNode);

            return new AstBuildResult(null, null, true);
        }

        // [0] : ExpSt (AstNonTerminal)
        private AstBuildResult BuildCommonIncDec(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = p.SymbolTable;
            astNodes.Add(curNode);

            var result = (curNode[0] as AstNonTerminal).BuildLogic(p, astNodes);
            if(result.Data is LiteralData)
                curNode.ConnectedErrInfoList.Add(new MeaningErrInfo(curNode, nameof(AlarmCodes.MCL0008), AlarmCodes.MCL0008));
            else
            {
                var varData = (p.SymbolTable as MiniCSymbolTable).AllVarList.GetVarByName((result.Data as VarData).Name);

                var options = new IROptions(ReservedLabel);
                if ((varData is VirtualVarData))
                    return new AstBuildResult(result.Data, null, true);

                if (curNode.SignPost.MeaningUnit == this.PreInc)
                    curNode.ConnectedIrUnit = IRBuilder.CreatePreInc(options, varData);
                else if (curNode.SignPost.MeaningUnit == this.PreDec)
                    curNode.ConnectedIrUnit = IRBuilder.CreatePreDec(options, varData);
                else if (curNode.SignPost.MeaningUnit == this.PostInc)
                    curNode.ConnectedIrUnit = IRBuilder.CreatePostInc(options, varData);
                else if (curNode.SignPost.MeaningUnit == this.PostDec)
                    curNode.ConnectedIrUnit = IRBuilder.CreatePostDec(options, varData);
            }

            return new AstBuildResult(result.Data, null, true);
        }

        // format summary
        // [0] : VarNode (AstNonTerminal)
        // [1] : Exp (AstNonTerminal)
        private AstBuildResult BuildIndex(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = p.SymbolTable;

            var rightResult = (curNode[1] as AstNonTerminal).BuildLogic(p, astNodes);   // semantic parsing for index

            // ready to build
            var buildParams = p.Clone() as MiniCAstBuildParams;
            buildParams.BuildOption |= AstBuildOption.Reference;
            var leftResult = (curNode[0] as AstNonTerminal).BuildLogic(buildParams, astNodes); // semantic parsing for left exp

            var varData = leftResult.Data as VarData;
            var literalData = rightResult.Data as LiteralData;

            if ((literalData is IntLiteralData) == false) throw new Exception(AlarmCodes.MCL0006);

            var intLiteralData = literalData as IntLiteralData;
            if (varData.Length <= intLiteralData.RealValue)
            {
                int canByte = varData.Length;
                int factByte = intLiteralData.RealValue;
                curNode.ConnectedErrInfoList.Add(new MeaningErrInfo(curNode, nameof(AlarmCodes.MCL0007), string.Format(AlarmCodes.MCL0007, varData.Name, canByte, factByte)));
            }
            curNode.ConnectedIrUnit = IRBuilder.Add(ReservedLabel);
            curNode.ConnectedIrUnit = IRBuilder.Sti(ReservedLabel);

            // collect nodes
            astNodes.Add(curNode);

            return new AstBuildResult(varData, null, leftResult.Result);
        }

        private AstBuildResult BuildAssign(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildCommonAssign(curNode, p, astNodes);

        private AstBuildResult BuildAddAssign(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildCommonAssign(curNode, p, astNodes);

        private AstBuildResult BuildSubAssign(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildCommonAssign(curNode, p, astNodes);

        private AstBuildResult BuildMulAssign(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildCommonAssign(curNode, p, astNodes);

        private AstBuildResult BuildDivAssign(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildCommonAssign(curNode, p, astNodes);

        private AstBuildResult BuildModAssign(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildCommonAssign(curNode, p, astNodes);

        private AstBuildResult BuildAdd(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildOpNode(curNode, p, astNodes);

        private AstBuildResult BuildSub(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildOpNode(curNode, p, astNodes);

        private AstBuildResult BuildMul(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildOpNode(curNode, p, astNodes);

        private AstBuildResult BuildDiv(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildOpNode(curNode, p, astNodes);

        private AstBuildResult BuildMod(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildOpNode(curNode, p, astNodes);

        private AstBuildResult BuildPreInc(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildCommonIncDec(curNode, p, astNodes);

        private AstBuildResult BuildPreDec(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildCommonIncDec(curNode, p, astNodes);

        private AstBuildResult BuildPostInc(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildCommonIncDec(curNode, p, astNodes);

        private AstBuildResult BuildPostDec(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes) => BuildCommonIncDec(curNode, p, astNodes);
    }
}
