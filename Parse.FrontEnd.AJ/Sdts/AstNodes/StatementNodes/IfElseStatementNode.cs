using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class IfElseStatementNode : IfStatementNode
    {
        public override StatementNode FalseStatement => _falseStatement;


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
            _falseStatement = Items[4].Build(param.CloneForNewBlock()) as StatementNode;

            return this;
        }


        public StatementNode _falseStatement;
    }
}
