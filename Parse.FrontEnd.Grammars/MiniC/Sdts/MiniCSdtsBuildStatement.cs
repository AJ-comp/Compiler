using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.Types;
using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public partial class MiniCSdts
    {
        // [0] : DclList (AstNonTerminal)
        // [1] : StatList (AstNonTerminal) [epsilon able]
        private AstBuildResult BuildCompoundStNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            bool result = true;
            curNode.ClearConnectedInfo();

            // build DclList node
            var dclNode = curNode[0] as AstNonTerminal;
            var dclResult = dclNode.BuildLogic(p, astNodes);
            var buildParams = p.Clone() as MiniCAstBuildParams;
            buildParams.SymbolTable = dclResult.SymbolTable as MiniCSymbolTable;
            curNode.ConnectedSymbolTable = buildParams.SymbolTable;

            if (curNode.Count == 1) // case only DclList exist
                return new AstBuildResult(null, buildParams.SymbolTable, dclResult.Result);

            // build StatList node
            var statNode = curNode[1] as AstNonTerminal;
            var statResult = statNode.BuildLogic(buildParams, astNodes);

            // Even if doesn't exist a information in the node, it has to add.
            astNodes.Add(curNode);

            if (dclResult.Result == false || statResult.Result == false) result = false;

            return new AstBuildResult(null, buildParams.SymbolTable, result);
        }

        // format summary
        // IfSt | IfElseSt | WhileSt | ExpSt
        private AstBuildResult BuildStatListNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            curNode.ClearConnectedInfo();
            if (curNode.Count == 0) return new AstBuildResult(null, null, true);

            foreach (var item in curNode.Items)
            {
                var astNonTerminal = item as AstNonTerminal;
                var statResult = astNonTerminal.BuildLogic(p, astNodes);
            }

            // Even if doesn't exist a information in the node, it has to add.
            astNodes.Add(curNode);

            return new AstBuildResult(null, null, true);
        }

        // format summary
        // (AddAssign | SubAssign | MulAssign | DivAssign | ...) ;
        private AstBuildResult BuildExpStNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            curNode.ClearConnectedInfo();
            // epsilon
            if (curNode.Count == 0) return new AstBuildResult(null, null, true);

            var astNonTerminal = curNode[0] as AstNonTerminal;
            var expResult = astNonTerminal.BuildLogic(p, astNodes);

            // Even if doesn't exist a information in the node, it has to add.
            astNodes.Add(curNode);

            return expResult;
        }

        // [0] : if (Terminal)
        // [1] : logical_exp (NonTerminal)
        // [2] : statement (NonTerminal)
        private AstBuildResult BuildIfStNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            curNode.ClearConnectedInfo();

            // build logical_exp node
            var logicalResult = (curNode[1] as AstNonTerminal).BuildLogic(p, astNodes);

            // create IR
//            string newLabel = NewReservedLabel();
            var options = new IROptions(ReservedLabel);
            var irData = logicalResult.IRFormat.IRData;
            curNode.ConnectedIrUnit = IRBuilder.CretaeConditionalJump(options, irData as IRVar<Bit>, null, null);
            astNodes.Add(curNode);

            // build statement node
            var buildParams = p.Clone() as MiniCAstBuildParams;
            buildParams.BuildOption |= AstBuildOption.NotAssign;
            var stmtResult = (curNode[2] as AstNonTerminal).BuildLogic(buildParams, astNodes);
            ReservedLabel = newLabel;

            return new AstBuildResult(null, null, true);
        }

        private AstBuildResult BuildIfElseStNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            // Even if doesn't exist a information in the node, it has to add.
            astNodes.Add(curNode);

            return null;
        }

        // [0] : while (Terminal)
        // [1] : logical_exp (NonTerminal)
        // [2] : statement (NonTerminal)
        private AstBuildResult BuildWhileStNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            curNode.ClearConnectedInfo();

            string startLabel = NewReservedLabel();
            ReservedLabel = startLabel;

            // build logical_exp node
            var logicalResult = (curNode[1] as AstNonTerminal).BuildLogic(p, astNodes);

            string newLabel = NewReservedLabel();
            var options = new IROptions(ReservedLabel);
            var irData = logicalResult.IRFormat.IRData;
            curNode.ConnectedIrUnit = IRBuilder.CretaeConditionalJump(options, irData as IRVar<Bit>, null, null);
            astNodes.Add(curNode);

            // build statement node
            var buildParams = p.Clone() as MiniCAstBuildParams;
            buildParams.BuildOption |= AstBuildOption.NotAssign;
            var stmtResult = (curNode[2] as AstNonTerminal).BuildLogic(buildParams, astNodes);
            ReservedLabel = newLabel;

            // add UJP label (to back to start)
            astNodes.Last().ConnectedIrUnit = IRBuilder.UnConditionalJump(options, null);

            return new AstBuildResult(null, null, true);
        }

        // [0] : return (AstTerminal)
        // [1] : ExpSt (AstNonTerminal)
        private AstBuildResult BuildReturnStNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            curNode.ClearConnectedInfo();

            // build ExpSt node
            var expNode = curNode[1] as AstNonTerminal;
            var expResult = expNode.BuildLogic(p, astNodes);

            curNode.ConnectedIrUnit = IRBuilder.Command.RetFromProc(ReservedLabel);
            astNodes.Add(curNode);

            return expResult;
        }

        // [0] : Ident (AstTerminal)
        // [1] : ActualParam (AstNonTerminal)
        private AstBuildResult BuildCallNode(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            curNode.ClearConnectedInfo();

            var funcName = curNode[0] as AstTerminal;

            // build ActualParam node
            var node1 = curNode[1] as AstNonTerminal;
            var result = node1.BuildLogic(p, astNodes);

            var options = new IROptions(ReservedLabel);
            curNode.ConnectedIrUnit = IRBuilder.CreateCall(options, funcName.Token.Input);
            astNodes.Add(curNode);

            return result;
        }

        private AstBuildResult BuildActualParam(AstNonTerminal curNode, AstBuildParams p, List<AstSymbol> astNodes)
        {
            bool result = true;
            curNode.ClearConnectedInfo();

            foreach(var item in curNode.Items)
            {
                var nodeX = item as AstNonTerminal;
                var itemResult = nodeX.BuildLogic(p, astNodes);

                if (itemResult.Result == false) result = false;
            }

            // Even if doesn't exist a information in the node, it has to add.
            astNodes.Add(curNode);

            return new AstBuildResult(null, null, result);
        }
    }
}
