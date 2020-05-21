using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.Grammars.Properties;
using Parse.FrontEnd.InterLanguages;
using System;

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

        /// <summary>
        /// This function connect an Ucode for declaring variable to the 'curNode'.
        /// If it can't perform then MeaningErrInfo is connected to the 'curNode'.
        /// percondition for operation.
        /// 1. symbol table has to be connected to the parent node of the 'curNode'.
        /// </summary>
        /// <param name="curNode"></param>
        /// <param name="varIndex"></param>
        /// <returns></returns>
        private bool ConnectSimpleVarCode(AstTerminal curNode)
        {
            bool result = false;
            curNode.ClearConnectedInfo();
            var symbolTable = curNode.Parent.ConnectedSymbolTable;
            var varData = CheckExistVarInSymbolTable(curNode, symbolTable as MiniCSymbolTable);
            if (varData == null)
            {
                result = false;
                curNode.ConnectedErrInfoList.Add(new MeaningErrInfo(curNode.Token, string.Format(AlarmCodes.MCL0001, curNode.Token.Input)));
            }
            else
            {
                result = true;
                curNode.ConnectedInterLanguage.Add(UCode.Command.LoadVar(ReservedLabel, varData.DclData.BlockLevel, 
                                                                                                                varData.Offset, varData.DclData.DclItemData.Name));
            }

            return result;
        }

        private bool ReadyForIdentCalculateIdent(AstNonTerminal node, int leftIndex, int rightIndex)
        {
            bool result = true;
            var leftResult = ConnectSimpleVarCode(node[leftIndex] as AstTerminal);
            var rightResult = ConnectSimpleVarCode(node[rightIndex] as AstTerminal);

            if (leftResult == false || rightResult == false)
            {
                node.ConnectedInterLanguage.Clear();
                return false;
            }

            return result;
        }

        private bool ReadyForIdentCalculateDigit(AstNonTerminal node, int leftIndex, int rightIndex)
        {
            var leftResult = ConnectSimpleVarCode(node[leftIndex] as AstTerminal);

            if (leftResult == false)
            {
                node.ConnectedInterLanguage.Clear();
                return false;
            }

            var valueNode = node[rightIndex] as AstTerminal;
            node.ConnectedInterLanguage.Add(UCode.Command.DclValue(ReservedLabel, 
                                                                                                        System.Convert.ToInt32(valueNode.Token.Input), 
                                                                                                        valueNode.Token.Input));

            return true;
        }

        private bool ReadyForDigitCalculateDigit(AstNonTerminal node, int leftIndex, int rightIndex)
        {
            var leftValueNode = node[leftIndex] as AstTerminal;
            node.ConnectedInterLanguage.Add(UCode.Command.DclValue(ReservedLabel, 
                                                                                                        System.Convert.ToInt32(leftValueNode.Token.Input), 
                                                                                                        leftValueNode.Token.Input));

            var rightValueNode = node[rightIndex] as AstTerminal;
            node.ConnectedInterLanguage.Add(UCode.Command.DclValue(ReservedLabel, 
                                                                                                        System.Convert.ToInt32(rightValueNode.Token.Input), 
                                                                                                        rightValueNode.Token.Input));

            return true;
        }

        private bool ConnectVarOrDigitCode(AstTerminal curNode)
        {
            bool result = true;

            if (curNode.Token.Kind.TokenType is Identifier)
                result = ConnectSimpleVarCode(curNode);
            else if (curNode.Token.Kind.TokenType is Digit)
                curNode.ConnectedInterLanguage.Add(UCode.Command.DclValue(ReservedLabel, System.Convert.ToInt32(curNode.Token.Input)));
            else result = false;

            return result;
        }

        private bool ReadyForExpression(AstNonTerminal curNode, int leftIndex, int rightIndex)
        {
            bool result = true;

            AstTerminal leftNode = curNode[leftIndex] as AstTerminal;
            AstTerminal rightNode = curNode[rightIndex] as AstTerminal;
            if (leftNode.Token.Kind.TokenType is Identifier && rightNode.Token.Kind.TokenType is Identifier)
            {
                result = ReadyForIdentCalculateIdent(curNode, leftIndex, rightIndex);
            }
            else if (leftNode.Token.Kind.TokenType is Identifier && rightNode.Token.Kind.TokenType is Digit)
            {
                result = ReadyForIdentCalculateDigit(curNode, leftIndex, rightIndex);
            }
            else if (leftNode.Token.Kind.TokenType is Digit && rightNode.Token.Kind.TokenType is Identifier)
            {
                result = ReadyForIdentCalculateDigit(curNode, rightIndex, leftIndex);
            }
            else if (leftNode.Token.Kind.TokenType is Digit && rightNode.Token.Kind.TokenType is Digit)
            {
                result = ReadyForDigitCalculateDigit(curNode, leftIndex, rightIndex);
            }
            else result = false;

            return result;
        }

        /// <summary>
        /// This function connect an Ucode for assignment to the 'curNode'.
        /// If it can't perform then MeaningErrInfo is connected to the 'curNode'.
        /// percondition for operation.
        /// 1. 'curNode[leftIndex]' has to be TreeTerminal.
        /// 2. 'curNode[rightIndex]' has to be TreeTerminal.
        /// </summary>
        /// <param name="curNode">The target node to connect an Ucode or MeaningErrInfo</param>
        /// <param name="leftIndex">The index of the left expression</param>
        /// <param name="rightIndex">The index of the right expression</param>
        /// <returns>If Ucode was connected returns true, MeaningErrInfo has connected returns false.</returns>
        private bool ConnectAssignCode(AstNonTerminal curNode, int leftIndex, int rightIndex)
        {
            bool result = true;

            AstTerminal leftVarNode = curNode[leftIndex] as AstTerminal;
            AstTerminal rightVarNode = curNode[rightIndex] as AstTerminal;

            var leftVarData = CheckExistVarInSymbolTable(leftVarNode, curNode.ConnectedSymbolTable as MiniCSymbolTable);
            var rightVarData = CheckExistVarInSymbolTable(rightVarNode, curNode.ConnectedSymbolTable as MiniCSymbolTable);

            if(leftVarData != null && rightVarData != null)
            {
                if (leftVarData.DclData.DclSpecData.Const)
                {
                    result = false;
                    curNode.ConnectedErrInfoList.Add(new MeaningErrInfo(leftVarNode.Token, AlarmCodes.MCL0002));
                }

                if (leftVarData.DclData.DclSpecData.DataType != rightVarData.DclData.DclSpecData.DataType)
                {
                    result = false;
                    curNode.ConnectedErrInfoList.Add(new MeaningErrInfo(rightVarNode.Token, AlarmCodes.MCL0003));
                }
            }

            curNode.ConnectedInterLanguage.Add(UCode.Command.Store(ReservedLabel, leftVarData.DclData.BlockLevel, 
                                                                                                        leftVarData.Offset, leftVarData.DclData.DclItemData.Name));

            return result;
        }

        private bool BuildOnlyExpressionNode(AstNonTerminal curNode, SymbolTable baseSymbolTable)
        {
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = baseSymbolTable;

//            if (ReadyForExpression(curNode, 0, 2) == false) return false;

            if (curNode.SignPost.MeaningUnit == this.Add)
                curNode.ConnectedInterLanguage.Add(UCode.Command.Add(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.Sub)
                curNode.ConnectedInterLanguage.Add(UCode.Command.Sub(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.Mul)
                curNode.ConnectedInterLanguage.Add(UCode.Command.Multiple(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.Div)
                curNode.ConnectedInterLanguage.Add(UCode.Command.Div(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.Mod)
                curNode.ConnectedInterLanguage.Add(UCode.Command.Mod(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.LogicalNot)
                curNode.ConnectedInterLanguage.Add(UCode.Command.Not(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.LogicalAnd)
                curNode.ConnectedInterLanguage.Add(UCode.Command.And(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.LogicalOr)
                curNode.ConnectedInterLanguage.Add(UCode.Command.Or(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.Equal)
                curNode.ConnectedInterLanguage.Add(UCode.Command.Equal(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.NotEqual)
                curNode.ConnectedInterLanguage.Add(UCode.Command.NegativeEqual(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.GreaterThan)
                curNode.ConnectedInterLanguage.Add(UCode.Command.GreaterThan(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.GreaterEqual)
                curNode.ConnectedInterLanguage.Add(UCode.Command.GreaterEqual(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.LessThan)
                curNode.ConnectedInterLanguage.Add(UCode.Command.LessThan(ReservedLabel));
            else if (curNode.SignPost.MeaningUnit == this.LessEqual)
                curNode.ConnectedInterLanguage.Add(UCode.Command.LessEqual(ReservedLabel));

            return true;
        }

        /// <summary>
        /// This function builds for the expression on the basis of an operator.
        /// </summary>
        /// <param name="curNode"></param>
        /// <param name="baseSymbolTable"></param>
        /// <param name="blockLevel"></param>
        /// <param name="offset"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool BuildHalfExpression(AstSymbol curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            bool result = false;

            if (curNode is AstNonTerminal)
                result = BuildExpressionNode(curNode as AstNonTerminal, baseSymbolTable, blockLevel, offset).Result;
            else
                result = ConnectVarOrDigitCode(curNode as AstTerminal);

            return result;
        }

        private AstBuildResult BuildExpressionNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = baseSymbolTable;

            var leftResult = BuildHalfExpression(curNode[0], baseSymbolTable, blockLevel, offset);
            var rightResult = BuildHalfExpression(curNode[2], baseSymbolTable, blockLevel, offset);
            BuildOnlyExpressionNode(curNode, baseSymbolTable);

            var result = true;
            if (leftResult == false || rightResult == false) result = false;

            return new AstBuildResult(null, baseSymbolTable, result);
        }
    }
}
