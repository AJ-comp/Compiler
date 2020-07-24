using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.StatementNodes
{
    public class IfStatementNode : StatementNode
    {
        public IfStatementNode(AstSymbol node) : base(node)
        {
        }



        // [0] : TerminalNode [if]
        // [1] : ExprNode
        // [2] : StatementNode [statement]
        public override SdtsNode Build(SdtsParams param)
        {
            var node0 = Items[1].Build(param);
            var node1 = Items[2].Build(param.CloneForNewBlock());

            if(node0 is BinaryExprNode)
            {
                if ((node0 as BinaryExprNode).Result.IsZero)
                    ; // add node1 is can't reached.
            }

            return this;
        }
    }
}
