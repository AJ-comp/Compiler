using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes
{
    public class IfElseStatementNode : CondStatementNode
    {
        public StatementNode FalseStatement { get; private set; }

        public IfElseStatementNode(AstSymbol node) : base(node)
        {
        }


        // [0] : TerminalNode [if]
        // [1] : ExprNode
        // [2] : StatementNode [statement]
        // [3] : TerminalNode [else]
        // [4] : StatementNode [statement]
        public override SdtsNode Build(SdtsParams param)
        {
            base.Build(param);
            FalseStatement = Items[4].Build(param.CloneForNewBlock()) as StatementNode;

            return this;
        }
    }
}
