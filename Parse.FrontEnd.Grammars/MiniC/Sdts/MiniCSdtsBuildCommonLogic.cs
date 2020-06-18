using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat;
using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.Grammars.Properties;
using Parse.FrontEnd.InterLanguages;
using System.Collections.Generic;

using IR = Parse.FrontEnd.InterLanguages.Datas;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        /// <summary>
        /// This function checks whether variable exist in the 'baseSymbolTable'.
        /// </summary>
        /// <param name="childNodeToCheck">The child node to check</param>
        /// <param name="baseSymbolTable">The symbol table to reference</param>
        /// <returns></returns>
        private VarData CheckExistVarInSymbolTable(AstTerminal childNodeToCheck, SymbolTable baseSymbolTable)
        {
            VarData result = null;

            if (childNodeToCheck.Token.Kind.TokenType is Identifier)
                result = (baseSymbolTable as MiniCSymbolTable).AllVarList.GetVarByName(childNodeToCheck.Token.Input);

            return result;
        }

        private bool ConnectDclVarIRToNode(AstNonTerminal curNode, MiniCAstBuildParams p, DclData dclData)
        {
            bool result = false;

            var symbolTable = (p.SymbolTable as MiniCSymbolTable);
            if (symbolTable.VarDataList.GetVarByName(dclData.DclItemData.Name) != null)
            {
                curNode.ConnectedErrInfoList.Add(new MeaningErrInfo(curNode, nameof(AlarmCodes.MCL0009),
                                                string.Format(AlarmCodes.MCL0009, dclData.DclItemData.Name)));
            }
            else
            {
                IROptions option = new IROptions(ReservedLabel);
                IR.VarData irData = IRConverter.ToIRData(dclData);
                curNode.ConnectedIrUnits.Add(IRBuilder.CreateDclVar(option, irData, false));

                // add symbol information to the symbol table.
                RealVarData varData = new RealVarData(dclData);
                symbolTable.VarDataList.Add(varData);

                p.Offset++;

                result = true;
            }

            return result;
        }

        private bool ConnectBinOpToNode(AstNonTerminal curNode, VarData left, LiteralData right, IROperation operation)
        {
            bool result = false;

            if ((left is VirtualVarData) == false)
            {
                IROptions option = new IROptions(ReservedLabel);
                IR.VarData leftIR = IRConverter.ToIRData(left as RealVarData);
                IR.LiteralData rightIR = IRConverter.ToIRData(right);
                curNode.ConnectedIrUnits.Add(IRBuilder.CreateBinOP(option, leftIR, rightIR, operation));

                result = true;
            }

            return result;
        }

        /// <summary>
        /// This function connect an Ucode for declaring variable to the 'curNode'.
        /// If it can't perform then MeaningErrInfo is connected to the 'curNode'.
        /// percondition for operation.
        /// 1. symbol table has to be connected to the parent node of the 'curNode'.
        /// </summary>
        /// <param name="curNode"></param>
        /// <param name="varIndex"></param>
        /// <returns></returns>
        private VarData ConnectSimpleVarCode(AstTerminal curNode, AstBuildParams p)
        {
            curNode.ClearConnectedInfo();
            var symbolTable = curNode.Parent.ConnectedSymbolTable;
            var buildParams = p as MiniCAstBuildParams;

            VarData varData = CheckExistVarInSymbolTable(curNode, symbolTable as MiniCSymbolTable);
            if (varData == null)
            {
                curNode.ConnectedErrInfoList.Add(new MeaningErrInfo(curNode, nameof(AlarmCodes.MCL0001), string.Format(AlarmCodes.MCL0001, curNode.Token.Input)));
                varData = new VirtualVarData(curNode.Token, buildParams.BlockLevel, buildParams.Offset, 0);
                (curNode.Parent.ConnectedSymbolTable as MiniCSymbolTable).VarDataList.Add(varData);
            }
            else if(varData is RealVarData)
            {
                var realVarData = varData as RealVarData;
                bool bAddress = AstBuildOptionChecker.HasOption(buildParams.BuildOption, AstBuildOption.Reference);

                if (bAddress)
                    curNode.ConnectedIrUnits.Add(UCodeBuilder.Command.LoadVarAddress(ReservedLabel, realVarData.BlockLevel, realVarData.Offset, realVarData.VarName));
                else
                    curNode.ConnectedIrUnits.Add(UCodeBuilder.Command.LoadVarValue(ReservedLabel, realVarData.BlockLevel, realVarData.Offset, realVarData.VarName));
            }

            return varData;
        }

        private LiteralData BuildOperator(AstNonTerminal curNode, LiteralData left, LiteralData right)
        {
            curNode.ClearConnectedInfo();

            TypeChecker.Check(curNode, left, right);
            LiteralData result = null;

            var option = new IROptions(ReservedLabel);
            var leftIR = IRConverter.ToIRData(left);
            var rightIR = IRConverter.ToIRData(right);

            if (curNode.SignPost.MeaningUnit == this.Add)
            {
                result = left.Add(right);
                curNode.ConnectedIrUnits.Add(IRBuilder.CreateBinOP(option, leftIR, rightIR, IROperation.Add));
            }
            else if (curNode.SignPost.MeaningUnit == this.Sub)
            {
                result = left.Sub(right);
                curNode.ConnectedIrUnits.Add(IRBuilder.CreateBinOP(option, leftIR, rightIR, IROperation.Sub));
            }
            else if (curNode.SignPost.MeaningUnit == this.Mul)
            {
                result = left.Mul(right);
                curNode.ConnectedIrUnits.Add(IRBuilder.CreateBinOP(option, leftIR, rightIR, IROperation.Mul));
            }
            else if (curNode.SignPost.MeaningUnit == this.Div)
            {
                result = left.Div(right);
                curNode.ConnectedIrUnits.Add(IRBuilder.CreateBinOP(option, leftIR, rightIR, IROperation.Div));
            }
            else if (curNode.SignPost.MeaningUnit == this.Mod)
            {
                result = left.Mod(right);
                curNode.ConnectedIrUnits.Add(IRBuilder.CreateBinOP(option, leftIR, rightIR, IROperation.Mod));
            }
            else if (curNode.SignPost.MeaningUnit == this.LogicalNot)
                curNode.ConnectedIrUnits.Add(IRBuilder.Command.Not(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.LogicalAnd)
                curNode.ConnectedIrUnits.Add(UCodeBuilder.Command.And(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.LogicalOr)
                curNode.ConnectedIrUnits.Add(UCodeBuilder.Command.Or(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.Equal)
                curNode.ConnectedIrUnits.Add(UCodeBuilder.Command.Equal(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.NotEqual)
                curNode.ConnectedIrUnits.Add(UCodeBuilder.Command.NegativeEqual(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.GreaterThan)
                curNode.ConnectedIrUnits.Add(UCodeBuilder.Command.GreaterThan(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.GreaterEqual)
                curNode.ConnectedIrUnits.Add(UCodeBuilder.Command.GreaterEqual(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.LessThan)
                curNode.ConnectedIrUnits.Add(UCodeBuilder.Command.LessThan(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.LessEqual)
                curNode.ConnectedIrUnits.Add(UCodeBuilder.Command.LessEqual(ReservedLabel));

            return result;
        }

        private AstBuildResult BuildOpNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = p.SymbolTable;

            // build right ExpSt node
            var rightResult = (curNode[1] as AstNonTerminal).BuildLogic(p, astNodes);
            // build left ExpSt node
            var leftResult = (curNode[0] as AstNonTerminal).BuildLogic(p, astNodes);

            // set data
            var leftParam = (leftResult.Data is VarData) ? (leftResult.Data as VarData).Value : leftResult.Data as LiteralData;
            var rightParam = (rightResult.Data is VarData) ? (rightResult.Data as VarData).Value : rightResult.Data as LiteralData;

            // calculate to generate temporary data calculated.
            var literalData = BuildOperator(curNode, leftParam, rightParam);
            astNodes.Add(curNode);   // because of BuildOperator function add IE or ME.
            var result = (leftResult.Result == false || rightResult.Result == false) ? false : true;

            return new AstBuildResult(literalData, null, result);
        }
    }
}
