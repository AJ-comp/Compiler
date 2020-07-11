using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat;
using Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.VarDataFormat;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.Grammars.Properties;
using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using System.Collections.Generic;

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
                curNode.ConnectedErrInfoList.Add(new MeaningErrInfo(nameof(AlarmCodes.MCL0009),
                                                                                                string.Format(AlarmCodes.MCL0009, dclData.DclItemData.Name)));
            }
            else
            {
                IROptions option = new IROptions(ReservedLabel);
                // add symbol information to the symbol table.
                RealVarData varData = new RealVarData(dclData);
                symbolTable.VarDataList.Add(varData);
                curNode.ConnectedIrUnit = IRBuilder.CreateDclVar(option, varData, false);

                p.Offset++;

                result = true;
            }

            return result;
        }

        private bool ConnectBinOpToNode(AstNonTerminal curNode, VarData left, ValueData right, IROperation operation)
        {
            bool result = false;

            if ((left is VirtualVarData) == false)
            {
                IROptions option = new IROptions(ReservedLabel);
                curNode.ConnectedIrUnit = IRBuilder.CreateBinOP(option, left, right, operation);

                result = true;
            }

            return result;
        }

        /// <summary>
        /// This function connect an IR for declaring variable to the 'curNode'.
        /// If it can't perform then MeaningErrInfo is connected to the 'curNode'.
        /// The percondition for operation.
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
                curNode.ConnectedErrInfoList.Add(new MeaningErrInfo(nameof(AlarmCodes.MCL0001), 
                                                                                                string.Format(AlarmCodes.MCL0001, 
                                                                                                curNode.Token.Input)));

                varData = new VirtualVarData(curNode.Token, buildParams.BlockLevel, buildParams.Offset, 0);
                (curNode.Parent.ConnectedSymbolTable as MiniCSymbolTable).VarDataList.Add(varData);
            }
            else if(varData is RealVarData)
            {
                var realVarData = varData as RealVarData;
                bool bAddress = AstBuildOptionChecker.HasOption(buildParams.BuildOption, AstBuildOption.Reference);

                /*
                if (bAddress)
                    curNode.ConnectedIrUnit = IRBuilder.LoadVarAddress(ReservedLabel, realVarData.BlockLevel, realVarData.Offset, realVarData.VarName);
                else
                    curNode.ConnectedIrUnit = IRBuilder.(ReservedLabel, realVarData.BlockLevel, realVarData.Offset, realVarData.VarName);
                */
            }

            return varData;
        }

        private ValueData BuildOperator(AstNonTerminal curNode, ValueData left, ValueData right)
        {
            curNode.ClearConnectedInfo();
            ValueData result = null;

            var option = new IROptions(ReservedLabel);

            // if IRData exist use it. otherwise use parameter (left or right)
            var leftIRFormat = curNode[0].ConnectedIrUnit as IRFormat;
            var leftIR = leftIRFormat.IRData ?? left;

            var rightIRFormat = curNode[1].ConnectedIrUnit as IRFormat;
            var rightIR = rightIRFormat.IRData ?? right;

            // condition
            if (curNode.SignPost.MeaningUnit == this.Add)
            {
                result = left.Add(right) as ValueData;
                curNode.ConnectedIrUnit = IRBuilder.CreateBinOP(option, leftIR, rightIR, IROperation.Add);
            }
            else if (curNode.SignPost.MeaningUnit == this.Sub)
            {
                result = left.Sub(right) as ValueData;
                curNode.ConnectedIrUnit = IRBuilder.CreateBinOP(option, leftIR, rightIR, IROperation.Sub);
            }
            else if (curNode.SignPost.MeaningUnit == this.Mul)
            {
                result = left.Mul(right) as ValueData;
                curNode.ConnectedIrUnit = IRBuilder.CreateBinOP(option, leftIR, rightIR, IROperation.Mul);
            }
            else if (curNode.SignPost.MeaningUnit == this.Div)
            {
                result = left.Div(right) as ValueData;
                curNode.ConnectedIrUnit = IRBuilder.CreateBinOP(option, leftIR, rightIR, IROperation.Div);
            }
            else if (curNode.SignPost.MeaningUnit == this.Mod)
            {
                result = left.Mod(right) as ValueData;
                curNode.ConnectedIrUnit = IRBuilder.CreateBinOP(option, leftIR, rightIR, IROperation.Mod);
            }
            else if (curNode.SignPost.MeaningUnit == this.Equal)
                curNode.ConnectedIrUnit = IRBuilder.CreateLogicalOp(option, leftIR, rightIR, IRCondition.EQ);
            else if (curNode.SignPost.MeaningUnit == this.NotEqual)
                curNode.ConnectedIrUnit = IRBuilder.CreateLogicalOp(option, leftIR, rightIR, IRCondition.NE);
            else if (curNode.SignPost.MeaningUnit == this.GreaterThan)
                curNode.ConnectedIrUnit = IRBuilder.CreateLogicalOp(option, leftIR, rightIR, IRCondition.GT);
            else if (curNode.SignPost.MeaningUnit == this.GreaterEqual)
                curNode.ConnectedIrUnit = IRBuilder.CreateLogicalOp(option, leftIR, rightIR, IRCondition.GE);
            else if (curNode.SignPost.MeaningUnit == this.LessThan)
                curNode.ConnectedIrUnit = IRBuilder.CreateLogicalOp(option, leftIR, rightIR, IRCondition.LT);
            else if (curNode.SignPost.MeaningUnit == this.LessEqual)
                curNode.ConnectedIrUnit = IRBuilder.CreateLogicalOp(option, leftIR, rightIR, IRCondition.LE);
            //            else if (curNode.SignPost.MeaningUnit == this.LogicalNot)
            //                curNode.ConnectedIrUnit = IRBuilder.CreateNot(option, );

            return result;
        }

        private IRValue<Bit> LogicalBuildOperator(AstNonTerminal curNode, IRData<Bit> left, IRData<Bit> right)
        {
            var option = new IROptions(ReservedLabel);

            IRFormat result = (curNode.SignPost.MeaningUnit == this.LogicalAnd) ?
                                        IRBuilder.CreateAnd(option, left, right) :
                                        (curNode.SignPost.MeaningUnit == this.LogicalOr) ?
                                        IRBuilder.CreateOr(option, left, right) : null;

            curNode.ConnectedIrUnit = result;

            return result.IRData as IRValue<Bit>;
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
            var result = leftResult.Result != false && rightResult.Result != false;

            return new AstBuildResult(literalData, null, result);
        }

        private AstBuildResult BuildLogicalOpNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = p.SymbolTable;

            // build right ExpSt node
            var rightResult = (curNode[1] as AstNonTerminal).BuildLogic(p, astNodes);
            // build left ExpSt node
            var leftResult = (curNode[0] as AstNonTerminal).BuildLogic(p, astNodes);

            // set data
            var leftParam = VarToBitData(curNode[0], leftResult);
            var rightParam = VarToBitData(curNode[1], rightResult);

            // calculate to generate temporary data calculated.
            var literalData = LogicalBuildOperator(curNode, leftParam, rightParam);
            astNodes.Add(curNode);   // because of BuildOperator function add IE or ME.
            var result = leftResult.Result != false && rightResult.Result != false;

            return new AstBuildResult(literalData, null, result);
        }

        /// <summary>
        /// This function creates a bit result about whether the value of the variable is not equal 0.
        /// </summary>
        /// <param name="varNode"></param>
        /// <param name="buildResult"></param>
        /// <returns>returns true if the value of the variable is not equal 0. else returns false.</returns>
        private IRData<Bit> VarToBitData(AstSymbol varNode, AstBuildResult buildResult)
        {
            IRData<Bit> result = null;
            if (buildResult.Data is VarData)
            {
                var allToken = (varNode as AstNonTerminal).ConnectedParseTree.AllTokens;

                var irFormat = IRBuilder.CreateLogicalOp(null, buildResult.Data as VarData, new ConceptValueData<Int>(allToken, 0), IRCondition.NE);
                varNode.ConnectedIrUnit = irFormat;
                result = irFormat.IRData as IRVar<Bit>;
            }
            else if(buildResult.Data is LiteralData)
            {
                var literalData = buildResult.Data as IRValue;
                var allToken = (varNode as AstNonTerminal).ConnectedParseTree.AllTokens;

                result = ((double)literalData.Value != 0) ? new ConceptValueData<Bit>(allToken, true) : new ConceptValueData<Bit>(allToken, false);
            }
            else if(buildResult.Data is ConceptValueData)
            {
                var valueData = buildResult.Data as IRValue;
                var allToken = (varNode as AstNonTerminal).ConnectedParseTree.AllTokens;

                result = ((double)valueData.Value != 0) ? new ConceptValueData<Bit>(allToken, true) : new ConceptValueData<Bit>(allToken, false);
            }

            return result;
        }
    }
}
