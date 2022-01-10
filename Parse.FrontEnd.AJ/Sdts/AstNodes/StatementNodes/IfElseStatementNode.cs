using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;

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
        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);
            FalseStatement = Items[4].Compile(param) as StatementNode;

            return this;
        }
    }
}
