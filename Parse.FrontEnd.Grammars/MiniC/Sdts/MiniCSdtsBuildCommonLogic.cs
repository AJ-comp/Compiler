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
        private VarData CheckExistVarInSymbolTable(AstTerminal childNodeToCheck, MiniCSymbolTable baseSymbolTable)
        {
            VarData result = null;

            if (childNodeToCheck.Token.Kind.TokenType is Identifier)
                result = baseSymbolTable.AllVarList.GetVarByName(childNodeToCheck.Token.Input);

            return result;
        }

        /// <summary>
        /// This function connect an Ucode for declaring variable to the 'curNode'.
        /// If it can't perform then MeaningErrInfo is connected to the 'curNode'.
        /// percondition for operation.
        /// 1. symbol table has to be connected to the 'node'.
        /// 2. The type 'node[varIndex]' has to be TreeTerminal.
        /// </summary>
        /// <param name="curNode"></param>
        /// <param name="varIndex"></param>
        /// <returns></returns>
        private bool ConnectSimpleVarCode(AstNonTerminal curNode, int varIndex)
        {
            bool result = false;
            AstTerminal varNode = curNode[varIndex] as AstTerminal;

            var varData = CheckExistVarInSymbolTable(varNode, curNode.ConnectedSymbolTable as MiniCSymbolTable);
            if (varData == null)
            {
                result = false;
                curNode.ConnectedErrInfoList.Add(new MeaningErrInfo(varNode.Token, string.Format(AlarmCodes.MCL0001, varNode.Token.Input)));
            }
            else
            {
                result = true;
                curNode.ConnectedInterLanguage.Add(UCode.Command.LoadVar(varData.DclData.BlockLevel, varData.Offset, varData.DclData.DclItemData.Name));
            }

            return result;
        }

        private bool ReadyForIdentCalculateIdent(AstNonTerminal node, int leftIndex, int rightIndex)
        {
            bool result = true;
            var leftResult = ConnectSimpleVarCode(node, leftIndex);
            var rightResult = ConnectSimpleVarCode(node, rightIndex);

            if (leftResult == false || rightResult == false)
            {
                node.ConnectedInterLanguage.Clear();
                return false;
            }

            var leftNode = node[leftIndex] as AstTerminal;
            var rightNode = node[rightIndex] as AstTerminal;

            return result;
        }

        private bool ReadyForIdentCalculateDigit(AstNonTerminal node, int leftIndex, int rightIndex)
        {
            var leftResult = ConnectSimpleVarCode(node, leftIndex);

            if (leftResult == false)
            {
                node.ConnectedInterLanguage.Clear();
                return false;
            }

            var valueNode = node[rightIndex] as AstTerminal;
            node.ConnectedInterLanguage.Add(UCode.Command.DclValue(System.Convert.ToInt32(valueNode.Token.Input), valueNode.Token.Input));

            return true;
        }

        private bool ReadyForDigitCalculateDigit(AstNonTerminal node, int leftIndex, int rightIndex)
        {
            var leftValueNode = node[leftIndex] as AstTerminal;
            node.ConnectedInterLanguage.Add(UCode.Command.DclValue(System.Convert.ToInt32(leftValueNode.Token.Input), leftValueNode.Token.Input));

            var rightValueNode = node[rightIndex] as AstTerminal;
            node.ConnectedInterLanguage.Add(UCode.Command.DclValue(System.Convert.ToInt32(rightValueNode.Token.Input), rightValueNode.Token.Input));

            return true;
        }

        private bool ConnectVarOrDigitCode(AstNonTerminal curNode, int index)
        {
            bool result = true;

            AstTerminal valueNode = curNode[index] as AstTerminal;
            if (valueNode.Token.Kind.TokenType is Identifier)
            {
                result = ConnectSimpleVarCode(curNode, index);
            }

            return result;
        }

        private bool ReadyForCalculate(AstNonTerminal curNode, int leftIndex, int rightIndex)
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

            curNode.ConnectedInterLanguage.Add(UCode.Command.Store(leftVarData.DclData.BlockLevel, leftVarData.Offset, leftVarData.DclData.DclItemData.Name));

            return result;
        }

        private bool BuildCommonCalculateNodeCore(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, ExpressionKind expressionKind, bool bAssign = false)
        {
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = baseSymbolTable;

            if (ReadyForCalculate(curNode, 0, 2) == false) return false;

            if (expressionKind == ExpressionKind.Add)
                curNode.ConnectedInterLanguage.Add(UCode.Command.Add());
            else if (expressionKind == ExpressionKind.Sub)
                curNode.ConnectedInterLanguage.Add(UCode.Command.Sub());
            else if (expressionKind == ExpressionKind.Mul)
                curNode.ConnectedInterLanguage.Add(UCode.Command.Multiple());
            else if (expressionKind == ExpressionKind.Div)
                curNode.ConnectedInterLanguage.Add(UCode.Command.Div());
            else if (expressionKind == ExpressionKind.Mod)
                curNode.ConnectedInterLanguage.Add(UCode.Command.Mod());

            bool result = true;
            if (bAssign) result = ConnectAssignCode(curNode, 0, 2);

            return result;
        }

        private NodeBuildResult BuildCommonCalculateNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, 
                                                                                    int blockLevel, int offset, ExpressionKind expressionKind, bool bAssign = false)
        {
            curNode.ClearConnectedInfo();
            curNode.ConnectedSymbolTable = baseSymbolTable;

            // if TreeNonTerminal doesn't exist.
            if (curNode[0] is AstTerminal && curNode[2] is AstTerminal)
            {
                var calculateResult = BuildCommonCalculateNodeCore(curNode, baseSymbolTable, expressionKind, bAssign);
                return new NodeBuildResult(null, baseSymbolTable, calculateResult);
            }

            // if at least one TreeNonTerminal exist.
            var leftResult = BuildHalfExpression(curNode, baseSymbolTable, blockLevel, offset, 0);
            var rightResult = BuildHalfExpression(curNode, baseSymbolTable, blockLevel, offset, 2);

            var result = true;
            if (leftResult == false || rightResult == false) result = false;

            return new NodeBuildResult(null, baseSymbolTable, result);
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
        private bool BuildHalfExpression(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset, int index)
        {
            bool result = false;

            if (curNode[index] is AstNonTerminal)
                result = BuildCalculateNode(curNode[index] as AstNonTerminal, baseSymbolTable, blockLevel, offset).Result;
            else
                result = ConnectVarOrDigitCode(curNode, index);

            return result;
        }

        private NodeBuildResult BuildCalculateNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            var result = new NodeBuildResult(null, baseSymbolTable);

            if (curNode.SignPost.MeaningUnit == this.Add)
                result = this.BuildAddNode(curNode, baseSymbolTable, blockLevel, offset);
            else if (curNode.SignPost.MeaningUnit == this.Sub)
                result = this.BuildSubNode(curNode, baseSymbolTable, blockLevel, offset);
            else if (curNode.SignPost.MeaningUnit == this.Mul)
                result = this.BuildMulNode(curNode, baseSymbolTable, blockLevel, offset);
            else if (curNode.SignPost.MeaningUnit == this.Div)
                result = this.BuildDivNode(curNode, baseSymbolTable, blockLevel, offset);
            else if (curNode.SignPost.MeaningUnit == this.Mod)
                result = this.BuildModNode(curNode, baseSymbolTable, blockLevel, offset);

            return result;
        }
    }
}
