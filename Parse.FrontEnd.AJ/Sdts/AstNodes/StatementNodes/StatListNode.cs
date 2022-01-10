using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class StatListNode : StatementNode
    {
        public List<StatementNode> StatementNodes = new List<StatementNode>();

        public StatListNode(AstSymbol node) : base(node)
        {
        }


        // format summary
        // IfSt | IfElseSt | WhileSt | ExpSt | ReturnSt
        public override SdtsNode Compile(CompileParameter param)
        {
            foreach (var item in Items)
            {
                var statementNode = item.Compile(param) as StatementNode;
                StatementNodes.Add(statementNode);


                if (statementNode is IfStatementNode)
                {

                }
                else if (statementNode is IfElseStatementNode)
                {

                }
                else if (statementNode is WhileStatementNode)
                {

                }
                else if (statementNode is ExprStatementNode)
                {

                }
                else if (statementNode is ReturnStatementNode)
                {

                }
            }

            return this;
        }

        public override IRExpression To()
        {
            var result = new IRCompoundStatement();

            foreach (var statementNode in StatementNodes)
                result.Expressions.Add(statementNode.To());

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
