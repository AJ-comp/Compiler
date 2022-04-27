using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using System.Linq;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class IfElseStatementNode : IfStatementNode
    {
        public IfElseStatementNode(AstSymbol node) : base(node)
        {
        }


        // [0] : TerminalNode [if]
        // [1] : ExprNode
        // [2] : StatementNode [statement]
        // [3] : TerminalNode [else]
        // [4] : StatementNode [statement]
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);
            FalseStatement = Items.Last().Compile(param) as StatementNode;

            ClarifyReturn = TrueStatement.ClarifyReturn & FalseStatement.ClarifyReturn;

            return this;
        }
    }
}
