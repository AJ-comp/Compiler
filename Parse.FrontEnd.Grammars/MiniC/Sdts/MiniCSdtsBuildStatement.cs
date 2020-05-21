using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.FrontEnd.InterLanguages;
using Parse.Utilities;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        // [0] : DclList (AstNonTerminal)
        // [1] : StatList (AstNonTerminal) [epsilon able]
        private AstBuildResult BuildCompoundStNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            bool result = true;
            curNode.ClearConnectedInfo();

            // DclList
            var dclResult = this.BuildDclListNode(curNode[0] as AstNonTerminal, baseSymbolTable, blockLevel, offset);
            var newSymbolTable = dclResult.SymbolTable as MiniCSymbolTable;
            if (curNode.Count == 1) return new AstBuildResult(null, newSymbolTable, dclResult.Result);

            // StatList
            var statResult = this.BuildStatListNode(curNode[1] as AstNonTerminal, newSymbolTable, blockLevel, offset);

            if (dclResult.Result == false || statResult.Result == false) result = false;

            curNode.ConnectedSymbolTable = newSymbolTable;
            return new AstBuildResult(null, newSymbolTable, result);
        }

        // format summary
        // IfSt | IfElseSt | WhileSt | ExpSt
        private AstBuildResult BuildStatListNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();
            if (curNode.Count == 0) return new AstBuildResult(null, baseSymbolTable, true);

            foreach (var item in curNode.Items)
            {
                if (item is AstTerminal) continue;

                var astNonTerminal = item as AstNonTerminal;
                astNonTerminal.BuildLogic(baseSymbolTable, blockLevel, offset);
            }

            return new AstBuildResult(null, baseSymbolTable, true);
        }

        // format summary
        // (AddAssign | SubAssign | MulAssign | DivAssign | ...) ;
        private AstBuildResult BuildExpStNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();
            // epsilon
            if(curNode.Count == 0) return new AstBuildResult(null, baseSymbolTable, true);

            var astNonTerminal = curNode[0] as AstNonTerminal;
            return astNonTerminal.BuildLogic(baseSymbolTable, blockLevel, offset);
        }

        // [0] : if (Terminal)
        // [1] : logical_exp (NonTerminal)
        // [2] : statement (NonTerminal)
        private AstBuildResult BuildIfStNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();

            BuildExpressionNode(curNode[1] as AstNonTerminal, baseSymbolTable, blockLevel, offset);

            string labelString;
            do
            {
                labelString = StringUtility.RandomString(5, false);
            } while (_labels.Contains(labelString));
            _labels.Add(labelString);

            curNode.ConnectedInterLanguage.Add(UCode.Command.ConditionalJump(ReservedLabel, labelString, false));
            var result = (curNode[2] as AstNonTerminal).BuildLogic(baseSymbolTable, blockLevel, offset);
            ReservedLabel = labelString;

            return result;
        }

        private AstBuildResult BuildIfElseStNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }

        private AstBuildResult BuildWhileStNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            return null;
        }

        // [0] : return (AstTerminal)
        // [1] : ExpSt (AstNonTerminal)
        private AstBuildResult BuildReturnStNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();

            var stResult = this.BuildExpStNode(curNode[1] as AstNonTerminal, baseSymbolTable, blockLevel, offset);
            if (stResult.Result)
                curNode.ConnectedInterLanguage.Add(UCode.Command.RetFromProc(ReservedLabel));

            return stResult;
        }

        // [0] : Ident (AstTerminal)
        // [1] : ActualParam (AstNonTerminal)
        private AstBuildResult BuildCallNode(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            curNode.ClearConnectedInfo();

            var funcName = curNode[0] as AstTerminal;
            var result = BuildActualParam(curNode[1] as AstNonTerminal, baseSymbolTable, blockLevel, offset);
            if (result.Result)
                curNode.ConnectedInterLanguage.Add(UCode.Command.ProcCall(ReservedLabel, funcName.Token.Input));

            return result;
        }

        private AstBuildResult BuildActualParam(AstNonTerminal curNode, SymbolTable baseSymbolTable, int blockLevel, int offset)
        {
            bool result = true;
            curNode.ClearConnectedInfo();

            foreach(var item in curNode.Items)
            {
                var astNonTerminal = item as AstNonTerminal;
                if (astNonTerminal.BuildLogic(baseSymbolTable, blockLevel, offset).Result == false)
                    result = false;
            }

            return new AstBuildResult(null, baseSymbolTable, result);
        }
    }
}
