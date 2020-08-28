using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.StatementNodes;
using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.LLVM;
using Parse.MiddleEnd.IR.LLVM.Expressions;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.IRGenerator
{
    public partial class IRExpressionGenerator
    {
        public static string ToLLVMCode(MiniCNode rootNode)
        {
            string result = string.Empty;
            if (rootNode.IsNeedWhileIRGeneration == false) return result;

            foreach (var node in rootNode.Items)
            {
                result += ToLLVMCode(node as MiniCNode);
            }

            return result;
        }

        public static LLVMExpression GenerateLLVMExpression(MiniCNode rootNode)
        {
            ConnectGenerateHandler(rootNode);

            return rootNode.ExecuteToIRExpression(new LLVMSSATable()) as LLVMExpression;
        }

        private static void ConnectGenerateHandler(MiniCNode rootNode)
        {
            if (rootNode is ProgramNode) rootNode.ConvertingToIRExpression = ProgramNodeToIRExpression;
            else if (rootNode is FuncDefNode) rootNode.ConvertingToIRExpression = FuncDefNodeToIRExpression;
            else if (rootNode is CompoundStNode) rootNode.ConvertingToIRExpression = CompoundStNodeToIRExpression;
            else if (rootNode is IfStatementNode) rootNode.ConvertingToIRExpression = CommonCondStatementToIRExpression;
            else if (rootNode is IfElseStatementNode) rootNode.ConvertingToIRExpression = IfElseStatementToIRExpression;
            else if (rootNode is WhileStatementNode) rootNode.ConvertingToIRExpression = CommonCondStatementToIRExpression;
            else if (rootNode is ReturnStatementNode) rootNode.ConvertingToIRExpression = ReturnStatementToIRExpression;
            else if (rootNode is ExprStatementNode) rootNode.ConvertingToIRExpression = ExprStatementNodeToIRExpression;
            //else if (rootNode is CallNode) //    rootNode.ConvertingToIRExpression = 
            else if (rootNode is AssignNode) rootNode.ConvertingToIRExpression = AssignNodeToIRExpression;
            else if (rootNode is AddExprNode) rootNode.ConvertingToIRExpression = AddExprNodeToIRExpression;
            else if (rootNode is SubExprNode) rootNode.ConvertingToIRExpression = SubExprNodeToIRExpression;
            else if (rootNode is MulExprNode) rootNode.ConvertingToIRExpression = MulExprNodeToIRExpression;
            else if (rootNode is ModExprNode) rootNode.ConvertingToIRExpression = DivExprNodeToIRExpression;
            else if (rootNode is DivExprNode) rootNode.ConvertingToIRExpression = ModExprNodeToIRExpression;
            else if (rootNode is EqualExprNode) rootNode.ConvertingToIRExpression = EqualExprNodeToIRExpression;
            else if (rootNode is NotEqualExprNode) rootNode.ConvertingToIRExpression = NotEqualExprNodeToIRExpression;
            else if (rootNode is GreaterThanNode) rootNode.ConvertingToIRExpression = GreaterThanExprNodeToIRExpression;
            else if (rootNode is GreaterEqualNode) rootNode.ConvertingToIRExpression = GreaterEqualExprNodeToIRExpression;
            else if (rootNode is LessThanNode) rootNode.ConvertingToIRExpression = LessThanExprNodeToIRExpression;
            else if (rootNode is LessEqualNode) rootNode.ConvertingToIRExpression = LessEqualExprNodeToIRExpression;
            else if (rootNode is PreIncExprNode) rootNode.ConvertingToIRExpression = PreIncExprNodeToIRExpression;
            else if (rootNode is PostIncExprNode) rootNode.ConvertingToIRExpression = PostIncExprNodeToIRExpression;
            else if (rootNode is PreDecExprNode) rootNode.ConvertingToIRExpression = PreDecExprNodeToIRExpression;
            else if (rootNode is PostDecExprNode) rootNode.ConvertingToIRExpression = PostDecExprNodeToIRExpression;

            else if (rootNode is UseIdentNode) rootNode.ConvertingToIRExpression = UseIdentNodeToIRExpression;
            else if (rootNode is LiteralNode) rootNode.ConvertingToIRExpression = LiteralNodeToIRExpression;

            // remove for a while for debugging
//            else throw new NotImplementedException();


            foreach (var item in rootNode.Items)
                ConnectGenerateHandler(item as MiniCNode);
        }

        private static IRExpression ProgramNodeToIRExpression(SdtsNode sender, object param)
        {
            var cNode = sender as ProgramNode;
            var ssaTable = param as LLVMSSATable;

            LLVMRootExpression result = new LLVMRootExpression();

            foreach (var varRecord in cNode.SymbolTable.VarList)
                result.FirstLayers.Add(new LLVMGlobalVariableExpression(varRecord.VarField, ssaTable));

            foreach (var funcDef in cNode.FuncDefNodes)
                result.FirstLayers.Add(funcDef.ExecuteToIRExpression(ssaTable.Clone()) as LLVMFirstLayerExpression);

            return result;
        }


        private static IRExpression FuncDefNodeToIRExpression(SdtsNode node, object param)
        {
            var cNode = node as FuncDefNode;
            var ssaTable = param as LLVMSSATable;

            return new LLVMFuncDefExpression(cNode.ToIRFuncData(),
                                                                    cNode.CompoundSt.ExecuteToIRExpression(ssaTable) as LLVMBlockExpression,
                                                                    ssaTable);
        }
    }
}
