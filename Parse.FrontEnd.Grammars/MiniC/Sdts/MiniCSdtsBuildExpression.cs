using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.Grammars.Properties;
using Parse.FrontEnd.InterLanguages;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        private enum ExpressionKind { None, Add, Sub, Mul, Div, Mod };

        /// <summary>
        /// This function checks whether exist variable in the 'baseSymbolTable'.
        /// </summary>
        /// <param name="childNodeToCheck">The child node to check</param>
        /// <param name="baseSymbolTable">The symbol table to reference</param>
        /// <returns></returns>
        private VarData CheckExistVarInSymbolTable(TreeTerminal childNodeToCheck, MiniCSymbolTable baseSymbolTable)
        {
            VarData result = null;

            if (childNodeToCheck.Token.Kind.TokenType is Identifier)
                result = baseSymbolTable.AllVarList.GetVarByName(childNodeToCheck.Token.Input);

            return result;
        }

        private VarData BuildSimpleAssignForVar(TreeNonTerminal node, int varIndex)
        {
            TreeTerminal varNode = node[varIndex] as TreeTerminal;
            var varData = CheckExistVarInSymbolTable(varNode, node.ConnectedSymbolTable as MiniCSymbolTable);
            if (varData == null)
            {
                node.ConnectedErrInfoList.Add(new MeaningErrInfo(varNode.Token,
                                                                                            string.Format(AlarmCodes.MCL0001, varNode.Token.Input)));
            }
            else if (varData.DclData.DclSpecData.Const)
                node.ConnectedErrInfoList.Add(new MeaningErrInfo(varNode.Token, AlarmCodes.MCL0002));
            else
                node.ConnectedInterLanguage.Add(UCode.Command.LoadVar(varData.DclData.BlockLevel, varData.Offset, varData.DclData.DclItemData.Name));

            return varData;
        }

        private bool ReadyIdentCalculateIdent(TreeNonTerminal node, int leftIndex, int rightIndex)
        {
            var leftData = BuildSimpleAssignForVar(node, leftIndex);
            var rightData = BuildSimpleAssignForVar(node, rightIndex);

            if (leftData == null || rightData == null)
            {
                node.ConnectedInterLanguage.Clear();
                return false;
            }

            var rightNode = node[rightIndex] as TreeTerminal;

            if (leftData.DclData.DclSpecData.DataType != rightData.DclData.DclSpecData.DataType)
            {
                node.ConnectedErrInfoList.Add(new MeaningErrInfo(rightNode.Token, AlarmCodes.MCL0003));
                return false;
            }

            return true;
        }

        private bool ReadyIdentCalculateDigit(TreeNonTerminal node, int leftIndex, int rightIndex)
        {
            var leftData = BuildSimpleAssignForVar(node, leftIndex);

            if (leftData == null)
            {
                node.ConnectedInterLanguage.Clear();
                return false;
            }

            var valueNode = node[rightIndex] as TreeTerminal;
            node.ConnectedInterLanguage.Add(UCode.Command.DclValue(System.Convert.ToInt32(valueNode.Token.Input), valueNode.Token.Input));

            return true;
        }

        private bool ReadyDigitCalculateDigit(TreeNonTerminal node, int leftIndex, int rightIndex)
        {
            var leftValueNode = node[leftIndex] as TreeTerminal;
            node.ConnectedInterLanguage.Add(UCode.Command.DclValue(System.Convert.ToInt32(leftValueNode.Token.Input), leftValueNode.Token.Input));

            var rightValueNode = node[rightIndex] as TreeTerminal;
            node.ConnectedInterLanguage.Add(UCode.Command.DclValue(System.Convert.ToInt32(rightValueNode.Token.Input), rightValueNode.Token.Input));

            return true;
        }

        private NodeBuildResult BuildCommonCalculateNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, ExpressionKind expressionKind, bool bAssign = false)
        {
            curNode.ClearConnectedInfo();
            var result = new NodeBuildResult(null, baseSymbolTable);
            if (curNode.HasVirtualChild) return result;

            curNode.ConnectedSymbolTable = baseSymbolTable;

            // if valueTerminal is Ident
            TreeTerminal leftNode = curNode[0] as TreeTerminal;
            TreeTerminal rightNode = curNode[2] as TreeTerminal;
            if (leftNode.Token.Kind.TokenType is Identifier && rightNode.Token.Kind.TokenType is Identifier)
            {
                if (ReadyIdentCalculateIdent(curNode, 0, 2) == false) return result;
            }
            else if (leftNode.Token.Kind.TokenType is Identifier && rightNode.Token.Kind.TokenType is Digit)
            {
                if (ReadyIdentCalculateDigit(curNode, 0, 2) == false) return result;
            }
            else if (leftNode.Token.Kind.TokenType is Digit && rightNode.Token.Kind.TokenType is Identifier)
            {
                if (ReadyIdentCalculateDigit(curNode, 2, 0) == false) return result;
            }
            else if (leftNode.Token.Kind.TokenType is Digit && rightNode.Token.Kind.TokenType is Digit)
            {
                if (ReadyDigitCalculateDigit(curNode, 0, 2) == false) return result;
            }
            else return result;

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

            if(bAssign)
            {
                TreeTerminal varNode = curNode[0] as TreeTerminal;
                var varData = CheckExistVarInSymbolTable(varNode, curNode.ConnectedSymbolTable as MiniCSymbolTable);
                curNode.ConnectedInterLanguage.Add(UCode.Command.Store(varData.DclData.BlockLevel, varData.Offset, varData.DclData.DclItemData.Name));
            }
            return result;
        }

        // format summary
        // ident = (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildAssignNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, ExpressionKind.None, true);

        // format summary
        // ident += (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildAddAssignNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, ExpressionKind.Add, true);

        // format summary
        // ident -= (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildSubAssignNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, ExpressionKind.Sub, true);

        // format summary
        // ident *= (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildMulAssignNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, ExpressionKind.Mul, true);

        // format summary
        // ident /= (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildDivAssignNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, ExpressionKind.Div, true);

        // format summary
        // ident %= (ident | digit)
        //  [0]   [1]   [2]
        private NodeBuildResult BuildModAssignNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, ExpressionKind.Mod, true);

        // format summary
        // (ident | digit) + (ident | digit)
        private NodeBuildResult BuildAddNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, ExpressionKind.Add);

        // format summary
        // (ident | digit) - (ident | digit)
        private NodeBuildResult BuildSubNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, ExpressionKind.Sub);

        // format summary
        // (ident | digit) * (ident | digit)
        private NodeBuildResult BuildMulNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, ExpressionKind.Mul);

        // format summary
        // (ident | digit) / (ident | digit)
        private NodeBuildResult BuildDivNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, ExpressionKind.Div);

        // format summary
        // (ident | digit) % (ident | digit)
        private NodeBuildResult BuildModNode(TreeNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
            => BuildCommonCalculateNode(curNode, baseSymbolTable, ExpressionKind.Mod);
    }
}
