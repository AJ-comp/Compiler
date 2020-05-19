using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.InterLanguages;
using Parse.Utilities;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        // [0] : DclList (AstNonTerminal)
        // [1] : StatList (AstNonTerminal) [epsilon able]
        private NodeBuildResult BuildCompoundStNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            bool result = true;
            curNode.ClearConnectedInfo();

            // DclList
            var dclResult = this.BuildDclListNode(curNode[0] as AstNonTerminal, baseSymbolTable, blockLevel, offset);
            var newSymbolTable = dclResult.symbolTable as MiniCSymbolTable;
            if (curNode.Count == 1) return new NodeBuildResult(null, newSymbolTable, dclResult.Result);

            // StatList
            var statResult = this.BuildStatListNode(curNode[1] as AstNonTerminal, newSymbolTable, blockLevel, offset);

            if (dclResult.Result == false || statResult.Result == false) result = false;

            curNode.ConnectedSymbolTable = newSymbolTable;
            return new NodeBuildResult(null, newSymbolTable, result);
        }

        // format summary
        // IfSt | IfElseSt | WhileSt | ExpSt
        private NodeBuildResult BuildStatListNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();
            if (curNode.Count == 0) return new NodeBuildResult(null, baseSymbolTable, true);

            foreach (var item in curNode.Items)
            {
                if (item is AstTerminal) continue;

                var astNonTerminal = item as AstNonTerminal;
                StatementHub(astNonTerminal, baseSymbolTable, blockLevel, offset);
            }

            return new NodeBuildResult(null, baseSymbolTable, true);
        }

        // format summary
        // (AddAssign | SubAssign | MulAssign | DivAssign | ...) ;
        private NodeBuildResult BuildExpStNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();
            // epsilon
            if(curNode.Count == 0) return new NodeBuildResult(null, baseSymbolTable, true);

            var astNonTerminal = curNode[0] as AstNonTerminal;
            return ExpressionStatementHub(astNonTerminal, baseSymbolTable, blockLevel, offset);
        }

        private NodeBuildResult ExpressionStatementHub(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            if (curNode.SignPost.MeaningUnit == this.Assign)
                return this.BuildAssignNode(curNode, baseSymbolTable, blockLevel, offset);
            if (curNode.SignPost.MeaningUnit == this.AddAssign)
                return this.BuildAddAssignNode(curNode, baseSymbolTable, blockLevel, offset);
            if (curNode.SignPost.MeaningUnit == this.SubAssign)
                return this.BuildSubAssignNode(curNode, baseSymbolTable, blockLevel, offset);
            if (curNode.SignPost.MeaningUnit == this.MulAssign)
                return this.BuildMulAssignNode(curNode, baseSymbolTable, blockLevel, offset);
            if (curNode.SignPost.MeaningUnit == this.DivAssign)
                return this.BuildDivAssignNode(curNode, baseSymbolTable, blockLevel, offset);
            if (curNode.SignPost.MeaningUnit == this.ModAssign)
                return this.BuildModAssignNode(curNode, baseSymbolTable, blockLevel, offset);
            if (curNode.SignPost.MeaningUnit == this.Add)
                return this.BuildAddNode(curNode, baseSymbolTable, blockLevel, offset);
            if (curNode.SignPost.MeaningUnit == this.Sub)
                return this.BuildSubNode(curNode, baseSymbolTable, blockLevel, offset);
            if (curNode.SignPost.MeaningUnit == this.Mul)
                return this.BuildMulNode(curNode, baseSymbolTable, blockLevel, offset);
            if (curNode.SignPost.MeaningUnit == this.Div)
                return this.BuildDivNode(curNode, baseSymbolTable, blockLevel, offset);
            if (curNode.SignPost.MeaningUnit == this.Mod)
                return this.BuildModNode(curNode, baseSymbolTable, blockLevel, offset);
            if (curNode.SignPost.MeaningUnit == this.Call)
                return this.BuildCallNode(curNode, baseSymbolTable, blockLevel, offset);

            return new NodeBuildResult(null, baseSymbolTable);
        }

        private NodeBuildResult StatementHub(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            if (curNode.SignPost.MeaningUnit == this.CompoundSt)
                return this.BuildCompoundStNode(curNode, baseSymbolTable, blockLevel, offset);
            else if (curNode.SignPost.MeaningUnit == this.ExpSt)
                return this.BuildExpStNode(curNode, baseSymbolTable, blockLevel, offset);
            else if (curNode.SignPost.MeaningUnit == this.IfSt)
                return this.BuildIfStNode(curNode, baseSymbolTable, blockLevel, offset);
            else if (curNode.SignPost.MeaningUnit == this.IfElseSt)
                return this.BuildIfElseStNode(curNode, baseSymbolTable, blockLevel, offset);
            else if (curNode.SignPost.MeaningUnit == this.WhileSt)
                return this.BuildWhileStNode(curNode, baseSymbolTable, blockLevel, offset);
            else if (curNode.SignPost.MeaningUnit == this.ReturnSt)
                return this.BuildReturnStNode(curNode, baseSymbolTable, blockLevel, offset);

            return new NodeBuildResult(null, baseSymbolTable);
        }

        // [0] : if (Terminal)
        // [1] : logical_exp (NonTerminal)
        // [2] : statement (NonTerminal)
        private NodeBuildResult BuildIfStNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();

            BuildLogicalNode(curNode[1] as AstNonTerminal, baseSymbolTable, blockLevel, offset);

            string labelString;
            do
            {
                labelString = StringUtility.RandomString(5, false);
            } while (_labels.Contains(labelString));
            _labels.Add(labelString);

            curNode.ConnectedInterLanguage.Add(UCode.Command.ConditionalJump(ReservedLabel, labelString, false));
            var result = StatementHub(curNode[2] as AstNonTerminal, baseSymbolTable, blockLevel, offset);
            ReservedLabel = labelString;

            return result;
        }

        private NodeBuildResult BuildIfElseStNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }

        private NodeBuildResult BuildWhileStNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }

        // [0] : return (AstTerminal)
        // [1] : ExpSt (AstNonTerminal)
        private NodeBuildResult BuildReturnStNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();

            var stResult = this.BuildExpStNode(curNode[1] as AstNonTerminal, baseSymbolTable, blockLevel, offset);
            if (stResult.Result)
                curNode.ConnectedInterLanguage.Add(UCode.Command.RetFromProc(ReservedLabel));

            return stResult;
        }

        // [0] : Ident (AstTerminal)
        // [1] : ActualParam (AstNonTerminal)
        private NodeBuildResult BuildCallNode(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();

            var funcName = curNode[0] as AstTerminal;
            var result = BuildActualParam(curNode[1] as AstNonTerminal, baseSymbolTable, blockLevel, offset);
            if (result.Result)
                curNode.ConnectedInterLanguage.Add(UCode.Command.ProcCall(ReservedLabel, funcName.Token.Input));

            return result;
        }

        private NodeBuildResult BuildActualParam(AstNonTerminal curNode, MiniCSymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            bool result = true;
            curNode.ClearConnectedInfo();

            foreach(var item in curNode.Items)
            {
                if (ExpressionStatementHub(item as AstNonTerminal, baseSymbolTable, blockLevel, offset).Result == false)
                    result = false;
            }

            return new NodeBuildResult(null, baseSymbolTable, result);
        }
    }
}
