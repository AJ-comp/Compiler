using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes
{
    public abstract class CondStatementNode : StatementNode
    {
        public ExprNode Condition { get; protected set; }
        public StatementNode TrueStatement { get; protected set; }


        protected CondStatementNode(AstSymbol node) : base(node)
        {
        }


        // [0] : TerminalNode [if]
        // [1] : ExprNode
        // [2] : StatementNode [statement]
        public override SdtsNode Build(SdtsParams param)
        {
            Condition = Items[1].Build(param) as ExprNode;

            TrueStatement = Items[2].Build(param) as StatementNode;

            /*
            if(node0 is BinaryExprNode)
            {
                var cNode = (node0 as BinaryExprNode);
                if (cNode.Result == null) return this;

                bool isZero = (bool)cNode.Result.Value;

                if (isZero) node1.IsNotUsed = true;
            }
            */

            return this;
        }
    }
}
